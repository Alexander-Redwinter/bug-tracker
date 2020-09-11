using BugTracker.Enums;
using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker
{
    /// <summary>
    /// Controller that implements operations with tickets
    /// </summary>
    public class TicketController : Controller
    {
        #region Private Members
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IStringLocalizer<SharedResources> localizer;
        [BindProperty]
        private Ticket Ticket { get; set; }
        #endregion

        #region Constructors
        public TicketController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResources> localizer)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.localizer = localizer;
        }
        #endregion

        #region Public Methods

        #endregion
        /// <summary>
        /// Default view for Tickets controller
        /// </summary>
        /// <returns>Index ViewResult</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// "Create ticket" view
        /// </summary>
        /// <param name="projectId">Optional project ID for the created ticket</param>
        /// <returns>Create Ticket ViewResult</returns>
        public async Task<IActionResult> Create(int? projectId)
        {
            CreateTicketViewModel model = new CreateTicketViewModel();

            var _user = await userManager.GetUserAsync(HttpContext.User);
            model.Projects = await dbContext.GetProjectsVisibleForUserAsync(_user, userManager);

            //TODO return index for now, later create a view with "No projects, want to create one?"
            if (model.Projects.Count == 0)
                return View("Index");

            if (projectId != null)
            {
                model.ProjectId = (int)projectId;
                model.returnToProject = true;
            }

            return View(model);
        }

        /// <summary>
        /// Adds created ticket to the DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Redirects to ticket Index, if "returnToProject" specified in the model, returns to details page of that project</returns>
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
                var user = await userManager.GetUserAsync(HttpContext.User);
                ticket.ApplicationUser = user;
                TicketApplicationUser tau = new TicketApplicationUser();
                tau.ApplicationUser = user;
                tau.Ticket = ticket;
                ticket.TicketPriority = model.TicketPriority;
                ticket.TicketStatus = model.TicketStatus;
                ticket.TicketType = model.TicketType;
                ticket.TicketApplicationUsers.Add(tau);
                ticket.Created = DateTime.Now;
                ticket.Project = dbContext.Projects.Find(model.ProjectId);

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

                dbContext.Tickets.Add(ticket);
                dbContext.SaveChanges();

                if (model.returnToProject)
                    return RedirectToAction("Tickets", "Projects", new { id = model.ProjectId });

                return View("Index");
            }

            return BadRequest();
        }

        /// <summary>
        /// "Edit ticket" view
        /// </summary>
        /// <param name="projectId">Ticket ID</param>
        /// <returns>Edit Ticket ViewResult</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await dbContext.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), ticket);
        }


        /// <summary>
        /// Adds edited ticket to the DB
        /// </summary>
        /// <param name="id">Ticket's ID</param>
        /// <param name="ticket">Edited Ticket</param>
        /// <returns>Redirects to index page or to "Edit Ticket" view if model is not valid</returns>
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
                var editedTicket = dbContext.Tickets.Single(t => t.Id == id);
                editedTicket.Title = ticket.Title;
                editedTicket.TicketType = ticket.TicketType;
                editedTicket.TicketStatus = ticket.TicketStatus;
                editedTicket.TicketPriority = ticket.TicketPriority;
                editedTicket.Description = ticket.Description;
                editedTicket.Created = ticket.Created;
                dbContext.Update(editedTicket);
                await dbContext.SaveChangesAsync();
                return View(nameof(Index));
            }

            return View(ticket);
        }

        /// <summary>
        /// Returns page with attachment
        /// </summary>
        /// <param name="id">ID of attachment</param>
        /// <returns></returns>
        public IActionResult ViewAttachment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TicketAttachment img = dbContext.TicketAttachments.Where(ta => ta.Id == id).Include(a => a.ApplicationUser).Include(a => a.Ticket).SingleOrDefault();
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

        /// <summary>
        /// View for resolved tickets
        /// </summary>
        /// <returns>Resolved Tickets ViewResult</returns>
        public IActionResult ViewResolved()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> CreateAttachment(int id)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            Ticket ticket = dbContext.Tickets.Find(id);
            foreach (var file in Request.Form.Files)
            {

                TicketAttachment attachment = new TicketAttachment();
                attachment.ApplicationUser = user;
                attachment.Ticket = ticket;
                attachment.Name = file.FileName;

                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                attachment.Image = ms.ToArray();

                dbContext.TicketAttachments.Add(attachment);

            }
            dbContext.SaveChanges();
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,Project Manager,Demo")]

        public IActionResult AssignUser(int id)
        {
            ViewBag.ticketId = id;

            var ticket = dbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketApplicationUsers).FirstOrDefault();

            if (ticket == null)
            {
                return NotFound();
            }

            var model = new List<EditUsersInRoleViewModel>();
            var users = userManager.Users.ToList();
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

                var ticket = dbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketApplicationUsers).SingleOrDefault();
                if (ticket == null)
                {
                    return NotFound("No ticket ID provided. This might be bad.");
                }
                for (int i = 0; i < model.Count; i++)
                {
                    var user = await userManager.FindByIdAsync(model[i].UserId);
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
                        dbContext.Tickets.Update(ticket);
                        //maybe better to put it before return, im too tired and afraid at this point to optimize
                        dbContext.SaveChanges();

                    }
                    else if (!model[i].IsSelected && ticket.TicketApplicationUsers.Any(tau => tau.ApplicationUser == user))
                    {
                        //not even going in here
                        var found = ticket.TicketApplicationUsers.FirstOrDefault(x => x.ApplicationUser == user);
                        if (found != null) ticket.TicketApplicationUsers.Remove(found);
                        dbContext.Tickets.Update(ticket);
                        //maybe better to put it before return, im too tired and afraid at this point to optimize
                        dbContext.SaveChanges();
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
            var user = await userManager.GetUserAsync(HttpContext.User);
            TicketComment comm = new TicketComment();
            comm.Ticket = dbContext.Tickets.Find(id);
            comm.Comment = comment;
            comm.Created = DateTime.Now;
            comm.ApplicationUser = user;
            dbContext.TicketComments.Add(comm);
            dbContext.SaveChanges();
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await dbContext.Tickets.Where(m => m.Id == id).Include(t => t.Project).FirstOrDefaultAsync();
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
            var user = await userManager.GetUserAsync(HttpContext.User);

            var tickets = await dbContext.Tickets
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
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = await dbContext.Tickets.Where(t => t.TicketStatus == TicketStatus.Resolved).Include(t => t.ApplicationUser).ToListAsync();
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
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = await dbContext.Tickets
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
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = await dbContext.Tickets.Where(t => t.TicketStatus == TicketStatus.Resolved).Include(t => t.TicketApplicationUsers).ToListAsync();
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
            var ticket = await dbContext.Tickets.FirstOrDefaultAsync(u => u.Id == id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            ticket.TicketStatus = TicketStatus.Resolved;
            ticket.LastResolveDate = DateTime.Now;
            dbContext.Tickets.Update(ticket);
            await dbContext.SaveChangesAsync();
            return Json(new { success = true, message = localizer["Ticket Resolved"] });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await dbContext.TicketAttachments.FirstOrDefaultAsync(u => u.Id == id);
            if (attachment == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            dbContext.TicketAttachments.Remove(attachment);
            await dbContext.SaveChangesAsync();
            return Json(new { success = true, message = localizer["Attachment Deleted"] });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager,Developer,Tester")]

        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await dbContext.TicketComments.FirstOrDefaultAsync(u => u.Id == id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            dbContext.TicketComments.Remove(comment);
            await dbContext.SaveChangesAsync();
            return Json(new { success = true, message = localizer["Comment Deleted"] });
        }


        [HttpDelete]
        [Authorize(Roles = "Admin,Project Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = dbContext.Tickets.Where(u => u.Id == id).Include(t => t.TicketAttachments)
                .Include(t => t.TicketComments).SingleOrDefault();
            if (ticket == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            foreach (TicketAttachment ta in ticket.TicketAttachments)
            {
                dbContext.TicketAttachments.Remove(ta);
            }
            foreach (TicketComment tc in ticket.TicketComments)
            {
                dbContext.TicketComments.Remove(tc);
            }
            await dbContext.SaveChangesAsync();
            dbContext.Tickets.Remove(ticket);
            await dbContext.SaveChangesAsync();
            return Json(new { success = true, message = localizer["Delete Successful"] });
        }

        public IActionResult GetComments(int? id)
        {
            var ticket = dbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketComments).SingleOrDefault();
            if (ticket == null)
            {
                return NotFound();
            }
            var model = ticket.TicketComments;
            return Json(new { data = model });
        }

        public IActionResult GetAttachments(int? id)
        {
            var ticket = dbContext.Tickets.Where(t => t.Id == id).Include(t => t.TicketAttachments).SingleOrDefault();
            if (ticket == null)
            {
                return NotFound();
            }
            var model = ticket.TicketAttachments;
            return Json(new { data = model });
        }

        public IActionResult GetHistories(int? id)
        {

            var model = dbContext.TicketHistories.Where(t => t.KeyValue == "{\"Id\":" + id.ToString() + "}").ToList();
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
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = dbContext.Tickets.Where(p => p.TicketStatus != TicketStatus.Resolved).Include(p => p.ApplicationUser).ToList();
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
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = dbContext.Tickets.Where(p => p.TicketStatus != TicketStatus.Resolved).Include(p => p.TicketApplicationUsers).ToList();


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
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = await dbContext.Tickets.Where(t => t.TicketStatus != TicketStatus.Resolved).Include(t => t.TicketApplicationUsers).ToListAsync();

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
            list.Add(new JsonDto() { key = localizer["Open"], value = open.ToString() });
            list.Add(new JsonDto() { key = localizer["In Progress"], value = inprogress.ToString() });
            list.Add(new JsonDto() { key = localizer["Require Additional Info"], value = requireadditionalinfo.ToString() });
            list.Add(new JsonDto() { key = localizer["Unknown"], value = unknown.ToString() });
            return Json(new { data = list });
        }
        public async Task<IActionResult> GetAllAssignedPriority()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var tickets = await dbContext.Tickets.Where(t => t.TicketStatus != TicketStatus.Resolved).Include(t => t.TicketApplicationUsers).ToListAsync();

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
            list.Add(new JsonDto() { key = localizer["Low"], value = low.ToString() });
            list.Add(new JsonDto() { key = localizer["Medium"], value = medium.ToString() });
            list.Add(new JsonDto() { key = localizer["High"], value = high.ToString() });
            list.Add(new JsonDto() { key = localizer["None"], value = none.ToString() });
            return Json(new { data = list });
        }
        #endregion

    }
}