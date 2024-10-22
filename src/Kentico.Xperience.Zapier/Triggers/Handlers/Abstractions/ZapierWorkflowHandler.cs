using CMS.Core;

using Kentico.Integration.Zapier;

using Microsoft.AspNetCore.Http;

namespace Kentico.Xperience.Zapier.Triggers.Handlers.Abstractions;

internal abstract class ZapierWorkflowHandler : ZapierTriggerHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;
    protected readonly IAdminLinkService AdminLinkService;


    protected ZapierWorkflowHandler(
        ZapierTriggerInfo zapierTrigger,
        IEventLogService? eventLogService,
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IAdminLinkService adminLinkService)
        : base(zapierTrigger, eventLogService, client)
    {
        this.httpContextAccessor = httpContextAccessor;
        AdminLinkService = adminLinkService;
    }


    protected Uri GetHostDomain() => new($"https://{httpContextAccessor.HttpContext?.Request.Host.Value ?? string.Empty}");
}
