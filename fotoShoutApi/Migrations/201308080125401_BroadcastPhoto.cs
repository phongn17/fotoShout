namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BroadcastPhoto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Photos", "Error", c => c.String());
            AddColumn("dbo.EventBroadcasts", "Thumbnails", c => c.String());
            AddColumn("dbo.EventBroadcasts", "PermaLinks", c => c.String());
            AddColumn("dbo.EventBroadcasts", "Error", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventBroadcasts", "Error");
            DropColumn("dbo.EventBroadcasts", "PermaLinks");
            DropColumn("dbo.EventBroadcasts", "Thumbnails");
            DropColumn("dbo.Photos", "Error");
        }
    }
}
