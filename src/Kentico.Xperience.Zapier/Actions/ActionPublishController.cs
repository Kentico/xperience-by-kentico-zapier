using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Headless.Internal;
using CMS.Membership;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Zapier.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.Zapier.Actions;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class ActionPublishController : ControllerBase
{
    private readonly ILogger<ActionPublishController> logger;
    private readonly IContentItemManager contentItemManager;
    private readonly IWebPageManagerFactory webPageManagerFactory;
    private readonly IHeadlessItemManagerFactory headlessItemManagerFactory;

    public ActionPublishController(
        ILogger<ActionPublishController> logger,
        IWebPageManagerFactory webPageManagerFactory,
        IContentItemManagerFactory contentItemManagerFactory,
        IHeadlessItemManagerFactory headlessItemManagerFactory)
    {
        this.logger = logger;
        this.webPageManagerFactory = webPageManagerFactory;
        this.headlessItemManagerFactory = headlessItemManagerFactory;
        contentItemManager = contentItemManagerFactory.Create(AuthenticationHelper.GlobalPublicUser.UserID);
    }

    [HttpPost]
    [Route($"zapier/actions/publish/{ClassContentTypeType.REUSABLE}/{{id}}/{{languageName}}")]
    public async Task<ActionResult<object>> PublishReusable(
        int id,
        string languageName)
    {
        try
        {
            if (await contentItemManager.TryPublish(id, languageName))
            {
                logger.LogInformation($"Reusable item successfully published. (id: {id}, language: {languageName}");
                return Ok(new { Id = id, LanguageName = languageName, Type = ClassContentTypeType.REUSABLE });
            }
            else
            {
                logger.LogInformation($"Reusable item already published. (id: {id}, language: {languageName}");
                return Ok(new { Message = "Already published", Id = id, LanguageName = languageName, Type = ClassContentTypeType.REUSABLE });
            }
        }
        catch (Exception)
        {
            logger.LogError($"Error occured during reusable item publishing (id: {id}, language: {languageName})");
            return BadRequest($"Error occured during reusable item publising (id: {id}, language: {languageName})");
        }
    }

    [HttpPost]
    [Route($"zapier/actions/publish/{ClassContentTypeType.WEBSITE}/{{websiteChannelID}}/{{id}}/{{languageName}}")]
    public async Task<ActionResult<object>> PublishPage(
        int websiteChannelID,
        int id,
        string languageName)
    {
        var webPageManager = webPageManagerFactory.Create(websiteChannelID, AuthenticationHelper.GlobalPublicUser.UserID);

        try
        {
            if (await webPageManager.TryPublish(id, languageName))
            {
                logger.LogInformation($"Page successfully published (id: {id}, language: {languageName})");
                return Ok(new { Id = id, LanguageName = languageName, Type = ClassContentTypeType.WEBSITE });
            }
            else
            {
                logger.LogInformation($"Page already published (id: {id}, language: {languageName})");
                return Ok(new { Message = "Already published", Id = id, LanguageName = languageName, Type = ClassContentTypeType.WEBSITE });
            }
        }
        catch (Exception)
        {
            logger.LogError($"Error occured during page publishing (id: {id}, language: {languageName})");
            return BadRequest($"Error occured during page publishing (id: {id}, language: {languageName})");
        }
    }

    [HttpPost]
    [Route($"zapier/actions/publish/{ClassContentTypeType.HEADLESS}/{{headlessChannelID}}/{{id}}/{{languageName}}")]
    public async Task<ActionResult<object>> PublishHeadlessItem(
        int headlessChannelID,
        int id,
        string languageName)
    {
        var headlessItemManager = headlessItemManagerFactory.Create(headlessChannelID, AuthenticationHelper.GlobalPublicUser.UserID);

        try
        {
            if (await headlessItemManager.TryPublish(id, languageName))
            {
                logger.LogInformation($"Headless item successfully published (id: {id}, language: {languageName})");
                return Ok(new { Id = id, LanguageName = languageName, Type = ClassContentTypeType.HEADLESS });
            }
            else
            {
                logger.LogInformation($"Headless item already published (id: {id}, language: {languageName})");
                return Ok(new { Message = "Already published", Id = id, LanguageName = languageName, Type = ClassContentTypeType.HEADLESS });
            }
        }
        catch (Exception)
        {
            logger.LogError($"Error occured during headless item publishing (id: {id}, language: {languageName})");
            return BadRequest($"Error occured during headless item publishing (id: {id}, language: {languageName})");
        }
    }
}

