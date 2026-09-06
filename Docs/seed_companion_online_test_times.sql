-- زمان‌های ارائه‌ی خدمت "مربی سگ" (CompanionAssistanceId = 49) برای محمد قادرپناه - چند روز با ساعت‌های متنوع
-- WeekDayId: ۱=شنبه، ۲=یکشنبه، ۳=دوشنبه، ۴=سه‌شنبه، ۵=چهارشنبه، ۶=پنجشنبه، ۷=جمعه

DECLARE @CompanionAssistanceId BIGINT = 49;

INSERT INTO CompanionAssistanceTimes (StartTime, EndTime, Active, Deleted, WeekDayId, CompanionAssistanceId)
VALUES
    (N'09:00', N'13:00', 1, 0, 1, @CompanionAssistanceId), -- شنبه صبح
    (N'15:00', N'19:00', 1, 0, 1, @CompanionAssistanceId), -- شنبه عصر
    (N'10:00', N'14:00', 1, 0, 3, @CompanionAssistanceId), -- دوشنبه
    (N'08:00', N'12:00', 1, 0, 5, @CompanionAssistanceId), -- چهارشنبه صبح
    (N'16:00', N'20:00', 1, 0, 5, @CompanionAssistanceId), -- چهارشنبه عصر
    (N'11:00', N'15:00', 1, 0, 7, @CompanionAssistanceId); -- جمعه

SELECT * FROM CompanionAssistanceTimes WHERE CompanionAssistanceId = @CompanionAssistanceId;
