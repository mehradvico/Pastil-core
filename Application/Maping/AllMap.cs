using Application.Common.Dto.LocationPoint;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.PermissionSrv.Dto;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.Accounting.RolePermission.Dto;
using Application.Services.Accounting.ScoreTransactionSrv.Dto;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Accounting.UserProductSrv.Dto;
using Application.Services.Accounting.UserTokenSrv.Dto;
using Application.Services.CategorySrv.Dto;
using Application.Services.CommonSrv.CommentLikeSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceQuestionnaireSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackagePictureSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentRateSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.Content.ContactUsGroupSrv.Dto;
using Application.Services.Content.ContactUsItemSrv.Dto;
using Application.Services.Content.ContactUsSrv.Dto;
using Application.Services.Content.DetailSrv.Dto;
using Application.Services.Content.DiscussionAnswerLikeSrv.Dto;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Services.Content.GalleryItemSrv.Dto;
using Application.Services.Content.GallerySrv.Dto;
using Application.Services.Content.HashtagSrv.Dto;
using Application.Services.Content.NewsletterSrv.Dto;
using Application.Services.Content.PostCommentSrv.Dto;
using Application.Services.Content.PostFileSrv.Dto;
using Application.Services.Content.PostPictureSrv.Dto;
using Application.Services.Content.PostProductSrv.Dto;
using Application.Services.Content.PostSrv.Dto;
using Application.Services.Content.StaticPageSrv.Dto;
using Application.Services.Content.StoryGroupSrv.Dto;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Content.StoryUserLikeSrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.FinanceSrvs.BankCardSrv.Dto;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.CountrySrv.Dto;
using Application.Services.LocationFields.LocationSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.LocationFields.ParkPictureSrv.Dto;
using Application.Services.LocationFields.ParkSrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.LocationFields.UserCurrentLocationSrv.Dto;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.BankSrv.Dto;
using Application.Services.Order.CartItemSrv.Dto;
using Application.Services.Order.CartSrv.Dto;
using Application.Services.Order.CartStoreSrv.Dto;
using Application.Services.Order.DeliveryDistanceSrv.Dto;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.ProductOrderItemSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Application.Services.PansionSrvs.PansionPetSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto;
using Application.Services.ProductSrvs.BrandCategorySrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.DiscountGroupSrv.Dto;
using Application.Services.ProductSrvs.DiscountSrv.Dto;
using Application.Services.ProductSrvs.FeatureItemSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Dto;
using Application.Services.ProductSrvs.ProductCommentSrv.Dto;
using Application.Services.ProductSrvs.ProductFeatureValueSrv.Dto;
using Application.Services.ProductSrvs.ProductFileSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductLikeSrv.Dto;
using Application.Services.ProductSrvs.ProductPictureSrv.Dto;
using Application.Services.ProductSrvs.ProductReportSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.StoreCommentSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.ProductSrvs.VarietyItemSrv.Dto;
using Application.Services.ProductSrvs.VarietySrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Dto;
using Application.Services.ReminderSrvs.ReminderTypeSrv.Dto;
using Application.Services.Setting.BaseDetailSrv.Dto;
using Application.Services.Setting.CodeGroupSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Application.Services.Setting.MapKeyService.Dto;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Application.Services.TripSrv.TripOptionSrv.Dto;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripStopSrv.Dto;
using Application.Services.WeekDaySrv.WeekDaySrv.Dto;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.LocationField;
using Entities.Entities.PansionField;
using Entities.Entities.PastilMatchField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using Resource;
using System;
using System.Linq;

namespace Application.Maping
{
    public class AllMap : Profile
    {

