using CMS.DataEngine;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Kentico.Xperience.Zapier.Actions;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class ActionFormInsertController : ControllerBase
{
    private readonly ILogger<ActionFormInsertController> logger;
    private readonly ZapierConfiguration zapierConfiguration;

    public ActionFormInsertController(ILogger<ActionFormInsertController> logger,
        IOptionsMonitor<ZapierConfiguration> zapierConfiguration)
    {
        this.logger = logger;
        this.zapierConfiguration = zapierConfiguration.CurrentValue;
    }

    [HttpPost]
    [Route("zapier/actions/biz-form/{classname}")]
    public ActionResult<object> InsertFormRecord(
        string classname,
        [FromBody] IDictionary<string, JsonElement> values)
    {

        var newFormItem = BizFormItem.New(classname);
        if (newFormItem == null || !zapierConfiguration.AllowedObjects.Contains(classname))
        {
            return BadRequest();
        }
        foreach (var item in values)
        {
            switch (item.Value.ValueKind)
            {
                case JsonValueKind.Number:
                case JsonValueKind.String:
                    newFormItem.SetValue(item.Key, item.Value.GetString());
                    break;
                case JsonValueKind.True:
                    newFormItem.SetValue(item.Key, true);
                    break;
                case JsonValueKind.False:
                    newFormItem.SetValue(item.Key, false);
                    break;
                case JsonValueKind.Undefined:
                    break;
                case JsonValueKind.Object:
                    break;
                case JsonValueKind.Array:
                    break;
                case JsonValueKind.Null:
                    break;
                default:
                    break;
            }
        }
        var sb = new StringBuilder();
        foreach (var kvp in values)
        {
            sb.Append($"{kvp.Key}: {kvp.Value}\n");
        }
        try
        {
            newFormItem.Insert();
            logger.LogInformation("Zapier InsertFormRecordAction | Record saved into {0}. \n Values: {1}", classname, sb.ToString());
            return Ok(new { FormId = newFormItem.ItemID });
        }
        catch (Exception)
        {
            logger.LogWarning("Zapier InsertFormRecordAction | Error occured during inserting into form {0}. \n Values: {1}", classname, sb.ToString());
            return BadRequest();
        }
    }

    [HttpGet]
    [Route("zapier/actions/biz-form/{classname}")]
    public async Task<IActionResult> GetFormFields(string classname)
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();
        var dataClass = DataClassInfoProvider.ProviderObject.Get()
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.FORM)
            .WhereEquals(nameof(DataClassInfo.ClassName), classname)
            .WhereNotEmpty(nameof(DataClassInfo.ClassFormDefinition))
            .FirstOrDefault();
        if (dataClass == null)
        {
            return BadRequest();
        }
        return Ok(dataClass.ClassFormDefinition);
    }

    [HttpGet]
    [Route("zapier/actions/biz-form/classnames")]
    public async Task<ActionResult<IEnumerable<object>>> GetFormClassnames()
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();
        var res = DataClassInfoProvider.ProviderObject.Get()
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.FORM)
            .WhereNotEmpty(nameof(DataClassInfo.ClassFormDefinition))
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResult();
        return res.Select(x => new { Id = x.ClassName, Name = x.ClassDisplayName }).ToList();
    }
}

