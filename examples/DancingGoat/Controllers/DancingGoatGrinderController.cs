using CMS.ContentEngine;

using DancingGoat;
using DancingGoat.Controllers;
using DancingGoat.Models;

using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;

using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWebPageRoute(GrinderPage.CONTENT_TYPE_NAME, typeof(DancingGoatGrinderController), WebsiteChannelNames = new[] { DancingGoatConstants.WEBSITE_CHANNEL_NAME }, ActionName = nameof(DancingGoatGrinderController.Detail))]

namespace DancingGoat.Controllers
{
    public class DancingGoatGrinderController : Controller
    {
        private readonly ProductPageRepository productPageRepository;
        private readonly IWebPageDataContextRetriever webPageDataContextRetriever;
        private readonly IPreferredLanguageRetriever currentLanguageRetriever;
        private readonly ITaxonomyRetriever taxonomyRetriever;


        public DancingGoatGrinderController(ProductPageRepository productPageRepository,
            IWebPageDataContextRetriever webPageDataContextRetriever,
            IPreferredLanguageRetriever currentLanguageRetriever,
            ITaxonomyRetriever taxonomyRetriever)
        {
            this.productPageRepository = productPageRepository;
            this.webPageDataContextRetriever = webPageDataContextRetriever;
            this.currentLanguageRetriever = currentLanguageRetriever;
            this.taxonomyRetriever = taxonomyRetriever;
        }


        public async Task<IActionResult> Detail()
        {
            string languageName = currentLanguageRetriever.Get();
            int webPageItemId = webPageDataContextRetriever.Retrieve().WebPage.WebPageItemID;

            var grinder = await productPageRepository.GetProduct<GrinderPage>(GrinderPage.CONTENT_TYPE_NAME, webPageItemId, languageName, cancellationToken: HttpContext.RequestAborted);

            return View(await GrinderDetailViewModel.GetViewModel(grinder, languageName, taxonomyRetriever));
        }
    }
}
