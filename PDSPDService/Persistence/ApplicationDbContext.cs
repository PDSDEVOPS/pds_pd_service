using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ProductionWebService.Models;

namespace ProductionWebService.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        const string CONNECTION_STRING_KEY = "TicketAdderAuth";
        public DbSet<WebAPIUser> WebAPIUsers { get; set; }
        public DbSet<ExSysRouteAccess> ExSysRouteAccesses { get; set; }
        public DbSet<UserRouteQueryParameter> UserRouteQueryParameters { get; set; }
        public DbSet<WebAPIClient> WebAPIClients { get; set; }

        public ApplicationDbContext() : base(CONNECTION_STRING_KEY)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}