namespace WorkforceManager.Web
{
    using IdentityServer4.Models;
    using System.Collections.Generic;
    using System.Security.Claims;
    using WorkforceManager.Data.Constants;

    public class IdentityConfig
    {
        public static IEnumerable<Client> Clients =>
           new List<Client>
           {
                new Client
                {
                    ClientId = IdentityConfigConstants.ClientId,
                    AllowOfflineAccess = IdentityConfigConstants.AllowOfflineAccess,

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "users", "offline_access", IdentityConfigConstants.ClientId, "roles" }
                }
           };
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResource("roles", new[] { "role" })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                    new ApiScope("users", "My API", new string[]{ ClaimTypes.Name, ClaimTypes.Role }),
                    new ApiScope("offline_access", "RefereshToken"),
                    new ApiScope(IdentityConfigConstants.ClientId, "app")
            };
    }
}
