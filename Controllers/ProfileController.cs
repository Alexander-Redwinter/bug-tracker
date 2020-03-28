using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;


        [BindProperty]
        public IdentityUser identityUser { get; set; }

        public ProfileController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return View(user);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var user = await _userManager.FindByIdAsync(identityUser.Id);
            user.UserName = identityUser.UserName;
            user.Email = identityUser.Email;
            await _userManager.UpdateAsync(user);
            return View();
        }


    }
}