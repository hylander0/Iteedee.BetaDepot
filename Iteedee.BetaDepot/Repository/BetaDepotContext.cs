using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository
{
    public class BetaDepotContext : DbContext
    {

        public BetaDepotContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<ApplicationBuild> Builds { get; set; }
        public DbSet<BuildEnvironment> Environments { get; set; }
        public DbSet<ApplicationTeamMember> ApplicationTeamMembers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }



    }


}