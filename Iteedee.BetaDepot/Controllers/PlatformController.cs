using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;
using System.IO;


namespace Iteedee.BetaDepot.Controllers
{
    [Authorize]
    public class PlatformController : Controller
    {
        //
        // GET: /Platform/
        public ActionResult Index(string id)
        {
            string platform = id;
            Models.PlatformViewModel mdl = new Models.PlatformViewModel();
            mdl.CurrentPlatform = platform;
            using (var context = new Repository.BetaDepotContext())
            {

                if (platform != null)
                {
                    string userName = User.Identity.GetUserName();
                    List<Repository.ApplicationTeamMember> membershipList = context.ApplicationTeamMembers
                                                                .Where(wt => wt.TeamMember.UserName == userName).ToList();

                    var apps = (from a in context.Applications
                               join tm in context.ApplicationTeamMembers on a.Id equals tm.ApplicationId
                               where tm.TeamMember.UserName == userName
                               select a).ToList();

                    //List<Repository.Application> apps = context.Applications.Where(w =>
                    //            w.AssignedMembers.Contains(
                    //                            context.ApplicationTeamMembers.Where(wt => wt.TeamMember.UserName.ToLower() == userName.ToLower()).FirstOrDefault())
                    //                            && w.Platform.ToLower() == platform.ToLower()
                    //                            ).ToList();
                    foreach(Repository.Application a in apps)
                    {
                        List<Repository.ApplicationBuild> latestBuilds = Repository.Managers.ApplicationBuildMgr.GetLastestBuildsByApplicationAndPlatform(a.Id, platform);
                         foreach(Repository.ApplicationBuild b in latestBuilds)
                         {
                             mdl.Applications.Add(new Models.PlatformViewModel.PlatformBuildDetail()
                             {
                                 AppId = a.Id,
                                 AppName = a.Name,
                                 Environment = b.Environment.EnvironmentName,
                                 AppIconUrl = Platforms.Common.GenerateAppIconUrl(a.ApplicationIdentifier),
                                 InstallUrl = Platforms.Common.GeneratePackageInstallUrl("App", "Download", a.Platform, b.UniqueIdentifier.ToString()),
                                 Platform = platform,
                                 UploadedByName = String.Format("{0} {1}", b.AddedBy.FirstName, b.AddedBy.LastName),
                                 UploadedDtm = Common.Functions.GetPrettyDate(b.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                                 BuildNotes = b.Notes,
                                 VersionNumber = b.versionNumber
                             });     
                         }
  
                    }


                }
            }

            return View(mdl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteBuild(int id)
        {
            using (var context = new Repository.BetaDepotContext())
            {
                //Repository.ApplicationBuild build = context.Builds.Where(w => w.Id == id).FirstOrDefault();
                //context.Builds.Remove(build);
                //System.IO.File.Delete(Platforms.Common.GetLocalBuildFileLocation(Server.MapPath("~"),
                //                                                build.UniqueIdentifier.ToString(), 
                //                                                build.Platform)
                //                        );
                //context.SaveChanges();
            }

            return Json(new
            {
                Status = "OK",
                Msg = ""
            });
        }
        public ActionResult BuildHistory(string Platform, int id, string environment)
        {

            Models.PlatformViewAppBuildHistory mdl = new Models.PlatformViewAppBuildHistory();
            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), id))
                throw new HttpException(403, "You are not a team member of this app.");

            string currentUser = User.Identity.GetUserName().ToLower();
            using(var context = new BetaDepot.Repository.BetaDepotContext())
            {
                Repository.Application app = context.Applications.Where(wa => wa.Id == id).FirstOrDefault();
                mdl.CurrentUsersMembershipRole = context.ApplicationTeamMembers
                                                .Where(w => w.TeamMember.UserName.ToLower() == currentUser
                                                            && w.ApplicationId == id).FirstOrDefault().MemberRole;

                List<Repository.ApplicationBuild> builds = context.Builds
                                                                .Where(w => w.Application.Id == id
                                                                        && (environment == null || w.Environment.EnvironmentName == environment))
                                                                .OrderByDescending(o => o.AddedDtm)
                                                                .ToList();

                mdl.AppIconUrl = Platforms.Common.GenerateAppIconUrl(app.ApplicationIdentifier);
                mdl.AppId = id;
                mdl.AppName = app.Name;
                mdl.Platform = app.Platform;
                mdl.selectedEnvironment = environment ?? string.Empty;
                builds.ForEach(f => {
                    mdl.Builds.Add(new Models.PlatformViewAppBuildHistory.BuildHistory()
                        {
                            BuildId = f.Id,
                            BuildNotes = f.Notes,
                            Environment = f.Environment.EnvironmentName,
                            UploadedByName = String.Format("{0} {1}", f.AddedBy.FirstName, f.AddedBy.LastName),
                            UploadedDtm = Common.Functions.GetPrettyDate(f.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                            VersionNumber = string.IsNullOrEmpty(f.versionCode) ? f.versionNumber : string.Format("{0} ({1})", f.versionNumber, f.versionCode),
                            InstallUrl = Platforms.Common.GeneratePackageInstallUrl("App", "Download", f.Platform, f.UniqueIdentifier.ToString()),
                            Platform = f.Platform
                            
                        });
                });
            }
            return View(mdl);
        }

        public ActionResult Manage(string id)
        {
            string platform = id;
            string userName = User.Identity.GetUserName();
            Models.PlatformViewManage mdl = new Models.PlatformViewManage();
            mdl.Platform = platform.ToUpper();
            if (id.ToUpper() == Common.Constants.BUILD_PLATFORM_ANDROID)
                mdl.PlatformDesc = "Android";
            else if (id.ToUpper() == Common.Constants.BUILD_PLATFORM_IOS)
                mdl.PlatformDesc = "iOS";
          
            using (var context = new BetaDepot.Repository.BetaDepotContext())
            {


                List<Repository.Application> apps = (from a in context.Applications
                                            join tm in context.ApplicationTeamMembers on a.Id equals tm.ApplicationId
                                            where tm.TeamMember.UserName == userName
                                                && a.Platform == platform
                                            select a).ToList();
                foreach(Repository.Application a in apps)
                {

                    mdl.Apps.Add(new Models.PlatformViewManage.PlatformViewManageApp()
                        {
                            ApplicationIdentifier = a.ApplicationIdentifier,
                            Id = a.Id,
                            Name = a.Name,
                            Platform = a.Platform,
                            TeamMemberCount = a.AssignedMembers.Count(),
                            ApplicationRole = a.AssignedMembers.Where(w => w.TeamMember.UserName.ToLower() == userName.ToLower()).FirstOrDefault().MemberRole,
                            UploadedBuildCount = context.Builds.Where(w => w.Application.Id == a.Id).Count(),
                            AppIconUrl = Platforms.Common.GenerateAppIconUrl(a.ApplicationIdentifier)

                        });
                }
            }
            return View(mdl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GenerateNewTokenForApp(int id)
        {
            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), id))
                throw new HttpException(403, "You are not a team member of this app.");
            String token = string.Empty;
            try
            {
                using (var context = new Repository.BetaDepotContext())
                {
                    Repository.Application app = context.Applications.Where(w => w.Id == id).FirstOrDefault();
                    int currentTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    token = Common.Functions.GenerateMD5Hash(string.Format("apptoken|{0}|{1}", app.ApplicationIdentifier.ToLower(), currentTimestamp));
                    app.AppToken = token;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return Json(new { Msg = Common.Constants.APPLICATION_JSON_RESULT_ERROR });
            }


            return Json(new 
                {
                    Msg = Common.Constants.APPLICATION_JSON_RESULT_SUCCESS,
                    AppToken = token
                }
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConfigureAppForCI(int id, bool shouldConfigure)
        {
            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), id))
                throw new HttpException(403, "You are not a team member of this app.");

            Repository.TeamMember CIUser = Repository.Managers.ApplicationMgr.GetSystemCIUser();
            try
            {
                if (shouldConfigure)
                {
                    //Check to make sure the CI user is not already a member of the APP
                    if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(CIUser.UserName, id))
                    {
                        using (var context = new Repository.BetaDepotContext())
                        {
                            Repository.Application app = context.Applications.Where(w => w.Id == id).FirstOrDefault();
                            Repository.ApplicationTeamMember membership = new Repository.ApplicationTeamMember()
                            {
                                TeamMember = CIUser,
                                MemberRole = Common.Constants.APPLICATION_MEMBER_ROLE_CONTINUOUS_INTEGRATION
                            };

                            app.AssignedMembers.Add(membership);
                            context.SaveChanges();
                        }
                    }
                }
                else
                {
                    //Check to make sure the CI user is not already a member of the APP
                    if (Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), id))
                    {
                        using (var context = new Repository.BetaDepotContext())
                        {
                            Repository.Application app = context.Applications.Where(w => w.Id == id).FirstOrDefault();
                            app.AppToken = null;
                            Repository.ApplicationTeamMember membership = context.ApplicationTeamMembers
                                                                                .Where(w => w.TeamMember.UserName.ToLower() == CIUser.UserName)
                                                                                .FirstOrDefault();

                            app.AssignedMembers.Remove(membership);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { Msg = Common.Constants.APPLICATION_JSON_RESULT_ERROR });
            }


