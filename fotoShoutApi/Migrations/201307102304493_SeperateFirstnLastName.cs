namespace FotoShoutApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeperateFirstnLastName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventOptions", "FirstNameOption", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.EventOptions", "LastNameOption", c => c.Boolean(nullable: false, defaultValue: true));
            DropColumn("dbo.EventOptions", "NameOption");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventOptions", "NameOption", c => c.Boolean(nullable: false));
            DropColumn("dbo.EventOptions", "LastNameOption");
            DropColumn("dbo.EventOptions", "FirstNameOption");
        }
    }
}
