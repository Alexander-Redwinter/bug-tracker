using BugTracker.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
  : base(options)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProjectApplicationUser>().HasKey(e => new { e.ProjectId, e.ApplicationUserId });
            modelBuilder.Entity<TicketApplicationUser>().HasKey(e => new { e.TicketId, e.ApplicationUserId });
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var temoraryAuditEntities = await AuditNonTemporaryProperties();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await AuditTemporaryProperties(temoraryAuditEntities);
            return result;
        }

        async Task<IEnumerable<Tuple<EntityEntry, TicketHistory>>> AuditNonTemporaryProperties()
        {
            ChangeTracker.DetectChanges();
            var entitiesToTrack = ChangeTracker.Entries().Where(e => !(e.Entity is TicketHistory)
            && e.State != EntityState.Detached && e.State != EntityState.Unchanged
            && (e.Entity is Ticket));

            await TicketHistories.AddRangeAsync(
                entitiesToTrack.Where(e => !e.Properties.Any(p => p.IsTemporary)).Select(e => new TicketHistory()
                {
                    Action = Enum.GetName(typeof(EntityState), e.State),
                    When = DateTime.Now,
                    ApplicationUser = this._httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                    KeyValue = JsonConvert.SerializeObject(e.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()),
                    NewValue = JsonConvert.SerializeObject(e.Properties.Where(p => e.State == EntityState.Added || e.State == EntityState.Modified).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()),
                    OldValue = JsonConvert.SerializeObject(e.Properties.Where(p => e.State == EntityState.Deleted || e.State == EntityState.Modified).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue).NullIfEmpty())
                }).ToList()
            );;

            //Return list of pairs of EntityEntry and ToolAudit  
            return entitiesToTrack.Where(e => e.Properties.Any(p => p.IsTemporary))
                 .Select(e => new Tuple<EntityEntry, TicketHistory>(
                     e,
                 new TicketHistory()
                 {
                     Action = Enum.GetName(typeof(EntityState), e.State),
                     When = DateTime.Now,
                     ApplicationUser = this._httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                     NewValue = JsonConvert.SerializeObject(e.Properties.Where(p => !p.Metadata.IsPrimaryKey()).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty())
                 }
                 )).ToList();
        }

        async Task AuditTemporaryProperties(IEnumerable<Tuple<EntityEntry, TicketHistory>> temporatyEntities)
        {
            if (temporatyEntities != null && temporatyEntities.Any())
            {
                await TicketHistories.AddRangeAsync(
                temporatyEntities.ForEach(t => t.Item2.KeyValue = JsonConvert.SerializeObject(t.Item1.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()))
                    .Select(t => t.Item2)
                );
                await SaveChangesAsync();
            }
            await Task.CompletedTask;
        }


        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

    }
}
