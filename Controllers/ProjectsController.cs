using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.ViewModels;

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
                Project project = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsOpen = true
                };

                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

                var projectUser = new ProjectApplicationUser
                {
                    Project = project,
                    ApplicationUser = user
                };

                project.ProjectApplicationUsers.Add(projectUser);
                user.ProjectApplicationUsers.Add(projectUser);

                _DbContext.Projects.Add(project);
                await _userManager.UpdateAsync(user);

                await _DbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

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
                        return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public IActionResult AssignUser(int id)
        {
            ViewBag.ticketId = id;

            var project = _DbContext.Projects.Where(t => t.Id == id).Include(t => t.ProjectApplicationUsers).FirstOrDefault();

            if (project == null)
            {
                return NotFound();
            }

            var model = new List<EditUsersInRoleViewModel>();

            foreach (var user in _userManager.Users)
            {
                var EditUsersInRoleViewModel = new EditUsersInRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                EditUsersInRoleViewModel.IsSelected = project.ProjectApplicationUsers.Any(tau => tau.ApplicationUser == user) ? true : false;

                model.Add(EditUsersInRoleViewModel);
            }
            return View(model);

        }

        public IActionResult GetTickets(int? id)
        {
            var project = _DbContext.Projects.Where(p => p.Id == id).Include(p => p.Tickets).SingleOrDefault();
            if (project == null)
            {
                return NotFound();
            }
            var model = project.Tickets;
            return Json(new { data = model });
        }

        [HttpPost]
        public async Task<IActionResult> AssignUser(List<EditUsersInRoleViewModel> model, int id)
        {
            try
            {

                var project = _DbContext.Projects.Where(t => t.Id == id).Include(t => t.ProjectApplicationUsers).SingleOrDefault();
                if (project == null)
                {
                    return NotFound("No project ID provided. This might be bad.");
                }
                for (int i = 0; i < model.Count; i++)
                {
                    var user = await _userManager.FindByIdAsync(model[i].UserId);
                    if (user == null)
                    {
                        return NotFound("No user ID provided. This might be bad.");

                    }

                    if (model[i].IsSelected && !project.ProjectApplicationUsers.Any(tau => tau.ApplicationUser == user))
                    {
                        ProjectApplicationUser pau = new ProjectApplicationUser();

                        pau.ApplicationUser = user;
                        pau.Project = project;

                        project.ProjectApplicationUsers.Add(pau);
                        _DbContext.Projects.Update(project);
                        //maybe better to put it before return, im too tired and afraid at this point to optimize
                        _DbContext.SaveChanges();

                    }
                    else if (!model[i].IsSelected && project.ProjectApplicationUsers.Any(tau => tau.ApplicationUser == user))
                    {
                        //not even going in here
                        var found = project.ProjectApplicationUsers.FirstOrDefault(x => x.ApplicationUser == user);
                        if (found != null) project.ProjectApplicationUsers.Remove(found);
                        _DbContext.Projects.Update(project);
                        //maybe better to put it before return, im too tired and afraid at this point to optimize
                        _DbContext.SaveChanges();
                    }
                    else
                    {
                        continue;
                    }
                }

                return RedirectToAction("Details", new { id = id });

            }
            catch (Exception e)
            {
                return Json(new { e.Message });
            }
        }

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
        public async Task<IActionResult> Open(int id)
        {
            var project = await _DbContext.Projects.FindAsync(id);
            project.IsOpen = true;
            _DbContext.Projects.Update(project);
            await _DbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Reopened Project" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            //Very expensive operation with multiple DB calls, not recommended to delete projects forever
            try
            {
                //no way to currently include more than one related field in ThenInclude
                var project =  _DbContext.Projects.Where(u => u.Id == id)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketAttachments)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketComments)
                    .SingleOrDefault();
                if(project == null)
                {
                    return Json(new { success = false, message = "No such project exists? Contact developer" });
                }
                foreach(Ticket t in project.Tickets)
                {
                    foreach(TicketAttachment ta in t.TicketAttachments)
                    {
                        _DbContext.TicketAttachments.Remove(ta);
                    }
                    foreach (TicketComment tc in t.TicketComments)
                    {
                        _DbContext.TicketComments.Remove(tc);
                    }
                    await _DbContext.SaveChangesAsync();
                    _DbContext.Tickets.Remove(t);
                }
                await _DbContext.SaveChangesAsync();
                _DbContext.Projects.Remove(project);
                await _DbContext.SaveChangesAsync();
                return Json(new { success = true, message = "Delete successful" });
            }
            catch
            {
                return Json(new { success = false, message = "Delete failed" });
            }

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
                        sortedProjects.Add(p);
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
                //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
                foreach (Project p in allProjects)
                {
                    p.ProjectApplicationUsers.Clear();
                }
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
                //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
                foreach (Project p in sortedProjects)
                {
                    p.ProjectApplicationUsers.Clear();
                }
                return Json(new { data = sortedProjects });
            }
        }

    }



}
