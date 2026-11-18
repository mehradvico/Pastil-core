using Entities.Entities;
using Entities.Entities.PastilClubField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Helpers
{
    /// <summary>کد اختصاصیِ خودِ مرکز از سهم همان مرکز تأمین می‌شود؛ کد عمومی قواعد قبلی را نگه می‌دارد.</summary>
    public static class VenueFundedRebateRules
    {
        public static bool IsFundedBy(Rebate rebate, ClubRewardTargetTypeEnum type, long venueId) =>
            rebate != null && rebate.TargetType == type && rebate.TargetId == venueId;

        public static decimal SitePercent(Rebate rebate) => rebate?.SiteFundedPercent ?? 0m;
        public static bool ValidPercents(decimal? site, decimal? provider) =>
            (!site.HasValue && !provider.HasValue) ||
            (site is >= 0m and <= 100m && provider is >= 0m and <= 100m &&
             decimal.Round(site.Value, 2) == site.Value && decimal.Round(provider.Value, 2) == provider.Value &&
             site + provider == 100m);

        public static (double SiteDiscount, double ProviderDiscount) DiscountParts(double discount, decimal sitePercent)
        {
            var site = Math.Min(discount, Math.Max(0, (double)Math.Round((decimal)discount * sitePercent / 100m, 0, MidpointRounding.AwayFromZero)));
            return (site, discount - site);
        }

        public static bool CanFund(double grossPrice, double discount, double grossSiteShare, decimal sitePercent)
        {
            if (discount < 0 || sitePercent < 0 || sitePercent > 100) return false;
            var (site, provider) = DiscountParts(discount, sitePercent);
            return site <= grossSiteShare + 0.000001 && provider <= grossPrice - grossSiteShare + 0.000001;
        }

        public static (double Provider, double Site) Split(double grossPrice, double discount, double grossSiteShare, decimal sitePercent)
        {
            if (!CanFund(grossPrice, discount, grossSiteShare, sitePercent))
                throw new ArgumentOutOfRangeException(nameof(discount), "Discount exceeds funding shares.");
            var (site, provider) = DiscountParts(discount, sitePercent);
            return (grossPrice - grossSiteShare - provider, grossSiteShare - site);
        }

        public static List<(double Provider, double Site)> Allocate(
            IReadOnlyList<(double Provider, double Site)> grossShares, double discount, decimal sitePercent)
        {
            var grossSite = grossShares.Sum(x => x.Site);
            var grossProvider = grossShares.Sum(x => x.Provider);
            if (!CanFund(grossSite + grossProvider, discount, grossSite, sitePercent))
                throw new ArgumentOutOfRangeException(nameof(discount));
            var (siteDiscount, providerDiscount) = DiscountParts(discount, sitePercent);
            var result = new List<(double Provider, double Site)>(grossShares.Count);
            decimal siteRemaining = (decimal)siteDiscount, providerRemaining = (decimal)providerDiscount;
            for (var index = 0; index < grossShares.Count; index++)
            {
                var gross = grossShares[index];
                var last = index == grossShares.Count - 1;
                var sitePart = last ? siteRemaining : grossSite <= 0 ? 0m :
                    (decimal)siteDiscount * (decimal)gross.Site / (decimal)grossSite;
                var providerPart = last ? providerRemaining : grossProvider <= 0 ? 0m :
                    (decimal)providerDiscount * (decimal)gross.Provider / (decimal)grossProvider;
                siteRemaining -= sitePart;
                providerRemaining -= providerPart;
                result.Add(((double)((decimal)gross.Provider - providerPart), (double)((decimal)gross.Site - sitePart)));
            }
            return result;
        }

        public static bool CanFund(double grossPrice, double discount, double grossSiteShare) =>
            discount >= 0 && discount <= Math.Max(0, grossPrice - grossSiteShare) + 0.000001;

        public static (double Provider, double Site) Split(double grossPrice, double discount, double grossSiteShare)
        {
            if (!CanFund(grossPrice, discount, grossSiteShare))
                throw new ArgumentOutOfRangeException(nameof(discount), "Discount exceeds venue share.");
            return (grossPrice - grossSiteShare - discount, grossSiteShare);
        }
    }
}
