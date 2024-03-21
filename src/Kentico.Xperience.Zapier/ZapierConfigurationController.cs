using System.Collections;
using System.Data;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.OnlineForms;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Kentico.Xperience.Zapier.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;

namespace Kentico.Xperience.Zapier;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class ZapierConfigurationController : ControllerBase
{

    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;

    private readonly ZapierConfiguration zapierConfiguration;

    private readonly IWorkflowScopeService workflowScopeService;

    private readonly IContentQueryExecutor contentQueryExecutor;

    private readonly IContentQueryResultMapper contentQueryResultMapper;
    private readonly IWebPageQueryResultMapper webPageQueryResultMapper;

    public static readonly Dictionary<string, object> WorkflowEvents = new()
    {
        { "DisplayName", "Content item name" },
        { "ContentTypeName", "ContentType.Type" },
        { "StepName", "Step name" },
        { "OriginalStepName", "Original step name" },
        { "UserID", 132 },
        { "AdminLink", "https://xperienceTestApp.com/admin/#item#"}
    };

    public static readonly List<ZapierTriggerEvents> InfoEvents = [ZapierTriggerEvents.Create, ZapierTriggerEvents.Update, ZapierTriggerEvents.Delete];

    public static readonly List<ZapierTriggerEvents> ContentItemEvents = [ZapierTriggerEvents.Publish];


    public ZapierConfigurationController(
        IZapierTriggerInfoProvider zapierTriggerInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IWorkflowScopeService workflowScopeService,
        IContentQueryExecutor contentQueryExecutor,
        IOptionsMonitor<ZapierConfiguration> zapierConfiguration,
        IContentQueryResultMapper contentQueryResultMapper,
        IWebPageQueryResultMapper webPageQueryResultMapper)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.websiteChannelContext = websiteChannelContext;
        this.workflowScopeService = workflowScopeService;
        this.contentQueryExecutor = contentQueryExecutor;
        this.zapierConfiguration = zapierConfiguration.CurrentValue;
        this.contentQueryResultMapper = contentQueryResultMapper;
        this.webPageQueryResultMapper = webPageQueryResultMapper;
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
            List<string> typesAllowingEvents = [ClassContentTypeType.WEBSITE, ClassContentTypeType.REUSABLE, ClassContentTypeType.HEADLESS];

            if (!typesAllowingEvents.Contains(dataClass.ClassContentTypeType))
            {
                return new List<EventItemDto>();
            }

            int contentTypeID = dataClass.ClassID;
            var events = ContentItemEvents.Select(e => new EventItemDto(e.ToString(), e.ToString())).ToList();

            var workflowSteps = workflowScopeService.GetEventItemsByContentType(contentTypeID);

            events.AddRange(workflowSteps);

