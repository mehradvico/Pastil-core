using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Enumerable.Code
{
    public enum PushMessageTypeEnum
    {
        PushMessageType_Admin = 131,
        PushMessageType_Companion = 132,
        PushMessageType_Pansion = 133,
        PushMessageType_EndUser = 134,
        PushMessageType_Store = 135,
        PushMessageType_Operator = 136,
        PushMessageType_All = 137,
        // مخاطب «ارائه‌دهندگان» به تفکیک نقش (۲۰۲۶-۱۱-۱۵). مقدار عددی فقط برای مرتب‌بودن است؛ Code در دیتابیس با Label خوانده می‌شود
        // (فروشگاه = PushMessageType_Store و پانسیون = PushMessageType_Pansion که از قبل بودند).
        PushMessageType_Clinic = 138,
        PushMessageType_Coach = 139,
        PushMessageType_Groomer = 140,
        PushMessageType_School = 141
    }
}
