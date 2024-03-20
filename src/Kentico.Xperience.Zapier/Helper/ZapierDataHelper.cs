
using CMS.ContentWorkflowEngine;

namespace Kentico.Xperience.Zapier.Helper;

public static class ZapierDataHelper
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
        result.TryAdd("StepName", stepName);
        result.TryAdd("OriginalStepName", originalStepName);
        result.TryAdd("UserID", userID);
        return result;
    }



}
