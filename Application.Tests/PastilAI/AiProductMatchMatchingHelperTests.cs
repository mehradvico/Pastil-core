using Application.Services.ProductSrvs.AiProductMatchSrv;
using System.Collections.Generic;
using Xunit;

namespace Application.Tests.PastilAI;

public class AiProductMatchMatchingHelperTests
{
    [Fact]
    public void Composite_confidence_pulls_down_a_confident_model_score_on_a_weak_name_match()
    {
        var confidence = AiProductMatchMatchingHelper.ComputeCompositeConfidence(
            modelConfidence: 0.95,
            detectedName: "تشویقی سگ پدیگری مرغ ۸۰ گرم",
            detectedBrand: null,
            candidateName: "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم",
            candidateBrandName: "رویال کنین");

        Assert.True(confidence < 0.70);
    }

    [Fact]
    public void Composite_confidence_stays_high_for_a_near_identical_name_and_matching_brand()
    {
        var confidence = AiProductMatchMatchingHelper.ComputeCompositeConfidence(
            modelConfidence: 0.85,
            detectedName: "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم",
            detectedBrand: "رویال کنین",
            candidateName: "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم",
            candidateBrandName: "رویال کنین");

        Assert.True(confidence >= 0.85);
    }

    [Fact]
    public void A_clearly_read_brand_that_conflicts_with_the_candidate_is_heavily_penalized_even_with_a_confident_model_score()
    {
        var confidence = AiProductMatchMatchingHelper.ComputeCompositeConfidence(
            modelConfidence: 0.93,
            detectedName: "غذای خشک گربه ایندور",
            detectedBrand: "پرینا",
            candidateName: "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم",
            candidateBrandName: "رویال کنین");

        Assert.True(confidence < 0.40);
    }

    [Fact]
    public void No_detected_brand_falls_back_to_name_similarity_only()
    {
        var confidence = AiProductMatchMatchingHelper.ComputeCompositeConfidence(
            modelConfidence: 0.85,
            detectedName: "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم",
            detectedBrand: null,
            candidateName: "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم",
            candidateBrandName: "رویال کنین");

        Assert.True(confidence >= 0.85);
    }

    [Fact]
    public void Two_close_candidates_are_flagged_as_ambiguous()
    {
        var isAmbiguous = AiProductMatchMatchingHelper.IsAmbiguous(
            bestConfidence: 0.82, secondConfidence: 0.78, minimumConfidence: 0.60, marginThreshold: 0.08);

        Assert.True(isAmbiguous);
    }

    [Fact]
    public void A_clear_margin_between_candidates_is_not_ambiguous()
    {
        var isAmbiguous = AiProductMatchMatchingHelper.IsAmbiguous(
            bestConfidence: 0.92, secondConfidence: 0.61, minimumConfidence: 0.60, marginThreshold: 0.08);

        Assert.False(isAmbiguous);
    }

    [Fact]
    public void A_weak_second_candidate_does_not_count_toward_ambiguity()
    {
        var isAmbiguous = AiProductMatchMatchingHelper.IsAmbiguous(
            bestConfidence: 0.65, secondConfidence: 0.59, minimumConfidence: 0.60, marginThreshold: 0.08);

        Assert.False(isAmbiguous);
    }

    [Fact]
    public void Duplicate_rows_from_overlapping_shelf_photos_collapse_into_one()
    {
        var rows = new List<AiProductMatchWorkingRow>
        {
            new() { RowId = "shelf-1-1", Name = "غذای خشک گربه رویال کنین ایندور ۲ کیلوگرم" },
            new() { RowId = "shelf-2-1", Name = "غذای خشک گربه رویال كنين ايندور ٢ كيلوگرم" },
            new() { RowId = "shelf-2-2", Name = "تشویقی سگ پدیگری مرغ ۸۰ گرم" }
        };

        var result = AiProductMatchMatchingHelper.DeduplicateRows(rows);

        Assert.Equal(2, result.Count);
        Assert.Equal("shelf-1-1", result[0].RowId);
        Assert.Equal("shelf-2-2", result[1].RowId);
    }

    [Fact]
    public void Duplicate_rows_from_overlapping_table_screenshots_collapse_by_matching_external_code_even_if_the_name_text_differs_slightly()
    {
        var rows = new List<AiProductMatchWorkingRow>
        {
            new() { RowId = "table-1-1", Name = "کنسرو گربه شایر مرغ و بوقلمون 400 گرم", ExternalCode = "SPD-7788" },
            new() { RowId = "table-2-1", Name = "کنسرو شایر مرغ/بوقلمون 400گرم", ExternalCode = "SPD-7788" },
            new() { RowId = "table-2-2", Name = "کنسرو گربه شایر مرغ و بوقلمون 400 گرم", ExternalCode = "SPD-9911" }
        };

        var result = AiProductMatchMatchingHelper.DeduplicateRows(rows);

        Assert.Equal(2, result.Count);
        Assert.Equal("table-1-1", result[0].RowId);
        Assert.Equal("table-2-2", result[1].RowId);
    }

    [Fact]
    public void A_row_without_external_code_still_dedupes_by_name_even_when_other_rows_have_codes()
    {
        var rows = new List<AiProductMatchWorkingRow>
        {
            new() { RowId = "table-1-1", Name = "کنسرو گربه شایر مرغ و بوقلمون 400 گرم", ExternalCode = null },
            new() { RowId = "table-2-1", Name = "کنسرو گربه شایر مرغ و بوقلمون 400 گرم", ExternalCode = null }
        };

        var result = AiProductMatchMatchingHelper.DeduplicateRows(rows);

        Assert.Single(result);
        Assert.Equal("table-1-1", result[0].RowId);
    }
}
