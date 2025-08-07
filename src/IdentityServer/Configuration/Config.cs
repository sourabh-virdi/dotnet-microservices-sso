using IdentityServer4.Models;

namespace IdentityServer.Configuration;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("userservice.read", "Read access to User Service"),
            new ApiScope("userservice.write", "Write access to User Service"),
            new ApiScope("productservice.read", "Read access to Product Service"),
            new ApiScope("productservice.write", "Write access to Product Service"),
            new ApiScope("orderservice.read", "Read access to Order Service"),
            new ApiScope("orderservice.write", "Write access to Order Service"),
            new ApiScope("api.full", "Full access to all APIs")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("userservice", "User Service")
            {
                Scopes = new List<string> { "userservice.read", "userservice.write", "api.full" },
                UserClaims = new List<string> { "role" }
            },
            new ApiResource("productservice", "Product Service")
            {
                Scopes = new List<string> { "productservice.read", "productservice.write", "api.full" },
                UserClaims = new List<string> { "role" }
            },
            new ApiResource("orderservice", "Order Service")
            {
                Scopes = new List<string> { "orderservice.read", "orderservice.write", "api.full" },
                UserClaims = new List<string> { "role" }
            }
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // React SPA Client
            new Client
            {
                ClientId = "react-spa",
                ClientName = "React SPA",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                AllowOfflineAccess = true,
                
                RedirectUris = { 
                    "http://localhost:3000/callback",
                    "https://localhost:3000/callback" 
                },
                PostLogoutRedirectUris = { 
                    "http://localhost:3000",
                    "https://localhost:3000" 
                },
                AllowedCorsOrigins = { 
                    "http://localhost:3000",
                    "https://localhost:3000" 
                },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "email",
                    "role",
                    "api.full",
                    "userservice.read",
                    "userservice.write",
                    "productservice.read",
                    "productservice.write",
                    "orderservice.read",
                    "orderservice.write"
                },

                AccessTokenLifetime = 3600,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400
            },

            // API Gateway Client
            new Client
            {
                ClientId = "apigateway",
                ClientName = "API Gateway",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("gateway-secret".Sha256()) },

                AllowedScopes = { "api.full" }
            },

            // Service-to-Service Client
            new Client
            {
                ClientId = "service-client",
                ClientName = "Service Client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("service-secret".Sha256()) },

                AllowedScopes = { 
                    "api.full",
                    "userservice.read",
                    "userservice.write",
                    "productservice.read",
                    "productservice.write",
                    "orderservice.read",
                    "orderservice.write"
                }
            }
        };
} 