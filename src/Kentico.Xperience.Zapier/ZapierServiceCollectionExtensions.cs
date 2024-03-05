using AspNetCore.Authentication.ApiKey;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Kentico.Xperience.Zapier;

public static class ZapierServiceCollectionExtensions
{
    public static IServiceCollection AddKenticoZapier(this IServiceCollection services)
    {
        services.AddSingleton<IZapierModuleInstaller, ZapierModuleInstaller>();

        services.AddAuthentication()
            .AddApiKeyInAuthorizationHeader(ApiKeyDefaults.AuthenticationScheme, options =>
            {
                options.Realm = "XbyKZapier";
                options.KeyName = HeaderNames.Authorization;
                options.Events = new ApiKeyEvents
                {
                    OnValidateKey = context =>
                    {
                        var provider = context.HttpContext.RequestServices.GetRequiredService<IApiKeyInfoProvider>();
                        string token = provider.Get().FirstOrDefault()?.ApiKeyToken ?? string.Empty;

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
