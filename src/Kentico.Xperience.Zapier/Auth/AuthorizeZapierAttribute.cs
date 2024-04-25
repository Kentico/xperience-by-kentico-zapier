using Kentico.Xperience.Zapier.Admin;

using Microsoft.AspNetCore.Authorization;

namespace Kentico.Xperience.Zapier.Auth;

internal class AuthorizeZapierAttribute : AuthorizeAttribute
{
    public AuthorizeZapierAttribute() => AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme;
}
