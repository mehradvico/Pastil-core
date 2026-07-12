using System;

namespace Application.Common.Dto.LocationPoint
{
    public class PointDto
    {
        public PointDto()
        {
        }

        public PointDto(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x { get; set; }
        public double y { get; set; }
        public string Location => FormattableString.Invariant($"{x},{y}");
        public double DistanceMeter { get; set; }
    }
}
