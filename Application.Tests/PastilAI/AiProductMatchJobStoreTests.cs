using Application.Services.ProductSrvs.AiProductMatchSrv;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using Xunit;

namespace Application.Tests.PastilAI;

public class AiProductMatchJobStoreTests
{
    [Fact]
    public void A_new_job_starts_as_processing_with_zero_completed_batches()
    {
        var store = new AiProductMatchJobStore();

        var job = store.Create(totalBatches: 10);

        Assert.Equal("processing", job.Status);
        Assert.Equal(10, job.TotalBatches);
        Assert.Equal(0, job.CompletedBatches);
        Assert.NotNull(store.Get(job.JobId));
    }

    [Fact]
    public void Progress_reports_are_reflected_when_read_back()
    {
        var store = new AiProductMatchJobStore();
        var job = store.Create(totalBatches: 5);

        store.ReportProgress(job.JobId, 3);

        Assert.Equal(3, store.Get(job.JobId).CompletedBatches);
        Assert.Equal("processing", store.Get(job.JobId).Status);
    }

    [Fact]
    public void Completing_a_job_stores_the_result_and_marks_all_batches_done()
    {
        var store = new AiProductMatchJobStore();
        var job = store.Create(totalBatches: 4);
        var result = new AiProductMatchAnalyzeResultDto();

        store.Complete(job.JobId, result);

        var loaded = store.Get(job.JobId);
        Assert.Equal("completed", loaded.Status);
        Assert.Same(result, loaded.Result);
        Assert.Equal(4, loaded.CompletedBatches);
    }

    [Fact]
    public void Failing_a_job_stores_the_error_and_never_a_result()
    {
        var store = new AiProductMatchJobStore();
        var job = store.Create(totalBatches: 2);

        store.Fail(job.JobId, "provider unavailable", 6);

        var loaded = store.Get(job.JobId);
        Assert.Equal("failed", loaded.Status);
        Assert.Equal(6, loaded.ErrorCode);
        Assert.Null(loaded.Result);
    }

    [Fact]
    public void An_unknown_job_id_returns_null_instead_of_throwing()
    {
        var store = new AiProductMatchJobStore();

        Assert.Null(store.Get("does-not-exist"));
        Assert.Null(store.Get(null));
    }
}
