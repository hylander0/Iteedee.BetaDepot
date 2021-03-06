﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DefaultValue(false)]
        public bool IsSystemUser { get; set; }
        public virtual ICollection<ApplicationTeamMember> UserMemberships { get; set; }

        public IEnumerable<Application> UserApps
        {
            get { return UserMemberships.Select(ab => ab.Application); }
        }



    }
}