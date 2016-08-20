using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace Identity.Server.Configurations
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "spa",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = new List<string>
                    {
                        "http://localhost:4200/user-center"
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid", "profile",
                        "api.github"
                    }
                }
            };
        }
    }
}
