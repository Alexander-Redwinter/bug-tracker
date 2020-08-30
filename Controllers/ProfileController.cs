using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;


        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; }

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return View(user);
        }


        //TODO Add confirmation
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var user = await _userManager.FindByIdAsync(ApplicationUser.Id);
            user.UserName = ApplicationUser.UserName;
            user.Email = ApplicationUser.Email;
            await _userManager.UpdateAsync(user);
            return View();
        }


    }
}