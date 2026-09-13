namespace Application.Services.CompanionSrv.CompanionTimeSrv.Iface
{
    public interface ICompanionTimeSearchFields
    {
        public long? WeekDayId { get; set; }
        public long? CompanionId { get; set; }
        public bool? Active { get; set; }
    }
}
