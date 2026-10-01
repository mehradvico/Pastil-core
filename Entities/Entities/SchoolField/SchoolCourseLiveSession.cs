using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.SchoolField
{
    // پخشِ زنده‌ی «پاستیل لایو» برای یک جلسه‌ی مشخصِ یک دوره‌ی زنده (Live). هر SchoolCourseSession
    // حداکثر یک ردیف اینجا دارد - همین که مربی روی «شروع» بزند ساخته می‌شود.
    // StatusId: Application.Common.Enumerable.SchoolLiveStatusEnum (NotStarted/Live/Ended)
    // RecordingStatusId: Application.Common.Enumerable.SchoolLiveRecordingStatusEnum
    public class SchoolCourseLiveSession : Id_Field
    {
        public long SchoolCourseSessionId { get; set; }
        // نام اتاق روی LiveKit - یکتا، مستقل از Id داخلی (برای جلوگیری از افشای شناسه‌ی دیتابیس در توکن‌ها)
        public string RoomName { get; set; }
        public int StatusId { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? EndedDate { get; set; }

        // انتخاب مربی هنگام شروع لایو
        public bool WantsRecording { get; set; }
        public string RecordingName { get; set; }
        public string RecordingDescription { get; set; }

        // پیگیری ضبط سمت LiveKit Egress - غیرهمزمان کامل می‌شود (وب‌هوک)
        public int RecordingStatusId { get; set; }
        public string EgressId { get; set; }
        // بعد از کامل شدن ضبط، یک ردیف SchoolCourseVideo برای این دوره ساخته می‌شود تا کاربر از
        // همان لیست ویدیوهای دوره (بخش دوره‌های پروفایلش) بتواند دوباره ببیندش - همان مکانیزم قبلی.
        public long? SchoolCourseVideoId { get; set; }

        public SchoolCourseSession SchoolCourseSession { get; set; }
        public SchoolCourseVideo SchoolCourseVideo { get; set; }
        public ICollection<SchoolLiveComment> SchoolLiveComments { get; set; }
    }
}
