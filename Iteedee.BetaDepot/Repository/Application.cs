using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository
{
    public class Application
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string ApplicationIdentifier { get; set; }
        public virtual ICollection<ApplicationTeamMember> AssignedMembers { get; set; }
        public string Platform { get; set; }
        public string AppToken { get; set; }

    }
}