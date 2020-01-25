namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PhotoEmail : DbMigration
    {
        public override void Up()
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PhotoEmails");
        }
    }
}
