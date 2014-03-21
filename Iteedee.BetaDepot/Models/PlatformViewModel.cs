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
        public string CurrentUsersMembershipRole { get; set; }
        public List<BuildHistory> Builds { get; set; }

        public class BuildHistory
        {
            public int BuildId { get; set; }
            public string InstallUrl { get; set; }
            public string BuildNotes { get; set; }
            public string VersionNumber { get; set; }
            public string UploadedByName { get; set; }
            public string UploadedDtm { get; set; }
            public string Environment { get; set; }
        }
    }


    public class PlatformViewManage
    {
        public PlatformViewManage()
        {
            Apps = new List<PlatformViewManageApp>();
            
        }
        public List<PlatformViewManageApp> Apps { get; set; }
        public string Platform { get; set; }
        public string PlatformDesc { get; set; }

        public class PlatformViewManageApp
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ApplicationIdentifier { get; set; }
            public int TeamMemberCount { get; set; }
            public int UploadedBuildCount { get; set; }
            public string ApplicationRole { get; set; }
            public string Platform { get; set; }
            public string AppIconUrl { get; set; }

        }

    }

    public class PlatformViewTeamMembers
    {
        public PlatformViewTeamMembers()
        {
            this.MemberList = new List<PlatformViewTeamMembers.Members>();
        }
        public int AppId { get; set; }
        public String AppIconUrl { get; set; }
        public String AppName { get; set; }
        public String CurrentUsersMembershipRole { get; set; }
        public List<PlatformViewTeamMembers.Members> MemberList { get; set; }


        public class Members
        {
            public int Id { get; set; }
            public String GravitarUrl { get; set; }
            public String Name { get; set; }
            public String UserName { get; set; }
            public String MembershipRole { get; set; }
            public String EmailAddress { get; set; }
            public int AssignAppCount { get; set; }
        }
    }

    public class PlatformContinuousIntegration
    {
        public PlatformContinuousIntegration()
        {
        }
        public int AppId { get; set; }
        public String AppIconUrl { get; set; }
        public String AppName { get; set; }
        public String AppToken { get; set; }
        public bool IsContinuousIntegrationConfigured { get; set; }
    }
}

