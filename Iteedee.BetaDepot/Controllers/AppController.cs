using Iteedee.BetaDepot.Platforms;
using Iteedee.BetaDepot.Repository.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Iteedee.BetaDepot.Common;
using Iteedee.BetaDepot.Repository;

namespace Iteedee.BetaDepot.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        public FileResult Download(string FileName, string Platform)
        {
            string fileExtension = Path.GetExtension(FileName);
            if (fileExtension.ToLower() == ".plist")
            {
                //pList are generated
                string ipaFileName = String.Format("{0}.ipa", Path.GetFileNameWithoutExtension(FileName));
                string ipaFilePath = Path.Combine(Server.MapPath("~/App_Data/Files"), ipaFileName);



                string xml = Platforms.iOS.iOSBundle.GenerateBundlesSoftwarePackagePlist(
                    ipaFilePath,
                   string.Format("{0}App/Download?FileName={1}", BaseUrl(), ipaFileName));

                var bytes = Encoding.UTF8.GetBytes(xml);
                var result = new FileContentResult(bytes, System.Net.Mime.MediaTypeNames.Text.Xml);
                result.FileDownloadName = FileName;
                return result;
            }
            else
            {
                string filePath = Path.Combine(Server.MapPath("~/App_Data/Files"), FileName);
                //Return actual file
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, FileName);
            }

        }
        [HttpGet]
        [ActionName("AppIconImage")]
        public FileResult AppIconImage(string AppUniqueIdentifier)
        {
            string filePath = Path.Combine(Server.MapPath("~/App_Data/Files/Icons"), AppUniqueIdentifier + ".png");
            if(!System.IO.File.Exists(filePath))
                filePath = Path.Combine(Server.MapPath("~/App_Data/Files"), AppUniqueIdentifier + ".jpg");
            if (!System.IO.File.Exists(filePath))
                return null;

            
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Image.Jpeg, filePath);
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
        [HttpPost]
        [ActionName("SaveBuild")]
        public ActionResult SaveBuild(string BuildNotes, string FileName, int EnvironmentId)
        {
            string BuildType = Platforms.Common.GetFilesBuildPlatform(FileName);
            string filePath = Path.Combine(Server.MapPath("~/App_Data/Files"), FileName);
            Iteedee.BetaDepot.Repository.Managers.ApplicationBuildMgr.SaveBuild(BuildNotes, filePath, User.Identity.GetUserName(), EnvironmentId);
            return Redirect("~/Home/Index");
        }


        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            String name = "";
            String filePath = "";
            if (files == null || files.Count() == 0)
                return Json(new { Status = "ERROR",  Message = "No file was uploaded" });
            else if (files.Count() > 1)
                return Json(new { Status = "ERROR",  Message = "Only one file upload is supported" });
            //else if (!Platforms.Common.isBuildFileSupported(files.ElementAt(0).FileName))
            //    return Json(new {  Status = "ERROR",  Message = string.Format("This file '{0}' is not supported", files.ElementAt(0).FileName) });


            try
            {
                name = string.Format("{0}{1}", Guid.NewGuid(), Path.GetExtension(files.ElementAt(0).FileName));
                filePath = Path.Combine(Server.MapPath("~/App_Data/Files"), name);
                files.ElementAt(0).SaveAs(filePath);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "ERROR", Message = "An error occured reading the file." });
            }

            if (Platforms.Common.GetFilesBuildPlatform(files.ElementAt(0).FileName) == Constants.BUILD_PLATFORM_ANDROID)
            {
                //Android
                Platforms.Android.AndroidManifestData data = Platforms.Android.AndroidPackage.GetManifestData(filePath);
                Platforms.Android.AndroidPackage.ExtractPackageAppIconIfNotExists(filePath, Server.MapPath("~/App_Data/Files/Icons"), data.PackageName);
                return Json(new
                {
                    Status = "OK",
                    FileName = name,
                    Platform = Constants.BUILD_PLATFORM_ANDROID,
                    PackageName = data.PackageName,
                    AppName = data.ApplicationName,
                    VersionName = data.VersionName

                });
            }
            else if (Platforms.Common.GetFilesBuildPlatform(files.ElementAt(0).FileName) == Constants.BUILD_PLATFORM_IOS)
            {

                //iOS
                Platforms.iOS.iOSBundleData bundleData = Platforms.iOS.iOSBundle.GetIPABundleData(filePath);
                Platforms.iOS.iOSBundle.ExtractBundleAppIconIfNotExists(filePath, Server.MapPath("~/App_Data/Files/Icons"), bundleData.BundleIdentifier);
                return Json(new
                {
                    Status = "OK",
                    FileName = name,
                    Platform = Constants.BUILD_PLATFORM_IOS,
                    PackageName = bundleData.BundleIdentifier,
                    AppName = bundleData.BundleAppName,
                    VersionName = bundleData.BundleVersion

                });
            }
            else
            {
                return Json(new { Status = "ERROR", Message = string.Format("This file '{0}' is not supported", files.ElementAt(0).FileName) });
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GenerateInviteUrl(string email, string assignedRole, int appId)
        {
            string url = "";
            string msg = "OK";
            bool isAlreadyTeamMember = Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(email, appId);
            if(!isAlreadyTeamMember)
            {
                int TeamMemberId = -1;
                using(var context = new Repository.BetaDepotContext())
                {
                    Application app = context.Applications.Where(w => w.Id == appId).FirstOrDefault();
                    TeamMember tm = context.TeamMembers.Where(w => w.UserName == email).FirstOrDefault();
                    if(tm == null)
                    {
                        tm = new TeamMember()
                        {
                            EmailAddress = email,
                            UserName = email
                        };
                        context.TeamMembers.Add(tm);
                    }
                    context.SaveChanges();
                    TeamMemberId = tm.Id;
                    int timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    string uHash = Common.Functions.GenerateMD5Hash(string.Format("{0}|{1}|{2}", app.ApplicationIdentifier.ToLower(), email.ToLower(), timestamp));
                    string rHash = Common.Functions.GenerateMD5Hash(string.Format("{0}|{1}|{2}", app.ApplicationIdentifier.ToLower(), assignedRole.ToLower(), timestamp));
                    url = string.Format("{0}App/AcceptInvite/?uHash={1}&rHash={2}&appId={3}&userName={4}", BaseUrl(), uHash, rHash, app.Id, Url.Encode(email.ToLower()));
                }
            }
            else
                msg = "Error";

            return Json(new {
                Msg = msg,
                Url = url
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateUserRole(int appId, int memberId, string roleToUpdate)
        {
            if(!Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(User.Identity.GetUserName(), appId,Constants.APPLICATION_MEMBER_ROLE_ADMINISTRATOR))
            {
                return Json(new {
                    Status = "Error",
                    Msg = "You are not a administrator of this app"
                });
            }

            using(var context = new Repository.BetaDepotContext())
            {

                ApplicationTeamMember membership = context.ApplicationTeamMembers
                                            .Where(w => w.TeamMemberId == memberId
                                                        && w.ApplicationId == appId)
                                            .FirstOrDefault();

                if (membership != null)
                    membership.MemberRole = roleToUpdate.ToUpper();

                context.SaveChanges();
            }
            return Json(new
            {
                Status = "OK",
                Msg = ""
            });

        }

        [HttpGet]
        public ActionResult AcceptInvite(string uHash, string rHash, int appId, string userName)
        {
            string platform = string.Empty;
            if(uHash != null
                && rHash != null
                && appId > 0
                && userName != null)
            {
                using(var context = new Repository.BetaDepotContext())
                {
                    Application app = context.Applications.Where(w => w.Id == appId).FirstOrDefault();
                    platform = app.Platform;
                    TeamMember member = context.TeamMembers.Where(w => w.UserName == userName).FirstOrDefault();
                    if (app != null && member != null)
                    {
                        if(isUserInviteTokenValid(app.ApplicationIdentifier, userName, uHash))
                        {
                            if(Repository.Managers.ApplicationBuildMgr.IsUserAnAppTeamMember(userName, appId))
                                throw new HttpException(400, "Your invite is invalid.");

                            string role = getRoleFromRoleInviteToken(app.ApplicationIdentifier, rHash);
                            if(role != null)
                            {
                                

                                ApplicationTeamMember membership =  new ApplicationTeamMember()
                                        {
                                            TeamMember = member,
                                            MemberRole = role
                                        };

                                app.AssignedMembers.Add(membership);
                                context.SaveChanges();
                            }
                            else
                                throw new HttpException(400, "Your invite is invalid.");
                        }
                    }
                    else
                        throw new HttpException(400, "Your invite is invalid.");

                }
            }
            

            return RedirectToAction("Index", "Platform", new { id = platform });

        }
        private bool isUserInviteTokenValid(string appIdentifier, string userName, string uHash)
        {
            int currentTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            for (int i = currentTimestamp; i > (currentTimestamp - 86400); i--)
            {
                if (uHash.ToLower() == Common.Functions.GenerateMD5Hash(string.Format("{0}|{1}|{2}", appIdentifier.ToLower(), userName.ToLower(), i)))
                    return true;
            }
            return false;
        }
        private string getRoleFromRoleInviteToken(string appIdentifier, string uHash)
        {
            int currentTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string[] roles = { 
                                 Common.Constants.APPLICATION_MEMBER_ROLE_ADMINISTRATOR,  
                                 Common.Constants.APPLICATION_MEMBER_ROLE_DEVELOPER,  
                                 Common.Constants.APPLICATION_MEMBER_ROLE_TESTER
                             };
            for (int i = currentTimestamp; i > (currentTimestamp - 86400); i--)
            {
                foreach(string role in roles)
                {
                    if (uHash.ToLower() == Common.Functions.GenerateMD5Hash(string.Format("{0}|{1}|{2}", appIdentifier.ToLower(), role.ToLower(), i)))
                        return role;
                }
            }
            return null;
        }
	}
}