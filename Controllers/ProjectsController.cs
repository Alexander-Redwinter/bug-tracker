using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        [BindProperty]
        public Project Project { get; set; }


        public ProjectsController(ApplicationDbContext DbContext, UserManager<ApplicationUser> userManager)
        {
            _DbContext = DbContext;
            _userManager = userManager;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ClosedProjects()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _DbContext.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Project model)
        {
            if (ModelState.IsValid)
            {
                Project project = new Project();
                project.Name = model.Name;
                project.Description = model.Description;
                project.IsOpen = true;

                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

                var projectUser = new ProjectApplicationUser();

                projectUser.Project = project;
                projectUser.ApplicationUser = user;

                project.ProjectApplicationUsers.Add(projectUser);
                user.ProjectApplicationUsers.Add(projectUser);

                _DbContext.Projects.Add(project);
                await _userManager.UpdateAsync(user);

                await _DbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        /*
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _DbContext.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(project);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }
        */


        [HttpDelete]
        public async Task<IActionResult> Close(int id)
        {
            var project = await _DbContext.Projects.FindAsync(id);
            project.IsOpen = false;
            _DbContext.Projects.Update(project);
            await _DbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Project closed" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _DbContext.Projects.FindAsync(id);
            _DbContext.Projects.Remove(project);
            await _DbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }

        public async Task<IActionResult> GetProjects()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var allProjects = _DbContext.Projects.Where(p => p.IsOpen).Include(p => p.ProjectApplicationUsers).ToList();


            //Admin and PM can see all projects
            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Project Manager"))
            {
                //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
                foreach (Project p in allProjects)
                {
                    p.ProjectApplicationUsers.Clear();
                }
                return Json(new { data = allProjects });
            }
            //Everyone else sees only projects they are assigned to
            //there's gotta be a better way to sort
            List<Project> sortedProjects = new List<Project>();
            foreach (Project p in allProjects)
            {
                foreach (ProjectApplicationUser pau in p.ProjectApplicationUsers)
                {
                    if (pau.ApplicationUserId == user.Id)
                    {
                        sortedProjects.Add(allProjects.Find(fp => fp.Id == p.Id));
                    }
                }

            }
            //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
            foreach (Project p in allProjects)
            {
                p.ProjectApplicationUsers.Clear();
            }
            return Json(new { data = sortedProjects });
        }


        public async Task<IActionResult> GetClosedProjects()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var allProjects = _DbContext.Projects.Where(p => !p.IsOpen).Include(p => p.ProjectApplicationUsers).ToList();

            //Admin and PM can see all projects
            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Project Manager"))
            {
                return Json(new { data = allProjects });
            }
            //Everyone else sees only projects they are assigned to
            else
            {
                //there's gotta be a better way to sort
                List<Project> sortedProjects = new List<Project>();
                foreach (Project p in allProjects)
                {
                    foreach (ProjectApplicationUser pau in p.ProjectApplicationUsers)
                    {
                        if (pau.ApplicationUserId == user.Id)
                        {
                            sortedProjects.Add(p);
                        }
                    }

                }

                return Json(new { data = sortedProjects });
            }
        }

    }



}
