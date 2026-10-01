using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Storage;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [Authorize]
    public class PictureUploadController : ControllerBase
    {
        private readonly IPictureService pictureService;
        private readonly IObjectStorageService objectStorageService;
        public PictureUploadController(IPictureService pictureService, IObjectStorageService objectStorageService)
        {
            this.pictureService = pictureService;
            this.objectStorageService = objectStorageService;
        }

        private async Task SaveAsync(string relativeDir, string fileName, Stream content, string contentType, CancellationToken cancellationToken)
        {
            if (objectStorageService.IsConfigured)
            {
                content.Position = 0;
                await objectStorageService.UploadAsync($"{relativeDir}/{fileName}", content, contentType, cancellationToken);
            }
            else
            {
                var diskDir = Path.Combine("wwwroot", relativeDir.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(diskDir);
                content.Position = 0;
                await using var fs = System.IO.File.Create(Path.Combine(diskDir, fileName));
                await content.CopyToAsync(fs, cancellationToken);
            }
        }

        [HttpPost]
        [EnableRateLimiting("PictureUpload")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 15 * 1024 * 1024)]
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

            if (allowVideoExtensions.Contains(extension))
            {
                await using var signatureStream = PictureFile.OpenReadStream();
                if (!await UploadGuards.UploadSignature.MatchesExtensionAsync(signatureStream, extension, HttpContext.RequestAborted))
                    return Ok(new BaseResultDto(false, Resource.Notification.FileNotAllow));
            }

            var originalName = Path.GetFileName(PictureFile.FileName);
            var guid = Guid.NewGuid().ToString("N");
            var relativeDir = string.Join('/', "Media", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());

            if (allowVideoExtensions.Contains(extension))
            {
                await using (var vs = PictureFile.OpenReadStream())
                {
                    await SaveAsync(relativeDir, guid + extension, vs, PictureFile.ContentType, HttpContext.RequestAborted);
                }

                var dtoVideo = new PictureDto
                {
                    Size = PictureFile.Length,
                    ContentType = PictureFile.ContentType,
                    CreateDate = now,
                    Extension = extension,
                    Name = guid + extension,
                    GuidName = guid,
                    Url = "/" + relativeDir,
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

            using var mainBuffer = new MemoryStream();
            await image.SaveAsync(mainBuffer, encoder);
            var mainSize = mainBuffer.Length;
            await SaveAsync(relativeDir, guid + ".webp", mainBuffer, "image/webp", HttpContext.RequestAborted);

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
                using var thumbBuffer = new MemoryStream();
                await clone.SaveAsync(thumbBuffer, encoder);
                await SaveAsync(relativeDir, $"{guid}-{s.Key}.webp", thumbBuffer, "image/webp", HttpContext.RequestAborted);
            }

            var dto = new PictureDto
            {
                Size = mainSize,
                ContentType = "image/webp",
                CreateDate = now,
                Extension = ".webp",
                Name = guid + ".webp",
                GuidName = guid,
                Url = "/" + relativeDir,
                OrginalName = originalName
            };

            var result = await pictureService.InsertAsyncDto(dto);
            return Ok(result);
        }


        [HttpPut]
        [Authorize(Policy = "AdminOnly")]
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
