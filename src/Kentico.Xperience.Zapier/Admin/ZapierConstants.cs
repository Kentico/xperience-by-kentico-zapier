﻿

namespace Kentico.Xperience.Zapier.Admin;

internal static class ZapierConstants
{
    public const string AppId = "Xperience by Kentico Zapier";

    internal static class TriggerResourceConstants
    {
        public const string ResourceDisplayName = "Kentico Integration - Zapier Trigger";
        public const string ResourceName = "CMS.Integration.ZapierTrigger";
        public const string ResourceDescription = "Kentico Zapier triggers";
        public const bool ResourceIsInDevelopment = false;
    }

    internal static class ApiKeyResourceConstants
    {
        public const string ResourceDisplayName = "Kentico Integration - Zapier Api Key";
        public const string ResourceName = "CMS.Integration.ZapierKey";
        public const string ResourceDescription = "Kentico Zapier API keys";
        public const bool ResourceIsInDevelopment = false;
    }

    internal static class Permissions
    {
        public const string GENERATE = "Generate";
    }

    internal static class AuthenticationScheme
    {
        public const string XbyKZapierApiKeyScheme = "XbyKZapierApiKey";
    }
}
