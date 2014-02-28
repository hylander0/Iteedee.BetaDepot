using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Repository
{
    public class BuildEnvironment
    {
        [Key]
        public int Id { get; set; }
        public string EnvironmentName { get; set; }

    }

}