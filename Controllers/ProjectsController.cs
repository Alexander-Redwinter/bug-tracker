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

        public ProjectsController(ApplicationDbContext DbContext, UserManager<ApplicationUser> userManager)
        {
            _DbContext = DbContext;
            _userManager = userManager;

        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);


            //TODO maybe faster to get all user roles in list and check that way? only one request to DB

            /*Admin and PM can see all projects
            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Project Manager"))
            {
                var list = await _DbContext.Projects.Where(p => p.IsOpen).ToListAsync();
                return View(list);
            }
            else
            {
                //TODO no idea what it does
                var list = _DbContext.Projects.Select(p => new
                {
                    CurrentUser = p.ProjectApplicationUsers.Where(pau => pau.ApplicationUserId == user.Id).Select(u => u.ApplicationUser)
                });
            }*/
            var list = _DbContext.Projects.Select(p => new
            {
                CurrentUserProjects = p.ProjectApplicationUsers.Where(pau => pau.ApplicationUserId == user.Id).Select(p => p.Project)
            });
            return View(list);
        }

        // GET: Projects/Details/5
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

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                _DbContext.Add(project);
                await _DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
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

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _DbContext.Projects.FindAsync(id);
            _DbContext.Projects.Remove(project);
            await _DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _DbContext.Projects.Any(e => e.Id == id);
        }
    }
}
