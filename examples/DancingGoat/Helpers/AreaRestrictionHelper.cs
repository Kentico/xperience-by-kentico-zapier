﻿using Kentico.Builder.Web.Mvc;

using WidgetDefinition = Kentico.PageBuilder.Web.Mvc.WidgetDefinition;

namespace DancingGoat.Helpers
{
    /// <summary>
    /// Provides filter methods to restrict the list of allowed widgets for editable areas.
    /// </summary>
    public static class AreaRestrictionHelper
    {
        /// <summary>
        /// Gets list of widget identifiers allowed for landing page.
        /// </summary>
        public static string[] GetLandingPageRestrictions()
        {
            string[] allowedScopes = new[] { "Kentico.", "DancingGoat.General.", "DancingGoat.LandingPage." };

            return GetWidgetsIdentifiers()
                .Where(id => allowedScopes.Any(scope => id.StartsWith(scope, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }


        private static IEnumerable<string> GetWidgetsIdentifiers() => new ComponentDefinitionProvider<WidgetDefinition>()
                   .GetAll()
                   .Select(definition => definition.Identifier);
    }
}
