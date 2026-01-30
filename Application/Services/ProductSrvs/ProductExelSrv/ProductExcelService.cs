using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.ProductSrvs.FeatureItemSrv.Dto;
using Application.Services.ProductSrvs.ProductExelSrv.Dto;
using Application.Services.ProductSrvs.ProductExelSrv.iface;
using Application.Services.ProductSrvs.ProductFeatureValueSrv.Dto;
using Application.Services.ProductSrvs.ProductFeatureValueSrv.Iface;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using AutoMapper;
using AutoMapper.Features;
using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersianDate.Standard;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductExelSrv
{
    public class ProductExcelService : IProductExcelService
    {

        private static string connectionString;
        private readonly DataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IProductFeatureValueService _productFeatureValueService;

        public ProductExcelService(DataBaseContext context, IConfiguration config, IMapper mapper, IProductService productService, IProductFeatureValueService productFeatureValueService)
        {
            _context = context;
            _mapper = mapper;
            _productService = productService;
            _productFeatureValueService = productFeatureValueService;
            connectionString = config.GetValue<string>(
           "connection");
        }




        public async Task<MemoryStream> ImportProductsAsync(IFormFile file)
        {
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);
            var worksheet = workbook.Worksheet("Products");
            var rows = worksheet.RangeUsed().RowsUsed().Skip(2);
            var green = XLColor.LightGreen;
            var red = XLColor.LightPink;

            foreach (var row in rows)
            {
                try
                {
                    var productDto = new ProductDto
                    {
                        Name = row.Cell(1).GetValue<string>(),
                        ProductLabel = row.Cell(2).GetValue<string>(),
                        SecondName = row.Cell(3).GetValue<string>(),
                        SellLimitCount = row.Cell(4).GetValue<int?>(),
                        CategoryId = row.Cell(5).GetValue<long?>(),
                        BrandId = row.Cell(6).GetValue<long?>(),
                        PictureId = row.Cell(7).GetValue<long?>(),
                        CodeValue = row.Cell(8).GetValue<string>(),
                        StoreId = row.Cell(9).GetValue<long?>(),
                        AdminDescription = row.Cell(10).GetValue<string>(),
                        VarietyId = row.Cell(12).GetValue<long?>(),
                        Variety2Id = row.Cell(13).GetValue<long?>(),
                        Summary = row.Cell(14).GetValue<string>(),
                        Description = row.Cell(15).GetValue<string>(),
                        StatusId = (long)ProductStatusEnum.ProductStatus_Available,
                        TypeId = (long)ProductTypeEnum.ProductType_Product,
                        SellCount = 0,
                        VisitCount = 0,
                        RateAvg = 0,
                        RateCount = 0,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        Active = true
                    };

                    var insertResult = await _productService.InsertAsyncDto(productDto);
                    if (!insertResult.IsSuccess || insertResult.Data?.Id == null)
                    {
                        row.Style.Fill.BackgroundColor = red;
                        continue;
                    }

                    var productId = insertResult.Data.Id;

                    var featureColumns = worksheet.Row(2).CellsUsed().Where(c => c.Address.ColumnNumber >= 16).ToList();

                    var featureValues = new List<ProductFeatureValueDto>();

                    foreach (var featureCol in featureColumns)
                    {
                        var featureName = featureCol.GetValue<string>()?.Trim();
                        if (string.IsNullOrEmpty(featureName))
                            continue;

                        var featureValue = row.Cell(featureCol.Address.ColumnNumber).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(featureValue))
                            continue;

                        var feature = await _context.Features.FirstOrDefaultAsync(f => f.Name == featureName);
                        if (feature == null)
                            continue;

                        featureValues.Add(new ProductFeatureValueDto
                        {
                            ProductId = productId,
                            FeatureId = feature.Id,
                            Name = featureValue
                        });
                    }

                    if (featureValues.Any())
                    {
                        var fvAddDto = new ProductFeatureValueAddDto
                        {
                            ProductId = productId,
                            ProductFeatures = featureValues
                        };

                        var fvResult = _productFeatureValueService.SetForProduct(fvAddDto);
                        if (!fvResult.IsSuccess)
                        {
                            row.Style.Fill.BackgroundColor = red;
                            continue;
                        }
                    }

                    row.Style.Fill.BackgroundColor = green;
                }
                catch
                {
                    row.Style.Fill.BackgroundColor = red;
                }
            }

            var resultStream = new MemoryStream();
            workbook.SaveAs(resultStream);
            resultStream.Position = 0;
            return resultStream;
        }




        public MemoryStream GetProductExcelTemplate()
        {
            var features = _context.Features
                .OrderBy(f => f.Id)
                .Select(f => new { f.Id, f.Name })
                .ToList();

            var emptyList = new List<ProductExcelVDto>();

            var workbook = new XLWorkbook { RightToLeft = true };
            var worksheet = workbook.Worksheets.Add("Products");
            worksheet.RightToLeft = true;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.Columns().Width = 30;

            worksheet.Cell(1, 1).Value = Resource.Lang.ProductsList;
            int totalColumns = 16 + features.Count;
            string lastColumn = XLHelper.GetColumnLetterFromNumber(totalColumns);
            worksheet.Range($"A1:{lastColumn}1").Merge();
            worksheet.Cell(1, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(1, 1).Style.Font.SetFontSize(14);

            // ---- Header ----
            worksheet.Cell(2, 1).Value = Resource.Lang.ProductName;
            worksheet.Cell(2, 2).Value = Resource.Lang.ProductLabel;
            worksheet.Cell(2, 3).Value = Resource.Lang.SecondName;
            worksheet.Cell(2, 4).Value = Resource.Lang.SellLimitCount;
            worksheet.Cell(2, 5).Value = Resource.Lang.CategoryId;
            worksheet.Cell(2, 6).Value = Resource.Lang.BrandId;
            worksheet.Cell(2, 7).Value = Resource.Lang.PictureId;
            worksheet.Cell(2, 8).Value = Resource.Lang.CodeValue;
            worksheet.Cell(2, 9).Value = Resource.Lang.StoreId;
            worksheet.Cell(2, 10).Value = Resource.Lang.AdminDescription;
            worksheet.Cell(2, 11).Value = Resource.Lang.Location;
            worksheet.Cell(2, 12).Value = Resource.Lang.VarietyId;
            worksheet.Cell(2, 13).Value = Resource.Lang.Variety2Id;
            worksheet.Cell(2, 14).Value = Resource.Lang.Summary;
            worksheet.Cell(2, 15).Value = Resource.Lang.Description;

            for (int i = 0; i < features.Count; i++)
            {
                worksheet.Cell(2, 16 + i).Value = features[i].Name;
            }

            var headerRange = worksheet.Range($"A2:{lastColumn}2");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDDDDD");
            headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.SheetView.FreezeRows(2);

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
