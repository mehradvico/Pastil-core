//using Application.Common.Dto.LocationPoint;
//using Application.Common.Dto.Result;
//using Application.Common.Enumerable.Code;
//using Application.Common.Geography.Iface;
//using Application.Services.Accounting.UserSrv.Dto;
//using Application.Services.ProductSrvs.ProductCommentSrv.Dto;
//using Application.Services.ProductSrvs.ProductCommentSrv.Iface;
//using Application.Services.TripSrv.PriceCalculationSrv.Iface;
//using Application.Services.TripSrv.TripSrv.Dto;
//using Application.Services.TripSrv.TripSrv.Iface;
//using Microsoft.AspNetCore.SignalR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Common.SignalR.Driver
//{
//    public class DriverHub : Hub
//    {
//        // لیست رانندگان متصل به سیستم
//        public static List<DriverDto> _drivers = new List<DriverDto>();

//        // لیست سفرهای در انتظار راننده
//        //public static List<TripVDto> _trips = new List<TripVDto>();

//        // دیکشنری برای مدیریت زمان‌بندی لغو خودکار سفرها (۵ دقیقه‌ای)
//        private static Dictionary<long, CancellationTokenSource> _tripCancellationTokens = new();

//        private readonly ITripService _tripService;
//        private readonly IProductCommentService _productCommentService;
//        private readonly IGeographyService _geographyService;
//        private readonly IPriceCalculationService _priceCalculationService;
//        public DriverHub(ITripService tripService, IProductCommentService productCommentService, IGeographyService geographyService, IPriceCalculationService priceCalculationService)
//        {
//            _tripService = tripService;
//            _productCommentService = productCommentService;
//            _geographyService = geographyService;
//            _priceCalculationService = priceCalculationService;
//        }

//        // اضافه کردن یا به‌روزرسانی اطلاعات راننده
//        public void AddOrUpdateDrivers(long userId, string userToken, double Latitude, double Longitude, string name = null)
//        {
//            var driver = _drivers.FirstOrDefault(s => s.UserId == userId);
//            if (driver != null)
//            {
//                driver.Location = new PointDto(Latitude, Longitude);
//                driver.ConnctionId = Context.ConnectionId;
//                driver.UserToken = userToken;
//                driver.Name = name;
//            }
//            else
//            {
//                driver = new DriverDto()
//                {
//                    UserId = userId,
//                    Location = new PointDto(Latitude, Longitude),
//                    ConnctionId = Context.ConnectionId,
//                    UserToken = userToken,
//                    Name = name

//                };
//                _drivers.Add(driver);
//            }
//        }

//        // حذف راننده از لیست بر اساس userId
//        private void RemoveDriver(long userId)
//        {
//            var item = _drivers.FirstOrDefault(s => s.UserId == userId);
//            if (item != null)
//            {
//                _drivers.Remove(item);
//            }
//        }

//        // حذف راننده از لیست بر اساس ConnectionId
//        private void RemoveDriver(string ConctionId)
//        {
//            var item = _drivers.FirstOrDefault(s => s.ConnctionId == ConctionId);
//            if (item != null)
//            {
//                _drivers.Remove(item);
//            }
//        }

//        // حذف سفر از لیست و اطلاع دادن به رانندگان
//        //private async Task RemoveTrip(long userId)
//        //{
//        //    var item = _trips.FirstOrDefault(s => s.PassengerId == userId);
//        //    if (item != null)
//        //    {
//        //        _trips.Remove(item);
//        //    }
//        //    foreach (var driver in _drivers)
//        //    {
//        //        await Clients.Client(driver.ConnctionId).SendAsync("RemoveTrip", userId);
//        //    }
//        //}
//        private async Task SendTripToDrivers(BaseResultDto<TripVDto> tripDto)
//        {

//            var neareDrivers = _drivers.Where(s => CalculateDistance(s.Location, tripDto.Data.Origin) < 3000);
//            foreach (var driver in neareDrivers)
//            {
//                await Clients.Client(driver.ConnctionId).SendAsync("AddTrip", tripDto);
//            }
//        }

//        // دریافت اطلاعات راننده متصل به هاب
//        private DriverDto GetDriver()
//        {
//            var userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);
//            return _drivers.FirstOrDefault(s => s.UserId == userId);
//        }

//        // بررسی اینکه آیا راننده با دستگاه دیگری متصل شده است
//        private async Task DriverConnectdOtherDevice(long userId, string token)
//        {
//            var item = _drivers.FirstOrDefault(s => s.UserId == userId && s.UserToken != token);
//            if (item != null)
//                await Clients.Client(item.ConnctionId).SendAsync("ConnectedOtherDevice");
//        }
//        // تابع برای محاسبه فاصله بین دو نقطه (Latitude, Longitude) با استفاده از Haversine Formula
//        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
//        {
//            const double EarthRadius = 6371; // شعاع زمین به کیلومتر
//            double dLat = (lat2 - lat1) * (Math.PI / 180); // تبدیل درجه به رادیان
//            double dLon = (lon2 - lon1) * (Math.PI / 180); // تبدیل درجه به رادیان

