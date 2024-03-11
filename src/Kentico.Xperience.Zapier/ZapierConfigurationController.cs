using System.Collections;
using System.Data;
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
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;

namespace Kentico.Xperience.Zapier;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class ZapierConfigurationController : ControllerBase
{

    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;

    private readonly IWorkflowScopeService workflowScopeService;


    public static readonly List<ZapierTriggerEvents> InfoEvents = [ZapierTriggerEvents.Create, ZapierTriggerEvents.Update, ZapierTriggerEvents.Delete];

    public static readonly List<ZapierTriggerEvents> ContentItemEvents = [ZapierTriggerEvents.Create, ZapierTriggerEvents.Update, ZapierTriggerEvents.Delete, ZapierTriggerEvents.Publish];


    public ZapierConfigurationController(
        IZapierTriggerInfoProvider zapierTriggerInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IWorkflowScopeService workflowScopeService)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.websiteChannelContext = websiteChannelContext;
        this.workflowScopeService = workflowScopeService;
    }

    [HttpGet]
    [Route("auth/me")]
    public IActionResult CheckAuth() => Ok(new { Message = Localization.auth_successtext, Channel = websiteChannelContext?.WebsiteChannelName ?? "Unknown" });

    [HttpGet]
    [Route("zapier/data/events")]
    public ActionResult<IEnumerable<EventItemDtoOld>> Events() => InfoEvents.Select(e => new EventItemDtoOld((int)e, e.ToString())).ToList();


    [HttpGet]
    [Route("zapier/data/events/{objectType}")]
    public async Task<ActionResult<IEnumerable<EventItemDto>>> EventsByType(string objectType)
    {
        var dataClass = DataClassInfoProvider.ProviderObject.Get(objectType);

        if (dataClass.IsContentType())
        {
            int contentTypeID = dataClass.ClassID;
            var events = ContentItemEvents.Select(e => new EventItemDto(e.ToString(), e.ToString())).ToList();

            if (dataClass.ClassContentTypeType == ClassContentTypeType.WEBSITE)
            {
                var workflowSteps = workflowScopeService.GetEventItemsByContentType(contentTypeID);

                events.AddRange(workflowSteps);
                return events;
            }

            if (dataClass.ClassContentTypeType == ClassContentTypeType.REUSABLE)
            {
                var workflowSteps = workflowScopeService.GetEventItemsByContentType(contentTypeID);

                events.AddRange(workflowSteps);
                return events;
            }

            return events;
        }

        return InfoEvents.Select(e => new EventItemDto(e.ToString(), e.ToString())).ToList();
    }


    [HttpGet]
    [Route("zapier/data/objects")]
    public ActionResult<List<KenticoInfoDto>> Objects()
    {
        var infoObjects = DataClassInfoProvider.ProviderObject.Get()
            .Where(w =>
                w.WhereNull(nameof(DataClassInfo.ClassContentTypeType))
                    .Or()
                    .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.REUSABLE)
                    .Or()
                    .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.WEBSITE))
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassType), nameof(DataClassInfo.ClassContentTypeType))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResult();

        return infoObjects.Select(x => new KenticoInfoDto(x.ClassName, $"{x.ClassDisplayName} ({x.ClassType}{(!string.IsNullOrEmpty(x.ClassContentTypeType) ? $" - {x.ClassContentTypeType}" : string.Empty)})"))
            .ToList();
    }


    [HttpGet]
    [Route("zapier/object/{objectType}")]
    public ActionResult GetSampleObject(string objectType)
    {
        var dataClass = DataClassInfoProvider.ProviderObject.Get(objectType);

        if (dataClass.IsContentType())
        {
            var result = new Dictionary<string, object>();

            var classStructure = ClassStructureInfo.GetClassInfo(objectType);
            var defs = classStructure.ColumnDefinitions;

            var forbiddenCols = new List<string> { "ContentItemDataID", "ContentItemDataCommonDataID", "ContentItemDataGUID" };

            foreach (var colDef in defs.Where(x => !forbiddenCols.Contains(x.ColumnName)))
            {
                result[colDef.ColumnName] = GetDefaultValue(colDef.ColumnType);
            }

            result.TryAdd("ContentTypeName", objectType);
            return Ok(result);
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
            ZapierTriggerEventType = newTrigger.EventType,
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

        //serialized strings
        if (t == typeof(string))
        {
            return "text or serialized content";
        }

        if (t.IsAssignableTo(typeof(IEnumerable)))
        {
            return Array.Empty<object>();
        }


        return new object();
    }
}

public record EventItemDto(string Id, string Name);
public record KenticoInfoDto(string Id, string Name);
public record TriggerDto(string Name, string ObjectType, string EventType, string ZapierUrl);




public record EventItemDtoOld(int Id, string Name);
public record TriggerDtoOld(string Name, string ObjectType, int EventType, string ZapierUrl);

