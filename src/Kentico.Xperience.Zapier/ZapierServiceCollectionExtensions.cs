using AspNetCore.Authentication.ApiKey;
using CMS.Localization;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

[assembly: RegisterLocalizationResource(markedType: typeof(Localization), cultureCodes: "en-us")]

namespace Kentico.Xperience.Zapier;

public static class ZapierServiceCollectionExtensions
{
    public static IServiceCollection AddKenticoZapier(this IServiceCollection services)
    {
        services.AddSingleton<IZapierModuleInstaller, ZapierModuleInstaller>();
        services.AddSingleton<IZapierRegistrationService, ZapierRegistrationService>();
        services.AddSingleton<IApiKeyCachedService, ApiKeyCachedService>();

        services.AddAuthentication()
            .AddApiKeyInAuthorizationHeader(ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme, options =>
            {
                options.Realm = "XbyKZapier";
                options.KeyName = HeaderNames.Authorization;
                options.Events = new ApiKeyEvents
                {
                    OnValidateKey = context =>
                    {
                        var apiKeyProvider = context.HttpContext.RequestServices.GetRequiredService<IApiKeyCachedService>();

                        string? token = apiKeyProvider.GetApiKeyToken();

                        if (token is null)
                        {
                            context.ValidationFailed();
                            return Task.CompletedTask;
                        }

                        bool isValid = ApiKeyHelper.VerifyToken(context.ApiKey, token);

                        if (isValid)
                        {
                            context.ValidationSucceeded();
                        }
                        else
                        {
                            context.ValidationFailed();
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;

    }
}
