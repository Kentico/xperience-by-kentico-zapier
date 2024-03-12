using System.Collections;
using System.Data;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.Websites.Routing;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Kentico.Xperience.Zapier.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class ZapierConfigurationController : ControllerBase
{

    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;

    private readonly IContentQueryExecutor contentQueryExecutor;
    private readonly IContentQueryResultMapper mapper;

    public static readonly List<ZapierTriggerEvents> EnabledEvents = [ZapierTriggerEvents.Create, ZapierTriggerEvents.Update, ZapierTriggerEvents.Delete];

    public ZapierConfigurationController(
        IZapierTriggerInfoProvider zapierTriggerInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor contentQueryExecutor,
        IContentQueryResultMapper mapper)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.websiteChannelContext = websiteChannelContext;
        this.contentQueryExecutor = contentQueryExecutor;
        this.mapper = mapper;
    }

    [HttpGet]
    [Route("auth/me")]
    public IActionResult CheckAuth() => Ok(new { Message = Localization.auth_successtext, Channel = websiteChannelContext?.WebsiteChannelName ?? "Unknown" });

    [HttpGet]
    [Route("zapier/data/events")]
    public ActionResult<IEnumerable<EventItemDto>> Events() => EnabledEvents.Select(e => new EventItemDto((int)e, e.ToString())).ToList();

    [HttpGet]
    [Route("zapier/data/objects")]
    public ActionResult<List<KenticoInfoDto>> Objects()
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
            var objectTypeInfo = BizFormItemProvider.GetTypeInfo(objectType);
            return Ok(GetDataFromObjectTypeInfo(objectTypeInfo));
        }

        var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
        return Ok(GetDataFromObjectTypeInfo(typeInfo));

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



    private Dictionary<string, object> GetDataFromObjectTypeInfo(ObjectTypeInfo objectTypeInfo)
    {
        var result = new Dictionary<string, object>();

        if (objectTypeInfo is null)
        {
            return result;
        }

        var sensitiveCols = objectTypeInfo.SensitiveColumns ?? [];

        foreach (var coldef in objectTypeInfo.ClassStructureInfo.ColumnDefinitions)
        {
            if (!sensitiveCols.Contains(coldef.ColumnName))
            {
                result[coldef.ColumnName] = GetDefaultValue(coldef.ColumnType);
            }
        }
        return result;
    }

    private object GetDefaultValue(Type t)
    {
        if (t.IsValueType)
        {
            return Activator.CreateInstance(t)!;
        }

        if (t == typeof(string))
        {
            return "text";
        }

        if (t.IsAssignableTo(typeof(IEnumerable)))
        {
            return Array.Empty<object>();
        }


        return new object();
    }
}

public record EventItemDto(int Id, string Name);

public record KenticoInfoDto(string Id, string Name);

public record TriggerDto(string Name, string ObjectType, int EventType, string ZapierUrl);
