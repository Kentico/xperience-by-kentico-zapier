using CMS.Websites.Routing;

using Kentico.Xperience.Zapier.Resources;

using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier.Auth;


[AuthorizeZapier]
[ApiController]
public class ZapierAuthController : ControllerBase
{
    private readonly IWebsiteChannelContext websiteChannelContext;


    public ZapierAuthController(
        IWebsiteChannelContext websiteChannelContext) => this.websiteChannelContext = websiteChannelContext;


    [HttpGet("auth/me")]
    public IActionResult CheckAuth() =>
        Ok(new CheckAuthResponse(Localization.auth_successtext, websiteChannelContext?.WebsiteChannelName ?? "Unknown"));
}


public sealed record CheckAuthResponse(string Message, string Channel);


