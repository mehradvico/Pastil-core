using Application.Services.PastilClubSrvs.PointEventSrv;
using Application.Services.PastilClubSrvs.PointEventSrv.Dto;
using Entities.Entities.PastilClubField;
using System;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubPointEventKeyFactoryTests
    {
        [Fact]
        public void BuildMemorySourceKey_ForSameTehranDay_ReturnsSameKey()
        {
            var morning = new DateTimeOffset(2026, 8, 10, 0, 30, 0, TimeSpan.FromHours(3.5));
            var eveningAsUtc = new DateTimeOffset(2026, 8, 10, 16, 0, 0, TimeSpan.Zero);

            var first = ClubPointEventKeyFactory.BuildMemorySourceKey(15, morning);
            var second = ClubPointEventKeyFactory.BuildMemorySourceKey(15, eveningAsUtc);

            Assert.Equal(first, second);
        }

        [Fact]
        public void BuildMemorySourceKey_ForDifferentTehranDays_ReturnsDifferentKeys()
        {
            var firstDay = new DateTimeOffset(2026, 8, 10, 23, 59, 0, TimeSpan.FromHours(3.5));
            var secondDay = firstDay.AddMinutes(2);

            var first = ClubPointEventKeyFactory.BuildMemorySourceKey(15, firstDay);
            var second = ClubPointEventKeyFactory.BuildMemorySourceKey(15, secondDay);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void AwardAndReverseKeys_AreDeterministicAndSeparated()
        {
            var dto = new ClubPointEventDto
            {
                UserId = 15,
                EventType = ClubPointEventTypeEnum.ProductOrderCompleted,
                SourceType = ClubPointSourceTypeEnum.ProductOrder,
                SourceKey = " Order-100 "
            };

            var award = ClubPointEventKeyFactory.BuildAwardKey(dto);
            var reverse = ClubPointEventKeyFactory.BuildReverseKey(dto);

            Assert.Equal("club-point:productordercompleted:order-100", award);
            Assert.Equal("club-point:reverse:productordercompleted:order-100", reverse);
        }
    }
}
