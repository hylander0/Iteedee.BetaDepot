namespace Iteedee.BetaDepot.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_IsSystemUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeamMembers", "IsSystemUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeamMembers", "IsSystemUser");
        }
    }
}
