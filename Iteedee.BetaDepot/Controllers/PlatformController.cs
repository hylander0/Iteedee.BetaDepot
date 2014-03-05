﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;


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
                    List<Repository.Application> apps = context.Applications.Where(w =>
                                w.AssignedMembers.Contains(
                                                context.TeamMembers.Where(wt => wt.UserName == userName).FirstOrDefault())
                                                && w.Platform.ToLower() == platform.ToLower()
                                                ).ToList();
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
                                 AppIconUrl = string.Format("{0}App/AppIconImage/?AppUniqueIdentifier={1}", BaseUrl(), a.ApplicationIdentifier),
                                 InstallUrl = Platforms.Common.GeneratePackageInstallUrl(BaseUrl(), "App", "Download", a.Platform, b.UniqueIdentifier.ToString()),
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

        public ActionResult BuildHistory(string id, string environment, int appId)
        {

            Models.PlatformViewAppBuildHistory mdl = new Models.PlatformViewAppBuildHistory();
            if (!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), appId))
                throw new HttpException(403, "You are not a team member of this app.");
            using(var context = new BetaDepot.Repository.BetaDepotContext())
            {
                Repository.Application app = context.Applications.Where(wa => wa.Id == appId).FirstOrDefault();
                List<Repository.ApplicationBuild> builds = context.Builds
                                                                .Where(w => w.Application.Id == appId
                                                                        && (environment == null || w.Environment.EnvironmentName.ToUpper() == environment.ToUpper()))
                                                                .OrderByDescending(o => o.AddedDtm)
                                                                .ToList();

                mdl.AppIconUrl = Platforms.Common.GenerateAppIconUrl(BaseUrl(), app.ApplicationIdentifier);
                mdl.AppId = appId;
                mdl.AppName = app.Name;
                mdl.Platform = app.Platform;
                mdl.selectedEnvironment = environment ?? string.Empty;
                builds.ForEach(f => {
                    mdl.Builds.Add(new Models.PlatformViewAppBuildHistory.BuildHistory()
                        {
                            BuildNotes = f.Notes,
                            Environment = f.Environment.EnvironmentName,
                            UploadedByName = String.Format("{0} {1}", f.AddedBy.FirstName, f.AddedBy.LastName),
                            UploadedDtm = Common.Functions.GetPrettyDate(f.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                            VersionNumber = string.IsNullOrEmpty(f.versionCode) ? f.versionNumber : string.Format("{0} ({1})", f.versionNumber, f.versionCode),
                            InstallUrl = Platforms.Common.GeneratePackageInstallUrl(BaseUrl(), "App", "Download", f.Platform, f.UniqueIdentifier.ToString())
                            
                        });
                });
            }
            return View(mdl);
        }

        public ActionResult Manage(string id)
        {
            string platform = id;

            return View();
        }

        private string BaseUrl()
        {
            //return string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            //return string.Format("{0}://{1}{2}", Request.Url.Scheme, "JustinHyland-PC.na.awwweb.com", Url.Content("~"));
            if (Request.Url.Port == 80)
                return string.Format("{0}://{1}{2}", Request.Url.Scheme, "localhost", Url.Content("~"));
            else
                return string.Format("{0}://{1}:{2}{3}", Request.Url.Scheme, "localhost", Request.Url.Port, Url.Content("~"));
        }
	}
}