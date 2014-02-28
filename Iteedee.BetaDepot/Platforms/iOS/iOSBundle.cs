using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Iteedee.BetaDepot.Platforms.iOS
{
    public class iOSBundle
    {
        public static iOSBundleData GetIPABundleData(string filePath)
        {

            Dictionary<string, object> plist = new Dictionary<string,object>();
            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(filePath));
            using (var filestream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                ICSharpCode.SharpZipLib.Zip.ZipEntry item;


                while ((item = zip.GetNextEntry()) != null)
                {
                    if (item.Name.ToLower() == "payload/mymc.app/info.plist")
                    {
                        byte[] bytes = new byte[50 * 1024];

                        Stream strm = zipfile.GetInputStream(item);
                        int size = strm.Read(bytes, 0, bytes.Length);

                        using (BinaryReader s = new BinaryReader(strm))
                        {
                            byte[] bytes2 = new byte[size];
                            Array.Copy(bytes, bytes2, size);
                            plist = (Dictionary<string, object>)PlistCS.readPlist(bytes2);
                        }

                        break;
                    }
                }
            }

            return new iOSBundleData()
            {
                BundleAppName = plist["CFBundleName"].ToString(),
                BundleIdentifier = plist["CFBundleIdentifier"].ToString(),
                BundleVersion = plist["CFBundleVersion"].ToString()
            };
        }

        public static string GenerateBundlesSoftwarePackagePlist(string ipaFilePath, string ipaDownloadUrl)
        {
 
            string template = GetiOSDeploymentPlistTemplate();
            iOSBundleData data = iOSBundle.GetIPABundleData(ipaFilePath);
            template = template.Replace("${DOWNLOAD_URL}", ipaDownloadUrl);
            template = template.Replace("${BUNDLE_ID}", data.BundleIdentifier);
            template = template.Replace("${BUNDLE_VERSION}", data.BundleVersion);
            template = template.Replace("${APP_NAME}", data.BundleAppName);
            return template;
        }
        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        private static string GetiOSDeploymentPlistTemplate()
        {
            string result;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "AndroidTeamTest.Platforms.iOS.iOSDeploymentPlistTemplate.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}