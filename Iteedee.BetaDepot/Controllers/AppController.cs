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
                   string.Format("{0}Home/Download?FileName={1}", BaseUrl(), ipaFileName));

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
        private string BaseUrl()
        {
            //return string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            //return string.Format("{0}://{1}{2}", Request.Url.Scheme, "JustinHyland-PC.na.awwweb.com", Url.Content("~"));
            return string.Format("{0}://{1}{2}", Request.Url.Scheme, Dns.GetHostEntry("localhost").HostName, Url.Content("~"));
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
                Platforms.Android.AndroidManifestData data = Platforms.Android.AndroidManifest.GetManifestData(filePath);
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

	}
}