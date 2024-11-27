using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.Controllers
{
    public class HttpErrorsController : Controller
    {
        public IActionResult Error(int code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (code == 404)
            {
                return View("NotFound");
            }

            return StatusCode(code);
        }
    }
}
