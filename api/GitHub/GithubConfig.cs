using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace WebapiStarter.GitHub
{
    public static class GithubConfig
    {
        public static string GetToken(IHttpContextAccessor httpContextAccessor)
        {
            var principal = httpContextAccessor.HttpContext.User;
            var token = principal.Claims.FirstOrDefault(c => c.Type == TokenClaimType)?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }
            return null;
        }

        public static string ClientId { get; set; }
        public static string Secret { get; set; }
        

        public const string BaseAddress = "https://github.com";
        public const string BaseApiAddress = "https://api.github.com";
        public const string AuthorizeEndpoint = BaseAddress + "/login/oauth/authorize";
        public const string TokenEndpoint = BaseAddress + "/login/oauth/access_token";
        public const string UserInfoEndPoint = BaseApiAddress + "/user";
        public const string ClaimIssure = "OAuth2-GithubConfig";
        public const string TokenClaimType = "access_token";
        public const string UrlClaimType = "urn:github:url";
        public const string NameClaimType = "urn:github:name";

    }
    }

