﻿using Microsoft.AspNetCore.Authentication;
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
                        OnTokenValidated = context => HandleTokenValidatedAsync(context),
                        OnRedirectToIdentityProvider = context => HandleRedirectToIdentityProvider(context),
                        OnRemoteFailure = context => HandleRemoteFailure(context)
                    };

                });

            return services;
        }

        private static Task HandleTokenValidatedAsync(TokenValidatedContext context)
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new UnauthorizedAccessException("Unauthorized Access");
            }
            var email = identity.FindFirst(ClaimTypes.Email)?.Value
                        ?? identity.FindFirst("preferred_username")?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                //Authenticated user
            }
            else
            {
                throw new UnauthorizedAccessException("User Identifier not received from IDP");
            }
            return Task.CompletedTask;
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
            // Check if the redirect URI starts with "http://"
            if (context.ProtocolMessage.RedirectUri?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Replace "http://" with "https://"
                context.ProtocolMessage.RedirectUri =
                    string.Concat("https://", context.ProtocolMessage.RedirectUri.AsSpan("http://".Length));
            }
            return Task.CompletedTask;
        }

        private static Task HandleRemoteFailure(RemoteFailureContext context)
        {
            if (context.Failure?.InnerException is UnauthorizedAccessException unauthorizedEx)
            {
                throw unauthorizedEx;
            }

            throw context.Failure ?? new Exception("Unknown authentication error.");
        }
    }
 }
