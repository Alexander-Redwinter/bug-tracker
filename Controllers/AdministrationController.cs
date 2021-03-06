﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker
{

    [Authorize(Roles = "Admin,Demo")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult AssignRoles()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string id)
        {
            ViewBag.roleId = id;

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var model = new List<EditUsersInRoleViewModel>();
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var EditUsersInRoleViewModel = new EditUsersInRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                EditUsersInRoleViewModel.IsSelected = await _userManager.IsInRoleAsync(user, role.Name) ? true : false;

                model.Add(EditUsersInRoleViewModel);
            }
            return View(model);
        }

        public async Task<IActionResult> SeedRoles()
        {

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                IdentityRole[] roles = {
                new IdentityRole()
                {
                    Name = "Admin"
                },
                new IdentityRole()
                {
                    Name = "Project Manager"
                },
                new IdentityRole()
                {
                    Name = "Developer"
                },
                new IdentityRole()
                {
                    Name = "Tester"
                },
                new IdentityRole()
                {
                    Name = "Demo"
                }};

                foreach (var r in roles)
                {
                    await _roleManager.CreateAsync(r);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUsersInRole(List<EditUsersInRoleViewModel> model, string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound("No role ID provided. This might be bad.");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                if (user == null)
                {
                    return NotFound("No user ID provided. This might be bad.");

                }

                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }
            }

            return RedirectToAction("AssignRoles");
        }

        //TODO localize
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Delete successful" });
            }
            return Json(new { success = false, message = "Something went wrong" });

        }

        #region APICalls
        [HttpGet]
        public IActionResult GetRolesList()
        {
            var roles = _roleManager.Roles;
            return Json(new { data = roles });

        }

        [HttpGet]
        public IActionResult GetUserList()
        {
            var roles = _userManager.Users;
            return Json(new { data = roles });

        }
        #endregion
    }
}