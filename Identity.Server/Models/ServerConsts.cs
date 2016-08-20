namespace Identity.Server.Models
{
    public static class ServerConsts
    {
        public const string BaseAddress = "https://localhost:44555";
        public const string AccountApiBaseAddress = "api/account";

        public static class AccountApi
        {
            public const string LoginGithubEndpoint = "login-github";
            public const string LoginLocallyEndpoint = "login-local";
            public const string CreatePasswordEndpoint = "create-password";
            public const string LogoutEndpoint = "logout";
            public const string ForgetPasswordEndpoint = "forgot-password";
            public const string ResetPasswordEndpoint = "reset-password";
            public const string ChangePasswordEndpoint = "change-password";
            public const string CheckStatusEndpoint = "check-status";
        }
    }
}
