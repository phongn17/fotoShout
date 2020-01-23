namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropChannelGroupTable : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.EventChannelGroups");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.EventChannelGroups",
                c => new
                    {
                        EventChannelGroupId = c.Int(nullable: false, identity: true),
                        ChannelGroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventChannelGroupId);
            
        }
    }
}
