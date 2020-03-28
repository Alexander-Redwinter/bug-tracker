using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProjectApplicationUser>().HasKey(e => new { e.ProjectId, e.ApplicationUserId});
            modelBuilder.Entity<TicketApplicationUser>().HasKey(e => new { e.TicketId, e.ApplicationUserId });
        }

        public DbSet<ApplicationUser> ApplicationUsers{ get; set; }
        public DbSet<Project> Projects{ get; set; }
        public DbSet<TicketAttachment> TicketAttachments{ get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

    }
}
