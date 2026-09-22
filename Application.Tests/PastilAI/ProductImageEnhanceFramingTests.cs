using Application.Services.ProductSrvs.ProductImageEnhanceSrv;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace Application.Tests.PastilAI;

public class ProductImageEnhanceFramingTests
{
    private static byte[] Render(Image<Rgba32> image)
    {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    [Fact]
    public void Off_center_subject_is_cropped_centered_and_padded_on_a_square_white_canvas()
    {
        // محصول ساختگی: یک مستطیل قرمز گوشه‌ی بالا-چپ یک تصویر سفیدِ غیرمربع
        using var source = new Image<Rgba32>(400, 300, Color.White);
        source.Mutate(x => x.Fill(Color.Red, new RectangleF(20, 10, 100, 60)));

        var output = ProductImageEnhanceService.FrameOnWhiteCanvas(Render(source), outputSize: 1024, paddingPercent: 7);

        Assert.NotNull(output);
        using var framed = Image.Load<Rgba32>(output);
        Assert.Equal(1024, framed.Width);
        Assert.Equal(1024, framed.Height);

        // حاشیه باید سفید و دست‌نخورده بماند
        Assert.Equal(new Rgba32(255, 255, 255, 255), framed[2, 2]);
        Assert.Equal(new Rgba32(255, 255, 255, 255), framed[1021, 1021]);

        // محصول باید وسط بوم باشد
        Assert.Equal(new Rgba32(255, 0, 0, 255), framed[512, 512]);

        // و پهنایش نباید از بوم منهای حاشیه‌ی ۷٪ بیشتر شود (۱۰۲۴ - ۲×۷۱ ≈ ۸۸۲)
        var left = 0;
        while (left < framed.Width && framed[left, 512].R >= 245 && framed[left, 512].G >= 245) left++;
        var right = framed.Width - 1;
        while (right > 0 && framed[right, 512].R >= 245 && framed[right, 512].G >= 245) right--;
        Assert.InRange(right - left + 1, 1, 884);
        Assert.InRange(left, 60, 500);
    }

    [Fact]
    public void A_blank_white_image_is_reported_as_having_no_subject()
    {
        using var source = new Image<Rgba32>(300, 300, Color.White);

        Assert.Null(ProductImageEnhanceService.FrameOnWhiteCanvas(Render(source), outputSize: 1024, paddingPercent: 7));
    }
}
