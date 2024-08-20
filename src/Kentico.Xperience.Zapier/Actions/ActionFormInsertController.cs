using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using CMS.DataEngine;
using CMS.OnlineForms;

using Kentico.Xperience.Zapier.Auth;
using Kentico.Xperience.Zapier.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Zapier.Actions;

[AuthorizeZapier]
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


    [HttpPost("zapier/actions/biz-form/{classname}")]
    public ActionResult<FormInsertActionResponse> InsertFormRecord(
        string classname,
        [FromBody] IDictionary<string, JsonElement> values)
    {
        var newFormItem = BizFormItem.New(classname);
        if (newFormItem == null || !zapierConfiguration.AllowedObjects.Contains(classname))
        {
            return BadRequest();
        }

        var dataClass = DataClassInfoProvider.ProviderObject.Get()
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.FORM)
            .WhereEquals(nameof(DataClassInfo.ClassName), classname)
            .WhereNotEmpty(nameof(DataClassInfo.ClassFormDefinition))
            .FirstOrDefault();

        Form? form;
        var serializer = new XmlSerializer(typeof(Form));

        using (var reader = new StringReader(dataClass?.ClassFormDefinition ?? string.Empty))
        {
            form = (Form?)serializer.Deserialize(reader);
        }

        foreach (var item in values)
        {
            switch (item.Value.ValueKind)
            {
                case JsonValueKind.Number:
                    if (item.Value.TryGetInt32(out int intValue))
                    {
                        newFormItem.SetValue(item.Key, intValue);
                    }
                    else if (item.Value.TryGetDouble(out double doubleValue))
                    {
                        newFormItem.SetValue(item.Key, doubleValue);
                    }
                    else
                    {
                        newFormItem.SetValue(item.Key, item.Value.GetString());
                    }
                    break;
                case JsonValueKind.String:
                    string value = item.Value.GetString() ?? string.Empty;

                    var stringField = form?.Fields?.Find(x => x.Column == item.Key);

                    if (stringField != null && stringField.ColumnType == FieldDataType.RichTextHTML)
                    {
                        value = RichTextHtmlHelper.EnsureValidHtmlValue(value);
                    }

                    newFormItem.SetValue(item.Key, value);
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
            return Ok(new FormInsertActionResponse(newFormItem.ItemID));
        }
        catch (Exception)
        {
            logger.LogWarning("Zapier InsertFormRecordAction | Error occured during inserting into form {0}. \n Values: {1}", classname, sb.ToString());
            return BadRequest();
        }
    }


    [HttpGet("zapier/actions/biz-form/{classname}")]
    public IActionResult GetFormFields(string classname)
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();
        if (!allowedObjects.Contains(classname, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest();
        }
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


    public sealed record FormInsertActionResponse(int Id);
}

