using CMS.ContentWorkflowEngine;
using CMS.ContentWorkflowEngine.Internal;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Zapier.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Zapier.Actions;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]

public class ActionMoveToStepWorkflowController : ControllerBase
{
    private readonly IWorkflowScopeService workflowScopeService;
    private readonly IContentItemWorkflowManager contentItemWorkflowManager;
    private readonly IWebPageWorkflowManager webPageWorkflowManager;
    private readonly IHeadlessItemWorkflowManager headlessItemWorkflowManager;
    private readonly ZapierConfiguration zapierConfiguration;
    private readonly ILogger<ActionMoveToStepWorkflowController> logger;

    public ActionMoveToStepWorkflowController(IContentItemWorkflowManagerFactory contentItemWorkflowManagerFactory, IWebPageWorkflowManagerFactory webPageWorkflowManagerFactory, ILogger<ActionMoveToStepWorkflowController> logger, IWorkflowScopeService workflowScopeService, IOptionsMonitor<ZapierConfiguration> zapierConfiguration, IHeadlessItemWorkflowManagerFactory headlessItemWorkflowManagerFactory)
    {
        contentItemWorkflowManager = contentItemWorkflowManagerFactory.Create(UserInfoProvider.AdministratorUser.UserID);
        webPageWorkflowManager = webPageWorkflowManagerFactory.Create(UserInfoProvider.AdministratorUser.UserID);
        headlessItemWorkflowManager = headlessItemWorkflowManagerFactory.Create(UserInfoProvider.AdministratorUser.UserID);
        this.logger = logger;
        this.workflowScopeService = workflowScopeService;
        this.zapierConfiguration = zapierConfiguration.CurrentValue;
    }

    [HttpPost]
    [Route($"zapier/actions/movetostep/{ClassContentTypeType.REUSABLE}/{{stepName}}/{{id}}/{{languageName}}")]
    public async Task<ActionResult<object>> MoveToStepReusable(
        string stepName,
        int id,
        string languageName)
    {
        try
        {
            await contentItemWorkflowManager.MoveToStep(id, languageName, stepName);

            return Ok(new { Id = id, LanguageName = languageName, Type = ClassContentTypeType.REUSABLE });
        }
        catch (Exception)
        {
            logger.LogError($"Error occured during reusable item moving to step {stepName} (id: {id}, language: {languageName})");
            return BadRequest($"Error occured during reusable item moving to step {stepName} (id: {id}, language: {languageName})");
        }
    }



    [HttpPost]
    [Route($"zapier/actions/movetostep/{ClassContentTypeType.WEBSITE}/{{stepName}}/{{websiteChannelID}}/{{id}}/{{languageName}}")]
    public async Task<ActionResult<object>> MoveToStepPage(
        string stepName,
        int websiteChannelID,
        int id,
        string languageName)
    {

        try
        {
            await webPageWorkflowManager.MoveToStep(id, languageName, stepName);

            return Ok(new { Id = id, LanguageName = languageName, Type = ClassContentTypeType.WEBSITE });
        }
        catch (Exception)
        {
            logger.LogError($"Error occured during page moving to step (id: {id}, language: {languageName}, websiteChannelID: {websiteChannelID})");
            return BadRequest($"Error occured during page moving to step (id: {id}, language: {languageName}, websiteChannelID: {websiteChannelID})");
        }
    }

    [HttpPost]
    [Route($"zapier/actions/movetostep/{ClassContentTypeType.HEADLESS}/{{stepName}}/{{headlessChannelID}}/{{id}}/{{languageName}}")]
    public async Task<ActionResult<object>> MoveToStepHeadlessItem(
        string stepName,
        int headlessChannelID,
        int id,
        string languageName)
    {

        try
        {
            await headlessItemWorkflowManager.MoveToStep(id, languageName, stepName);

            return Ok(new { Id = id, LanguageName = languageName, Type = ClassContentTypeType.HEADLESS });
        }
        catch (Exception)
        {
            logger.LogError($"Error occured during headless item moving to step (id: {id}, language: {languageName}, headlessChannelID: {headlessChannelID})");
            return BadRequest($"Error occured during headless item moving to step (id: {id}, language: {languageName}, headlessChannelID: {headlessChannelID})");
        }
    }



    [HttpGet]
    [Route($"zapier/actions/movetostep/steps/{{className}}")]
    public ActionResult<IEnumerable<object>> GetSteps(string className)
    {
        if (!zapierConfiguration.AllowedObjects.Contains(className))
        {
            return BadRequest();
        }
        var data = DataClassInfoProvider.ProviderObject.Get(className);
        var steps = workflowScopeService.GetStepsByContentTypeID(data.ClassID)
        .Where(step => step.ContentWorkflowStepType == ContentWorkflowStepType.Custom);
        return Ok(steps
            .Select(s => new
            {
                Id = s.ContentWorkflowStepName,
                Name = s.ContentWorkflowStepDisplayName
            }));
    }

}
