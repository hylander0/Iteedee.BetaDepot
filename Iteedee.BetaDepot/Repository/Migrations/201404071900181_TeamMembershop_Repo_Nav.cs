namespace Iteedee.BetaDepot.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TeamMembershop_Repo_Nav : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Applications", "TeamMember_Id", "dbo.TeamMembers");
            DropIndex("dbo.Applications", new[] { "TeamMember_Id" });
            DropColumn("dbo.Applications", "TeamMember_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Applications", "TeamMember_Id", c => c.Int());
            CreateIndex("dbo.Applications", "TeamMember_Id");
            AddForeignKey("dbo.Applications", "TeamMember_Id", "dbo.TeamMembers", "Id");
        }
    }
}
