using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BugTracker.Enums;
using BugTracker.Models;
using BugTracker.ViewModels;
using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {

        ApplicationDbContext _DbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Ticket Ticket { get; set; }

        public TicketController(ApplicationDbContext DbContext, UserManager<ApplicationUser> userManager)
        {
            _DbContext = DbContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Create(int? projectId)
        {
            CreateTicketViewModel model = new CreateTicketViewModel();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var allProjects = _DbContext.Projects.Where(p => p.IsOpen).Include(p => p.ProjectApplicationUsers).ToList();
            if (projectId != null)
            {
                model.ProjectId = (int)projectId;
            }


            //Admin and PM can see all projects
            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Project Manager"))
            {
                model.Projects = allProjects;
            }
            else
            {
                //Everyone else sees only projects they are assigned to
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
                model.Projects = sortedProjects;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket();
                ticket.Title = model.Title;
                ticket.Description = model.Description;
                if (model.Description == null)
                {
                    ticket.Description = "No Description";
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);
                ticket.ApplicationUser = user;
                TicketApplicationUser tau = new TicketApplicationUser();
                tau.ApplicationUser = user;
                tau.Ticket = ticket;
                ticket.TicketPriority = model.TicketPriority;
                ticket.TicketStatus = model.TicketStatus;
                ticket.TicketType = model.TicketType;
                ticket.TicketApplicationUsers.Add(tau);
                ticket.Created = DateTime.Now;
                ticket.Project = _DbContext.Projects.Find(model.ProjectId);


                foreach (var file in Request.Form.Files)
                {

                    TicketAttachment attachment = new TicketAttachment();
                    attachment.ApplicationUser = user;
                    attachment.Ticket = ticket;
                    attachment.Name = file.FileName;
                    MemoryStream ms = new MemoryStream();
                    file.CopyTo(ms);
                    attachment.Image = ms.ToArray();

                    ticket.TicketAttachments.Add(attachment);
                }
                _DbContext.Tickets.Add(ticket);
                _DbContext.SaveChanges();
                return View("Index");
            }
            return BadRequest();
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _DbContext.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return RedirectToAction("Details", ticket);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var transfer = _DbContext.Tickets.Single(t => t.Id == id);
                transfer.Title = ticket.Title;
                transfer.TicketType = ticket.TicketType;
                transfer.TicketStatus = ticket.TicketStatus;
                transfer.TicketPriority = ticket.TicketPriority;
                transfer.Description = ticket.Description;
                transfer.Created = ticket.Created;
                _DbContext.Update(transfer);
                await _DbContext.SaveChangesAsync();
                return View("Index");
            }
            return View(ticket);
        }
        public IActionResult ViewAttachment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TicketAttachment img = _DbContext.TicketAttachments.Where(ta => ta.Id == id).Include(a => a.ApplicationUser).Include(a => a.Ticket).SingleOrDefault();
            if (img == null)
            {
                return NotFound();
            }
            string imageBase64Data = Convert.ToBase64String(img.Image);
            string imageDataURL = string.Format("data:image/jpg;base64,{0}", imageBase64Data);
            ViewBag.UserName = img.ApplicationUser.UserName;
            ViewBag.TicketName = img.Ticket.Title;
            ViewBag.ImageDataUrl = imageDataURL;
            return View();
        }

        public IActionResult ViewResolved()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> CreateAttachment(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            Ticket ticket = _DbContext.Tickets.Find(id);
            foreach (var file in Request.Form.Files)
            {

                TicketAttachment attachment = new TicketAttachment();
                attachment.ApplicationUser = user;
                attachment.Ticket = ticket;
                attachment.Name = file.FileName;

                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                attachment.Image = ms.ToArray();

                _DbContext.TicketAttachments.Add(attachment);

            }
            _DbContext.SaveChanges();
            //TODO ADD TOASTER NOTIF
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,Project Manager,Demo")]

        public IActionResult AssignUser(int id)
        {
            ViewBag.ticketId = id;

            var ticket = _DbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketApplicationUsers).FirstOrDefault();

            if (ticket == null)
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

                EditUsersInRoleViewModel.IsSelected = ticket.TicketApplicationUsers.Any(tau => tau.ApplicationUser == user) ? true : false;

                model.Add(EditUsersInRoleViewModel);
            }
            return View(model);

        }

        [Authorize(Roles = "Admin,Project Manager")]
        [HttpPost]
        public async Task<IActionResult> AssignUser(List<EditUsersInRoleViewModel> model, int id)
        {
            try
            {

                var ticket = _DbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketApplicationUsers).SingleOrDefault();
                if (ticket == null)
                {
                    return NotFound("No ticket ID provided. This might be bad.");
                }
                for (int i = 0; i < model.Count; i++)
                {
                    var user = await _userManager.FindByIdAsync(model[i].UserId);
                    if (user == null)
                    {
                        return NotFound("No user ID provided. This might be bad.");

                    }

                    if (model[i].IsSelected && !ticket.TicketApplicationUsers.Any(tau => tau.ApplicationUser == user))
                    {
                        TicketApplicationUser tau = new TicketApplicationUser();

                        tau.ApplicationUser = user;
                        tau.Ticket = ticket;

                        ticket.TicketApplicationUsers.Add(tau);
                        _DbContext.Tickets.Update(ticket);
                        //maybe better to put it before return, im too tired and afraid at this point to optimize
                        _DbContext.SaveChanges();

                    }
                    else if (!model[i].IsSelected && ticket.TicketApplicationUsers.Any(tau => tau.ApplicationUser == user))
                    {
                        //not even going in here
                        var found = ticket.TicketApplicationUsers.FirstOrDefault(x => x.ApplicationUser == user);
                        if (found != null) ticket.TicketApplicationUsers.Remove(found);
                        _DbContext.Tickets.Update(ticket);
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

        [HttpPost]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> CreateComment(int id, string comment)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            TicketComment comm = new TicketComment();
            comm.Ticket = _DbContext.Tickets.Find(id);
            comm.Comment = comment;
            comm.Created = DateTime.Now;
            comm.ApplicationUser = user;
            _DbContext.TicketComments.Add(comm);
            _DbContext.SaveChanges();
            //TODO ADD TOASTER NOTIF
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _DbContext.Tickets.Where(m => m.Id == id).Include(t => t.Project).FirstOrDefaultAsync();
            if (ticket == null)
            {
                return NotFound();
            }
            ViewBag.Project = ticket.Project.Name;
            return View(ticket);
        }


        #region APICalls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var tickets = await _DbContext.Tickets
                .Where(t => t.TicketStatus != TicketStatus.Resolved)
                .Include(t => t.ApplicationUser)
                .Include(t => t.Project)
                .ToListAsync();

            List<Ticket> sortedList = new List<Ticket>();

            foreach (Ticket t in tickets)
            {
                if (t.ApplicationUser == user)
                {
                    //stub project to only provide name(and prevent cyclic JSON)
                    Project stubProject = new Project { Name = t.Project.Name };
                    t.Project = stubProject;
                    sortedList.Add(t);
                }
            }
            //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
            foreach (Ticket t in sortedList)
            {
                t.ApplicationUser = null;
                t.TicketApplicationUsers.Clear();
                t.TicketComments.Clear();
                t.TicketAttachments.Clear();
            }
            return Json(new { data = sortedList });
        }


        [HttpGet]
        public async Task<IActionResult> GetAllResolved()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = await _DbContext.Tickets.Where(t => t.TicketStatus == TicketStatus.Resolved).Include(t => t.ApplicationUser).ToListAsync();
            List<Ticket> sortedList = new List<Ticket>();
            foreach (Ticket t in tickets)
            {
                if (t.ApplicationUser == user)
                {


                    sortedList.Add(t);
                }
            }
            //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
            foreach (Ticket t in sortedList)
            {
                t.ApplicationUser = null;
                t.TicketApplicationUsers.Clear();
                t.TicketComments.Clear();
                t.TicketAttachments.Clear();
            }
            return Json(new { data = sortedList });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssigned()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = await _DbContext.Tickets
                .Where(t => t.TicketStatus != TicketStatus.Resolved)
                .Include(t => t.TicketApplicationUsers)
                .Include(t => t.Project)
                .ToListAsync();
            List<Ticket> sortedList = new List<Ticket>();
            foreach (Ticket t in tickets)
            {
                foreach (TicketApplicationUser pau in t.TicketApplicationUsers)
                {
                    if (pau.ApplicationUserId == user.Id)
                    {
                        //stub project to only provide name(and prevent cyclic JSON)
                        Project stubProject = new Project { Name = t.Project.Name };
                        t.Project = stubProject;

                        sortedList.Add(t);
                    }
                }
            }
            //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
            foreach (Ticket t in sortedList)
            {
                t.ApplicationUser = null;
                t.TicketApplicationUsers.Clear();
                t.TicketComments.Clear();
                t.TicketAttachments.Clear();
            }
            return Json(new { data = sortedList });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResolvedAssigned()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = await _DbContext.Tickets.Where(t => t.TicketStatus == TicketStatus.Resolved).Include(t => t.TicketApplicationUsers).ToListAsync();
            List<Ticket> sortedList = new List<Ticket>();
            foreach (Ticket t in tickets)
            {
                foreach (TicketApplicationUser pau in t.TicketApplicationUsers)
                {
                    if (pau.ApplicationUserId == user.Id)
                    {
                        sortedList.Add(t);
                    }
                }
            }
            //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
            foreach (Ticket t in sortedList)
            {
                t.ApplicationUser = null;
                t.TicketApplicationUsers.Clear();
                t.TicketComments.Clear();
                t.TicketAttachments.Clear();
            }
            return Json(new { data = sortedList });
        }
        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager")]
        public async Task<IActionResult> Resolve(int id)
        {
            var ticket = await _DbContext.Tickets.FirstOrDefaultAsync(u => u.Id == id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            ticket.TicketStatus = TicketStatus.Resolved;
            ticket.LastResolveDate = DateTime.Now;
            _DbContext.Tickets.Update(ticket);
            await _DbContext.SaveChangesAsync();
            //TODO LOCALIZE ME
            return Json(new { success = true, message = "Ticket Resolved" });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _DbContext.TicketAttachments.FirstOrDefaultAsync(u => u.Id == id);
            if (attachment == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _DbContext.TicketAttachments.Remove(attachment);
            await _DbContext.SaveChangesAsync();
            //TODO LOCALIZE ME
            return Json(new { success = true, message = "Attachment Deleted" });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]

        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _DbContext.TicketComments.FirstOrDefaultAsync(u => u.Id == id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _DbContext.TicketComments.Remove(comment);
            await _DbContext.SaveChangesAsync();
            //TODO LOCALIZE ME
            return Json(new { success = true, message = "Comment Deleted" });
        }


        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = _DbContext.Tickets.Where(u => u.Id == id).Include(t => t.TicketAttachments)
                .Include(t => t.TicketComments).SingleOrDefault();
            if (ticket == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            foreach (TicketAttachment ta in ticket.TicketAttachments)
            {
                _DbContext.TicketAttachments.Remove(ta);
            }
            foreach (TicketComment tc in ticket.TicketComments)
            {
                _DbContext.TicketComments.Remove(tc);
            }
            await _DbContext.SaveChangesAsync();
            _DbContext.Tickets.Remove(ticket);
            await _DbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Delete Successful" });
        }

        public IActionResult GetComments(int? id)
        {
            var ticket = _DbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketComments).SingleOrDefault();
            if (ticket == null)
            {
                return NotFound();
            }
            var model = ticket.TicketComments;
            return Json(new { data = model });
        }

        public IActionResult GetAttachments(int? id)
        {
            var ticket = _DbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketAttachments).SingleOrDefault();
            if (ticket == null)
            {
                return NotFound();
            }
            var model = ticket.TicketAttachments;
            return Json(new { data = model });
        }

        public IActionResult GetHistories(int? id)
        {

            var model = _DbContext.TicketHistories.Where(t => t.KeyValue == "{\"Id\":" + id.ToString() + "}").ToList();
            List<TicketHistoryDto> list = new List<TicketHistoryDto>();

            foreach (var m in model)
            {
                //FIXME histories still look ugly
                var jdp = new JsonDiffPatch();
                string rawString = jdp.Diff(m.OldValue, m.NewValue);
                m.OldValue = rawString;
            }
            return Json(new { data = model });
        }



        public async Task<IActionResult> GetMadeByCurrentUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = _DbContext.Tickets.Where(p => p.TicketStatus != TicketStatus.Resolved).Include(p => p.ApplicationUser).ToList();
            List<Ticket> sorted = new List<Ticket>();

            //there's gotta be a better way to sort
            List<Project> sortedProjects = new List<Project>();
            foreach (Ticket t in tickets)
            {

                if (t.ApplicationUser.Id == user.Id)
                {
                    sorted.Add(t);
                }

            }
            return Json(new { data = sortedProjects });
        }

        public async Task<IActionResult> GetAssignedToCurrentUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = _DbContext.Tickets.Where(p => p.TicketStatus != TicketStatus.Resolved).Include(p => p.TicketApplicationUsers).ToList();


            //there's gotta be a better way to sort
            List<Ticket> sorted = new List<Ticket>();
            foreach (Ticket t in tickets)
            {
                foreach (TicketApplicationUser tap in t.TicketApplicationUsers)
                {
                    if (tap.ApplicationUserId == user.Id)
                    {
                        sorted.Add(t);
                    }
                }
            }
            return Json(new { data = sorted });
        }

        public async Task<IActionResult> GetAllAssignedStatus()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = await _DbContext.Tickets.Where(t => t.TicketStatus != TicketStatus.Resolved).Include(t => t.TicketApplicationUsers).ToListAsync();

            List<Ticket> sortedList = new List<Ticket>();
            foreach (Ticket t in tickets)
            {
                foreach (TicketApplicationUser pau in t.TicketApplicationUsers)
                {
                    if (pau.ApplicationUserId == user.Id)
                    {
                        sortedList.Add(t);
                    }
                }
            }
            int open = 0;
            int inprogress = 0;
            int requireadditionalinfo = 0;
            int unknown = 0;
            //"Unloading" related data because EF insists on bringing it and it makes JSON too complex
            foreach (Ticket t in sortedList)
            {
                switch (t.TicketStatus)
                {
                    case TicketStatus.Open:
                        open++;
                        break;
                    case TicketStatus.InProgress:
                        inprogress++;
                        break;
                    case TicketStatus.RequireAdditionalInfo:
                        requireadditionalinfo++;
                        break;
                    case TicketStatus.Unknown:
                        unknown++;
                        break;
                }

            }
            List<JsonDto> list = new List<JsonDto>();
            list.Add(new JsonDto() { title = "Open", value = open.ToString() });
            list.Add(new JsonDto() { title = "In Progress", value = inprogress.ToString() });
            list.Add(new JsonDto() { title = "Additional Info Required", value = requireadditionalinfo.ToString() });
            list.Add(new JsonDto() { title = "Unknown", value = unknown.ToString() });
            return Json(new { data = list });
        }
        public async Task<IActionResult> GetAllAssignedPriority()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tickets = await _DbContext.Tickets.Where(t => t.TicketStatus != TicketStatus.Resolved).Include(t => t.TicketApplicationUsers).ToListAsync();

            List<Ticket> sortedList = new List<Ticket>();
            foreach (Ticket t in tickets)
            {
                foreach (TicketApplicationUser pau in t.TicketApplicationUsers)
                {
                    if (pau.ApplicationUserId == user.Id)
                    {
                        sortedList.Add(t);
                    }
                }
            }
            int low = 0;
            int medium = 0;
            int high = 0;
            int none = 0;
            foreach (Ticket t in sortedList)
            {
                switch (t.TicketPriority)
                {
                    case TicketPriority.Low:
                        low++;
                        break;
                    case TicketPriority.Medium:
                        medium++;
                        break;
                    case TicketPriority.High:
                        high++;
                        break;
                    case TicketPriority.None:
                        none++;
                        break;
                }

            }
            List<JsonDto> list = new List<JsonDto>();
            list.Add(new JsonDto() { title = "Low", value = low.ToString() });
            list.Add(new JsonDto() { title = "Medium", value = medium.ToString() });
            list.Add(new JsonDto() { title = "High", value = high.ToString() });
            list.Add(new JsonDto() { title = "None", value = none.ToString() });
            return Json(new { data = list });
        }
        #endregion

    }
}