using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.DependencyInjection;

public static class IdentityServerConfig
{
    public static void AddIdentityServerConfiguration(this IServiceCollection services)
    {
        services.AddIdentityServer()
            .AddInMemoryClients(new List<Client>
            {
                new Client
                {
                    ClientId = "client_id",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
                }
            })
            .AddInMemoryApiScopes(new List<ApiScope>
            {
                new ApiScope("api1", "My API")
            })
            .AddDeveloperSigningCredential();
    }
}