        public AllMap()
        {
            //From A to Z


            //Address
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<Address, AddressVDto>().ReverseMap();
            CreateMap<Address, AdminAddressVDto>();
            //Address End ----------------------------------------------


            //Assistance
            CreateMap<AssistanceGroup, AssistanceGroupDto>().ReverseMap();
            CreateMap<AssistanceGroup, AssistanceGroupVDto>();
            CreateMap<Assistance, AssistanceDto>().ReverseMap();
            CreateMap<Assistance, AssistanceVDto>();
            CreateMap<AssistanceQuestionnaire, AssistanceQuestionnaireDto>().ReverseMap();
            CreateMap<AssistanceQuestionnaire, AssistanceQuestionnaireVDto>();
            CreateMap<Expertise, ExpertiseDto>().ReverseMap();
            CreateMap<Expertise, ExpertiseVDto>();
            //Assistance End ----------------------------------------------


            //Bank
            CreateMap<Bank, BankDto>().ReverseMap();
            CreateMap<Bank, BankVDto>();
            CreateMap<BankCard, BankCardDto>().ReverseMap();
            CreateMap<BankCard, BankCardVDto>();
            CreateMap<UserBankCard, UserBankCardDto>();
            CreateMap<UserBankCardDto, UserBankCard>().ForMember(x => x.CreateDate, y => y.Ignore()).ForMember(x => x.LastUpdateDate, y => y.Ignore()).ForMember(x => x.Approved, y => y.Ignore()).ForMember(x => x.AdminDetail, y => y.Ignore());
            CreateMap<UserBankCard, UserBankCardApproveDto>().ReverseMap();
            CreateMap<UserBankCard, UserBankCardVDto>();
            //Bank End ----------------------------------------------


            //Banner
            CreateMap<Banner, BannerDto>().ReverseMap();
            CreateMap<Banner, BannerVDto>();
            //Banner End ----------------------------------------------


            //Brand
            CreateMap<Brand, BrandVDto>();
            CreateMap<Brand, BrandDto>();
            CreateMap<BrandDto, Brand>().ForMember(x => x.Picture, y => y.Ignore());
            CreateMap<Brand, BrandCategoryDto>().ForMember(x => x.BrandDto, y => y.MapFrom(y => y)).ForMember(x => x.CategoryDto, y => y.MapFrom(y => y.Categories));
            //Brand End ----------------------------------------------


            //Cargo
            CreateMap<Cargo, CargoDto>();
            CreateMap<CargoDto, Cargo>().ForMember(x => x.Wallet, y => y.Ignore()).ForMember(x => x.FromWallet, y => y.Ignore()).ForMember(x => x.WalletPrice, y => y.Ignore()).ForMember(x => x.RebatePrice, y => y.Ignore()).ForMember(x => x.Discount, y => y.Ignore()).ForMember(x => x.RebateId, y => y.Ignore()).ForMember(x => x.Rebate, y => y.Ignore()).ForMember(x => x.Status, y => y.Ignore()).ForMember(x => x.Price, y => y.Ignore()).ForMember(x => x.UserPet, y => y.Ignore()).ForMember(x => x.FromState, y => y.Ignore()).ForMember(x => x.ToState, y => y.Ignore()).ForMember(x => x.DefaultPrice, y => y.Ignore()).ForMember(x => x.NotAccompanyPrice, y => y.Ignore()).ForMember(x => x.ReturnPrice, y => y.Ignore()).ForMember(x => x.StatusDetail, y => y.Ignore());
            CreateMap<Cargo, CargoVDto>();
            CreateMap<CargoUpdateStatusDto, Cargo>();
            CreateMap<CargoSetRebateCodeDto, Cargo>();
            CreateMap<CargoSetWalletDto, Cargo>();
            //CargoEnd ----------------------------------------------


            //Cart
            CreateMap<Cart, CartVDto>().ReverseMap();
            CreateMap<CartItem, CartItemVDto>().ReverseMap();
            CreateMap<Cart, ProductOrderDto>().ForMember(x => x.DiscountPrice, o => o.MapFrom(m => m.BasePrice - m.Price)).ForMember(x => x.ProductOrderStores, o => o.MapFrom(m => m.CartStores.Where(s => s.Active))).ForMember(x => x.DeliveryTypeId, o => o.MapFrom(m => m.Delivery.DeliveryTypeId));
            CreateMap<CartStore, ProductOrderStoreDto>().ForMember(x => x.DiscountPrice, o => o.MapFrom(m => m.BasePrice - m.Price)).ForMember(x => x.ProductOrderItems, o => o.MapFrom(m => m.CartItems)).ForMember(x => x.Id, y => y.Ignore());
            CreateMap<CartStore, ProductOrderDto>();
            CreateMap<CartStore, CartStoreDto>().ReverseMap();
            CreateMap<CartStore, CartStoreVDto>();
            CreateMap<CartItem, ProductOrderItemDto>().ForMember(x => x.DiscountPrice, o => o.MapFrom(m => m.ProductItem.BasePrice - m.ProductItem.Price)).ForMember(x => x.DiscountPercent, o => o.MapFrom(m => m.ProductItem.DiscountPercent)).ForMember(x => x.BasePrice, o => o.MapFrom(m => m.ProductItem.BasePrice)).ForMember(x => x.Price, o => o.MapFrom(m => m.ProductItem.Price)).ForMember(x => x.Id, y => y.Ignore());
            //Cart End ----------------------------------------------


            //Category
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>().ForMember(x => x.Picture, y => y.Ignore()).ForMember(x => x.Icon, y => y.Ignore());
            CreateMap<Category, CategoryVDto>();
            CreateMap<Category, CategoryParentVDto>();
            CreateMap<Category, CategoryChildrenVDto>();
            CreateMap<Category, CategoryCompleteVDto>();
            CreateMap<Category, SearchCategoryDto>();
            //Category End ----------------------------------------------


            //ClubReward
            CreateMap<ClubReward, ClubRewardDto>().ReverseMap();
            CreateMap<ClubReward, ClubRewardVDto>();
            //ClubReward End ----------------------------------------------


            //Code
            CreateMap<CodeGroup, CodeGroupDto>().ReverseMap();
            CreateMap<Code, CodeDto>().ReverseMap();
            CreateMap<Code, CodeVDto>();
            //Code End ----------------------------------------------


            //ContactUs
            CreateMap<ContactUs, ContactUsDto>().ReverseMap();
            CreateMap<ContactUs, ContactUsVDto>().ReverseMap();
            CreateMap<ContactUsItem, ContactUsItemDto>().ReverseMap();
            CreateMap<ContactUsGroup, ContactUsGroupDto>().ReverseMap();
            CreateMap<ContactUsItem, ContactUsItemDto>().ReverseMap();
            //ContactUs End ----------------------------------------------


            //Comment
            CreateMap<CommentLike, CommentLikeDto>().ReverseMap();
            //Comment End ----------------------------------------------


            //Companion
            CreateMap<Companion, CompanionGoldAccountDto>().ReverseMap();
            CreateMap<Companion, CompanionSilverAccountDto>().ReverseMap();
            CreateMap<Companion, CompanionDto>();
            CreateMap<Companion, CompanionActivationDto>().ReverseMap();
            CreateMap<CompanionDto, Companion>()
                .ForMember(x => x.CompanionPets, y => y.Ignore()).ForMember(x => x.CompanionTypes, y => y.Ignore()).ForMember(x => x.ActivationValue, y => y.Ignore())
                .ForMember(x => x.RateAvg, y => y.Ignore()).ForMember(x => x.RateCount, y => y.Ignore()).ForMember(x => x.Picture, y => y.Ignore())
                .ForMember(x => x.Icon, y => y.Ignore()).ForMember(x => x.GoldAccountDate, y => y.Ignore()).ForMember(x => x.SilverAccountDate, y => y.Ignore())
                .ForMember(x => x.SilverAccountCreateDate, y => y.Ignore()).ForMember(x => x.Owner, y => y.Ignore()).ForMember(x => x.BackgroundPicture, y => y.Ignore())
                .ForMember(x => x.ReferralCode, y => y.Ignore());
            CreateMap<Companion, CompanionVDto>()
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.Picture))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
                .ForMember(dest => dest.IsGold, opt => opt.MapFrom(src => src.GoldAccountDate.HasValue && src.GoldAccountDate > DateTime.Now))
                .ForMember(dest => dest.IsSilver, opt => opt.MapFrom(src => src.SilverAccountDate.HasValue && src.SilverAccountDate > DateTime.Now))
                .ForMember(dest => dest.HasPansion, opt => opt.MapFrom(src => src.Pansions != null && src.Pansions.Any()));
            CreateMap<Companion, NearbyCompanionVDto>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
                .ForMember(dest => dest.NeighborhoodName, opt => opt.MapFrom(src => src.Neighborhood != null ? src.Neighborhood.Name : null))
                .ForMember(dest => dest.IsGold, opt => opt.MapFrom(src => src.GoldAccountDate.HasValue && src.GoldAccountDate > DateTime.Now))
                .ForMember(dest => dest.IsSilver, opt => opt.MapFrom(src => src.SilverAccountDate.HasValue && src.SilverAccountDate > DateTime.Now))
                .ForMember(dest => dest.HasPansion, opt => opt.MapFrom(src => src.Pansions != null && src.Pansions.Any()))
                .ForMember(dest => dest.DistanceMeter, opt => opt.Ignore())
                .ForMember(dest => dest.HasServiceZone, opt => opt.Ignore())
                .ForMember(dest => dest.IsInServiceArea, opt => opt.Ignore());
            CreateMap<Companion, CompanionFinanceVDto>();
            CreateMap<Companion, CompanionFinanceDetailVDto>();
            CreateMap<Companion, CompanionMinVDto>()
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.IsGold, opt => opt.MapFrom(src => src.GoldAccountDate.HasValue && src.GoldAccountDate > DateTime.Now))
                .ForMember(dest => dest.IsSilver, opt => opt.MapFrom(src => src.SilverAccountDate.HasValue && src.SilverAccountDate > DateTime.Now));
            CreateMap<CompanionAssistanceDto, CompanionAssistance>()
                .ForMember(x => x.ActivationValue, y => y.Ignore()).ForMember(x => x.Companion, y => y.Ignore()).ForMember(x => x.Assistance, y => y.Ignore());
            CreateMap<CompanionAssistance, CompanionAssistanceDto>()
                .ForMember(dest => dest.CompanionAssistanceTypeIds, opt => opt.MapFrom(src => src.Codes.Select(s => s.Id)));
            CreateMap<CompanionAssistanceDto, CompanionAssistance>()
                .ForMember(dest => dest.CompanionType, opt => opt.Ignore());
            CreateMap<CompanionAssistance, CompanionAssistanceVDto>();
            CreateMap<CompanionAssistance, CompanionAssistanceFinanceVDto>().ForMember(d => d.HasCommission, o => o.MapFrom(s => s.CommissionPercent > 0));
            CreateMap<CompanionAssistance, CompanionAssistanceActivationDto>().ReverseMap();
            CreateMap<CompanionAssistancePackageDto, CompanionAssistancePackage>()
                .ForMember(x => x.ActivationValue, y => y.Ignore()).ForMember(x => x.CompanionAssistance, y => y.Ignore());
            CreateMap<CompanionAssistancePackage, CompanionAssistancePackageDto>();
            CreateMap<CompanionAssistancePackage, CompanionAssistancePackageActivationDto>().ReverseMap();
            CreateMap<CompanionAssistancePackage, CompanionAssistancePackageVDto>();
            CreateMap<CompanionAssistancePackagePicture, CompanionAssistancePackagePictureDto>().ReverseMap();
            CreateMap<CompanionAssistancePackagePicture, CompanionAssistancePackagePictureVDto>();
            CreateMap<CompanionAssistanceTime, CompanionAssistanceTimeDto>().ReverseMap();
            CreateMap<CompanionAssistanceTime, CompanionAssistanceTimeVDto>();
            CreateMap<IGrouping<WeekDay, CompanionAssistanceTime>, CompanionAssistanceTimeUpdateDto>()
                .ForMember(s => s.WeekDay, y => y.MapFrom(m => m.Key)).ForMember(s => s.CompanionAssistanceTimes, y => y.MapFrom(m => m.ToList()));
            CreateMap<CompanionAssistanceUserDto, CompanionAssistanceUser>()
                .ForMember(x => x.ActivationValue, y => y.Ignore()).ForMember(x => x.CompanionAssistance, y => y.Ignore()).ForMember(x => x.User, y => y.Ignore());
            CreateMap<CompanionAssistanceUser, CompanionAssistanceUserDto>();
            CreateMap<CompanionAssistanceUser, CompanionAssistanceUserActivationDto>().ReverseMap();
            CreateMap<CompanionAssistanceUser, CompanionAssistanceUserVDto>();
            CreateMap<CompanionReserve, CompanionReserveCancelDto>().ReverseMap();
            CreateMap<CompanionReserve, CompanionReserveOperatorDto>().ReverseMap();
            CreateMap<CompanionReserveDto, CompanionReserve>()
                .ForMember(x => x.ReserveCode, y => y.Ignore())
                .ForMember(x => x.FromWallet, y => y.Ignore()).ForMember(x => x.WalletPrice, y => y.Ignore()).ForMember(x => x.Wallet, y => y.Ignore())
                .ForMember(x => x.Address, y => y.Ignore()).ForMember(x => x.Booker, y => y.Ignore()).ForMember(x => x.CompanionAssistance, y => y.Ignore())
                .ForMember(x => x.CompanionAssistanceTime, y => y.Ignore()).ForMember(x => x.CompanionAssistanceUser, y => y.Ignore())
                .ForMember(x => x.UserPets, y => y.Ignore()).ForMember(x => x.State, y => y.Ignore()).ForMember(x => x.CompanionAssistanceType, y => y.Ignore())
                .ForMember(x => x.OperatorStateId, y => y.Ignore()).ForMember(x => x.CompanionAssistancePackages, y => y.Ignore());
            CreateMap<CompanionReserveSetRebateCodeDto, CompanionReserve>();
            CreateMap<CompanionReserveSetWalletDto, CompanionReserve>();
            CreateMap<CompanionReserve, CompanionReserveAdminVDto>();
            CreateMap<CompanionReserveChangeStateDto, CompanionReserve>().ReverseMap();
            CreateMap<CompanionReserve, CompanionReserveDto>();
            CreateMap<CompanionReserve, CompanionUpdateCommissionDto>().ReverseMap();
            CreateMap<CompanionReserve, CompanionReserveVDto>().ForMember(d => d.PetCount, opt => opt.MapFrom(s => s.UserPets.Count));
            CreateMap<CompanionReserveComment, CompanionReserveCommentDto>();
            CreateMap<CompanionReserveCommentDto, CompanionReserveComment>().ForMember(x => x.CompanionReserveCommentRates, y => y.Ignore());
            CreateMap<CompanionReserveComment, CompanionReserveCommentVDto>();
            CreateMap<CompanionReserveCommentRate, CompanionReserveCommentRateDto>().ReverseMap();
            CreateMap<CompanionReserveCommentRate, CompanionReserveCommentRateVDto>();
            CreateMap<CompanionUser, CompanionUserVDto>();
            CreateMap<CompanionUser, CompanionUserDto>();
            CreateMap<CompanionUserDto, CompanionUser>().ForMember(x => x.Companion, y => y.Ignore()).ForMember(x => x.User, y => y.Ignore()).ForMember(x => x.UserId, y => y.Ignore());
            CreateMap<CompanionReportDto, CompanionReport>().ForMember(x => x.User, y => y.Ignore()).ForMember(x => x.Companion, y => y.Ignore());
            CreateMap<CompanionReport, CompanionReportDto>();
            CreateMap<CompanionReport, CompanionReportVDto>();
            CreateMap<CompanionAssistanceReportDto, CompanionAssistanceReport>().ForMember(x => x.CompanionAssistance, y => y.Ignore()).ForMember(x => x.User, y => y.Ignore());
            CreateMap<CompanionAssistanceReport, CompanionAssistanceReportDto>();
            CreateMap<CompanionAssistanceReport, CompanionAssistanceReportVDto>();
            CreateMap<CompanionInsurancePackage, CompanionInsurancePackageDto>();
            CreateMap<CompanionInsurancePackageDto, CompanionInsurancePackage>().ForMember(x => x.Pet, y => y.Ignore()).ForMember(x => x.Companion, y => y.Ignore()).ForMember(x => x.Active, y => y.Ignore());
            CreateMap<CompanionInsurancePackage, CompanionInsurancePackageVDto>();
            CreateMap<CompanionInsurancePackageActivationDto, CompanionInsurancePackage>();
            CreateMap<CompanionInsurancePackageSale, CompanionInsurancePackageSaleDto>();
            CreateMap<CompanionInsurancePackageSaleSetWalletDto, CompanionInsurancePackageSale>();
            CreateMap<CompanionInsurancePackageSaleDto, CompanionInsurancePackageSale>()
                .ForMember(x => x.Wallet, y => y.Ignore()).ForMember(x => x.FromWallet, y => y.Ignore()).ForMember(x => x.WalletPrice, y => y.Ignore())
                .ForMember(x => x.PaymentPrice, y => y.Ignore()).ForMember(x => x.Discount, y => y.Ignore()).ForMember(x => x.RebatePrice, y => y.Ignore())
                .ForMember(x => x.RebateId, y => y.Ignore()).ForMember(x => x.Rebate, y => y.Ignore()).ForMember(x => x.Address, y => y.Ignore()).ForMember(x => x.UserPet, y => y.Ignore())
                .ForMember(x => x.Address, y => y.Ignore()).ForMember(x => x.IsPaid, y => y.Ignore()).ForMember(x => x.Price, y => y.Ignore())
                .ForMember(x => x.CompanionInsurancePackage, y => y.Ignore());
            CreateMap<CompanionInsurancePackageSaleSetRebateCodeDto, CompanionInsurancePackageSale>();
            CreateMap<CompanionInsurancePackageSale, CompanionInsurancePackageSaleVDto>();
            CreateMap<WeekDay, WeekDayDto>().ReverseMap();
            CreateMap<WeekDay, WeekDayVDto>();
            CreateMap<CompanionPet, CompanionPetVDto>();
            CreateMap<CompanionPet, CompanionPetDto>();
            CreateMap<CompanionPetDto, CompanionPet>().ForMember(x => x.Pet, y => y.Ignore()).ForMember(x => x.Companion, y => y.Ignore());
            CreateMap<CompanionType, CompanionTypeVDto>();
            CreateMap<CompanionType, CompanionTypeDto>();
            CreateMap<CompanionTypeDto, CompanionType>().ForMember(x => x.Type, y => y.Ignore()).ForMember(x => x.Companion, y => y.Ignore());
            CreateMap<CompanionZone, CompanionZoneDto>().ReverseMap();
            CreateMap<CompanionZone, CompanionZoneVDto>();
            //Companion End ----------------------------------------------


            //City
            CreateMap<City, CityDto>().ReverseMap();
            CreateMap<City, CityVDto>();
            CreateMap<City, CityStateVDto>().ForMember(x => x.StateName, o => o.MapFrom(m => m.State.Name)).AfterMap((src, dest) => dest.FullName = $"{dest.StateName} - {dest.Name}");
            //City End ----------------------------------------------


            //Country
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Country, CountryVDto>();
            //Country End ----------------------------------------------


            //Country
            CreateMap<ClubReward, ClubRewardDto>().ReverseMap();
            CreateMap<ClubReward, ClubRewardVDto>();
            //Country End ----------------------------------------------


            //Delivery
            CreateMap<Delivery, DeliveryDto>().ReverseMap();
            CreateMap<Delivery, DeliveryVDto>();
            CreateMap<Delivery, DeliveryResultVDto>();
            CreateMap<DeliveryDistance, DeliveryDistanceVDto>();
            CreateMap<DeliveryDistance, DeliveryDistanceDto>().ReverseMap();
            //Delivery End ----------------------------------------------


            //Detail
            CreateMap<Detail, DetailDto>().ReverseMap();
            CreateMap<Detail, DetailVDto>().ReverseMap();
            CreateMap<BaseDetail, BaseDetailDto>().ReverseMap();
            //Detail End ----------------------------------------------


            //Discussion
            CreateMap<DiscussionAnswerLike, DiscussionAnswerLikeDto>().ReverseMap();
            CreateMap<DiscussionQuestion, DiscussionQuestionDto>().ReverseMap();
            CreateMap<DiscussionQuestion, DiscussionQuestionVDto>();
            CreateMap<DiscussionAnswer, DiscussionAnswerDto>().ReverseMap();
            CreateMap<DiscussionAnswer, DiscussionAnswerVDto>().ForMember(x => x.UserIsLike, o => o.MapFrom(m => (m.DiscussionAnswerLikes != null && m.DiscussionAnswerLikes.Any()) ? (bool?)m.DiscussionAnswerLikes.First().IsLike : null));
            //Discussion End ----------------------------------------------


            //Discount
            CreateMap<Discount, DiscountVDto>();
            CreateMap<Discount, DiscountDto>();
            CreateMap<DiscountDto, Discount>().ForMember(x => x.Synced, y => y.Ignore());
            CreateMap<DiscountGroup, DiscountGroupVDto>();
            CreateMap<DiscountGroup, DiscountGroupDto>();
            CreateMap<DiscountGroupDto, DiscountGroup>().ForMember(x => x.Picture, y => y.Ignore());
            //Discount End ----------------------------------------------


            //Driver
            CreateMap<Driver, DriverDto>().ReverseMap();
            CreateMap<Driver, DriverVDto>().ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner));
            //Driver End ----------------------------------------------


            //Feature
            CreateMap<Feature, FeatureDto>().ReverseMap();
            CreateMap<Feature, FeatureVDto>().ReverseMap();
            CreateMap<Feature, FeatureMinVDto>();
            CreateMap<FeatureItem, FeatureItemDto>().ReverseMap();
            CreateMap<FeatureItem, FeatureItemVDto>();
            CreateMap<ProductFeatureValue, ProductFeatureValueDto>().ReverseMap();
            CreateMap<ProductFeatureValue, ProductFeatureValueVDto>();
            CreateMap<ProductFeatureValue, ProductFeatureValueMinVDto>();
            CreateMap<Category, CategoryFeatureDto>().ReverseMap();
            CreateMap<Feature, ProductFeatureVDto>();
            CreateMap<IGrouping<Feature, ProductFeatureValue>, ProductFeatureVDto>().ForMember(x => x.ProductFeatureValues, o => o.MapFrom(m => m.ToList()));
            //Feature End ----------------------------------------------


            //File
            CreateMap<File, FileDto>().ReverseMap();
            CreateMap<File, FileVDto>().ForMember(x => x.Url, y => y.MapFrom(w => w.Url + "/" + w.Name));
            //File End ----------------------------------------------


            //Gallery
            CreateMap<Gallery, GalleryVDto>();
            CreateMap<Gallery, GalleryDto>().ReverseMap();
            CreateMap<GalleryItem, GalleryItemDto>().ReverseMap();
            CreateMap<GalleryItem, GalleryItemVDto>();
            //Gallery End ----------------------------------------------


            //Hashtag
            CreateMap<Hashtag, HashtagDto>();
            //Hashtag End ----------------------------------------------


            //Location
            CreateMap<State, LocationBoundaryVDto>();
            CreateMap<City, LocationBoundaryVDto>();
            CreateMap<Neighborhood, LocationBoundaryVDto>();
            //Location End ----------------------------------------------


            //Map
            CreateMap<MapKey, MapKeyDto>().ReverseMap();
            CreateMap<MapKey, MapKeyVDto>();
            //Map End ----------------------------------------------


            //Merchant 
            CreateMap<Merchant, MerchantDto>().ReverseMap();
            CreateMap<Merchant, MerchantVDto>().ReverseMap();
            //Merchant End ----------------------------------------------


            //Neighborhood
            CreateMap<Neighborhood, NeighborhoodDto>().ReverseMap();
            CreateMap<Neighborhood, NeighborhoodVDto>();
            //Neighborhood End ----------------------------------------------


            //NewsLetter
            CreateMap<Newsletter, NewsletterDto>().ReverseMap();
            //NewsLetter ----------------------------------------------


            //Notification
            CreateMap<NoticeType, NoticeTypeVDto>();
            CreateMap<NoticeRead, NoticeReadDto>();
            CreateMap<Notice, NoticeDto>()
                .ForMember(x => x.Metadata, opt => opt.MapFrom(s => NoticeMetadataSerializer.Deserialize(s.MetadataJson)))
                .ForMember(x => x.IsRead, opt => opt.MapFrom(s => s.Read != null));
            CreateMap<Notice, NoticeVDto>()
                .ForMember(x => x.Metadata, opt => opt.MapFrom(s => NoticeMetadataSerializer.Deserialize(s.MetadataJson)))
                .ForMember(x => x.IsRead, opt => opt.MapFrom(s => s.Read != null));
            CreateMap<NoticeDto, Notice>()
                .ForMember(x => x.NoticeType, y => y.Ignore())
                .ForMember(x => x.ActorUser, y => y.Ignore())
                .ForMember(x => x.Read, y => y.Ignore())
                .ForMember(x => x.PushNotifications, y => y.Ignore());
            //Notification


            //Pansion
            CreateMap<Pansion, PansionDto>();
            CreateMap<PansionDto, Pansion>().ForMember(x => x.CommentCount, y => y.Ignore()).ForMember(x => x.RateAvg, y => y.Ignore()).ForMember(x => x.RateCount, y => y.Ignore());
            CreateMap<Pansion, PansionVDto>();
            CreateMap<Pansion, PansionActiveDto>().ReverseMap();
            CreateMap<Pansion, PansionApproveDto>().ReverseMap();
            CreateMap<Pansion, PansionDto>().ReverseMap();
            CreateMap<Pansion, PansionVDto>();
            CreateMap<Pansion, PansionMinVDto>();
            CreateMap<Pansion, PansionFinanceVDto>().ForMember(d => d.HasCommission, o => o.MapFrom(s => s.DailyCommissionPercent > 0 && s.HourlyCommissionPercent > 0));
            CreateMap<PansionComment, PansionCommentDto>().ReverseMap();
            CreateMap<PansionComment, PansionCommentVDto>().ForMember(x => x.CreateDate, o => o.MapFrom(m => m.CreateDate.ToShortDate())).ForMember(x => x.PansionName, o => o.MapFrom(m => m.Pansion.Name));
            CreateMap<PansionPictureDto, PansionPicture>().ForMember(x => x.Id, opt => opt.Ignore()).ForMember(x => x.Picture, opt => opt.Ignore());
            CreateMap<PansionPicture, PansionPictureDto>();
            CreateMap<PansionPicture, PansionPictureVDto>();
            CreateMap<PansionPet, PansionPetDto>().ReverseMap();
            CreateMap<PansionPet, PansionPetVDto>();
            CreateMap<PansionReserve, PansionReserveCancelDto>().ReverseMap();
            CreateMap<PansionReserveDto, PansionReserve>()
                .ForMember(x => x.ReserveCode, y => y.Ignore())
                .ForMember(x => x.FromWallet, y => y.Ignore()).ForMember(x => x.WalletPrice, y => y.Ignore()).ForMember(x => x.Wallet, y => y.Ignore())
                .ForMember(x => x.Booker, y => y.Ignore()).ForMember(x => x.StatusId, y => y.Ignore());
            CreateMap<PansionReserveRebateCodeDto, PansionReserve>();
            CreateMap<PansionReserveWalletDto, PansionReserve>();
            CreateMap<PansionReserveStatusDto, PansionReserve>().ReverseMap();
            CreateMap<PansionReserve, PansionReserveDto>();
            CreateMap<PansionReserve, PansionReserveVDto>();
            CreateMap<PansionReserve, PansionUpdateCommissionDto>();
            CreateMap<Pansion, SearchPansionDto>().ForMember(d => d.Picture, o => o.MapFrom(s => s.Picture));
            //Pansion ----------------------------------------------


            //Park
            CreateMap<Park, ParkDto>().ReverseMap();
            CreateMap<Park, ParkVDto>();
            CreateMap<ParkPicture, ParkPictureDto>().ReverseMap();
            CreateMap<ParkPicture, ParkPictureVDto>();
            //Park End ----------------------------------------------


            //PastilMatch
            CreateMap<PastilMatchProfile, PastilMatchProfileDto>();
            CreateMap<PastilMatchProfileDto, PastilMatchProfile>();
            CreateMap<PastilMatchProfile, PastilMatchProfileVDto>();
            CreateMap<PastilMatchProfileGoal, PastilMatchProfileGoalDto>();
            CreateMap<PastilMatchProfileGoalDto, PastilMatchProfileGoal>();
            CreateMap<PastilMatchProfileGoal, PastilMatchProfileGoalVDto>().ForMember(x => x.PastilMatchGoalName, x => x.MapFrom(s => s.PastilMatchGoal.Name))
                .ForMember(x => x.PastilMatchGoalLabel, x => x.MapFrom(s => s.PastilMatchGoal.Label));
            CreateMap<PastilMatchProfileLike, PastilMatchProfileLikeDto>();
            CreateMap<PastilMatchProfileLikeDto, PastilMatchProfileLike>().ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.CreateDate, x => x.Ignore())
                .ForMember(x => x.LikerProfile, x => x.Ignore()).ForMember(x => x.LikedProfile, x => x.Ignore());
            CreateMap<PastilMatchRequest, PastilMatchRequestDto>();
            CreateMap<PastilMatchRequestDto, PastilMatchRequest>()
                .ForMember(x => x.SenderProfile, x => x.Ignore()).ForMember(x => x.ReceiverProfile, x => x.Ignore()).ForMember(x => x.PastilMatchGoal, x => x.Ignore()).ForMember(x => x.Status, x => x.Ignore());
            CreateMap<PastilMatchRequest, PastilMatchRequestVDto>();
            CreateMap<PastilMatch, PastilMatchDto>();
            CreateMap<PastilMatchDto, PastilMatch>().ForMember(x => x.PastilMatchRequest, x => x.Ignore()).ForMember(x => x.FirstProfile, x => x.Ignore())
                .ForMember(x => x.SecondProfile, x => x.Ignore()).ForMember(x => x.PastilMatchGoal, x => x.Ignore()).ForMember(x => x.Status, x => x.Ignore());
            CreateMap<PastilMatch, PastilMatchVDto>();
            CreateMap<PastilMatchBlock, PastilMatchBlockDto>();
            CreateMap<PastilMatchBlockDto, PastilMatchBlock>().ForMember(x => x.BlockerUserId, x => x.Ignore()).ForMember(x => x.Deleted, x => x.Ignore())
                .ForMember(x => x.CreateDate, x => x.Ignore()).ForMember(x => x.BlockerUser, x => x.Ignore()).ForMember(x => x.BlockedUser, x => x.Ignore())
                .ForMember(x => x.PastilMatch, x => x.Ignore());
            CreateMap<PastilMatchBlock, PastilMatchBlockVDto>();
            CreateMap<PastilMatchReport, PastilMatchReportDto>();
            CreateMap<PastilMatchReportDto, PastilMatchReport>().ForMember(x => x.ReporterUserId, x => x.Ignore())
                .ForMember(x => x.AdminDescription, x => x.Ignore()).ForMember(x => x.ReviewDate, x => x.Ignore()).ForMember(x => x.CreateDate, x => x.Ignore())
                .ForMember(x => x.ReporterUser, x => x.Ignore()).ForMember(x => x.ReportedUser, x => x.Ignore()).ForMember(x => x.ReportedProfile, x => x.Ignore())
                .ForMember(x => x.PastilMatch, x => x.Ignore()).ForMember(x => x.PastilMatchMessage, x => x.Ignore()).ForMember(x => x.PastilMatchReportReason, x => x.Ignore());
            CreateMap<PastilMatchReport, PastilMatchReportVDto>().ForMember(x => x.PastilMatchReportReasonTitle, x => x.MapFrom(s => s.PastilMatchReportReason.Title)).ForMember(x => x.PastilMatchReportReasonDescription, x => x.MapFrom(s => s.PastilMatchReportReason.Description));
            CreateMap<PastilMatchReportReason, PastilMatchReportReasonDto>();
            CreateMap<PastilMatchReportReasonDto, PastilMatchReportReason>().ForMember(x => x.Deleted, x => x.Ignore());
            CreateMap<PastilMatchReportReason, PastilMatchReportReasonVDto>();
            CreateMap<PastilMatchMessage, PastilMatchMessageDto>();
            CreateMap<PastilMatchMessageDto, PastilMatchMessage>()
                .ForMember(x => x.IsEdited, x => x.Ignore()).ForMember(x => x.EditDate, x => x.Ignore()).ForMember(x => x.IsPinned, x => x.Ignore())
                .ForMember(x => x.PinDate, x => x.Ignore()).ForMember(x => x.DeliveredDate, x => x.Ignore()).ForMember(x => x.ReadDate, x => x.Ignore())
                .ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.CreateDate, x => x.Ignore()).ForMember(x => x.PastilMatch, x => x.Ignore())
                .ForMember(x => x.SenderProfile, x => x.Ignore()).ForMember(x => x.PastilMatchMessageType, x => x.Ignore()).ForMember(x => x.ReplyToMessage, x => x.Ignore())
                .ForMember(x => x.Attachments, x => x.Ignore()).ForMember(x => x.Reactions, x => x.Ignore());
            CreateMap<PastilMatchMessage, PastilMatchMessageVDto>()
                .ForMember(x => x.Attachments, x => x.MapFrom(s => s.Attachments.Where(a => !a.Deleted).OrderBy(a => a.Order)))
                .ForMember(x => x.Reactions, x => x.MapFrom(s => s.Reactions.Where(r => !r.Deleted)));
            CreateMap<PastilMatchMessage, PastilMatchMessageReplyVDto>().ForMember(x => x.Content, x => x.MapFrom(s => s.Deleted ? null : s.Content));
            CreateMap<PastilMatchMessageAttachment, PastilMatchMessageAttachmentItemVDto>();
            CreateMap<PastilMatchMessageReaction, PastilMatchMessageReactionItemVDto>();
            CreateMap<PastilMatchMessageAttachment, PastilMatchMessageAttachmentDto>();
            CreateMap<PastilMatchMessageAttachmentDto, PastilMatchMessageAttachment>().ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.PastilMatchMessage, x => x.Ignore());
            CreateMap<PastilMatchMessageAttachment, PastilMatchMessageAttachmentVDto>();
            CreateMap<PastilMatchMessageAttachment, PastilMatchMessageAttachmentItemVDto>();
            CreateMap<PastilMatchMessageReaction, PastilMatchMessageReactionDto>();
            CreateMap<PastilMatchMessageReactionDto, PastilMatchMessageReaction>()
                .ForMember(x => x.ReactorProfileId, x => x.Ignore()).ForMember(x => x.Deleted, x => x.Ignore())
                .ForMember(x => x.PastilMatchMessage, x => x.Ignore()).ForMember(x => x.ReactorProfile, x => x.Ignore());
            CreateMap<PastilMatchMessageReaction, PastilMatchMessageReactionVDto>();
            CreateMap<PastilMatchMessageReaction, PastilMatchMessageReactionItemVDto>();
            //PastilMatch End ----------------------------------------------


            //Pet
            CreateMap<Pet, PetDto>();
            CreateMap<PetDto, Pet>().ForMember(x => x.Companions, y => y.Ignore());
            CreateMap<Pet, PetVDto>();
            CreateMap<UserPet, UserPetDto>();
            CreateMap<UserPetDto, UserPet>().ForMember(x => x.UserPetRecords, y => y.Ignore()).ForMember(x => x.Picture, y => y.Ignore());
            CreateMap<UserPet, UserPetVDto>();
            CreateMap<UserPetRecord, UserPetRecordMinVDto>();
            CreateMap<UserPetRecord, UserPetRecordDto>();
            CreateMap<UserPetRecordDto, UserPetRecord>().ForMember(x => x.UserPet, y => y.Ignore()).ForMember(x => x.Operator, y => y.Ignore());
            CreateMap<UserPetRecord, UserPetRecordVDto>();
            CreateMap<UserPetPicture, UserPetPictureDto>().ReverseMap();
            CreateMap<UserPetPicture, UserPetPictureVDto>();
            CreateMap<PetBreed, PetBreedDto>().ReverseMap();
            CreateMap<PetBreed, PetBreedVDto>();
            //Pet End ----------------------------------------------


            //Permission
            CreateMap<Permission, PermissionDto>().ReverseMap();
            CreateMap<Permission, PermissionVDto>();
            CreateMap<Permission, PermissionMenuDto>();
            CreateMap<Role, RolePermissionDto>().ForMember(x => x.RoleDto, y => y.MapFrom(y => y)).ForMember(x => x.PermissionsDto, y => y.MapFrom(y => y.Permissions));
            //Permission End ----------------------------------------------


            //Picture
            CreateMap<Picture, PictureVDto>().ForMember(x => x.BaseUrl, o => o.MapFrom(m => m.Url)).ForMember(x => x.Url, o => o.MapFrom(m => m.Url + "/" + m.Name));
            CreateMap<Picture, PictureDto>().ReverseMap();
            CreateMap<PictureVDto, Picture>().IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
            //Picture End ----------------------------------------------


            //Post
            CreateMap<PostPicture, PostPictureVDto>();
            CreateMap<PostFile, PostFileVDto>();
            CreateMap<Post, PostVDto>().ForMember(x => x.PostPictures, o => o.MapFrom(m => m.PostPictures)).ForMember(x => x.PostFiles, o => o.MapFrom(m => m.PostFiles))/*.ForMember(x => x.CreateDate, o => o.MapFrom(m => m.CreateDate.ToShortDate())).ForMember(x => x.PublishDate, o => o.MapFrom(m => m.PublishDate.ToShortDate()))*/;
            CreateMap<Post, PostDto>().ForMember(x => x.PostPicturesList, o => o.MapFrom(m => m.PostPictures)).ForMember(x => x.PostFilesList, o => o.MapFrom(m => m.PostFiles)).ForMember(x => x.CategoryIds, o => o.MapFrom(m => m.Categories.Select(s => s.Id))).ForMember(x => x.HashTagList, o => o.MapFrom(m => m.Hashtags.Select(s => s.Name)));
            CreateMap<PostDto, Post>().ForMember(x => x.CommentCount, y => y.Ignore()).ForMember(x => x.CreateDate, y => y.Ignore()).ForMember(x => x.VisitCount, y => y.Ignore()).ForMember(x => x.Categories, y => y.Ignore()).ForMember(x => x.PostFiles, y => y.Ignore()).ForMember(x => x.Hashtags, y => y.Ignore()).ForMember(x => x.Picture, y => y.Ignore());
            CreateMap<PostComment, PostCommentVDto>().ForMember(x => x.CreateDate, o => o.MapFrom(m => m.CreateDate.ToShortDate())).ForMember(x => x.PostName, o => o.MapFrom(m => m.Post.Name));
            CreateMap<PostComment, PostCommentDto>().ReverseMap();
            CreateMap<PostFileDto, PostFile>().ForMember(x => x.Id, opt => opt.Ignore()).ForMember(x => x.File, opt => opt.Ignore());
            CreateMap<PostFile, PostFileDto>();
            CreateMap<PostPictureDto, PostPicture>().ForMember(x => x.Id, opt => opt.Ignore()).ForMember(x => x.Picture, opt => opt.Ignore());
            CreateMap<PostPicture, PostPictureDto>();
            CreateMap<Post, PostProductDto>().ForMember(s => s.Products, o => o.MapFrom(m => m.Products));
            //Post End ----------------------------------------------


            //Product
            CreateMap<Product, ProductDto>().ForMember(x => x.CategoryIds, o => o.MapFrom(m => m.Categories.Select(s => s.Id)));
            CreateMap<Product, ProductVDto>();
            CreateMap<Product, ProductGroupVDto>();
            CreateMap<Product, ProductMinVDto>();
            CreateMap<ProductDto, Product>().ForMember(x => x.VarietyId, y => y.Ignore()).ForMember(x => x.Variety2, y => y.Ignore()).ForMember(x => x.Picture, y => y.Ignore()).ForMember(x => x.CreateDate, y => y.Ignore()).ForMember(x => x.ProductPictures, y => y.Ignore());
            CreateMap<ProductDuplicateDto, Product>();
            CreateMap<ProductDuplicateDto, ProductDto>();
            CreateMap<Product, ProductItemForProductDto>().ForMember(x => x.Variety1, o => o.MapFrom(m => m.Variety));
            CreateMap<ProductItemForProductVDto, ProductItemForProductDto>();
            CreateMap<ProductPicture, ProductPictureVDto>();
            CreateMap<ProductPicture, ProductPictureDto>();
            CreateMap<ProductPictureDto, ProductPicture>().ForMember(x => x.Picture, y => y.Ignore());
            CreateMap<ProductFile, ProductFileDto>().ReverseMap();
            CreateMap<ProductFile, ProductFileVDto>();
            CreateMap<ProductComment, ProductCommentDto>().ReverseMap();
            CreateMap<ProductComment, ProductCommentVDto>().ForMember(x => x.UserIsLike, o => o.MapFrom(m => (m.CommentLikes != null && m.CommentLikes.Any()) ? (bool?)m.CommentLikes.First().IsLike : null)).ForMember(x => x.ProductName, o => o.MapFrom(m => m.Product.Name)); ;
            CreateMap<ProductLike, ProductLikeDto>().ReverseMap();
            CreateMap<ProductItemDto, ProductItem>().ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.BasePrice)).ForMember(dest => dest.VarietyItem, opt => opt.Ignore()).ForMember(dest => dest.VarietyItem2, opt => opt.Ignore()).ForMember(dest => dest.Store, opt => opt.Ignore());
            CreateMap<ProductItem, ProductItemDto>().ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.Price));
            CreateMap<ProductItem, ProductItemVDto>().ForMember(dest => dest.VarietyName, opt => opt.MapFrom(src => src.VarietyItem.Name));
            CreateMap<ProductItem, ProductItemFullVDto>();
            CreateMap<ProductItem, ProductItemShowVDto>();
            CreateMap<ProductItem, ProductItemForProductVDto>();
            CreateMap<ProductItemDto, ProductItemListUpdateDto>();
            CreateMap<Product, ProductItemForProductDto>().ForMember(x => x.Variety1, o => o.MapFrom(m => m.Variety));
            CreateMap<ProductItemForProductVDto, ProductItemForProductDto>();
            CreateMap<UserProduct, UserProductDto>().ReverseMap();
            CreateMap<UserProduct, UserProductVDto>();
            CreateMap<ProductReport, ProductReportDto>().ReverseMap();
            CreateMap<ProductReport, ProductReportVDto>();
            //Product End ----------------------------------------------


            //ProductOrder
            CreateMap<ProductOrder, ProductOrderDto>();
            CreateMap<ProductOrderDto, ProductOrder>()
                .ForMember(x => x.OrderCode, y => y.Ignore());
            CreateMap<ProductOrder, ProductOrderVDto>();
            CreateMap<ProductOrderStore, ProductOrderStoreDto>().ReverseMap();
            CreateMap<ProductOrderStore, ProductOrderStoreVDto>();
            CreateMap<ProductOrderItem, ProductOrderItemDto>().ReverseMap();
            CreateMap<ProductOrderItem, ProductOrderItemVDto>();
            //ProductOrder End ----------------------------------------------


            //Payment
            CreateMap<PaymentDto, Payment>().ForMember(x => x.User, y => y.Ignore()).ForMember(x => x.Merchant, y => y.Ignore()).ForMember(x => x.Type, y => y.Ignore());
            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<Payment, PaymentVDto>();
            CreateMap<PaymentStartDto, Payment>().ForMember(x => x.User, y => y.Ignore()).ForMember(x => x.Merchant, y => y.Ignore());
            //Payment End ----------------------------------------------


            //Point
            CreateMap<Point, PointDto>().ConvertUsing(source => source == null ? null : new PointDto(source.X, source.Y){ DistanceMeter = 0});
            CreateMap<PointDto, Point>().ConvertUsing(source => source == null ? null : new Point(source.x, source.y){SRID = 4326});
            //Point End ----------------------------------------------

            //UserCurrentLocation
            CreateMap<UserCurrentLocationDto, UserCurrentLocation>()
                .ForMember(x => x.User, y => y.Ignore())
                .ForMember(x => x.City, y => y.Ignore())
                .ForMember(x => x.Neighborhood, y => y.Ignore());
            CreateMap<UserCurrentLocation, UserCurrentLocationDto>();
            CreateMap<UserCurrentLocation, UserCurrentLocationVDto>();
            //UserCurrentLocation End ----------------------------------------------


            //Push
            CreateMap<PushMessage, PushMessageDto>().ReverseMap();
            CreateMap<PushMessage, PushMessageVDto>();
            CreateMap<PushMessage, PushPayloadDto>().ForMember(d => d.Icon, o => o.MapFrom(s => s.Picture.Url));
            CreateMap<PushSubscribeDto, PushSubscription>().ForMember(d => d.P256dh, o => o.MapFrom(s => s.Keys.P256dh)).ForMember(d => d.Auth, o => o.MapFrom(s => s.Keys.Auth))
                     .ForMember(d => d.Endpoint, o => o.MapFrom(s => s.Endpoint)).ForMember(d => d.UserAgent, o => o.MapFrom(s => s.UserAgent))
                     .ForMember(d => d.Id, o => o.Ignore()).ForMember(d => d.UserId, o => o.Ignore()).ForMember(d => d.DeviceKey, o => o.Ignore())
                     .ForMember(d => d.CreateDate, o => o.Ignore()).ForMember(d => d.LastSeen, o => o.Ignore()).ForMember(d => d.IsActive, o => o.Ignore());
            //Push End ----------------------------------------------


            //Rebate 
            CreateMap<RebateDto, Rebate>().ForMember(x => x.UsedCount, y => y.Ignore());
            CreateMap<Rebate, RebateDto>();
            CreateMap<Rebate, RebateVDto>();
            //Rebate End ----------------------------------------------


            //Reminder
            CreateMap<ReminderType, ReminderTypeDto>().ReverseMap();
            CreateMap<ReminderType, ReminderTypeVDto>();
            CreateMap<ReminderCycle, ReminderCycleDto>().ReverseMap();
            CreateMap<ReminderCycle, ReminderCycleVDto>();
            CreateMap<Reminder, ReminderDto>();
            CreateMap<ReminderDto, Reminder>().ForMember(x => x.UserPet, y => y.Ignore()).ForMember(x => x.ReminderCycle, y => y.Ignore()).ForMember(x => x.ReminderType, y => y.Ignore());
            CreateMap<Reminder, ReminderVDto>();
            //Reminder End ----------------------------------------------


            //ScoreTransaction
            CreateMap<ScoreTransaction, ScoreTransactionDto>().ReverseMap();
            CreateMap<ScoreTransaction, ScoreTransactionVDto>();
            //ScoreTransaction End ----------------------------------------------


            //Settlement
            CreateMap<Settlement, SettlementDto>().ReverseMap();
            CreateMap<Settlement, SettlementVDto>();
            CreateMap<SettlementStoreDto, SettlementStore>().ReverseMap();
            CreateMap<SettlementCompanionDto, SettlementCompanion>().ReverseMap();
            //Settlement End ----------------------------------------------


            //State
            CreateMap<State, StateDto>().ReverseMap();
            CreateMap<State, StateVDto>();
            //State End ----------------------------------------------


            //StaticPage
            CreateMap<StaticPage, StaticPageDto>().ReverseMap();
            CreateMap<StaticPage, StaticPageVDto>();
            //StaticPage ----------------------------------------------


            //Store
            CreateMap<Store, StoreDto>();
            CreateMap<StoreDto, Store>().ForMember(x => x.Users, opt => opt.Ignore()).ForMember(x => x.RateAvg, y => y.Ignore()).ForMember(x => x.RateCount, y => y.Ignore()).ForMember(x => x.CommentCount, y => y.Ignore()).ForMember(x => x.MaxDiscountPercent, y => y.Ignore()).ForMember(x => x.Picture, y => y.Ignore()).ForMember(x => x.Icon, y => y.Ignore()).ForMember(x => x.ReferralCode, y => y.Ignore());
            CreateMap<Store, StoreMinVDto>().ForMember(x => x.StoreId, o => o.MapFrom(m => m.Id));
            CreateMap<Store, StoreVDto>();
            CreateMap<Store, StoreFinanceVDto>().ForMember(d => d.HasCommission, o => o.MapFrom(s => s.CommissionPercent != 0));
            CreateMap<StoreComment, StoreCommentVDto>().ForMember(x => x.CreateDate, o => o.MapFrom(m => m.CreateDate.ToShortDate())).ForMember(x => x.StoreName, o => o.MapFrom(m => m.Store.Name));
            CreateMap<StoreComment, StoreCommentDto>().ReverseMap();
            CreateMap<Store, SearchStoreDto>().ForMember(d => d.Icon, o => o.MapFrom(s => s.Icon)).ForMember(d => d.Picture, o => o.MapFrom(s => s.Picture));
            //Store End ----------------------------------------------


            //Story
            CreateMap<StoryGroup, StoryGroupDto>().ReverseMap();
            CreateMap<StoryGroup, StoryGroupVDto>();
            CreateMap<StoryItem, StoryItemDto>().ReverseMap();
            CreateMap<StoryItem, StoryItemVDto>();
            CreateMap<StoryUserLike, StoryUserLikeDto>().ReverseMap();
            //Story End ----------------------------------------------


            //Ticket
            CreateMap<Ticket, TicketVDto>().ForMember(destination => destination.CanReply, option => option.Ignore()).ForMember( destination => destination.UnreadCount, option => option.Ignore()).ForMember( destination => destination.LastMessage, option => option.Ignore());
            CreateMap<TicketItem, TicketItemVDto>().ForMember(destination => destination.IsMine, option => option.Ignore()).ForMember( destination => destination.IsFromSupport, option => option.Ignore());
            CreateMap<TicketItem, TicketItemMinVDto>().ForMember(destination => destination.IsMine, option => option.Ignore()).ForMember(destination => destination.IsFromSupport, option => option.Ignore());
            //Ticket End ----------------------------------------------


            //Trip
            CreateMap<Trip, TripDto>();
            CreateMap<TripDto, Trip>().ForMember(x => x.FromWallet, y => y.Ignore()).ForMember(x => x.WalletPrice, y => y.Ignore()).ForMember(x => x.Wallet, y => y.Ignore()).ForMember(x => x.PaymentPrice, y => y.Ignore()).ForMember(x => x.Discount, y => y.Ignore()).ForMember(x => x.RebatePrice, y => y.Ignore()).ForMember(x => x.RebateId, y => y.Ignore()).ForMember(x => x.Rebate, y => y.Ignore()).ForMember(x => x.IsPaid, y => y.Ignore()).ForMember(x => x.ManualPayDate, y => y.Ignore()).ForMember(x => x.DriverStatus, y => y.Ignore()).ForMember(x => x.TripStatus, y => y.Ignore()).ForMember(x => x.UserPet, y => y.Ignore()).ForMember(x => x.Driver, y => y.Ignore()).ForMember(x => x.FromCity, y => y.Ignore()).ForMember(x => x.TripStop, y => y.Ignore()).ForMember(x => x.TripOptions, y => y.Ignore());
            CreateMap<Trip, TripVDto>();
            CreateMap<Trip, TripShareDto>().ReverseMap();
            CreateMap<ManualPayTripDto, Trip>();
            CreateMap<TripSetRebateCodeDto, Trip>();
            CreateMap<TripSetWalletDto, Trip>();
            CreateMap<TripAdminChooseDriverDto, Trip>();
            CreateMap<TripChangeStatusDto, Trip>();
            CreateMap<TripDriverChangeStatusDto, Trip>();
            CreateMap<TripUserChangeStatusDto, Trip>();
            CreateMap<TripVDto, TripDto>().ReverseMap();
            CreateMap<TripOption, TripOptionDto>();
            CreateMap<TripOptionDto, TripOption>().ForMember(x => x.Trips, y => y.Ignore());
            CreateMap<TripOption, TripOptionVDto>();
            CreateMap<TripStop, TripStopDto>();
            CreateMap<TripStopDto, TripStop>().ForMember(x => x.Trips, y => y.Ignore());
            CreateMap<TripStop, TripStopVDto>();
            CreateMap<TripAddress, TripAddressDto>().ReverseMap();
            CreateMap<TripAddress, TripAddressVDto>();
            CreateMap<PriceCalculation, PriceCalculationVDto>();
            //Trip End ----------------------------------------------


            //User
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<User, UserDto>().ForMember(x => x.Password, opt => opt.Ignore());
            CreateMap<UserDto, User>()
                .ForMember(x => x.Password, opt => opt.Ignore())
                .ForMember(x => x.Picture, opt => opt.Ignore())
                .ForMember(x => x.ReferralCode, opt => opt.Ignore())
                .ForMember(x => x.RegistrationReferralSource, opt => opt.Ignore())
                .ForMember(x => x.UsedReferralCode, opt => opt.Ignore())
                .ForMember(x => x.ReferredByUserId, opt => opt.Ignore())
                .ForMember(x => x.ReferredByCompanionId, opt => opt.Ignore())
                .ForMember(x => x.ReferredByStoreId, opt => opt.Ignore());
            CreateMap<User, UserVDto>().ForMember(x => x.FullName, o => o.MapFrom(m => string.Format("{0} {1}", m.FirstName, m.LastName))).ForMember(x => x.RoleName, o => o.MapFrom(m => string.Format("{0}", m.Role.Name)));
            CreateMap<User, UserMinVDto>().ForMember(x => x.FullName, o => o.MapFrom(m => string.Format("{0} {1}", m.FirstName, m.LastName)));
            CreateMap<User, CurrentUserDto>()
                .ForMember(x => x.CompanionId, o => o.MapFrom(m => m.Companions.Where(c => !c.Deleted && c.Active && c.Approved).Select(c => (long?)c.Id).FirstOrDefault()))
                .ForMember(x => x.DriverId, o => o.MapFrom(m => m.Driver != null && !m.Driver.Deleted && m.Driver.StatusId == (long)DriverRequestStatusEnum.DriverRequestStatus_Accepted ? (long?)m.Driver.Id : null))
                .ForMember(x => x.RoleEnum, o => o.MapFrom(m => m.Role != null ? m.Role.Label : null))
                .ForMember(x => x.RoleName, o => o.MapFrom(m => m.Role != null ? m.Role.Name : null))
                .ForMember(x => x.UserId, o => o.MapFrom(m => m.Id))
                .ForMember(x => x.IsFemale, o => o.MapFrom(m => m.IsFemale))
                .ForMember(x => x.FullName, o => o.MapFrom(m => $"{m.FirstName} {m.LastName}"))
                .ForMember(x => x.IsCompanionUser, o => o.MapFrom(m =>
                    m.Companions.Any(c => !c.Deleted && c.Active && c.Approved) ||
                    m.CompanionUsers.Any(cu =>
                        !cu.Deleted && cu.Active && cu.UserAccept == true &&
                        !cu.Companion.Deleted && cu.Companion.Active && cu.Companion.Approved)))
                .ForMember(x => x.CanSetExpertise, o => o.MapFrom(m =>
                    m.Companions.Any(c => !c.Deleted && c.Active && c.Approved) ||
                    m.CompanionUsers.Any(cu =>
                        !cu.Deleted && cu.Active && cu.UserAccept == true &&
                        !cu.Companion.Deleted && cu.Companion.Active && cu.Companion.Approved)))
                .ForMember(x => x.Expertise, o => o.MapFrom(m =>
                    m.Companions.Any(c => !c.Deleted && c.Active && c.Approved) ||
                    m.CompanionUsers.Any(cu =>
                        !cu.Deleted && cu.Active && cu.UserAccept == true &&
                        !cu.Companion.Deleted && cu.Companion.Active && cu.Companion.Approved)
                            ? m.Expertise
                            : null))
                .ForMember(x => x.PictureId, o => o.MapFrom(m => m.PictureId))
                .ForMember(x => x.StoreId, o => o.MapFrom(m => m.Stores.Where(c => !c.Deleted).Select(c => (long?)c.Id).FirstOrDefault()));

            CreateMap<SignUpDto, UserDto>()
                .ForMember(x => x.ReferralCode, opt => opt.Ignore())
                .ForMember(x => x.RegistrationReferralSource, opt => opt.Ignore())
                .ForMember(x => x.UsedReferralCode, opt => opt.Ignore())
                .ForMember(x => x.ReferredByUserId, opt => opt.Ignore())
                .ForMember(x => x.ReferredByCompanionId, opt => opt.Ignore())
                .ForMember(x => x.ReferredByStoreId, opt => opt.Ignore());
            CreateMap<CreateUserTokenDto, UserTokenDto>();
            CreateMap<CreateUserTokenDto, UserToken>();
            //User End ----------------------------------------------


            //Variety
            CreateMap<Variety, VarietyDto>().ReverseMap();
            CreateMap<Variety, VarietyVDto>();
            CreateMap<Variety, VarietyShowVDto>();
            CreateMap<VarietyItem, VarietyItemDto>().ReverseMap();
            CreateMap<VarietyItem, VarietyItemVDto>().ForMember(x => x.VarietyName, y => y.MapFrom(w => w.Variety.Name));
            CreateMap<VarietyItem, VarietyItemMinVDto>();
            CreateMap<IGrouping<VarietyItem, ProductItem>, VarietyItemShowVDto>().ForMember(x => x.VarietyItem, o => o.MapFrom(m => m.Key)).ForMember(x => x.ProductItems, o => o.MapFrom(m => m.Where(s => s.SystemActive).ToList()));
            //Variety End ----------------------------------------------


            //Wallet
            CreateMap<Wallet, WalletDto>().ReverseMap();
            CreateMap<Wallet, WalletVDto>()
                .ForMember(destination => destination.PaymentCode,
                    options => options.MapFrom(source => source.Payment == null ? null : source.Payment.PaymentCode))
                .ForMember(destination => destination.ReferenceCode,
                    options => options.MapFrom(source =>
                        source.Payment != null ? source.Payment.PaymentCode :
                        source.ProductOrder != null ? source.ProductOrder.OrderCode :
                        source.CompanionReserve != null ? source.CompanionReserve.ReserveCode :
                        source.PansionReserve != null ? source.PansionReserve.ReserveCode : null));
            CreateMap<Wallet, Wallet>().ForMember(x => x.Id, y => y.Ignore()).ForMember(x => x.ProductOrderId, y => y.Ignore()).ForMember(x => x.ProductOrder, y => y.Ignore());
            //Wallet End ----------------------------------------------
        }
    }
}
