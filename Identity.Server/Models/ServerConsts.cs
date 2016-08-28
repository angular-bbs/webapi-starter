namespace Identity.Server.Models
{
    public class ServerConsts
    {
        public const string BaseAddress = "https://localhost:44555";
        public const string AccountApiBaseAddress = "api/account";

        public class AccountApi
        {
            public const string ForgetPasswordEndpoint = "forgot-password";
            public const string ResetPasswordEndpoint = "reset-password";
            public const string ChangePasswordEndpoint = "change-password";
            public const string RegisterEndpoint = "register";
            public const string CreatePasswordEndpoint = "create-password";
        }
    }
}