            return Json(new { Msg = Common.Constants.APPLICATION_JSON_RESULT_SUCCESS });
        }
        public ActionResult ContinuousIntegration(int id, string platform)
        {
            Models.PlatformContinuousIntegration mdl = new Models.PlatformContinuousIntegration();

            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), id))
                throw new HttpException(403, "You are not a team member of this app.");

            string currentUser = User.Identity.GetUserName().ToLower();

            using (var context = new Repository.BetaDepotContext())
            {
                Repository.Application app = context.Applications.Where(w => w.Id == id).FirstOrDefault();
                if (app != null)
                {
                    mdl.AppId = app.Id;
                    mdl.AppName = app.Name;
                    mdl.AppIconUrl = Platforms.Common.GenerateAppIconUrl(app.ApplicationIdentifier);
                    mdl.AppToken = app.AppToken;
                    Repository.TeamMember CIUser = Repository.Managers.ApplicationMgr.GetSystemCIUser();
                    mdl.IsContinuousIntegrationConfigured = Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(CIUser.UserName, id);
                }
            }
            
            return View(mdl);
        }
        public ActionResult TeamMembers(int id, string platform)
        {
            Models.PlatformViewTeamMembers mdl = new Models.PlatformViewTeamMembers();
            

            
            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), id))
                throw new HttpException(403, "You are not a team member of this app.");

            string currentUser = User.Identity.GetUserName().ToLower();
            
            using(var context = new Repository.BetaDepotContext())
            {
                mdl.CurrentUsersMembershipRole = context.ApplicationTeamMembers
                                                .Where(w => w.TeamMember.UserName.ToLower() == currentUser
                                                            && w.ApplicationId == id).FirstOrDefault().MemberRole;
                Repository.Application app = context.Applications.Where(w => w.Id == id).FirstOrDefault();
                if (app != null)
                {
                    mdl.AppId = app.Id;
                    mdl.AppName = app.Name;
                    mdl.AppIconUrl = Platforms.Common.GenerateAppIconUrl(app.ApplicationIdentifier);
                }
                else
                    return View(mdl);

                var q = (from tm in context.TeamMembers
                         join atm in context.ApplicationTeamMembers on tm.Id equals atm.TeamMemberId
                         where atm.ApplicationId == id
                         select new {
                             TeamMemberId = tm.Id,
                             EmailAddress = tm.EmailAddress,
                             FirstName = tm.FirstName,
                             LastName = tm.LastName,
                             UserName = tm.UserName,
                             AssignAppCount = tm.UserMemberships.Count(),
                             TeamMembershipRole = atm.MemberRole
                         });

                q.ToList().ForEach(f => {
                    mdl.MemberList.Add(new Models.PlatformViewTeamMembers.Members()
                        {
                            UserName = f.UserName,
                            Id = f.TeamMemberId,
                            EmailAddress = f.EmailAddress,
                            GravitarUrl = string.Format("http://www.gravatar.com/avatar/{0}?s=36", Common.Functions.GenerateMD5Hash(f.EmailAddress.ToLower())),
                            Name = Common.Functions.FormatFullName(f.FirstName, f.LastName),
                            MembershipRole = f.TeamMembershipRole,
                            AssignAppCount = f.AssignAppCount,
                            
                        });
                });

            }
            return View(mdl);
        }

        public ViewResult AppDetail(string platform, int id, int buildId)
        {
            Repository.ApplicationBuild build;

            using (var context = new BetaDepot.Repository.BetaDepotContext())
            {
                build = context.Builds.Include("Environment").Include("Application").Include("AddedBy").Where(w => w.Id == buildId).FirstOrDefault();
            }

            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), build.Application.Id))
                throw new HttpException(403, "You are not a team member of this app.");

            return View(new Models.PlatformViewAppDetail()
                {
                    AppIconUrl = Platforms.Common.GenerateAppIconUrl(build.Application.ApplicationIdentifier),
                    AppId = build.Application.Id,
                    AppName = build.Application.Name,
                    BuildId = buildId,
                    Platform = platform,
                    BuildNotes = build.Notes,
                    Environment = build.Environment.EnvironmentName,
                    InstallUrl = Platforms.Common.GeneratePackageInstallUrl("App", "Download", platform, build.UniqueIdentifier.ToString()),
                    UploadedByName = String.Format("{0} {1}", build.AddedBy.FirstName, build.AddedBy.LastName),
                    UploadedDtm = String.Format("{0:f}",build.AddedDtm),
                    VersionNumber = build.versionNumber,
                    VersionCode = build.versionCode
                    
                });
        }

	}
}