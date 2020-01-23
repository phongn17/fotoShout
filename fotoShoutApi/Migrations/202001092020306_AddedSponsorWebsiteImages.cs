namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSponsorWebsiteImages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sponsors", "SponsorHeaderImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorHeaderUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorFooterImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorFooterUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorTopInfoBlockImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorTopInfoBlockUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorBottomInfoBlockImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorBottomInfoBlockUrl", c => c.String());
            DropTable("dbo.PhotoEmails");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PhotoEmails",
                c => new
                    {
                        PhotoEmailId = c.Int(nullable: false, identity: true),
                        PhotoId = c.Guid(nullable: false),
                        GuestId = c.Guid(nullable: false),
                        EventId = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                        Error = c.String(),
                    })
                .PrimaryKey(t => t.PhotoEmailId);
            
            DropColumn("dbo.Sponsors", "SponsorBottomInfoBlockUrl");
            DropColumn("dbo.Sponsors", "SponsorBottomInfoBlockImage");
            DropColumn("dbo.Sponsors", "SponsorTopInfoBlockUrl");
            DropColumn("dbo.Sponsors", "SponsorTopInfoBlockImage");
            DropColumn("dbo.Sponsors", "SponsorFooterUrl");
            DropColumn("dbo.Sponsors", "SponsorFooterImage");
            DropColumn("dbo.Sponsors", "SponsorHeaderUrl");
            DropColumn("dbo.Sponsors", "SponsorHeaderImage");
        }
    }
}
