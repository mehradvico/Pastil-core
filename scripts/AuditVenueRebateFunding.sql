-- Read-only audit of paid reservations with venue-targeted rebate codes.
-- ShareGap must be zero. OriginalSite/OriginalProvider reconstruct gross shares
-- from the stored rebate funding ratio without using today's commission settings.
WITH Paid AS (
    SELECT 'Coach' AS Venue, r.Id AS ReserveId, r.OnlineAmount AS GrossPrice, r.PaymentPrice,
           r.RebatePrice, r.SiteShare, r.ProviderShare, d.SiteFundedPercent, d.ProviderFundedPercent
    FROM dbo.CoachReserves r JOIN dbo.Rebate d ON d.Id=r.RebateId
    WHERE r.IsReserved=1 AND r.RebatePrice>0 AND d.TargetType=15 AND d.TargetId=r.CoachId
    UNION ALL
    SELECT 'Clinic', r.Id, r.OnlineAmount, r.PaymentPrice, r.RebatePrice, r.SiteShare, r.ProviderShare, d.SiteFundedPercent, d.ProviderFundedPercent
    FROM dbo.ClinicReserves r JOIN dbo.Rebate d ON d.Id=r.RebateId
    WHERE r.IsReserved=1 AND r.RebatePrice>0 AND d.TargetType=14 AND d.TargetId=r.ClinicId
    UNION ALL
    SELECT 'Groomer', r.Id, r.OnlineAmount, r.PaymentPrice, r.RebatePrice, r.SiteShare, r.ProviderShare, d.SiteFundedPercent, d.ProviderFundedPercent
    FROM dbo.GroomerReserves r JOIN dbo.Rebate d ON d.Id=r.RebateId
    WHERE r.IsReserved=1 AND r.RebatePrice>0 AND d.TargetType=13 AND d.TargetId=r.GroomerId
    UNION ALL
    SELECT 'Pansion', r.Id, r.Price, r.PaymentPrice, r.RebatePrice, r.SiteShare, r.CompanionShare, d.SiteFundedPercent, d.ProviderFundedPercent
    FROM dbo.PansionReserves r JOIN dbo.Rebate d ON d.Id=r.RebateId
    WHERE r.IsReserved=1 AND r.RebatePrice>0 AND d.TargetType=8 AND d.TargetId=r.PansionId
    UNION ALL
    SELECT 'School', r.Id, r.Price, r.PaymentPrice, r.RebatePrice, r.SiteShare, r.CompanionShare, d.SiteFundedPercent, d.ProviderFundedPercent
    FROM dbo.SchoolReserves r JOIN dbo.SchoolCourses c ON c.Id=r.SchoolCourseId JOIN dbo.Rebate d ON d.Id=r.RebateId
    WHERE r.IsReserved=1 AND r.RebatePrice>0 AND d.TargetType=12 AND d.TargetId=c.SchoolId
)
SELECT Venue, ReserveId, GrossPrice, PaymentPrice, RebatePrice,
       COALESCE(SiteFundedPercent,0) AS SiteFundingPercent,
       COALESCE(ProviderFundedPercent,100) AS ProviderFundingPercent,
       ROUND(RebatePrice * COALESCE(SiteFundedPercent,0) / 100.0, 0) AS SiteDiscount,
       RebatePrice - ROUND(RebatePrice * COALESCE(SiteFundedPercent,0) / 100.0, 0) AS ProviderDiscount,
       SiteShare, ProviderShare,
       SiteShare + ROUND(RebatePrice * COALESCE(SiteFundedPercent,0) / 100.0, 0) AS OriginalSite,
       ProviderShare + RebatePrice - ROUND(RebatePrice * COALESCE(SiteFundedPercent,0) / 100.0, 0) AS OriginalProvider,
       SiteShare + ProviderShare - PaymentPrice AS ShareGap
FROM Paid
ORDER BY Venue, ReserveId DESC;
