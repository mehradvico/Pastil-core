namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    // مختصات نسبی (۰..۱) نسبت به خود عکس ارسالی، مبدأ گوشهٔ بالا-چپ.
    public class AiProductMatchBoundingBoxDto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
