using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveExcelSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveExcelSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.ProductSrvs.ProductExelSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using AutoMapper;
using ClosedXML.Excel;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveExcelSrv
{
    internal class CompanionReserveExcelService : ICompanionReserveExcelService
    {
        private static string connectionString;
        private readonly DataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ICompanionReserveService _companionReserveService;

        public CompanionReserveExcelService(DataBaseContext context, IConfiguration config, IMapper mapper, ICompanionReserveService companionReserveService)
        {
            _context = context;
            _mapper = mapper;
            _companionReserveService = companionReserveService;
            connectionString = config.GetValue<string>(
           "connection");
        }

        public MemoryStream GetReserveExcel(SearchCompanionReserveExcelDto search)
        {
            var query = _context.CompanionReserves.Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(p => p.CompanionAssistanceUser).ThenInclude(s => s.User).Include(s => s.Booker).Include(s => s.UserPets).ThenInclude(s => s.Pet).AsQueryable();

            if (search.CompanionId.HasValue)
            {
                query = query.Where(s => s.CompanionAssistance.CompanionId == search.CompanionId);
            }
            if (search.BookerId.HasValue)
            {
                query = query.Where(s => s.BookerId == search.BookerId);
            }
            if (search.CompanionAssistanceId.HasValue)
            {
                query = query.Where(s => s.CompanionAssistanceId == search.CompanionAssistanceId);
            }
            if (search.FromDate.HasValue)
            {
                var from = search.FromDate.Value.Date;
                query = query.Where(s => s.DoneDate != null && s.DoneDate.Value.Date >= from);
            }

            if (search.ToDate.HasValue)
            {
                var to = search.ToDate.Value.Date;
                query = query.Where(s => s.DoneDate != null && s.DoneDate.Value.Date <= to);
            }
            if (search.ReserveState.HasValue)
            {
                if (search.ReserveState.Value == ReserveStateEnum.CompanionReserveState_CurrentDays)
                {
                    query = query.Where(s => s.IsReserved && !s.IsCancel && s.DoneDate == null && s.DoDate >= DateTime.Now);
                }
                if (search.ReserveState.Value == ReserveStateEnum.CompanionReserveState_Done)
                {
                    query = query.Where(s => s.IsReserved && !s.IsCancel && s.DoneDate.HasValue);
                }
                if (search.ReserveState.Value == ReserveStateEnum.CompanionReserveState_Expired)
                {
                    query = query.Where(s => s.IsReserved && !s.IsCancel && s.DoneDate == null && s.DoDate <= DateTime.Now);
                }
            }
            query = query.OrderBy(s => s.Id);

            var list = query.Select(s => new CompanionReserveExcelVDto
            {
                Id = s.Id,
                BookerName = string.Format("{0} {1}", s.Booker.FirstName, s.Booker.LastName),
                CompanionName = s.CompanionAssistance.Companion.Name,
                AssistanceName = s.CompanionAssistance.Assistance.Name,
                OperatorName = s.CompanionAssistanceUserId == null ? "کلینیک" : string.Format("{0} {1}", s.CompanionAssistanceUser.User.FirstName, s.CompanionAssistanceUser.User.LastName),
                PrePaymentPrice = s.PrePaymentPrice,
                PackagePrice = s.PackagePrice,
                OperatorWagesPrice = s.OperatorWagesPrice,
                OperatorStuffPrice = s.OperatorStuffPrice,
                OperatorFinalPrice = s.OperatorFinalPrice,
                OperatorDetail = s.OperatorDetail,
                RebatePrice = s.RebatePrice,
                //SharePercent = s.CompanionAssistance.Companion.SharePercent,
                CompanionShare = s.CompanionShare,
                SiteShare = s.SiteShare,
                PaymentPrice = s.PaymentPrice,
                StateId = s.State.Name,
                CreateDate = s.CreateDate.Date
            })
            .ToList();

            var excel = ExelExport(list);
            return excel;
        }

        private MemoryStream ExelExport(List<CompanionReserveExcelVDto> model)
        {
            var workbook = new XLWorkbook { RightToLeft = true };
            var worksheet = workbook.Worksheets.Add("CompanionReserves");
            worksheet.RightToLeft = true;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.PageSetup.Margins.Top = 0.25;
            worksheet.PageSetup.Margins.Bottom = 0.25;
            worksheet.PageSetup.Margins.Left = 0.25;
            worksheet.PageSetup.Margins.Right = 0.25;
            worksheet.Columns().Width = 35;

            // عنوان بالای صفحه
            worksheet.Row(1).InsertRowsAbove(1);
            worksheet.Cell(1, 1).Value = Resource.Lang.CompanionReserves;
            worksheet.Range("A1:R1").Merge();
            worksheet.Cell(1, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Cell(1, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Cell(1, 1).Style.Font.FontSize = 13;
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            // عناوین ستون‌ها
            string[] headers = {
        Resource.Lang.Row,
        Resource.Lang.ReserveId,
        Resource.Lang.CompanionName,
        Resource.Lang.AssistanceName,
        Resource.Lang.BookerName,
        Resource.Lang.PetType,
        Resource.Lang.CreateDate,
        Resource.Lang.OperatorName,
        Resource.Lang.OperatorDetail,
        Resource.Lang.OperatorWagesPrice,
        Resource.Lang.OperatorStuffPrice,
        Resource.Lang.PrePaymentPrice,
        Resource.Lang.OperatorFinalPrice,
        Resource.Lang.CompanionShare,
        Resource.Lang.SiteShare,
        Resource.Lang.SharePercent,
        Resource.Lang.PaymentPrice,
        Resource.Lang.State
    };

            for (int i = 0; i < headers.Length; i++)
                worksheet.Cell(2, i + 1).Value = headers[i];

            var headerRange = worksheet.Range("A2:R2");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 11;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#CFCFCF");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            // محاسبه و نمایش سطرها
            var persian = new PersianCalendar();

            for (int i = 0; i < model.Count; i++)
            {
                var item = model[i];
                int row = i + 3;

                // تبدیل تاریخ به شمسی
                string persianDate = "";
                if (item.CreateDate != default)
                {
                    var d = item.CreateDate;
                    persianDate = $"{persian.GetYear(d)}/{persian.GetMonth(d):00}/{persian.GetDayOfMonth(d):00}";
                }

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = item.Id;
                worksheet.Cell(row, 3).Value = item.CompanionName;
                worksheet.Cell(row, 4).Value = item.AssistanceName;
                worksheet.Cell(row, 5).Value = item.BookerName;
                worksheet.Cell(row, 6).Value = item.PetType;
                worksheet.Cell(row, 7).Value = persianDate;
                worksheet.Cell(row, 8).Value = item.OperatorName;
                worksheet.Cell(row, 9).Value = item.OperatorDetail;
                worksheet.Cell(row, 10).Value = item.OperatorWagesPrice;
                worksheet.Cell(row, 11).Value = item.OperatorStuffPrice;
                worksheet.Cell(row, 12).Value = item.PrePaymentPrice;
                worksheet.Cell(row, 13).Value = item.OperatorFinalPrice;
                worksheet.Cell(row, 14).Value = item.CompanionShare;
                worksheet.Cell(row, 15).Value = item.SiteShare;
                worksheet.Cell(row, 16).Value = item.SharePercent;
                worksheet.Cell(row, 17).Value = item.PaymentPrice;
                worksheet.Cell(row, 18).Value = item.StateId;

                // استایل ردیف‌ها
                var rowRange = worksheet.Range($"A{row}:R{row}");
                rowRange.Style.Fill.BackgroundColor = i % 2 == 0 ? XLColor.White : XLColor.FromHtml("#E0E0E0");
                rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rowRange.Style.Font.FontSize = 10;
            }

            worksheet.Columns().AdjustToContents();

            // Border کلی
            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E0E0E0");

            // Format ستون‌های عددی (قیمت‌ها)
            string priceFormat = "#,##0";
            worksheet.Column(10).Style.NumberFormat.Format = priceFormat; 
            worksheet.Column(11).Style.NumberFormat.Format = priceFormat;
            worksheet.Column(12).Style.NumberFormat.Format = priceFormat; 
            worksheet.Column(13).Style.NumberFormat.Format = priceFormat;
            worksheet.Column(14).Style.NumberFormat.Format = priceFormat; 
            worksheet.Column(15).Style.NumberFormat.Format = priceFormat; 
            worksheet.Column(17).Style.NumberFormat.Format = priceFormat; 

            // فریز کردن هدر
            worksheet.SheetView.FreezeRows(2);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

    }
}

