namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChannelGroup_MultipleChannels : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EventChannelGroups", "Event_EventId", "dbo.Events");
            DropIndex("dbo.EventChannelGroups", new[] { "Event_EventId" });
            AddColumn("dbo.Events", "ChannelGroupId", c => c.Int());
            DropColumn("dbo.EventChannelGroups", "Event_EventId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventChannelGroups", "Event_EventId", c => c.Int());
            DropColumn("dbo.Events", "ChannelGroupId");
            CreateIndex("dbo.EventChannelGroups", "Event_EventId");
            AddForeignKey("dbo.EventChannelGroups", "Event_EventId", "dbo.Events", "EventId");
        }
    }
}
