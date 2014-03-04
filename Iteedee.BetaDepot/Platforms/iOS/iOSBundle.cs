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
            Dictionary<string, object> plist = GetIpaPList(filePath);
          

            

            return new iOSBundleData()
            {
                BundleAppName = plist["CFBundleName"].ToString(),
                BundleIdentifier = plist["CFBundleIdentifier"].ToString(),
                BundleVersion = plist["CFBundleVersion"].ToString()
            };
        }

        public static void ExtractBundleAppIconIfNotExists(string ipaFilePath, string iconStoreDirectory, string appIdentifier)
        {
            //string retval = Path.Combine(iconStoreDirectory, Path.GetFileNameWithoutExtension(ipaFilePath) + ".png");
            //if(File.Exists(retval))
            //    return;
            //if (File.Exists(Path.Combine(iconStoreDirectory, Path.GetFileNameWithoutExtension(ipaFilePath) + ".jpg")))
            //    return Path.Combine(iconStoreDirectory, Path.GetFileNameWithoutExtension(ipaFilePath) + ".jpg");

            SaveUnCrushedAppIcon(ipaFilePath, iconStoreDirectory, appIdentifier, true);


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
        private static Dictionary<string,object> GetIpaPList(string filePath)
        {
            Dictionary<string, object> plist = new Dictionary<string, object>();
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

                        using( Stream strm = zipfile.GetInputStream(item))
                        {
                            int size = strm.Read(bytes, 0, bytes.Length);

                            using (BinaryReader s = new BinaryReader(strm))
                            {
                                byte[] bytes2 = new byte[size];
                                Array.Copy(bytes, bytes2, size);
                                plist = (Dictionary<string, object>)PlistCS.readPlist(bytes2);
                            }
                        }
                       

                        break;
                    }

                }




            }
            return plist;
        }
        private static List<String> GetiOSBundleIconNames(Dictionary<string, object> plist)
        {
            List<String> retval = new List<string>();
            Dictionary<string,object> element;
            //if(((Dictionary<string,object>)((Dictionary<string,object>)plist["CFBundleIcons"])["CFBundlePrimaryIcon"])["CFBundleIconFiles"])
            if (plist["CFBundleIcons"] != null && plist["CFBundleIcons"] is Dictionary<string, object>)
            { 
                element = ((Dictionary<string, object>)plist["CFBundleIcons"]);
                if(element["CFBundlePrimaryIcon"] != null && element["CFBundlePrimaryIcon"] is Dictionary<string, object>)
                {
                    element = ((Dictionary<string, object>)element["CFBundlePrimaryIcon"]);
                    if (element["CFBundleIconFiles"] != null && element["CFBundleIconFiles"] is List<object>)
                    {
                        foreach(var item in ((List<object>)element["CFBundleIconFiles"]))
                        {
                            retval.Add(item.ToString());
                        }
                    }
                }
            }

            return retval;


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
        private static void SaveUnCrushedAppIcon(string ipaFilePath, string iconDirectory, string appIdentifier, bool GetRetina)
        {
            Dictionary<string, object> plistInfo = GetIpaPList(ipaFilePath);

            List<string> iconFiles = GetiOSBundleIconNames(plistInfo);

            string fileName = string.Empty;
            if (iconFiles.Count > 1)
            {
                iconFiles.ForEach(f =>
                {
                    if (GetRetina && f.Contains("@2x"))
                        fileName = f;
                    else if (!GetRetina && !f.Contains("@2x"))
                        fileName = f;
                });
            }
            else if (iconFiles.Count == 1)
            {
                fileName = iconFiles[0];
            }
            //Rea
            string uniqueIconFileName = String.Format("{0}{1}", appIdentifier, Path.GetExtension(fileName));

            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(ipaFilePath));
            using (var filestream = new FileStream(ipaFilePath, FileMode.Open, FileAccess.Read))
            {
                ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                ICSharpCode.SharpZipLib.Zip.ZipEntry item;


                while ((item = zip.GetNextEntry()) != null)
                {
                    if (item.Name.ToLower() == string.Format("payload/mymc.app/{0}", fileName.ToLower()))
                    {
                        //byte[] bytes = new byte[50 * 1024];

                        byte[] bytes = new byte[50 * 1024];

                        using(Stream strm = zipfile.GetInputStream(item))
                        {
                            int size = strm.Read(bytes, 0, bytes.Length);

                            using (BinaryReader s = new BinaryReader(strm))
                            {
                                byte[] bytes2 = new byte[size];
                                Array.Copy(bytes, bytes2, size);

                                using (MemoryStream input = new MemoryStream(bytes2))
                                using (FileStream output = File.Create(Path.Combine(iconDirectory, uniqueIconFileName)))
                                {
                                    try
                                    {
                                        PNGDecrush.PNGDecrusher.Decrush(input, output);
                                    }
                                    catch (InvalidDataException ex)
                                    {
                                        throw ex;
                                    }
                                }

                            }
                        }
                      

                        break;
                    }

                }

            }



        }
    }
}