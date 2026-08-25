using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Trip : Id_Field
    {
        public Point Origin { get; set; }
        public Point Destination { get; set; }
        public Point SecondDestination { get; set; }
        public long RouteLength { get; set; }
        public long? FromCityId { get; set; }
        public bool RoundTrip { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public long? DriverId { get; set; }
        public double Price { get; set; }
        public string UserDetail { get; set; }
        public string UserComment { get; set; }
        public int UserRate { get; set; }
        public string ConnectionId { get; set; }
        public string UserToken { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? TripStartDateTime { get; set; }
        public bool IsOnline { get; set; }
        public long DriverStatusId { get; set; }
        public long TripStatusId { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? ManualPayDate { get; set; }
        public long? UserPetId { get; set; }
        public long UserId { get; set; }
        public long? TripStopId { get; set; }
        public double DriverShare { get; set; }
        public double SiteShare { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public double Discount { get; set; }
        public double PaymentPrice { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }

        // پیشرفت ریز سفر (TripProgressStageEnum) — مستقل از TripStatusId، فقط برای وضعیت Accepted معنا دارد.
        public int ProgressStageId { get; set; }
        // برای سفر رفت‌وبرگشت: بعد از ArrivedDestination، اگر RoundTrip=true، این true می‌شود و
        // ProgressStageId دوباره از EnRouteOrigin شروع می‌شود (این‌بار با مبدا/مقصد جابه‌جا‌شده).
        public bool IsReturnLeg { get; set; }
        public DateTime? ProgressUpdateDate { get; set; }

        // اتصال به رزرو کلینیک/نماینده (فقط برای سفرهای «رزرو هم‌زمان»، حالت دو پت‌رسان)
        public long? CompanionReserveId { get; set; }
        public int? ScheduledLeadMinutes { get; set; }
        public DateTime? ScheduledDepartureAt { get; set; }
        public bool OwnerRidesAlong { get; set; }
        public bool ScheduledDispatched { get; set; }

        public Code DriverStatus { get; set; }
        public Code TripStatus { get; set; }
        public UserPet UserPet { get; set; }
        public Driver Driver { get; set; }
        public City FromCity { get; set; }
        public TripStop TripStop { get; set; }
        public Rebate Rebate { get; set; }
        public Wallet Wallet { get; set; }
        public User User { get; set; }
        public CompanionReserve CompanionReserve { get; set; }
        public ICollection<TripOption> TripOptions { get; set; }
        public ICollection<TripPet> TripPets { get; set; }
    }
}
