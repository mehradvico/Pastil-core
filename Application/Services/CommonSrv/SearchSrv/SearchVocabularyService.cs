using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.SearchSrv
{
    /// <summary>
    /// واژگان جستجو را در حافظه نگه می‌دارد (Singleton). بار اول هم‌زمان ساخته می‌شود؛ بعد از آن هر ۱۰ دقیقه در پس‌زمینه تازه
    /// می‌شود و در این فاصله نسخه‌ی قبلی جواب می‌دهد. اگر ساختش خطا بدهد واژگان خالی برمی‌گردد و جستجو مثل قبل (بدون هوشمندی) کار می‌کند.
    /// </summary>
    public sealed class SearchVocabularyService : ISearchVocabularyService
    {
        private static readonly TimeSpan RefreshEvery = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan RetryAfterFailure = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SearchVocabularyService> _logger;
        private readonly Slot _products;
        private readonly Slot _global;

        public SearchVocabularyService(IServiceScopeFactory scopeFactory, ILogger<SearchVocabularyService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _products = new Slot("products", LoadProductTexts);
            _global = new Slot("global", LoadGlobalTexts);
        }

        public SearchVocabulary Products => Get(_products);
        public SearchVocabulary Global => Get(_global);

        private SearchVocabulary Get(Slot slot)
        {
            var current = slot.Current;
            if (current == null)
            {
                lock (slot.Gate)
                {
                    if (slot.Current == null) Refresh(slot);
                    return slot.Current;
                }
            }

            if (DateTime.UtcNow >= slot.NextRefreshUtc && Interlocked.CompareExchange(ref slot.Refreshing, 1, 0) == 0)
            {
                _ = Task.Run(() =>
                {
                    try { Refresh(slot); }
                    finally { Interlocked.Exchange(ref slot.Refreshing, 0); }
                });
            }
            return current;
        }

        private void Refresh(Slot slot)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IDataBaseContext>();
                slot.Current = SearchVocabulary.Build(slot.Load(context));
                slot.NextRefreshUtc = DateTime.UtcNow.Add(RefreshEvery);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Search vocabulary '{Name}' could not be built; search continues without smart matching.", slot.Name);
                slot.Current ??= SearchVocabulary.Empty;
                slot.NextRefreshUtc = DateTime.UtcNow.Add(RetryAfterFailure);
            }
        }

        // همان محصولاتی که مشتری می‌بیند (فعال و غیر پیش‌نویس)؛ اصلاح املایی به کلمه‌ای که فقط در محصول مخفی هست نتیجه‌ی خالی می‌داد
        private static List<string> LoadProductTexts(IDataBaseContext context)
        {
            var texts = new List<string>();
            foreach (var product in context.Products.AsNoTracking()
                         .Where(p => !p.Deleted && p.Active && p.StatusId != (long)ProductStatusEnum.ProductStatus_Draft)
                         .Select(p => new { p.Name, p.SecondName })
                         .ToList())
            {
                texts.Add(product.Name);
                texts.Add(product.SecondName);
            }
            return texts;
        }

        private static List<string> LoadGlobalTexts(IDataBaseContext context)
        {
            var texts = LoadProductTexts(context);
            foreach (var brand in context.Brands.AsNoTracking().Where(b => !b.Deleted && b.Active).Select(b => new { b.Name, b.SecondName }).ToList())
            {
                texts.Add(brand.Name);
                texts.Add(brand.SecondName);
            }
            texts.AddRange(context.Categories.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.FeatureItems.AsNoTracking().Where(x => !x.Deleted).Select(x => x.Name).ToList());
            texts.AddRange(context.Stores.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Companions.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Clinics.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Groomers.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Coaches.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Pansions.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Schools.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.SchoolCourses.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.Assistances.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.CompanionAssistancePackages.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            texts.AddRange(context.ConsultationPackages.AsNoTracking().Where(x => !x.Deleted && x.Active).Select(x => x.Name).ToList());
            return texts;
        }

        private sealed class Slot(string name, Func<IDataBaseContext, List<string>> load)
        {
            public readonly string Name = name;
            public readonly Func<IDataBaseContext, List<string>> Load = load;
            public readonly object Gate = new();
            public volatile SearchVocabulary Current;
            public DateTime NextRefreshUtc;
            public int Refreshing;
        }
    }
}
