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
                    item = zipfile.GetEntry(packageData.ApplicationIconName);
                    if (item == null)
                        return;
                    string fileType = Path.GetExtension(packageData.ApplicationIconName);

                    using (Stream strm = zipfile.GetInputStream(item))
                    using (FileStream output = File.Create(Path.Combine(iconStoreDirectory, packageName + fileType)))
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
        private static string GetBestIconAvailable(List<string> icons)
        {
            string retval = string.Empty;
            icons.ForEach(f => {
                if (f.Contains("mdpi"))
                {
                    retval =  f;
                    return;
                }
            });
            icons.ForEach(f =>
            {
                if (f.Contains("hdpi"))
                {
                    retval = f;
                    return;
                }
            });
            icons.ForEach(f =>
            {
                if (f.Contains("ldpi"))
                {
                    retval = f;
                    return;
                }
            });
            icons.ForEach(f =>
            {
                if (f.Contains("xhdpi"))
                {
                    retval = f;
                    return;
                }
            });
            return retval;
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
            string manifestXml = string.Empty;
            byte[] resourcesData = new byte[0];

            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(apkFilePath));


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
                            AndroidDecompress decompress = new AndroidDecompress();
                            manifestXml = decompress.decompressXML(bytes);
                        }

                    }
                    if (item.Name.ToLower() == "resources.arsc")
                    {


                        using (Stream strm = zipfile.GetInputStream(item))
                        {
                            //int size = strm.Read(bytes, 0, bytes.Length);

                            using (BinaryReader s = new BinaryReader(strm))
                            {
                                resourcesData = s.ReadBytes((int)s.BaseStream.Length);

                            }
                            //xDocManifest = XDocument.Parse(manifestXml);
                        }
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
            APKReader apkReader = new APKReader();
            ApkInfo info = apkReader.extractInfo(manifestXml, resourcesData);
            string AppName = info.label;


            return new AndroidManifestData()
            {
                VersionCode = info.versionCode,
                VersionName = info.versionName,
                PackageName = info.packageName,
                ApplicationName = info.label,
                ApplicationIconName = GetBestIconAvailable(info.iconFileNameToGet)

            };
            
        }
    }
}