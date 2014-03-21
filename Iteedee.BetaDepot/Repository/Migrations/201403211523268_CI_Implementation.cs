namespace Iteedee.BetaDepot.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CI_Implementation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Applications", "AppToken", c => c.String(maxLength: 4000));
            AddColumn("dbo.TeamMembers", "IsSystemUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeamMembers", "IsSystemUser");
            DropColumn("dbo.Applications", "AppToken");
        }
    }
}
