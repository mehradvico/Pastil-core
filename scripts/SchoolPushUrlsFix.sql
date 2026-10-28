-- اصلاح لینک پوش‌های جدید مدرسه (۱۶۹ تا ۱۷۲) تا با مسیرهای وب‌اپ بخواند (۲۰۲۶-۱۰-۲۸).
-- دیتابیسی که SchoolCourseFeatures را قبل از این اصلاح اجرا کرده. چند بار اجرا کردنش بی‌خطر است.
UPDATE PushPatterns SET Url = N'/school/course-{1}'       WHERE PushTypeId = 169;
UPDATE PushPatterns SET Url = N'/school/course-{2}'       WHERE PushTypeId = 170;
UPDATE PushPatterns SET Url = N'/school/course-{1}'       WHERE PushTypeId = 171;
UPDATE PushPatterns SET Url = N'/school/certificate-{2}'  WHERE PushTypeId = 172;
SELECT PushTypeId, Url FROM PushPatterns WHERE PushTypeId BETWEEN 169 AND 172;
