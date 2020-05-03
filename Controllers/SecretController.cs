using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class SecretController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Memory()
        {
            return View();
        }

        public IActionResult Frog()
        {
            return View();
        }

        public IActionResult Connect()
        {
            return View();
        }

        public IActionResult Gourd()
        {
            return View();
        }
    }
}