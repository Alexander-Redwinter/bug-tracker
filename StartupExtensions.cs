using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;

namespace BugTracker
{
    public static class StartupExtensions
    {

        public static IServiceCollection AddNpgsqlDataContext(this IServiceCollection services, string connectionString)
        {
            var databaseUri = new Uri(connectionString);
            var userInfo = databaseUri.UserInfo.Split(':');

            string connection = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/')
            }.ToString();

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(connection));

            return services;
        }
    }
}