//            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                       Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
//                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

//            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//            return EarthRadius * c; // فاصله بر حسب کیلومتر
//        }
//        // ملحق شدن راننده به سیستم و بررسی سفرهای جاری
//        public async Task JoinDriver(double Latitude, double Longitude)
//        {
//            var userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);
//            var httpContext = Context.GetHttpContext();
//            var token = httpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

//            await DriverConnectdOtherDevice(userId, token);

//            var currentTrip = await _tripService.GetCurrentTripForDriverAsync(userId);
//            if (currentTrip.IsSuccess)
//            {
//                await Clients.Caller.SendAsync("CurrentTrip", currentTrip);
//                return;
//            }
//            AddOrUpdateDrivers(userId, token, Latitude, Longitude);

//            var tripSearchDto = new TripInputDto() { PageSize = 100, DriverStatusId = (long)DriverStatusEnum.DriverStatus_Requested, TripStatusId = (long)TripStatusEnum.TripStatus_Requested, ToMinute = 5, Point = new PointDto { x = Latitude, y = Longitude } };
//            var nearbyTrips = _tripService.Search(tripSearchDto);


//            await Clients.Caller.SendAsync("NearbyTrips", nearbyTrips);

//        }
//        public async Task JoinChat(double Latitude, double Longitude)
//        {
//            long userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);
//            string fname = Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "firstname")?.Value;

//            var conid = Context.ConnectionId;
//            var token = Context.GetHttpContext()?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
//            AddOrUpdateDrivers(userId, token, Latitude, 0, fname);
//            await Clients.Caller.SendAsync("wellcom", fname);
//            await Clients.Clients(_drivers.Select(s => s.ConnctionId)).SendAsync("usersList", _drivers);

//        }
//        public async Task SendMessage(string contectionId, string message)
//        {
//            long userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);
//            string fname = Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "firstname")?.Value;
//            message = fname + " گفت  : " + message;
//            await Clients.Client(contectionId).SendAsync("chatList", message);
//        }
//        // قبول کردن سفر توسط راننده
//        //public async Task AcceptTrip(long userId)
//        //{
//        //    var trip = _trips.FirstOrDefault(s => s.PassengerId == userId);
//        //    if (trip != null)
//        //    {
//        //        var driver = GetDriver();
//        //        trip.DriverId = driver.UserId;
//        //        trip.TripStartDateTime = DateTime.Now;
//        //        trip.DriverStatusId = (long)DriverStatusEnum.DriverStatus_Accepted;
//        //        trip.TripStatusId = (long)TripStatusEnum.TripStatus_Accepted;

//        //        var tripVDto = await _tripService.UpdateVDtoAsync(trip);
//        //        if (tripVDto.IsSuccess)
//        //        {
//        //            await Clients.Client(driver.ConnctionId).SendAsync("TripAccepted", tripVDto);
//        //            await Clients.Client(trip.ConnctionId).SendAsync("TripAccepted", tripVDto);
//        //            _trips.Remove(trip);
//        //            _drivers.Remove(driver);
//        //        }
//        //        else
//        //        {
//        //            await Clients.Client(driver.ConnctionId).SendAsync("Error", tripVDto);
//        //        }
//        //    }
//        //}

//        // مدیریت اتصال و قطع اتصال کلاینت‌ها
//        public override async Task OnConnectedAsync()
//        {
//            var userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);
//            if (userId == 114)
//            {
//                return;
//            }

//            await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Text = Context.ConnectionId });

//            var httpContext = Context.GetHttpContext();
//            var token = httpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");


//            // بررسی سفر جاری مسافر
//            var currentTrip = await _tripService.GetCurrentTripForPassengerAsync(userId);
//            if (currentTrip.IsSuccess)
//            {
//                var tripDto = currentTrip.Data;
//                await PassengerConnectedOtherDevice(tripDto, token);
//                await Clients.Caller.SendAsync("CurrentTrip", currentTrip);
//                await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Answer = "con|||" + JsonSerializer.Serialize(currentTrip) });

//            }
//            else
//            {
//                await Clients.Caller.SendAsync("HaveNotTrip");
//                await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Answer = "No|||" });

//            }
//            await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Text = Context.ConnectionId });
//            await base.OnConnectedAsync();

//        }

//        public async override Task OnDisconnectedAsync(Exception exception)
//        {
//            var conid = Context.ConnectionId;
//            RemoveDriver(conid);
//            await Clients.Clients(_drivers.Select(s => s.ConnctionId)).SendAsync("usersList", _drivers);

//            //long userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);
//            //await _productCommentService.InsertAsyncDto(new ProductCommentDto() {UserId=userId, ProductId = 1, Text =  conid+"  |||disConnect||||"+userId });

//            //if (userId!=0)
//            //    RemoveDriver(Convert.ToInt64(userId));
//            //else
//            //    RemoveDriver(Context.ConnectionId);

//            await base.OnDisconnectedAsync(exception);
//        }

