using System.Collections;
using System.Data;

using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.OnlineForms;
using CMS.Websites;
using CMS.Websites.Routing;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Triggers.Extensions;

namespace Kentico.Xperience.Zapier.Triggers;

public interface IZapierTriggerService
{
    /// <summary>
    /// Creates trigger
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="eventType"></param>
    /// <param name="zapierUrl"></param>
    /// <returns></returns>
    int CreateTrigger(string objectType, string eventType, string zapierUrl);


    /// <summary>
    /// Deletes trigger
    /// </summary>
    /// <param name="info"></param>
    void DeleteTrigger(ZapierTriggerInfo info);


    /// <summary>
    /// Assigns event log severity to Zapier trigger
    /// </summary>
    /// <param name="zapierTriggerID"></param>
    /// <param name="severities"></param>
    void AssignEventSeverities(int zapierTriggerID, IEnumerable<string> severities);


    /// <summary>
    /// Gets the fallback data for given object type and event type
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    Task<IDictionary<string, object>?> GetFallbackDataAsync(string objectType, string eventType);
}
internal class ZapierTriggerService : IZapierTriggerService
{
    private readonly IInfoProvider<ZapierTriggerInfo> zapierTriggerInfoProvider;
    private readonly IInfoProvider<ZapierTriggerEventLogTypeInfo> zapierTriggerEventLogTypeInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;
    private readonly IWorkflowScopeService workflowScopeService;
    private readonly IContentQueryExecutor contentQueryExecutor;
    private readonly IContentQueryResultMapper contentQueryResultMapper;
    private readonly IWebPageQueryResultMapper webPageQueryResultMapper;


    public ZapierTriggerService(IWebsiteChannelContext websiteChannelContext, IWorkflowScopeService workflowScopeService, IContentQueryExecutor contentQueryExecutor, IContentQueryResultMapper contentQueryResultMapper, IWebPageQueryResultMapper webPageQueryResultMapper, IInfoProvider<ZapierTriggerEventLogTypeInfo> zapierTriggerEventLogTypeInfoProvider, IInfoProvider<ZapierTriggerInfo> zapierTriggerInfoProvider)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.websiteChannelContext = websiteChannelContext;
        this.workflowScopeService = workflowScopeService;
        this.contentQueryExecutor = contentQueryExecutor;
        this.contentQueryResultMapper = contentQueryResultMapper;
        this.webPageQueryResultMapper = webPageQueryResultMapper;
        this.zapierTriggerEventLogTypeInfoProvider = zapierTriggerEventLogTypeInfoProvider;
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
    }


    public int CreateTrigger(string objectType, string eventType, string zapierUrl)
    {
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerCodeName = ZapierTriggerExtensions.GetUniqueCodename(),
            ZapierTriggerObjectClassType = ZapierTriggerExtensions.GetType(objectType),
            ZapierTriggerEventType = eventType,
            ZapierTriggerObjectType = objectType,
            ZapierTriggerZapierURL = zapierUrl
        };

        zapierTriggerInfoProvider.Set(infoObject);
        return infoObject.ZapierTriggerID;
    }


    public async Task<IDictionary<string, object>?> GetFallbackDataAsync(string objectType, string eventType)
    {
        var dataClass = DataClassInfoProvider.ProviderObject.Get(objectType);

        if (dataClass.ClassContentTypeType.Equals(ClassContentTypeType.EMAIL, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!Enum.TryParse(eventType, out ZapierTriggerEvents eventTypeEnum))
        {
            bool isValid = workflowScopeService.IsMatchingWorflowEventPerObject(eventType, dataClass.ClassID);

            return isValid ? workflowEvents : null;
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


            if (dataClass.ClassContentTypeType.Equals(ClassContentTypeType.WEBSITE, StringComparison.OrdinalIgnoreCase))
            {
                var builder = new ContentItemQueryBuilder().ForContentType(objectType,
                    config => config
                        .WithLinkedItems(1)
                        .ForWebsite(websiteChannelContext.WebsiteChannelName ?? string.Empty)
                    .TopN(1));
                var pages = await contentQueryExecutor.GetWebPageResult(builder, webPageQueryResultMapper.MapPages);
                result = pages.FirstOrDefault();
            }

            return result ?? GetSampleContentItem(objectType);
        }

        if (dataClass.IsForm())
        {
            var actualForm = GetDataFromForm(objectType);
            if (actualForm != null)
            {
                return actualForm;
            }

            var objectTypeInfo = BizFormItemProvider.GetTypeInfo(objectType);
            return GetSampleDataFromObjectTypeInfo(objectTypeInfo);
        }

        var actualInfo = GetDataFromInfoObject(objectType);

        var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
        return actualInfo ?? GetSampleDataFromObjectTypeInfo(typeInfo);
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
            if (!sensitiveCols.Contains(coldef.ColumnName) &&
                !coldef.ColumnName.Equals(nameof(BizFormItem.FormUpdated), StringComparison.OrdinalIgnoreCase) &&
                !coldef.ColumnName.Equals(objectTypeInfo.IDColumn, StringComparison.OrdinalIgnoreCase))
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
        var formItem = formData.First();
        var result = new Dictionary<string, object>();

        foreach (string? col in cols)
        {
            if (!col.Equals(nameof(BizFormItem.FormUpdated), StringComparison.OrdinalIgnoreCase) &&
                !col.Equals(formItem.TypeInfo.IDColumn, StringComparison.OrdinalIgnoreCase))
            {
                result[col] = formItem.GetValue(col);
            }
        }
        return result;
    }


    public void DeleteTrigger(ZapierTriggerInfo info)
    {
        zapierTriggerEventLogTypeInfoProvider.BulkDelete(new WhereCondition()
            .WhereEquals(nameof(ZapierTriggerEventLogTypeInfo.ZapierTriggerEventLogTypeZapierTriggerID), info.ZapierTriggerID));
        zapierTriggerInfoProvider.Delete(info);
    }


    public void AssignEventSeverities(int zapierTriggerID, IEnumerable<string> severities)
    {
        foreach (string severity in severities)
        {
            var info = new ZapierTriggerEventLogTypeInfo
            {
                ZapierTriggerEventLogTypeZapierTriggerID = zapierTriggerID,
                ZapierTriggerEventLogTypeType = severity,
            };
            zapierTriggerEventLogTypeInfoProvider.Set(info);
        }
    }


    private static readonly Dictionary<string, object> workflowEvents = new()
    {
        { "DisplayName", "Content item name" },
        { "ContentTypeName", "ContentType.Type" },
        { "StepName", "Step name" },
        { "OriginalStepName", "Original step name" },
        { "UserID", 132 },
        { "AdminLink", "https://xperienceTestApp.com/admin/#item#"},
        { "UserName", "Michael Scott" },
        { "DateTime", DateTime.Now }
    };
}
