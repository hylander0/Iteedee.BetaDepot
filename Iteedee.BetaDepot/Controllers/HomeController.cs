using Iteedee.BetaDepot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Iteedee.BetaDepot.Common;
using Iteedee.BetaDepot.Repository;
using System.Net;

namespace Iteedee.BetaDepot.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        [Authorize]
        public ActionResult Index()
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

                Repository.Managers.ApplicationBuildMgr.GetAllBuildsAssignedToMember(User.Identity.GetUserName())
                     .ForEach(f => {

                         summary.ApplicationBuilds.Add(new ApplicationBuildSummaryModel()
                         {
                             AppId = f.Id,
                             AppName = f.Application.Name,
                             Environment = f.Environment.EnvironmentName,
                             InstallUrl = Platforms.Common.GeneratePackageInstallUrl("App", "Download", f.Platform, f.UniqueIdentifier.ToString()),
                             Platform = f.Platform,
                             UploadedByName = String.Format("{0} {1}", f.AddedBy.FirstName, f.AddedBy.LastName),
                             UploadedDtm = Common.Functions.GetPrettyDate(f.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                             BuildNotes = f.Notes

                         });
                     });
            }
            return View(summary);
        }
        [HttpGet]
        [Authorize]
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
                                                context.ApplicationTeamMembers.Where(wt => wt.TeamMember.UserName == userName).FirstOrDefault())
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
                                    Environment = b.Environment.EnvironmentName,
                                    InstallUrl = Platforms.Common.GeneratePackageInstallUrl("App", "Download", a.Platform, b.UniqueIdentifier.ToString()),
                                    Platform = platform,
                                    UploadedByName = String.Format("{0} {1}", b.AddedBy.FirstName, b.AddedBy.LastName),
                                    UploadedDtm = Common.Functions.GetPrettyDate(b.AddedDtm.ToLocalTime(), "MM/dd/yy"),
                                    BuildNotes = b.Notes

                                });
                            }
                    });
                }

            }
            return View(summary);
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