            return events;
        }

        return InfoEvents.Select(e => new EventItemDto(e.ToString(), e.ToString())).ToList();
    }


    [HttpGet]
    [Route("zapier/data/objects")]
    public ActionResult<List<KenticoInfoDto>> Objects()
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();

        var infoObjects = DataClassInfoProvider.ProviderObject.Get()
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .WhereNotEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.EMAIL)
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassType), nameof(DataClassInfo.ClassContentTypeType))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResult();

        return infoObjects.Select(x => new KenticoInfoDto(x.ClassName, $"{x.ClassDisplayName} ({x.ClassType}{(!string.IsNullOrEmpty(x.ClassContentTypeType) ? $" - {x.ClassContentTypeType}" : string.Empty)})"))
            .ToList();
    }


    [HttpGet]
    [Route("zapier/object/{objectType}/{eventType}")]
    public async Task<ActionResult> GetSampleObject(string objectType, string eventType)
    {
        var dataClass = DataClassInfoProvider.ProviderObject.Get(objectType);

        if (dataClass.ClassContentTypeType == ClassContentTypeType.EMAIL)
        {
            return BadRequest();
        }

        if (!Enum.TryParse(eventType, out ZapierTriggerEvents eventTypeEnum))
        {
            bool isValid = workflowScopeService.IsMatchingWorflowEventPerObject(eventType, dataClass.ClassID);

            return isValid ? Ok(WorkflowEvents) : BadRequest();
        }

        if (dataClass.IsContentType())
        {
            var result = new Dictionary<string, object>();

            if (dataClass.ClassContentTypeType is ClassContentTypeType.REUSABLE or
                ClassContentTypeType.HEADLESS)
            {
                var builder = new ContentItemQueryBuilder().ForContentType(objectType,
                    config => config
                        .WithLinkedItems(1)
                    .TopN(1));
                var reus = await contentQueryExecutor.GetResult(builder, contentQueryResultMapper.MapReusable);
                result = reus.FirstOrDefault();
            }


            if (dataClass.ClassContentTypeType == ClassContentTypeType.WEBSITE)
            {
                var builder = new ContentItemQueryBuilder().ForContentType(objectType,
                    config => config
                        .WithLinkedItems(1)
                        .ForWebsite(websiteChannelContext.WebsiteChannelName ?? string.Empty)
                    .TopN(1));
                var pages = await contentQueryExecutor.GetWebPageResult(builder, webPageQueryResultMapper.MapPages);
                result = pages.FirstOrDefault();
            }

            return Ok(result ?? GetSampleContentItem(objectType));
        }

        if (dataClass.IsForm())
        {
            var actualForm = GetDataFromForm(objectType);

            var objectTypeInfo = BizFormItemProvider.GetTypeInfo(objectType);
            return Ok(actualForm ?? GetSampleDataFromObjectTypeInfo(objectTypeInfo));
        }

        var actualInfo = GetDataFromInfoObject(objectType);

        var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
        return Ok(actualInfo ?? GetSampleDataFromObjectTypeInfo(typeInfo));
    }



    [HttpPost]
    [Route("zapier/trigger")]
    public ActionResult CreateTrigger(TriggerDto newTrigger)
    {
        string name = ZapierTriggerExtensions.GenerateWebhookName();
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerDisplayName = name,
            ZapierTriggerCodeName = ZapierTriggerExtensions.GetUniqueCodename(name),
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

    private Dictionary<string, object> GetSampleContentItem(string objectType)
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
        return result;
    }

    private Dictionary<string, object> GetSampleDataFromObjectTypeInfo(ObjectTypeInfo objectTypeInfo)
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

        if (!result.TryAdd("AppId", ZapierConstants.AppId))
        {
            return result;
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



    private Dictionary<string, object>? GetDataFromInfoObject(string objectType)
    {
        var generalizedInfo = (GeneralizedInfo)ModuleManager.GetObject(objectType);
        var type = ObjectTypeManager.GetTypeInfo(objectType);

        if (generalizedInfo == null || type == null)
        {
            return null;
        }

        var result = generalizedInfo.GetDataQuery(true, s => s.TopN(1));

        var dataSet = result.Result;

        var dataTable = dataSet.Tables[0];

        type?.SensitiveColumns?
            .ForEach(colName => dataSet.Tables[0].Columns.Remove(colName));

        var columns = dataTable.Columns.OfType<DataColumn>();

        var data = dataTable.Rows.OfType<DataRow>().Select(r => columns.ToDictionary(c => c.ColumnName, c => r[c]));

        return data.FirstOrDefault();
    }


    private Dictionary<string, object>? GetDataFromForm(string objectType)
    {
        var formData = BizFormItemProvider.GetItems(objectType).TopN(1).GetEnumerableTypedResult();

        if (formData is null || !formData.Any())
        {
            return null;
        }

        var cols = formData.FirstOrDefault()?.ColumnNames;

        if (cols is null)
        {
            return null;
        }

        var result = new Dictionary<string, object>();

        foreach (string? col in cols)
        {
            result[col] = formData.First().GetValue(col);
        }
        return result;
    }
}

public record EventItemDto(string Id, string Name);
public record KenticoInfoDto(string Id, string Name);
public record TriggerDto(string ObjectType, string EventType, string ZapierUrl);




public record EventItemDtoOld(int Id, string Name);
public record TriggerDtoOld(string Name, string ObjectType, int EventType, string ZapierUrl);

