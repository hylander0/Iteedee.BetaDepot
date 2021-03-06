﻿using Iteedee.BetaDepot.Common;
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

        public static ApplicationBuild SaveBuild(string BuildNotes, string FilePath, string CurrentUserName, int environmentId)
        {
            ApplicationBuild retval = null;
            string BuildType = Platforms.Common.GetFilesBuildPlatform(Path.GetFileName(FilePath));
            Guid uniqueBuildId = new Guid(System.IO.Path.GetFileNameWithoutExtension(FilePath));
            if (BuildType.ToUpper() == Constants.BUILD_PLATFORM_ANDROID)
            {
                Platforms.Android.AndroidManifestData data = Platforms.Android.AndroidPackage.GetManifestData(FilePath);
                CreateAndGetApplicationIfNoExists(data.ApplicationName, data.PackageName, Constants.BUILD_PLATFORM_ANDROID, CurrentUserName);

                using (var context = new Repository.BetaDepotContext())
                {
                    Repository.ApplicationBuild buildToSave = new Repository.ApplicationBuild()
                    {
                        AddedDtm = DateTime.UtcNow,
                        Application = context.Applications.Where(w => w.ApplicationIdentifier == data.PackageName).FirstOrDefault(),
                        UniqueIdentifier = uniqueBuildId,
                        Notes = BuildNotes,
                        versionNumber = data.VersionName,
                        versionCode = data.VersionCode,
                        Platform = Constants.BUILD_PLATFORM_ANDROID,
                        AddedBy = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault(),
                        Environment = context.Environments.Where(w => w.Id == environmentId).FirstOrDefault()
                    };
                    context.Builds.Add(buildToSave);
                    context.SaveChanges();
                    retval = buildToSave;
                }


            }
            else if (BuildType.ToUpper() == Constants.BUILD_PLATFORM_IOS)
            {
                Platforms.iOS.iOSBundleData data = Platforms.iOS.iOSBundle.GetIPABundleData(FilePath);
                CreateAndGetApplicationIfNoExists(data.BundleAppName, data.BundleIdentifier, Constants.BUILD_PLATFORM_IOS, CurrentUserName);

                using (var context = new Repository.BetaDepotContext())
                {
                    Repository.ApplicationBuild buildToSave = new Repository.ApplicationBuild()
                    {
                        AddedDtm = DateTime.UtcNow,
                        Application = context.Applications.Where(w => w.ApplicationIdentifier == data.BundleIdentifier).FirstOrDefault(),
                        UniqueIdentifier = Guid.NewGuid(),
                        Notes = BuildNotes,
                        versionNumber = data.BundleVersion,
                        Platform = Constants.BUILD_PLATFORM_IOS,
                        AddedBy = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault(),
                        Environment = context.Environments.Where(w => w.Id == environmentId).FirstOrDefault()
                    };

                    context.Builds.Add(buildToSave);
                    context.SaveChanges();
                    retval = buildToSave;
                }
            }

            return retval;
        }

        public static List<ApplicationBuild> GetLastestBuildsByApplicationAndEnvironment(int appId, string environmentName)
        {
            List<ApplicationBuild> retval = new List<ApplicationBuild>();
            using (var context = new Repository.BetaDepotContext())
            {
                BuildEnvironment e = context.Environments.Where(w => w.EnvironmentName == environmentName).FirstOrDefault();
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
                                    group b by new { b.Application.Id, b.Environment } into g
                                 select new { AppId = g.Key.Id, Date = g.Max(t => t.AddedDtm) }).ToList();

                Console.Write(latestBuilds.Count());

                var join = from l in latestBuilds
                           join b in context.Builds.Include("Environment")
                                                    .Include("Application")
                                                    .Include("AddedBy")
                                on l.AppId equals b.Application.Id
                                where l.Date == b.AddedDtm
                           select b;

                retval = join.ToList();
            }

            return retval;
        }
        //UNTESTED
        public static List<ApplicationBuild> GetAllBuildsAssignedToMember(string CurrentUserName)
        {
            using (var context = new Repository.BetaDepotContext())
            {
                //TeamMember currentMember = context.TeamMembers.Where(w => w.UserName == CurrentUserName).FirstOrDefault();
                //List<Application> apps = new List<Application>();
                //context.Applications
                
                var q = (from b in context.Builds.Include("Environment")
                                    .Include("Application")
                                    .Include("AddedBy")
                          join a in context.Applications on b.Application.Id equals a.Id
                          where a.AssignedMembers.Any(any => any.TeamMember.UserName == CurrentUserName)
                          select b).Include("Environment")
                                    .Include("Application")
                                    .Include("AddedBy");


                return q.ToList();

            }
            
        }


        public static bool IsUserAnAppTeamMember(string UserName, int AppId)
        {
            bool retval = false;
            using (var context = new Repository.BetaDepotContext())
            {
                var apps = (from a in context.Applications
                            join tm in context.ApplicationTeamMembers on a.Id equals tm.ApplicationId
                            where tm.TeamMember.UserName == UserName
                                && a.Id == AppId
                            select a).ToList();
                if (apps != null)
                    retval = apps.Count() > 0;
            }
            return retval;
        }
        //public static bool IsUserAnAppTeamMemberAndInMembershipRole(string UserName, int AppId, string role)
        //{
        //    bool retval = false;
        //    using (var context = new Repository.BetaDepotContext())
        //    {
        //        var apps = (from a in context.Applications
        //                    join tm in context.ApplicationTeamMembers on a.Id equals tm.ApplicationId
        //                    where tm.TeamMember.UserName == UserName
        //                        && a.Id == AppId
        //                        && tm.MemberRole.ToUpper() == role.ToUpper()
        //                    select a).ToList();
        //        if (apps != null)
        //            retval = apps.Count() > 0;
        //    }
        //    return retval;
        //}
        public static bool IsUserAnAppTeamMemberInRole(string UserName, int AppId, string Role)
        {
            bool retval = false;
            using (var context = new Repository.BetaDepotContext())
            {
                var apps = (from a in context.Applications
                            join tm in context.ApplicationTeamMembers on a.Id equals tm.ApplicationId
                            where tm.TeamMember.UserName.ToLower() == UserName.ToLower()
                                && a.Id == AppId
                                && tm.MemberRole.ToUpper() == Role.ToUpper()
                            select a).ToList();
                if (apps != null)
                    retval = apps.Count() > 0;
            }
            return retval;
        }
        public static bool IsUserAnAppTeamMember(string UserName, int AppId, string role)
        {
            bool retval = false;
            using (var context = new Repository.BetaDepotContext())
            {
                retval = context.Applications
                        .Where(w => w.Id == AppId)
                            .FirstOrDefault().AssignedMembers
                            .Where(w => w.TeamMember.UserName == UserName && w.MemberRole == role)
                            .Count() > 0;
            }
            return retval;
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
                        AssignedMembers = new List<ApplicationTeamMember>(){ 
                           new ApplicationTeamMember()
                           {
                               TeamMember = member,
                               MemberRole = Common.Constants.APPLICATION_MEMBER_ROLE_ADMINISTRATOR
                           }
                        },
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