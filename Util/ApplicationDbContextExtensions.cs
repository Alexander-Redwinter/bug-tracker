using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker
{
    /// <summary>
    /// Extensions for <see cref="ApplicationDbContext"/>. Purpose is to have complex DB access logic in one place
    /// </summary>
    public static class ApplicationDbContextExtensions
    {
        public static async Task<List<Project>> GetProjectsVisibleForUserAsync(this ApplicationDbContext context, ApplicationUser user, UserManager<ApplicationUser> userManager, bool closed = false)
        {

            var allProjects = context.Projects.Where(p => closed ? !p.IsOpen : p.IsOpen).Include(p => p.ProjectApplicationUsers).ToList();

            //Admin and PM can see all projects
            if (await userManager.IsInRoleAsync(user, "Admin") || await userManager.IsInRoleAsync(user, "Project Manager"))
            {
                return allProjects;
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
                return sortedProjects;
            }
        }
    }
}
