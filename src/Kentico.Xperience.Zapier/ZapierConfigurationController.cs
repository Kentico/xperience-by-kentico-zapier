using System.Data;
using AspNetCore.Authentication.ApiKey;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.Websites.Routing;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier;

[Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
[ApiController]
public class ZapierConfigurationController : ControllerBase
{

    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;

    private readonly IContentQueryExecutor contentQueryExecutor;
    private readonly IContentQueryResultMapper mapper;


    public ZapierConfigurationController(
        IZapierTriggerInfoProvider zapierTriggerInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor contentQueryExecutor,
        IContentQueryResultMapper mapper
        )
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.websiteChannelContext = websiteChannelContext;
        this.contentQueryExecutor = contentQueryExecutor;
        this.mapper = mapper;
    }

    [HttpGet]
    [Route("auth/me")]
    public IActionResult CheckAuth() => Ok(new { Message = "You are Authenticated", Channel = websiteChannelContext?.WebsiteChannelName ?? "Unknown" });

    [HttpGet]
    [Route("zapier/data/events")]
    public ActionResult<IEnumerable<EventItemDto>> Events()
    {
        var events = new List<ZapierTriggerEvents> { ZapierTriggerEvents.Create, ZapierTriggerEvents.Update, ZapierTriggerEvents.Delete };

        return events.Select(e => new EventItemDto((int)e, e.ToString())).ToList();
    }

    [HttpGet]
    [Route("zapier/data/objects")]
    public ActionResult<IEnumerable<KenticoInfoDto>> Objects()
    {
        var infoObjects = DataClassInfoProvider.ProviderObject.Get()
            .WhereNull(nameof(DataClassInfo.ClassContentTypeType))
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResult();

        return infoObjects.Select(x => new KenticoInfoDto(x.ClassName, x.ClassDisplayName)).ToList();
    }


    [HttpGet]
    [Route("zapier/object/{objectType}")]
    public ActionResult GetSampleObject(string objectType)
    {
        var dataClass = DataClassInfoProvider.ProviderObject.Get(objectType);

        //nebo switch
        if (dataClass.IsContentType())
        {
            //todo
            //IReusableContentItemReferenceProvider

            //var builder = new ContentItemQueryBuilder().ForContentType(objectType,
            //            config => config
            //                .WithLinkedItems(1)
            //            .TopN(1));

            //var result = await contentQueryExecutor.GetResult(builder,);

            return NotFound();
        }

        if (dataClass.IsForm())
        {
            return Ok(GetDataFromForm(objectType));
        }

        return Ok(GetDataFromInfoObject(objectType));

    }



    [HttpPost]
    [Route("zapier/trigger")]
    public ActionResult CreateTrigger(TriggerDto newTrigger)
    {
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerDisplayName = newTrigger.Name,
            ZapierTriggerCodeName = ValidationHelper.GetCodeName(newTrigger.Name),
            ZapierTriggerObjectClassType = ZapierTriggerExtensions.GetType(newTrigger.ObjectType),
            ZapierTriggerEnabled = true,
            ZapierTriggerEventType = ((ZapierTriggerEvents)newTrigger.EventType).ToString(),
            mZapierTriggerObjectType = newTrigger.ObjectType,
            ZapierTriggerZapierURL = newTrigger.ZapierUrl
        };

        zapierTriggerInfoProvider.Set(infoObject);

        return Ok(new { TriggerId = infoObject.ZapierTriggerID });
    }


    [HttpDelete]
    [Route("zapier/trigger/{zapId}")]
    public IActionResult DeleteTrigger(string zapId)
    {
        if (!int.TryParse(zapId, out int zapierTriggerID))
        {
            BadRequest();
        }

        var ZapInfoToDelete = zapierTriggerInfoProvider.Get(zapierTriggerID);

        if (ZapInfoToDelete != null)
        {
            zapierTriggerInfoProvider.Delete(ZapInfoToDelete);
        }

        return Ok(new { Status = ZapInfoToDelete != null ? "Success" : $"Info object with provided id {zapierTriggerID} does not exist" });
    }



    private Dictionary<string, object> GetDataFromInfoObject(string objectType)
    {
        var generalizedInfo = (GeneralizedInfo)ModuleManager.GetObject(objectType);
        var type = ObjectTypeManager.GetTypeInfo(objectType);

        if (generalizedInfo == null || type == null)
        {
            return [];
        }

        var result = generalizedInfo.GetDataQuery(true, s => s.TopN(1));

        var dataSet = result.Result;

        var dataTable = dataSet.Tables[0];

        type?.SensitiveColumns?
            .ForEach(colName => dataSet.Tables[0].Columns.Remove(colName));

        var columns = dataTable.Columns.OfType<DataColumn>();

        var data = dataTable.Rows.OfType<DataRow>().Select(r => columns.ToDictionary(c => c.ColumnName, c => r[c]));

        return data.FirstOrDefault() ?? [];
    }



    private Dictionary<string, object> GetDataFromForm(string objectType)
    {
        var formData = BizFormItemProvider.GetItems(objectType).TopN(1).GetEnumerableTypedResult();
        var cols = formData.FirstOrDefault()?.ColumnNames ?? new List<string> { };

        var result = new Dictionary<string, object>();

        foreach (string? col in cols)
        {
            result[col] = formData.First().GetValue(col);
        }
        return result;
    }
}


public record EventItemDto(int Id, string Name);

public record KenticoInfoDto(string Id, string Name);

public record TriggerDto(string Name, string ObjectType, int EventType, string ZapierUrl);
