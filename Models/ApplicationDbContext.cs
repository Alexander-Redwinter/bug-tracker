using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectApplicationUser>().HasKey(e => new { e.ProjectId, e.ApplicationUserId});
            modelBuilder.Entity<TicketApplicationUser>().HasKey(e => new { e.TicketId, e.ApplicationUserId });
        }

        public DbSet<Ticket> Tickets { get; set; }

    }
}
