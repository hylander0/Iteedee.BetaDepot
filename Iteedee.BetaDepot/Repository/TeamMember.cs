using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository
{
    public class TeamMember
    {
        [Key]
        public int Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String UserName { get; set; }
        public String EmailAddress { get; set; }

        public virtual ICollection<Application> AssignedApplications { get; set; }


    }
}