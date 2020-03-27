using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {

        ApplicationDbContext _Db;
        [BindProperty]
        public Ticket Ticket { get; set; }

        public TicketController(ApplicationDbContext db)
        {
            _Db = db;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Upsert(int? Id)
        {
            Ticket = new Ticket();
            if (Id == null)
            {
                return View(Ticket);
            }

            Ticket = _Db.Tickets.FirstOrDefault(t => t.Id == Id);
            if(Ticket == null)
            {
                return NotFound();
            }

            return View(Ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert()
        {
            if (ModelState.IsValid)
            {
                if (Ticket.Id == 0)
                {
                    _Db.Tickets.AddAsync(Ticket);
                }
                else
                {
                    _Db.Tickets.Update(Ticket);
                }
                _Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Ticket");
        }

        #region APICalls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _Db.Tickets.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var ticketsFromDb = await _Db.Tickets.FirstOrDefaultAsync(u => u.Id == id);
            if (ticketsFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _Db.Tickets.Remove(ticketsFromDb);
            await _Db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion

    }
}