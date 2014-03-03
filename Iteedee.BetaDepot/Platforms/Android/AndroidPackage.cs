using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Iteedee.BetaDepot.Platforms.Android
{
    public class AndroidPackage
    {
        public static void ExtractPackageAppIconIfNotExists(string apkFilePath, string iconStoreDirectory, string packageName)
        {
            Platforms.Android.AndroidManifestData packageData = GetManifestData(apkFilePath);

            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(apkFilePath));
            if (!string.IsNullOrEmpty(packageData.ApplicationIconName) && !string.IsNullOrEmpty(packageName))
            {
                XDocument xDocManifest = new XDocument();
                XDocument xDocResource = new XDocument();
                using (var filestream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read))
                {
                    ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                    ICSharpCode.SharpZipLib.Zip.ZipEntry item;
                    item = zipfile.GetEntry(string.Format("res/drawable-mdpi/{0}.png", packageData.ApplicationIconName));
                    if (item == null)
                        item = zipfile.GetEntry(string.Format("res/drawable-hdpi/{0}.png", packageData.ApplicationIconName));
                    if (item == null)
                        item = zipfile.GetEntry(string.Format("res/drawable-ldpi/{0}.png", packageData.ApplicationIconName));
                    if (item == null)
                        item = zipfile.GetEntry(string.Format("res/drawable-xhdpi/{0}.xml", packageData.ApplicationIconName));
                    if (item == null)
                        return;


                    using (Stream strm = zipfile.GetInputStream(item))
                    using (FileStream output = File.Create(Path.Combine(iconStoreDirectory, packageName)))
                    {
                        try
                        {
                            strm.CopyTo(output);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
          

            }
        }
        //private static void CopyStream(Stream input, Stream output)
        //{
        //    byte[] buffer = new byte[8 * 1024];
        //    int len;
        //    while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
        //    {
        //        output.Write(buffer, 0, len);
        //    }
        //}
        public static AndroidManifestData GetManifestData(String apkFilePath)
        {
            string content = "";
            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(apkFilePath));

            XDocument xDocManifest = new XDocument();
            XDocument xDocResource = new XDocument();
            using (var filestream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read))
            {
                ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                ICSharpCode.SharpZipLib.Zip.ZipEntry item;
                
                
                while ((item = zip.GetNextEntry()) != null)
                {
                    if (item.Name.ToLower() == "androidmanifest.xml")
                    {
                        byte[] bytes = new byte[50 * 1024];

                        using(Stream strm = zipfile.GetInputStream(item))
                        {
                            int size = strm.Read(bytes, 0, bytes.Length);

                            using (BinaryReader s = new BinaryReader(strm))
                            {
                                byte[] bytes2 = new byte[size];
                                Array.Copy(bytes, bytes2, size);
                                AndroidDecompress decompress = new AndroidDecompress();
                                content = decompress.decompressXML(bytes);
                            }
                            xDocManifest = XDocument.Parse(content);
                        }

                        break;
                    }
                    //if (item.Name.ToLower() == "resources.arsc")
                    //{
                    //    byte[] bytes = new byte[50 * 1024];

                    //    Stream strm = zipfile.GetInputStream(item);
                    //    int size = strm.Read(bytes, 0, bytes.Length);

                    //    using (BinaryReader s = new BinaryReader(strm))
                    //    {
                    //        byte[] bytes2 = new byte[size];
                    //        Array.Copy(bytes, bytes2, size);
                    //        AndroidDecompress decompress = new AndroidDecompress();
                    //        content = decompress.decompressXML(bytes);
                    //    }
                    //    xDocResource = XDocument.Parse(content);
                    //    //break;
                    //}
                }
                // XmlDocument manifest = new XmlDocument();
                // manifest.LoadXml(content);
            }

            string parcedAppName = xDocManifest.Descendants("manifest").ElementAt(0).Attribute("package").Value;
            string[] classes = parcedAppName.Split(Convert.ToChar("."));
            parcedAppName = classes[classes.Count() - 1];
            parcedAppName = parcedAppName.First().ToString().ToUpper() + String.Join("", parcedAppName.Skip(1));


            return new AndroidManifestData()
            {
                VersionCode = xDocManifest.Descendants("manifest").ElementAt(0).Attribute("versionCode").Value,
                VersionName = xDocManifest.Descendants("manifest").ElementAt(0).Attribute("versionName").Value,
                PackageName = xDocManifest.Descendants("manifest").ElementAt(0).Attribute("package").Value,
                ApplicationName = parcedAppName,
                ApplicationIconName = xDocManifest.Descendants("manifest").ElementAt(0).Descendants("application").ElementAt(0).Attribute("icon").Value

            };
            
        }
    }
}