using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Zapier.Triggers;


internal interface IAdminLinkService
{
    string GenerateAdminLink<T>(PageParameterValues parameters, Uri baseUri);
}


internal class AdminLinkService : IAdminLinkService
{
    private readonly IPageLinkGenerator pageLinkGenerator;

    public AdminLinkService(IPageLinkGenerator pageLinkGenerator) => this.pageLinkGenerator = pageLinkGenerator;

    public string GenerateAdminLink<T>(PageParameterValues parameters, Uri baseUri)
    {
        string relativePath = pageLinkGenerator.GetPath<T>(parameters);
        var adminUrl = new Uri(baseUri, $"/admin{relativePath}");

        return adminUrl.ToString();
    }
}