//        // مسافر به سیستم ملحق می‌شود
//        public async Task JoinPassenger(TripVDto tripDto)
//        {
//            var userId = Convert.ToInt64(Context.User.Claims.FirstOrDefault(s => s.Type.ToLower() == "userid")?.Value);

//            try
//            {
//                var httpContext = Context.GetHttpContext();
//                var token = httpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");


//                var currentTrip = await _tripService.GetCurrentTripForPassengerAsync(userId);
//                if (currentTrip.IsSuccess)
//                {
//                    tripDto = currentTrip.Data;
//                    await PassengerConnectedOtherDevice(tripDto, token);
//                    await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Answer = JsonSerializer.Serialize(tripDto) });

//                }
//                else
//                {
//                    double price = await _priceCalculationService.CalculateTripPrice(tripDto);
//                    tripDto.Price = price;
//                    // اضافه کردن یا بروزرسانی سفر مسافر
//                    tripDto.ConnectionId = Context.ConnectionId;
//                    tripDto.UserToken = token;
//                    tripDto.Price = price;

//                    currentTrip = await _tripService.InsertVDtoAsync(tripDto);
//                    await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Text = JsonSerializer.Serialize(currentTrip) });

//                }
//                await Clients.Caller.SendAsync("CurrentTrip", currentTrip);

//                if (currentTrip.Data.TripStatusId == (long)TripStatusEnum.TripStatus_Requested)
//                {
//                    await SendTripToDrivers(currentTrip);
//                }
//                return;
//            }
//            catch (Exception ex)
//            {
//                await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Text = JsonSerializer.Serialize(tripDto) + "||" + ex.Message });

//                await Clients.Caller.SendAsync("showError", ex.Message);

//            }




//            //await _productCommentService.InsertAsyncDto(new ProductCommentDto() { UserId = userId, ProductId = 1, Text = JsonSerializer.Serialize(tripDto) });

//        }



//        // متد برای محاسبه مسافت بین دو نقطه (به کیلومتر)
//        private double CalculateDistance(PointDto origin, PointDto destination)
//        {
//            // فرض کنید این متد مسافت بین دو نقطه را محاسبه می‌کند.
//            // برای مثال می‌توانید از فرمول Haversine استفاده کنید یا از یک API برای محاسبه فاصله استفاده کنید.
//            // اینجا برای سادگی فرض می‌کنیم که مسافت به کیلومتر محاسبه می‌شود.

//            double lat1 = origin.x;
//            double lon1 = origin.y;
//            double lat2 = destination.x;
//            double lon2 = destination.y;

//            // محاسبه فاصله بین دو نقطه
//            var r = 6371; // شعاع زمین به کیلومتر
//            var phi1 = lat1 * (Math.PI / 180);
//            var phi2 = lat2 * (Math.PI / 180);
//            var deltaPhi = (lat2 - lat1) * (Math.PI / 180);
//            var deltaLambda = (lon2 - lon1) * (Math.PI / 180);

//            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
//                    Math.Cos(phi1) * Math.Cos(phi2) *
//                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
//            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//            return r * c; // مسافت به کیلومتر
//        }
//        // بررسی اینکه مسافر با دستگاه دیگری متصل شده است
//        private async Task PassengerConnectedOtherDevice(TripVDto trip, string currentToken)
//        {
//            if (trip.UserToken != currentToken)
//            {
//                await Clients.Client(trip.ConnectionId).SendAsync("ConnectedOtherDevice");
//            }
//            trip.UserToken = currentToken;
//            await _tripService.ChangeTripConnectionIdAsync(trip);
//        }

//        // اضافه کردن یا به‌روزرسانی سفر و ایجاد تایمر 5 دقیقه‌ای
//        //public async Task AddTrips(TripVDto tripDto)
//        //{

//        //    var newTrip = await _tripService.InsertVDtoAsync(tripDto);
//        //    if (newTrip.IsSuccess)
//        //    {
//        //        tripDto.Id = newTrip.Data.Id;
//        //        AddOrUpdateTrips(tripDto);
//        //    }

//        //}
//        //public void AddOrUpdateTrips(TripVDto tripDto)
//        //{
//        //    var trip = _trips.FirstOrDefault(s => s.PassengerId == tripDto.PassengerId);
//        //    if (trip == null)
//        //    {
//        //        _trips.Add(tripDto);
//        //    }
//        //    else
//        //    {
//        //        trip = tripDto;
//        //    }

//        //}

//        // لغو کردن سفر قبل از اتمام تایمر 5 دقیقه‌ای
//        //public async Task CancelTrip(long passengerId)
//        //{
//        //    if (_tripCancellationTokens.TryGetValue(passengerId, out var cts))
//        //    {
//        //        cts.Cancel();
//        //        _tripCancellationTokens.Remove(passengerId);
//        //    }

//        //    var trip = _trips.FirstOrDefault(s => s.PassengerId == passengerId);
//        //    if (trip != null)
//        //    {
//        //        await RemoveTrip(passengerId);
//        //        await Clients.Client(trip.ConnctionId).SendAsync("TripCanceled");
//        //    }
//        //}
//    }
//}
