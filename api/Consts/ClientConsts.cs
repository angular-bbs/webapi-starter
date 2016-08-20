using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebapiStarter.Consts
{
    public static class ClientConsts
    {
        public const string BaseAddress = "http://localhost:4200";
        public const string AccountApiEndPoint = "/api/account";
        public const string PasswordResetEndpoint = BaseAddress + "/user-center/reset-password";
    }
}
