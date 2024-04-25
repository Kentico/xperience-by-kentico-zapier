
using CMS.ContentWorkflowEngine;
using CMS.DataEngine;
using CMS.Membership;

namespace Kentico.Xperience.Zapier.Triggers;

internal static class ZapierTriggerDataHelper
{
    public static Dictionary<string, object> GetZapierWorkflowPostObject(this ContentItemWorkflowMoveToStepArguments a) =>
        CreateDictionary(a.DisplayName, a.ContentTypeName, a.StepName, a.OriginalStepName, a.UserID);


    public static Dictionary<string, object> GetZapierWorkflowPostObject(this WebPageWorkflowMoveToStepArguments w) =>
        CreateDictionary(w.DisplayName, w.ContentTypeName, w.StepName, w.OriginalStepName, w.UserID);


    public static Dictionary<string, object> GetZapierWorkflowPostObject(this HeadlessItemWorkflowMoveToStepArguments w) =>
        CreateDictionary(w.DisplayName, w.ContentTypeName, w.StepName, w.OriginalStepName, w.UserID);


    private static Dictionary<string, object> CreateDictionary(string displayName, string contentTypeName, string stepName, string originalStepName, int userID)
    {
        Dictionary<string, object> result = [];

        result.TryAdd("DisplayName", displayName);
        result.TryAdd("ContentTypeName", contentTypeName);
        result.TryAdd("StepName", ContentWorkflowStepInfo.Provider.Get(stepName).ContentWorkflowStepDisplayName ?? stepName);
        result.TryAdd("OriginalStepName", ContentWorkflowStepInfo.Provider.Get(originalStepName).ContentWorkflowStepDisplayName ?? originalStepName);
        result.TryAdd("UserID", userID);
        result.TryAdd("UserName", string.IsNullOrEmpty(UserInfoProvider.ProviderObject.Get(userID).FullName) ?
            UserInfoProvider.ProviderObject.Get(userID).UserName :
            UserInfoProvider.ProviderObject.Get(userID).FullName);
        result.TryAdd("DateTime", DateTime.Now);
        return result;
    }


    public static Dictionary<string, object>? TozapierDictionary(this BaseInfo baseInfo)
    {
        if (baseInfo != null)
        {
            var obj = new Dictionary<string, object>();
            foreach (string? col in baseInfo.ColumnNames)
            {
                object val = baseInfo.GetValue(col);
                obj[col] = val;
            }

            return obj;
        }
        return null;
    }
}
