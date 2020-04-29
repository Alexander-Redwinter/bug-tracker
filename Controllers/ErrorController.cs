using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            if (statusCode == 404)
            {
                return RedirectToAction("Error404");
            }
            ViewBag.StatusCode = statusCode;
            return View("Error");
        }

        public IActionResult Error404()
        {
            return View();
        }


    }
}