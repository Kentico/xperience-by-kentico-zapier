using System.Data;

using CMS.DataEngine;

using Kentico.Xperience.Zapier.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Zapier.Common;

[AuthorizeZapier]
[ApiController]
public class ZapierDataController : ControllerBase
{
    private readonly ZapierConfiguration zapierConfiguration;


    public ZapierDataController(
        IOptionsMonitor<ZapierConfiguration> zapierConfiguration) => this.zapierConfiguration = zapierConfiguration.CurrentValue;


    [HttpGet($"zapier/data/types/{ClassType.FORM}")]
    public ActionResult<IEnumerable<SelectOptionItem>> GetFormTypes() =>
        DataClassInfoProvider.ProviderObject.GetZapierTypesQuery(classType: ClassType.FORM, zapierConfiguration.AllowedObjects.ToList())
        .Select(x => new SelectOptionItem(
            x.ClassName,
            $"{x.ClassDisplayName}"
        )).ToList();
}


public sealed record SelectOptionItem(string Id, string Name);
