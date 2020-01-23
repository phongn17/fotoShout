namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        EventName = c.String(nullable: false),
                        EventDescription = c.String(),
                        EventDate = c.DateTime(nullable: false),
                        EventLocation = c.String(),
                        EventFolder = c.String(nullable: false),
                        EventVirtualPath = c.String(),
                        CreatedBy = c.Int(nullable: false),
                        Created = c.DateTime(),
                        PublishAlbum = c.String(),
                        EventOption_EventOptionId = c.Int(nullable: false),
                        EmailTemplate_EmailTemplateId = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.EventOptions", t => t.EventOption_EventOptionId, cascadeDelete: true)
                .ForeignKey("dbo.EmailTemplates", t => t.EmailTemplate_EmailTemplateId)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.EventOption_EventOptionId)
                .Index(t => t.EmailTemplate_EmailTemplateId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Photos",
                c => new
                    {
                        PhotoId = c.Guid(nullable: false),
                        Folder = c.String(),
                        Filename = c.String(nullable: false),
                        Image = c.String(),
                        Status = c.Byte(nullable: false),
                        Thumbnail = c.String(),
                        Created = c.DateTime(nullable: false),
                        SubmittedBy = c.Int(nullable: false),
                        Submitted = c.DateTime(),
                        Rating = c.Int(nullable: false),
                        Event_EventId = c.Int(),
                    })
                .PrimaryKey(t => t.PhotoId)
                .ForeignKey("dbo.Events", t => t.Event_EventId)
                .Index(t => t.Event_EventId);
            
            CreateTable(
                "dbo.GuestPhotoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Event_EventId = c.Int(nullable: false),
                        AuthorizePublish = c.Boolean(nullable: false),
                        Guest_GuestId = c.Guid(),
                        Photo_PhotoId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Guests", t => t.Guest_GuestId)
                .ForeignKey("dbo.Photos", t => t.Photo_PhotoId)
                .Index(t => t.Guest_GuestId)
                .Index(t => t.Photo_PhotoId);
            
            CreateTable(
                "dbo.Guests",
                c => new
                    {
                        GuestId = c.Guid(nullable: false),
                        Salutation = c.String(),
                        LastName = c.String(),
                        FirstName = c.String(),
                        MiddleInitial = c.String(),
                        Email = c.String(),
                        PrimaryPhone = c.String(),
                        OtherPhone = c.String(),
                        Fax = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        City = c.String(),
                        Region = c.String(),
                        PostalCode = c.String(),
                        Country = c.String(),
                        Signature = c.Binary(),
                        Event_EventId = c.Int(),
                    })
                .PrimaryKey(t => t.GuestId)
                .ForeignKey("dbo.Events", t => t.Event_EventId)
                .Index(t => t.Event_EventId);
            
            CreateTable(
                "dbo.Sponsors",
                c => new
                    {
                        SponsorId = c.Int(nullable: false, identity: true),
                        SponsorName = c.String(nullable: false),
                        SponsorLogo = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.SponsorId)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Title = c.String(),
                        Phone = c.String(),
                        PhoneExt = c.String(),
                        Email = c.String(nullable: false),
                        Password = c.String(),
                        Status = c.String(),
                        SecurityQuestion = c.String(),
                        SecurityAnswer = c.String(),
                        CreatedBy = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Role_UserRoleId = c.Int(nullable: false),
                        Authorization_Id = c.Int(),
                        Account_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserRoles", t => t.Role_UserRoleId, cascadeDelete: true)
                .ForeignKey("dbo.UserAuthorizations", t => t.Authorization_Id)
                .ForeignKey("dbo.Accounts", t => t.Account_Id, cascadeDelete: true)
                .Index(t => t.Role_UserRoleId)
                .Index(t => t.Authorization_Id)
                .Index(t => t.Account_Id);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserRoleId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.UserRoleId);
            
            CreateTable(
                "dbo.UserAuthorizations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorizationKey = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(),
                        ApiKey = c.Guid(nullable: false),
                        Notes = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventChannelGroups",
                c => new
                    {
                        EventChannelGroupId = c.Int(nullable: false, identity: true),
                        ChannelGroupId = c.Int(nullable: false),
                        Event_EventId = c.Int(),
                    })
                .PrimaryKey(t => t.EventChannelGroupId)
                .ForeignKey("dbo.Events", t => t.Event_EventId)
                .Index(t => t.Event_EventId);
            
            CreateTable(
                "dbo.EventOptions",
                c => new
                    {
                        EventOptionId = c.Int(nullable: false, identity: true),
                        EventOptionName = c.String(nullable: false),
                        NameOption = c.Boolean(nullable: false),
                        EmailOption = c.Boolean(nullable: false),
                        PhoneOption = c.Boolean(nullable: false),
                        FaxOption = c.Boolean(nullable: false),
                        AddressOption = c.Boolean(nullable: false),
                        SignatureOption = c.Boolean(nullable: false),
                        SalutationOption = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventOptionId);
            
            CreateTable(
                "dbo.EmailTemplates",
                c => new
                    {
                        EmailTemplateId = c.Int(nullable: false, identity: true),
                        EmailTemplateName = c.String(nullable: false),
                        EmailSubject = c.String(nullable: false),
                        EmailContent = c.String(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.EmailTemplateId)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.EventBroadcasts",
                c => new
                    {
                        EventBroadcastId = c.Int(nullable: false, identity: true),
                        BroadcastId = c.Int(nullable: false),
                        PhotoId = c.Guid(nullable: false),
                        EventId = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.EventBroadcastId);
            
            CreateTable(
                "dbo.PublishAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(nullable: false),
                        ApiKey = c.Guid(nullable: false),
                        Email = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        Account_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.Account_Id)
                .Index(t => t.Account_Id);
            
            CreateTable(
                "dbo.EmailServerAccounts",
                c => new
                    {
                        EmailServerAccountId = c.Int(nullable: false, identity: true),
                        Server = c.String(nullable: false),
                        Port = c.Int(),
                        Domain = c.String(),
                        Username = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        EnableSSL = c.Boolean(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.EmailServerAccountId)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.SponsorEvents",
                c => new
                    {
                        Sponsor_SponsorId = c.Int(nullable: false),
                        Event_EventId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Sponsor_SponsorId, t.Event_EventId })
                .ForeignKey("dbo.Sponsors", t => t.Sponsor_SponsorId, cascadeDelete: true)
                .ForeignKey("dbo.Events", t => t.Event_EventId, cascadeDelete: true)
                .Index(t => t.Sponsor_SponsorId)
                .Index(t => t.Event_EventId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SponsorEvents", new[] { "Event_EventId" });
            DropIndex("dbo.SponsorEvents", new[] { "Sponsor_SponsorId" });
            DropIndex("dbo.EmailServerAccounts", new[] { "User_Id" });
            DropIndex("dbo.PublishAccounts", new[] { "Account_Id" });
            DropIndex("dbo.EmailTemplates", new[] { "User_Id" });
            DropIndex("dbo.EventChannelGroups", new[] { "Event_EventId" });
            DropIndex("dbo.Users", new[] { "Account_Id" });
            DropIndex("dbo.Users", new[] { "Authorization_Id" });
            DropIndex("dbo.Users", new[] { "Role_UserRoleId" });
            DropIndex("dbo.Sponsors", new[] { "User_Id" });
            DropIndex("dbo.Guests", new[] { "Event_EventId" });
            DropIndex("dbo.GuestPhotoes", new[] { "Photo_PhotoId" });
            DropIndex("dbo.GuestPhotoes", new[] { "Guest_GuestId" });
            DropIndex("dbo.Photos", new[] { "Event_EventId" });
            DropIndex("dbo.Events", new[] { "User_Id" });
            DropIndex("dbo.Events", new[] { "EmailTemplate_EmailTemplateId" });
            DropIndex("dbo.Events", new[] { "EventOption_EventOptionId" });
            DropForeignKey("dbo.SponsorEvents", "Event_EventId", "dbo.Events");
            DropForeignKey("dbo.SponsorEvents", "Sponsor_SponsorId", "dbo.Sponsors");
            DropForeignKey("dbo.EmailServerAccounts", "User_Id", "dbo.Users");
            DropForeignKey("dbo.PublishAccounts", "Account_Id", "dbo.Accounts");
            DropForeignKey("dbo.EmailTemplates", "User_Id", "dbo.Users");
            DropForeignKey("dbo.EventChannelGroups", "Event_EventId", "dbo.Events");
            DropForeignKey("dbo.Users", "Account_Id", "dbo.Accounts");
            DropForeignKey("dbo.Users", "Authorization_Id", "dbo.UserAuthorizations");
            DropForeignKey("dbo.Users", "Role_UserRoleId", "dbo.UserRoles");
            DropForeignKey("dbo.Sponsors", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Guests", "Event_EventId", "dbo.Events");
            DropForeignKey("dbo.GuestPhotoes", "Photo_PhotoId", "dbo.Photos");
            DropForeignKey("dbo.GuestPhotoes", "Guest_GuestId", "dbo.Guests");
            DropForeignKey("dbo.Photos", "Event_EventId", "dbo.Events");
            DropForeignKey("dbo.Events", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Events", "EmailTemplate_EmailTemplateId", "dbo.EmailTemplates");
            DropForeignKey("dbo.Events", "EventOption_EventOptionId", "dbo.EventOptions");
            DropTable("dbo.SponsorEvents");
            DropTable("dbo.EmailServerAccounts");
            DropTable("dbo.PublishAccounts");
            DropTable("dbo.EventBroadcasts");
            DropTable("dbo.EmailTemplates");
            DropTable("dbo.EventOptions");
            DropTable("dbo.EventChannelGroups");
            DropTable("dbo.Accounts");
            DropTable("dbo.UserAuthorizations");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Users");
            DropTable("dbo.Sponsors");
            DropTable("dbo.Guests");
            DropTable("dbo.GuestPhotoes");
            DropTable("dbo.Photos");
            DropTable("dbo.Events");
        }
    }
}
