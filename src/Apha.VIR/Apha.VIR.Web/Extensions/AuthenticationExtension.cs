using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Apha.VIR.Web.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApp(options =>
             {
                 configuration.Bind("AzureAd", options);
                 options.Events = new OpenIdConnectEvents
                 {
                     OnRedirectToIdentityProvider = context => HandleRedirectToIdentityProvider(context)
                 };

             });

            return services;
        }

        /// <summary>
        /// Ensures the redirect URI uses HTTPS instead of HTTP.
        /// This is typically used in OpenID Connect authentication
        /// to enforce secure redirection to the identity provider.
        /// </summary>
        /// <param name="context">
        /// The <see cref="RedirectContext"/> provided by the OIDC middleware.
        /// </param>
        /// <returns>A completed task.</returns>
        private static Task HandleRedirectToIdentityProvider(RedirectContext context)
        {
            Console.WriteLine($"Redirect URL : {context.ProtocolMessage.RedirectUri}");
            // Check if the redirect URI starts with "http://"
            if (context.ProtocolMessage.RedirectUri?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Replace "http://" with "https://"
                context.ProtocolMessage.RedirectUri =
                    string.Concat("https://", context.ProtocolMessage.RedirectUri.AsSpan("http://".Length));
                Console.WriteLine($"Redirect URL After replace : {context.ProtocolMessage.RedirectUri}");

            }
            return Task.CompletedTask;
        }

    }
}
