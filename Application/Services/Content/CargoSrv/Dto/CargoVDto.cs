using Application.Common.Dto.Field;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.CargoSrv.Dto
{
    public class CargoVDto : Id_FieldDto
    {
        public DateTime DateGone { get; set; }
        public DateTime? DateReturn { get; set; }
        public DateTime CreateDate { get; set; }
        public long FromStateId { get; set; }
        public long ToStateId { get; set; }
        public long UserPetId { get; set; }
        public bool Accompany { get; set; }
        public double Price { get; set; }
        public double DefaultPrice { get; set; }
        public double? NotAccompanyPrice { get; set; }
        public double? ReturnPrice { get; set; }
        public bool IsPaid { get; set; }
        public long StatusId { get; set; }
        public string StatusDetail { get; set; }

        public double Discount { get; set; }
        public double PaymentPrice { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }


        public UserPetVDto UserPet { get; set; }
        public StateVDto FromState { get; set; }
        public StateVDto ToState { get; set; }
        public CodeVDto Status { get; set; }
        public RebateVDto Rebate { get; set; }
    }
}
