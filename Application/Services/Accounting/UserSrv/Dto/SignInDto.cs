namespace Application.Services.Accounting.UserSrv.Dto
{
    public class SignInDto
    {

        public string Mobile { get; set; }

        public string Password { get; set; }
        public string Code { get; set; }
        public string CartCode { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSiteAdmin { get; set; }
        public bool RememberMe { get; set; }

    }
}
