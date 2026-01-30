using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;


namespace File.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PictureUploadController : ControllerBase
    {
        private readonly IPictureService pictureService;
        public PictureUploadController(IPictureService pictureService)
        {
            this.pictureService = pictureService;
        }

        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        public async Task<IActionResult> Post(IFormFile PictureFile)
        {
            const int maxWidth = 8000;
            const int maxHeight = 8000;
            const long maxPixels = 25_000_000;

            var sizes = new Dictionary<string, int>
            {
                ["lg"] = 900,
                ["md"] = 500,
                ["sm"] = 300
            };

            string[] allowPicExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
            string[] allowVideoExtensions = { ".mp4", ".webm", ".ogg" };

            if (PictureFile == null || PictureFile.Length <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.Unsuccess));

            var now = DateTime.Now;
            var extension = Path.GetExtension(PictureFile.FileName)?.ToLower();

            if (string.IsNullOrWhiteSpace(extension) ||
                !(allowPicExtensions.Contains(extension) || allowVideoExtensions.Contains(extension)))
                return Ok(new BaseResultDto(false, Resource.Notification.FileNotAllow));

            var originalName = Path.GetFileName(PictureFile.FileName);
            var guid = Guid.NewGuid().ToString("N");

            string filePath = Path.Combine("wwwroot", "Media", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
            Directory.CreateDirectory(filePath);

            if (allowVideoExtensions.Contains(extension))
            {
                var videoPath = Path.Combine(filePath, guid + extension);
                await using var vs = System.IO.File.Create(videoPath);
                await PictureFile.CopyToAsync(vs);

                var dtoVideo = new PictureDto
                {
                    Size = PictureFile.Length,
                    ContentType = PictureFile.ContentType,
                    CreateDate = now,
                    Extension = extension,
                    Name = guid + extension,
                    GuidName = guid,
                    Url = filePath.Replace("wwwroot", "").Replace("\\", "/"),
                    OrginalName = originalName
                };

                var vr = await pictureService.InsertAsyncDto(dtoVideo);
                return Ok(vr);
            }

            await using var headerStream = PictureFile.OpenReadStream();
            var info = await Image.IdentifyAsync(headerStream);
            if (info == null)
                return Ok(new BaseResultDto(false, Resource.Notification.FileNotAllow));

            if (info.Width > maxWidth || info.Height > maxHeight)
                return Ok(new BaseResultDto(false, Resource.Notification.FileNotAllow));

            if ((long)info.Width * info.Height > maxPixels)
                return Ok(new BaseResultDto(false, Resource.Notification.FileNotAllow));

            await using var imageStream = PictureFile.OpenReadStream();
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);

            bool hasAlpha = image.PixelType.AlphaRepresentation != SixLabors.ImageSharp.PixelFormats.PixelAlphaRepresentation.None;

            var encoder = hasAlpha
                ? new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { FileFormat = SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossless }
                : new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 85 };

            var mainPath = Path.Combine(filePath, guid + ".webp");
            await image.SaveAsync(mainPath, encoder);

            foreach (var s in sizes)
            {
                int width, height;
                if (image.Width <= s.Value)
                {
                    width = image.Width;
                    height = image.Height;
                }
                else
                {
                    width = s.Value;
                    var ratio = image.Height / (float)image.Width;
                    height = (int)(s.Value * ratio);
                }

                using var clone = image.Clone(x => x.Resize(width, height));
                var thumbPath = Path.Combine(filePath, $"{guid}-{s.Key}.webp");
                await clone.SaveAsync(thumbPath, encoder);
            }

            var mainSize = new FileInfo(mainPath).Length;

            var dto = new PictureDto
            {
                Size = mainSize,
                ContentType = "image/webp",
                CreateDate = now,
                Extension = ".webp",
                Name = guid + ".webp",
                GuidName = guid,
                Url = filePath.Replace("wwwroot", "").Replace("\\", "/"),
                OrginalName = originalName
            };

            var result = await pictureService.InsertAsyncDto(dto);
            return Ok(result);
        }


        [HttpPut]
        public IActionResult Put()
        {
            try
            {
                var dic = new Dictionary<string, int>();
                dic.Add("lg", 900);
                dic.Add("md", 500);
                dic.Add("sm", 300);
                const int quality = 85;
                var allPictures = pictureService.GetAll();
                foreach (var pic in allPictures)
                {

                    if (pic.Extension.ToLower() == ".jpg" || pic.Extension.ToLower() == ".jpeg" || pic.Extension.ToLower() == ".png" || pic.Extension.ToLower() == ".webp")
                        foreach (var item in dic)
                        {
                            IImageEncoder encoder = new JpegEncoder { Quality = quality };
                            if (pic.Extension.ToLower() == ".jpg" || pic.Extension.ToLower() == ".jpeg")
                                encoder = new JpegEncoder { Quality = quality };
                            else if (pic.Extension.ToLower() == ".webp")
                                encoder = new WebpEncoder { Quality = quality };
                            else if (pic.Extension.ToLower() == ".png")
                                encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.Level9 };
                            var path = Path.Combine(pic.Url.Replace("/", "\\") + "\\" + pic.Name);
                            path = "wwwroot" + path;
                            if (System.IO.File.Exists(path))
                                using (var image = SixLabors.ImageSharp.Image.Load(Path.Combine(path)))
                                {
                                    int height, width;
                                    if (image.Width <= item.Value)
                                    {
                                        width = image.Width;
                                        height = image.Height;
                                    }
                                    else
                                    {
                                        width = item.Value;
                                        var a = image.Height / (float)image.Width;
                                        height = (int)(item.Value * a);
                                    }
                                    var picname = pic.Name.Split('.')[0] + "-" + item.Key + pic.Extension;

                                    image.Mutate(x => x.Resize(width, height));
                                    var newPath = Path.Combine("wwwroot", pic.Url.Replace("/", "\\") + "\\" + picname);
                                    newPath = "wwwroot" + newPath;

                                    if (System.IO.File.Exists(newPath))
                                    {
                                        System.IO.File.Delete(newPath);
                                    }
                                    image.Save(newPath,
                                    encoder);
                                }
                        }
                }
                return Ok();
            }
            catch
            {
                return Ok(new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess));

            }


        }

    }
}
