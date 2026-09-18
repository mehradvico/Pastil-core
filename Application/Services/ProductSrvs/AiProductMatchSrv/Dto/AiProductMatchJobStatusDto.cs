namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    public class AiProductMatchJobStatusDto
    {
        // "processing" | "completed" | "failed"
        public string Status { get; set; }
        public int TotalBatches { get; set; }
        public int CompletedBatches { get; set; }

        // فقط وقتی Status == "completed" پر می‌شود؛ شکل دقیقاً همان AiProductMatchAnalyzeResultDto موجود است.
        public AiProductMatchAnalyzeResultDto Result { get; set; }
    }
}
