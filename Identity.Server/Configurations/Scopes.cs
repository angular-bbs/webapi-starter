using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace Identity.Server.Configurations
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                new Scope
                {
                    Name = "api.github",
                    DisplayName = "Github API",
                    Description = "email, public_repo",
                    Type = ScopeType.Resource,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("role")
                    }
                },
            };
        }
    }
}
