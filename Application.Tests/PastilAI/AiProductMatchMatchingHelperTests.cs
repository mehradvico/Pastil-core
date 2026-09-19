using Application.Services.ProductSrvs.AiProductMatchSrv;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
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
    public void Deduplicated_rows_keep_the_bounding_boxes_of_every_removed_duplicate()
    {
        var box = (double x) => new AiProductMatchBoundingBoxDto { X = x, Y = 0.1, Width = 0.2, Height = 0.3 };
        var rows = new List<AiProductMatchWorkingRow>
        {
            new() { RowId = "shelf-1-1", Name = "کنسرو گربه شایر ۴۰۰ گرم", BoundingBoxes = { box(0.1) }, NameConfidence = 0.6 },
            new() { RowId = "shelf-1-2", Name = "کنسرو گربه شایر ۴۰۰ گرم", BoundingBoxes = { box(0.4), box(0.7) }, NameConfidence = 0.9 }
        };

        var result = AiProductMatchMatchingHelper.DeduplicateRows(rows);

        Assert.Single(result);
        Assert.Equal(3, result[0].BoundingBoxes.Count);
        Assert.Equal(0.9, result[0].NameConfidence);
    }

    [Fact]
    public void Gemini_scale_boxes_are_converted_to_relative_top_left_boxes()
    {
        var boxes = AiProductMatchMatchingHelper.NormalizeBoxes(new[] { new[] { 100.0, 200.0, 500.0, 600.0 } });

        var box = Assert.Single(boxes);
        Assert.Equal(0.2, box.X);
        Assert.Equal(0.1, box.Y);
        Assert.Equal(0.4, box.Width);
        Assert.Equal(0.4, box.Height);
    }

    [Fact]
    public void Already_relative_boxes_are_kept_as_is()
    {
        var boxes = AiProductMatchMatchingHelper.NormalizeBoxes(new[] { new[] { 0.1, 0.2, 0.5, 0.6 } });

        Assert.Equal(0.2, Assert.Single(boxes).X);
    }

    [Fact]
    public void Out_of_image_boxes_are_clamped_and_tiny_or_inverted_boxes_are_dropped()
    {
        var boxes = AiProductMatchMatchingHelper.NormalizeBoxes(new[]
        {
            new[] { -50.0, 900.0, 400.0, 1300.0 },
            new[] { 100.0, 100.0, 110.0, 400.0 },
            new[] { 500.0, 500.0, 300.0, 300.0 }
        });

        var box = Assert.Single(boxes);
        Assert.Equal(0.0, box.Y);
        Assert.Equal(0.1, box.Width);
        Assert.Equal(0.4, box.Height);
    }

    [Fact]
    public void Package_size_is_formatted_in_persian_and_null_when_unreadable()
    {
        Assert.Equal("۴۰۰ گرم", AiProductMatchMatchingHelper.FormatPackageSize(400, "gram"));
        Assert.Equal("۳.۵ کیلوگرم", AiProductMatchMatchingHelper.FormatPackageSize(3.5, "kilogram"));
        Assert.Null(AiProductMatchMatchingHelper.FormatPackageSize(null, "gram"));
        Assert.Null(AiProductMatchMatchingHelper.FormatPackageSize(400, null));
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
