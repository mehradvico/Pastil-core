using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using System;
using System.Collections.Concurrent;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    public class AiProductMatchJobState
    {
        public string JobId { get; set; }
        public string Status { get; set; } // "processing" | "completed" | "failed"
        public int TotalBatches { get; set; }
        public int CompletedBatches { get; set; }
        public AiProductMatchAnalyzeResultDto Result { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public interface IAiProductMatchJobStore
    {
        AiProductMatchJobState Create(int totalBatches);
        void ReportProgress(string jobId, int completedBatches);
        void Complete(string jobId, AiProductMatchAnalyzeResultDto result);
        void Fail(string jobId, string errorMessage, int errorCode);
        AiProductMatchJobState Get(string jobId);
    }

    // فقط در حافظه — طول عمر یک Job چند ده ثانیه تا چند دقیقه است و فقط برای گزارش پیشرفت به همون
    // فروشنده در همون Session استفاده می‌شه. اگه سرور ری‌استارت بشه، Jobهای در حال پردازش گم می‌شن
    // (فرانت باید دوباره تحلیل رو شروع کنه)؛ برای این حجم و این نوع مصرف، دیتابیس/Redis اضافه‌کاریه.
    public class AiProductMatchJobStore : IAiProductMatchJobStore
    {
        private static readonly TimeSpan JobTtl = TimeSpan.FromMinutes(15);
        private readonly ConcurrentDictionary<string, AiProductMatchJobState> _jobs = new();

        public AiProductMatchJobState Create(int totalBatches)
        {
            Prune();
            var job = new AiProductMatchJobState
            {
                JobId = Guid.NewGuid().ToString("N"),
                Status = "processing",
                TotalBatches = totalBatches,
                CreatedAtUtc = DateTime.UtcNow
            };
            _jobs[job.JobId] = job;
            return job;
        }

        public void ReportProgress(string jobId, int completedBatches)
        {
            if (_jobs.TryGetValue(jobId, out var job))
                job.CompletedBatches = completedBatches;
        }

        public void Complete(string jobId, AiProductMatchAnalyzeResultDto result)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
                return;

            job.Status = "completed";
            job.Result = result;
            job.CompletedBatches = job.TotalBatches;
        }

        public void Fail(string jobId, string errorMessage, int errorCode)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
                return;

            job.Status = "failed";
            job.ErrorMessage = errorMessage;
            job.ErrorCode = errorCode;
        }

        public AiProductMatchJobState Get(string jobId)
            => !string.IsNullOrWhiteSpace(jobId) && _jobs.TryGetValue(jobId, out var job) ? job : null;

        private void Prune()
        {
            var cutoff = DateTime.UtcNow - JobTtl;
            foreach (var pair in _jobs)
                if (pair.Value.CreatedAtUtc < cutoff)
                    _jobs.TryRemove(pair.Key, out _);
        }
    }
}
