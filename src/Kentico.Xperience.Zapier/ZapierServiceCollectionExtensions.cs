using AspNetCore.Authentication.ApiKey;

using CMS.Localization;

using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Auth;
using Kentico.Xperience.Zapier.Resources;
using Kentico.Xperience.Zapier.Triggers;
using Kentico.Xperience.Zapier.Triggers.Handlers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

[assembly: RegisterLocalizationResource(markedType: typeof(Localization), cultureCodes: "en-us")]

namespace Kentico.Xperience.Zapier;


/// <summary>
/// Enables Kentico Zapier feature
/// </summary>
public static class ZapierServiceCollectionExtensions
{
    public static IServiceCollection AddKenticoZapier(this IServiceCollection services)
    {
        services.AddOptions<ZapierConfiguration>().BindConfiguration("ZapierConfiguration");
        services.AddSingleton<IZapierModuleInstaller, ZapierModuleInstaller>();
        services.AddSingleton<IZapierRegistrationService, ZapierRegistrationService>();
        services.AddSingleton<IApiKeyCachedService, ApiKeyCachedService>();
        services.AddSingleton<IZapierTriggerService, ZapierTriggerService>();
        services.AddSingleton<IZapierTokenManager, ZapierTokenManager>();
        services.AddSingleton<IZapierTriggerHandlerFactory, ZapierTriggerHandlerFactory>();

        services.AddSingleton<IWorkflowScopeService, WorkflowScopeService>();

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

                        string? tokenHash = apiKeyProvider.GetApiKeyTokenHash();

                        if (tokenHash is null)
                        {
                            context.ValidationFailed();
                            return Task.CompletedTask;
                        }
                        bool isValid = ApiKeyHelper.VerifyToken(context.ApiKey, tokenHash);

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


public class ZapierConfiguration
{
    public HashSet<string> AllowedObjects { get; set; } = [];
}
