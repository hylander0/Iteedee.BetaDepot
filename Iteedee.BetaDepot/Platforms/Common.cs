using Iteedee.BetaDepot.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Iteedee.BetaDepot.Platforms
{
    public class Common
    {
      
        public static bool isBuildFileSupported(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext.ToLower() == ".ipa" || ext.ToLower() == ".apk")
                return true;
            return false;
        }

        public static string GetFilesBuildPlatform(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext.ToLower() == ".ipa")
                return Constants.BUILD_PLATFORM_IOS;
            if (ext.ToLower() == ".apk")
                return Constants.BUILD_PLATFORM_ANDROID;

            return string.Empty;
        }

        public static string GeneratePackageInstallUrl(string BaseUrl, string Controller, string Action, string Platform, string buildUnqiueId)
        {
            string retval = "";
            if (Platform.ToUpper() == Constants.BUILD_PLATFORM_IOS)
            {
                string plistUrl = HttpUtility.UrlEncode(string.Format("{0}{1}/{2}/?FileName={3}&Platform={4}", 
                                                                BaseUrl,
                                                                Controller,
                                                                Action,
                                                                (buildUnqiueId + ".plist"),
                                                                Constants.BUILD_PLATFORM_IOS));

                retval = string.Format("itms-services://?action=download-manifest&url={0}", plistUrl);
            }
            else if (Platform.ToUpper() == Constants.BUILD_PLATFORM_ANDROID)
            {

                retval = string.Format("{0}{1}/{2}/?FileName={0}&Platform={1}", BaseUrl, Controller, Action, (buildUnqiueId + ".apk"), Constants.BUILD_PLATFORM_ANDROID);
            }

            return retval;
        }
    }
}