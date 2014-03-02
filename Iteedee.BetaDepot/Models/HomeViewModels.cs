using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Models
{
    public class HomeSummary
    {
        public HomeSummary()
        {
            ApplicationBuilds = new List<ApplicationBuildSummaryModel>();
            UploadForm = new UploadFormModel();
        }

        public List<ApplicationBuildSummaryModel> ApplicationBuilds { get; set; }
        public UploadFormModel UploadForm { get; set; }
    }

    public class ApplicationBuildSummaryModel
    {
        public int AppId { get; set; }
        public string AppName { get; set; }
        public string Platform { get; set; }
        public string InstallUrl { get; set; }
        public string BuildNotes { get; set; }
        public string VersionNumber { get; set; }
        public string UploadedByName { get; set; }
        public string UploadedDtm { get; set; }
        public string Environment { get; set; }

    }

    public class UploadFormModel
    {
        public UploadFormModel()
        {
            Environments = new List<Environments>();
        }
        public List<Environments> Environments { get; set; }

    }

    public class Environments
    {
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
    }
}