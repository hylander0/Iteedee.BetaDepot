using Iteedee.BetaDepot.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Iteedee.BetaDepot.Repository.Managers
{
    public static class ApplicationBuildMgr
    {

        public static void SaveBuild(string BuildNotes, string FilePath, string CurrentUserName, int environmentId)
        {
            string BuildType = Platforms.Common.GetFilesBuildPlatform(Path.GetFileName(FilePath));
            if (BuildType.ToUpper() == Constants.BUILD_PLATFORM_ANDROID)
            {
                Platforms.Android.AndroidManifestData data = Platforms.Android.AndroidManifest.GetManifestData(FilePath);
                CreateAndGetApplicationIfNoExists(data.ApplicationName, data.PackageName, Constants.BUILD_PLATFORM_ANDROID, CurrentUserName);

                using (var context = new Repository.BetaDepotContext())
                {
                    context.Builds.Add(new Repository.ApplicationBuild()
                    {
                        AddedDtm = DateTime.UtcNow,
                        Application = context.Applications.Where(w => w.ApplicationIdentifier == data.PackageName).FirstOrDefault(),
                        UniqueIdentifier = Guid.NewGuid(),
                        Notes = BuildNotes,
                        versionNumber = data.VersionName,
                        versionCode = data.VersionCode,
                        Platform = Constants.BUILD_PLATFORM_ANDROID,
                        AddedBy = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault(),
                        Environment = context.Environments.Where(w => w.Id == environmentId).FirstOrDefault()
                    });
                    context.SaveChanges();
                }


            }
            else if (BuildType.ToUpper() == Constants.BUILD_PLATFORM_IOS)
            {
                Platforms.iOS.iOSBundleData data = Platforms.iOS.iOSBundle.GetIPABundleData(FilePath);
                CreateAndGetApplicationIfNoExists(data.BundleAppName, data.BundleIdentifier, Constants.BUILD_PLATFORM_IOS, CurrentUserName);

                using (var context = new Repository.BetaDepotContext())
                {
                    context.Builds.Add(new Repository.ApplicationBuild()
                    {
                        AddedDtm = DateTime.UtcNow,
                        Application = context.Applications.Where(w => w.ApplicationIdentifier == data.BundleIdentifier).FirstOrDefault(),
                        UniqueIdentifier = Guid.NewGuid(),
                        Notes = BuildNotes,
                        versionNumber = data.BundleVersion,
                        Platform = Constants.BUILD_PLATFORM_IOS,
                        AddedBy = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault(),
                        Environment = context.Environments.Where(w => w.Id == environmentId).FirstOrDefault()
                    });
                    context.SaveChanges();
                }
            }
        }

        public static List<ApplicationBuild> GetLastestBuildsByApplicationAndEnvironment(int appId, string environmentName)
        {
            List<ApplicationBuild> retval = new List<ApplicationBuild>();
            using (var context = new Repository.BetaDepotContext())
            {
                BuildEnvironment e = context.Environments.Where(w => w.EnvironmentName.ToLower() == environmentName.ToLower()).FirstOrDefault();
                int environmentId = e == null ? -1 : e.Id;

                var latestBuilds = (from b in context.Builds
                                where (b.Application == context.Applications.Where(w => w.Id == appId).FirstOrDefault())
                                        && (b.Environment == context.Environments.Where(w => w.Id == environmentId).FirstOrDefault())
                                 group b by b.Id into g
                                select new { buildId = g.Key, Date = g.Max(t=>t.AddedDtm)}).ToList();

                var join = from l in latestBuilds
                           join b in context.Builds
                                on l.buildId equals b.Id
                           select b;

                retval = join.ToList();
            }

            return retval;
        }
        public static List<ApplicationBuild> GetLastestBuildsByApplicationAndPlatform(int appId, string platform)
        {
            List<ApplicationBuild> retval = new List<ApplicationBuild>();
            using (var context = new Repository.BetaDepotContext())
            {
                var latestBuilds = (from b in context.Builds
                                 where (b.Application == context.Applications.Where(w => w.Id == appId).FirstOrDefault())
                                         && (b.Platform == platform)
                                 group b by b.Id into g
                                 select new { buildId = g.Key, Date = g.Max(t => t.AddedDtm) }).ToList();

                var join = from l in latestBuilds
                           join b in context.Builds.Include("Environment")
                                                    .Include("Application")
                                                    .Include("AddedBy")
                                on l.buildId equals b.Id
                           select b;

                retval = join.ToList();
            }

            return retval;
        }

        public static List<ApplicationBuild> GetAllBuildsAssignedToMember(string CurrentUserName)
        {
            List<ApplicationBuild> retval;
            using (var context = new Repository.BetaDepotContext())
            {
                //TeamMember currentMember = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault();
                //List<Application> apps = new List<Application>();
                //context.Applications

                var q = (from b in context.Builds.Include("Environment")
                                    .Include("Application")
                                    .Include("AddedBy")
                          join a in context.Applications on b.Application.Id equals a.Id
                          where a.AssignedMembers.Any(any => any.UserName == CurrentUserName)
                          select b).Include("Environment")
                                    .Include("Application")
                                    .Include("AddedBy");


                return q.ToList();

            }
            
        }

        private static void CreateAndGetApplicationIfNoExists(string Name, string AppIdentifier, string Platform, string CurrentUserName)
        {
            Application app;
            using(var context = new Repository.BetaDepotContext())
            {
                app = context.Applications.Where(w => w.ApplicationIdentifier == AppIdentifier).FirstOrDefault();

                if (app == null)
                {
                    TeamMember member = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault();
                    app = new Application()
                    {
                        ApplicationIdentifier = AppIdentifier,
                        AssignedMembers = new List<TeamMember>(){ member },
                        Name = Name,
                        Platform = Platform
                        
                    };
                    context.Applications.Add(app);
                    context.SaveChanges();
                }

            }
        }

    }
}