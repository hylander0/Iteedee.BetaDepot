using Iteedee.BetaDepot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Iteedee.BetaDepot.Common;

namespace Iteedee.BetaDepot.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        [Authorize]
        public ActionResult Index(string platform)
        {
            HomeSummary summary = new HomeSummary();
            using (var context = new Repository.BetaDepotContext())
            {
                context.Environments.ToList().ForEach(f =>
                {
                    summary.UploadForm.Environments.Add(new Environments()
                    {
                        EnvironmentId = f.Id,
                        EnvironmentName = f.EnvironmentName
                    });
                });

                if(platform != null)
                {
                    string userName = User.Identity.GetUserName();
                    var apps = context.Applications.Where(w =>
                                w.AssignedMembers.Contains(
                                                context.TeamMembers.Where(wt => wt.UserName == userName).FirstOrDefault())
                                                ).ToList();

                    apps.ForEach(a =>
                    {
                        List<Repository.ApplicationBuild> builds = Repository.Managers.ApplicationBuildMgr
                                                                        .GetLastestBuildsByApplicationAndPlatform(a.Id, platform);

                        if (builds != null)
                            foreach (Repository.ApplicationBuild b in builds)
                            {
                                summary.ApplicationBuilds.Add(new ApplicationBuildSummaryModel()
                                {
                                    AppId = a.Id,
                                    AppName = a.Name,
                                    //Environment = b.environment.EnvironmentName,
                                    Environment = Common.Constants.BUILD_ENVIRONMENT_DEVELOPMENT,
                                    InstallUrl = Platforms.Common.GeneratePackageInstallUrl(BaseUrl(), "App", "Download", a.Platform, b.UniqueIdentifier.ToString()),
                                    Platform = platform,
                                    UploadedByUserName = String.Format("{0} {1}", b.AddedBy.FirstName, b.AddedBy.LastName),
                                    UploadedDtm = Common.Functions.GetPrettyDate(b.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                                    BuildNotes = b.Notes

                                });
                            }
                    });
                }
               
            }
            return View(summary);
        }

        public ActionResult Platform(string platform)
        {
            HomeSummary summary = new HomeSummary();
            using (var context = new Repository.BetaDepotContext())
            {
                context.Environments.ToList().ForEach(f =>
                {
                    summary.UploadForm.Environments.Add(new Environments()
                    {
                        EnvironmentId = f.Id,
                        EnvironmentName = f.EnvironmentName
                    });
                });

                if (platform != null)
                {
                    string userName = User.Identity.GetUserName();
                    var apps = context.Applications.Where(w =>
                                w.AssignedMembers.Contains(
                                                context.TeamMembers.Where(wt => wt.UserName == userName).FirstOrDefault())
                                                ).ToList();

                    apps.ForEach(a =>
                    {
                        List<Repository.ApplicationBuild> builds = Repository.Managers.ApplicationBuildMgr
                                                                        .GetLastestBuildsByApplicationAndPlatform(a.Id, platform);

                        if (builds != null)
                            foreach (Repository.ApplicationBuild b in builds)
                            {
                                summary.ApplicationBuilds.Add(new ApplicationBuildSummaryModel()
                                {
                                    AppId = a.Id,
                                    AppName = a.Name,
                                    //Environment = b.environment.EnvironmentName,
                                    Environment = Common.Constants.BUILD_ENVIRONMENT_DEVELOPMENT,
                                    InstallUrl = Platforms.Common.GeneratePackageInstallUrl(BaseUrl(), "App", "Download", a.Platform, b.UniqueIdentifier.ToString()),
                                    Platform = platform,
                                    UploadedByUserName = String.Format("{0} {1}", b.AddedBy.FirstName, b.AddedBy.LastName),
                                    UploadedDtm = Common.Functions.GetPrettyDate(b.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                                    BuildNotes = b.Notes

                                });
                            }
                    });
                }

            }
            return View(summary);
        }
        private string BaseUrl()
        {
            //return string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            return string.Format("{0}://{1}{2}", Request.Url.Scheme, "JustinHyland-PC.na.awwweb.com", Url.Content("~"));
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}