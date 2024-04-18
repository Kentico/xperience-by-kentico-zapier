using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
namespace Kentico.Xperience.Zapier.Auth;


internal interface IZapierTokenManager
{
    string AssignToken(ApiKeyInfo info);
}


internal class ZapierTokenManager : IZapierTokenManager
{
    public string AssignToken(ApiKeyInfo info)
    {
        string token = ApiKeyHelper.GenerateApiKey();
        string hash = ApiKeyHelper.GetToken(token);
        info.ApiKeyToken = hash;
        info.Update();
        return token;
    }
}
