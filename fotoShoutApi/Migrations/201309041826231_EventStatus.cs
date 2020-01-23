namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "EventStatus", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "EventStatus");
        }
    }
}
