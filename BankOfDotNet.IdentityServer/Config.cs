using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace BankOfDotNet.IdentityServer
{
    public class Config
    {
        internal static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>()
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        internal static List<TestUser> GetUsers()
        {
            return new List<TestUser>()
            {
                new TestUser()
                {
                    SubjectId = "1",
                    Username = "paulofoliveira",
                    Password = "paulo@00"
                },
                new TestUser()
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password"
                }
            };
        }

        internal static IEnumerable<ApiResource> GetAllApiResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("bankOfDotNetApi", "Customer API for BankOfDotNet")
            };
        }

        internal static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                // Client-credential Grant Type:

                new Client()
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "bankOfDotNetApi"}
                },

                // Resource Owner Password Grant Type:

                new Client()
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes= GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "bankOfDotNetApi" }
                },

                // Implicit Flow Grant Type:

                new Client()
                {
                    ClientId = "mvc",
                    ClientName ="MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    RedirectUris= { "http://localhost:5003/sign-oidc"},
                    PostLogoutRedirectUris = { "http://localhost:5003/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },

                // Swagger Client

                new Client()
                {
                    ClientId = "swaggerclientui",
                    ClientName = "Swagger Client API",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    RedirectUris= { "http://localhost:61807/swagger/oauth2-redirect.html"},
                    PostLogoutRedirectUris = { "http://localhost:61807/swagger" },

                    AllowedScopes = { "bankOfDotNetApi" },
                    AllowAccessTokensViaBrowser= true

                }
            };
        }
    }
}
