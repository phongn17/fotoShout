namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Website : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Websites",
                c => new
                    {
                        WebsiteId = c.Int(nullable: false, identity: true),
                        WebsiteName = c.String(nullable: false),
                        HeaderImage = c.String(),
                        HeaderUrl = c.String(),
                        FooterImage = c.String(),
                        FooterUrl = c.String(),
                        TopInfoBlockImage = c.String(),
                        TopInfoBlockUrl = c.String(),
                        BottomInfoBlockImage = c.String(),
                        BottomInfoBlockUrl = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.WebsiteId)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.Events", "WebsiteId", c => c.Int(nullable: true, identity: false));
            AddForeignKey("dbo.Events", "WebsiteId", "dbo.Websites", "WebsiteId");
            CreateIndex("dbo.Events", "WebsiteId");
            DropColumn("dbo.Sponsors", "SponsorHeaderImage");
            DropColumn("dbo.Sponsors", "SponsorHeaderUrl");
            DropColumn("dbo.Sponsors", "SponsorFooterImage");
            DropColumn("dbo.Sponsors", "SponsorFooterUrl");
            DropColumn("dbo.Sponsors", "SponsorTopInfoBlockImage");
            DropColumn("dbo.Sponsors", "SponsorTopInfoBlockUrl");
            DropColumn("dbo.Sponsors", "SponsorBottomInfoBlockImage");
            DropColumn("dbo.Sponsors", "SponsorBottomInfoBlockUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sponsors", "SponsorBottomInfoBlockUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorBottomInfoBlockImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorTopInfoBlockUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorTopInfoBlockImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorFooterUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorFooterImage", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorHeaderUrl", c => c.String());
            AddColumn("dbo.Sponsors", "SponsorHeaderImage", c => c.String());
            DropIndex("dbo.Websites", new[] { "User_Id" });
            DropIndex("dbo.Events", new[] { "WebsiteId" });
            DropForeignKey("dbo.Websites", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Events", "WebsiteId", "dbo.Websites");
            DropColumn("dbo.Events", "WebsiteId");
            DropTable("dbo.Websites");
        }
    }
}
