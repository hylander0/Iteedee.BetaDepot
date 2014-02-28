using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Iteedee.BetaDepot.Platforms.Android
{
    public class AndroidManifest
    {
        public static AndroidManifestData GetManifestData(String apkFilePath)
        {
            string content = "";
            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(apkFilePath));
            using (var filestream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read))
            {
                ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                ICSharpCode.SharpZipLib.Zip.ZipEntry item;

                
                while ((item = zip.GetNextEntry()) != null)
                {
                    if (item.Name == "AndroidManifest.xml")
                    {
                        byte[] bytes = new byte[50 * 1024];

                        Stream strm = zipfile.GetInputStream(item);
                        int size = strm.Read(bytes, 0, bytes.Length);

                        using (BinaryReader s = new BinaryReader(strm))
                        {
                            byte[] bytes2 = new byte[size];
                            Array.Copy(bytes, bytes2, size);
                            AndroidDecompress decompress = new AndroidDecompress();
                            content = decompress.decompressXML(bytes);
                        }

                        break;
                    }
                }
                // XmlDocument manifest = new XmlDocument();
                // manifest.LoadXml(content);
            }
           
            XDocument xDoc = XDocument.Parse(content);

            return new AndroidManifestData()
            {
                VersionCode = xDoc.Descendants("manifest").ElementAt(0).Attribute("versionCode").Value,
                VersionName = xDoc.Descendants("manifest").ElementAt(0).Attribute("versionName").Value,
                PackageName = xDoc.Descendants("manifest").ElementAt(0).Attribute("package").Value,
                ApplicationName = xDoc.Descendants("manifest").ElementAt(0).Attribute("package").Value //TODO: Change to Application/label

            };
            
        }
    }
}