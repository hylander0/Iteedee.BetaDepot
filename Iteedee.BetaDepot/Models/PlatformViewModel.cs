using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Models
{
    public class PlatformViewModel
    {
        public PlatformViewModel()
        { 
        
           // public int MyProperty { get; set; }
            Applications = new List<PlatformBuildDetail>();

        }
        public List<PlatformBuildDetail> Applications { get; set; }
        public string CurrentPlatform { get; set; }

        public class PlatformBuildDetail
        {
            public int AppId { get; set; }
            public string AppName { get; set; }
            public string AppIconUrl { get; set; }
            public string Platform { get; set; }
            public string InstallUrl { get; set; }
            public string BuildNotes { get; set; }
            public string VersionNumber { get; set; }
            public string UploadedByName { get; set; }
            public string UploadedDtm { get; set; }
            public string Environment { get; set; }


        }

    }


    public class PlatformViewAppBuildHistory
    {
        public PlatformViewAppBuildHistory()
        {
            Builds = new List<BuildHistory>();
        }
        public int AppId { get; set; }
        public string AppName { get; set; }
        public string AppIconUrl { get; set; }
        public string Platform { get; set; }
        public string selectedEnvironment { get; set; }
        public List<BuildHistory> Builds { get; set; }

        public class BuildHistory
        {
            public string InstallUrl { get; set; }
            public string BuildNotes { get; set; }
            public string VersionNumber { get; set; }
            public string UploadedByName { get; set; }
            public string UploadedDtm { get; set; }
            public string Environment { get; set; }
        }
    }
}