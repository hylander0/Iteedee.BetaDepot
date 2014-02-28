using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository
{
    public class ApplicationBuild
    {
        [Key]
        public int Id { get; set; }
        public string versionNumber { get; set; }
        public string versionCode { get; set; }
        public string Notes { get; set; }
        public Guid UniqueIdentifier { get; set; }
        public DateTime AddedDtm { get; set; }
        public virtual TeamMember AddedBy { get; set; }
        public virtual Application Application { get; set; }
        public virtual BuildEnvironment Environment { get; set; }
        public string Platform { get; set; }
    }
}