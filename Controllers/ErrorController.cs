using Microsoft.AspNetCore.Mvc;

namespace BugTracker
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("Error404");
            }
            ViewBag.StatusCode = statusCode;
            return View("Error");
        }


    }
}