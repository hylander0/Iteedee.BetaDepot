using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository
{
    public class ApplicationTeamMember
    {
        [Key, Column(Order = 0)]
        public virtual int MemberId { get; set; }
        [Key, Column(Order = 1)]
        public virtual int ApplicationId { get; set; }

        public virtual Application Application { get; set; }
        public virtual TeamMember TeamMember { get; set; }

        public String MemberRole { get; set; }
    }
}