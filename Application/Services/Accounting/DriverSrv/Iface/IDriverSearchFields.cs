namespace Application.Services.Accounting.DriverSrv.Iface
{
    public interface IDriverSearchFields
    {
        public long? OwnerId { get; set; }
        public string Vehicle { get; set; }
        public int? Rate { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public long? StatusId { get; set; }
        public bool? Approved { get; set; }
        public long? VehicleTypeId { get; set; }
    }
}
