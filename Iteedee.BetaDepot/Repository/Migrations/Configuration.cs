using Iteedee.BetaDepot.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository.Migrations
{
    public class Configuration : DbMigrationsConfiguration<BetaDepotContext>
    {


        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;    //TODO: REMOVE
            MigrationsDirectory = "Repository\\Migrations";


        }


        protected override void Seed(BetaDepotContext Context)
        {

            Context.Environments.AddOrUpdate(new BuildEnvironment()
            {
                Id = 1,
                EnvironmentName = Constants.BUILD_ENVIRONMENT_DEVELOPMENT

            });
            Context.Environments.AddOrUpdate(new BuildEnvironment()
            {
                Id = 2,
                EnvironmentName = Constants.BUILD_ENVIRONMENT_TEST

            });
            Context.Environments.AddOrUpdate(new BuildEnvironment()
            {
                Id = 3,
                EnvironmentName = Constants.BUILD_ENVIRONMENT_PRODUCTION

            });

            //This user is a default user that
            if (Context.TeamMembers.Where(w => w.UserName.ToLower() == "ci@betadepot.iteedee.com").FirstOrDefault() == null)
                Context.TeamMembers.Add(new TeamMember()
                {
                    EmailAddress = "CI@betadepot.iteedee.com",
                    UserName = "CI@betadepot.iteedee.com",
                    FirstName = "Continuous",
                    LastName = "Integration",
                    IsSystemUser = true
                });


            Context.SaveChanges();
        }


        public static void Init()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Repository.BetaDepotContext, Repository.Migrations.Configuration>());
            using (var context = new Repository.BetaDepotContext())
            {
                context.Database.Initialize(true);
            }
        }
    }
}