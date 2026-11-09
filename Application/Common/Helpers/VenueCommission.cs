namespace Application.Common.Helpers
{
    /// <summary>
    /// قاعده‌ی مشترک کمیسیون: درصد خود خدمت/دوره اگر بیشتر از صفر باشد، وگرنه کمیسیون اصلی مرکز.
    /// </summary>
    public static class VenueCommission
    {
        public static decimal Resolve(decimal itemPercent, decimal venuePercent) =>
            itemPercent > 0 ? itemPercent : venuePercent;
    }
}
