namespace Iteedee.BetaDepot.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReceiveBuildNotifications : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationTeamMembers", "ReceiveBuildNotifications", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationTeamMembers", "ReceiveBuildNotifications");
        }
    }
}
