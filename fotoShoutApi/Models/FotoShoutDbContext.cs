using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace FotoShoutApi.Models {
    public class FotoShoutDbContext : DbContext {
        public FotoShoutDbContext()
            : base(ConfigurationManager.AppSettings["DbContextName"]) {
        }
        public DbSet<Event> Events { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<GuestPhoto> GuestPhotos { get; set; }
        public DbSet<EventOption> EventOptions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<UserAuthorization> UserAuthorizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<EventBroadcast> EventBroadcasts { get; set; }
        public DbSet<PhotoEmail> PhotoEmails { get; set; }
        public DbSet<PublishAccount> PublishAccounts { get; set; }
        public DbSet<EmailServerAccount> EmailServerAccounts { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        public virtual void Initialize() {
            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[UserRoles] ON");
            UserRoles.AddOrUpdate(
              new UserRole { UserRoleId = 1, Name = "Event Manager" },
              new UserRole { UserRoleId = 2, Name = "Staff" }
            );
            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[UserRoles] OFF");

            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[UserAuthorizations] ON");
            UserAuthorizations.AddOrUpdate(
              new UserAuthorization { Id = 1, AuthorizationKey = Guid.Parse("18a4f579-f907-4f07-8ba8-84bbbecd99bd"), Created = DateTime.Now }
            );
            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[UserAuthorizations] OFF");

            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Accounts] ON");
            Accounts.AddOrUpdate(
              new Account { Id = 1, ApiKey = Guid.Parse("350e5070-0560-4181-9e72-332fa15bab28"), AccountName = "Default" }
            );
            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Accounts] OFF");

            UserRole userRole = UserRoles.Find(1);
            Account userAccount = Accounts.Find(1);
            UserAuthorization userAuth = UserAuthorizations.Find(1);
            
            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] ON");
            Users.AddOrUpdate(
              new User { Id = 1, FirstName = "Phong", LastName = "Nguyen", Email = "phong@vdr.com", Password = "password", Status = "Active", CreatedBy = 1, Created = DateTime.Now, Role = userRole, Account = userAccount, Authorization = userAuth }
            );
            Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] OFF");
        }
    }
}