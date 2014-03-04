using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace Iteedee.BetaDepot.Platforms.Android
{
    public class APKReader
    {

        //private static Logger log = Logger.getLogger("APKReader");

        static int BUFFER = 2048;

        private static int VER_ID = 0;
        private static int ICN_ID = 1;
        private static int LABEL_ID = 2;
        String[] VER_ICN = new String[3];

        static String TMP_PREFIX = "apktemp_";

        // Some possible tags and attributes
        String[] TAGS = { "manifest", "application", "activity" };
        String[] ATTRS = { "android:", "a:", "activity:", "_:" };

        object apkJar = null;
        Dictionary<String, object> entryList = new Dictionary<String, object>();

        List<String> tmpFiles = new List<String>();

        public String fuzzFindInDocument(XmlDocument doc, String tag, String attr)
        {
            foreach (String t in TAGS)
            {
                XmlNodeList nodelist = doc.GetElementsByTagName(t);
                for (int i = 0; i < nodelist.Count; i++)
                {
                    XmlNode element = (XmlNode)nodelist.Item(i);
                    if (element.NodeType == XmlNodeType.Element)
                    {
                        XmlAttributeCollection map = element.Attributes;
                        for (int j = 0; j < map.Count; j++)
                        {
                            XmlNode element2 = map.Item(j);
                            if (element2.Name.EndsWith(attr))
                            {
                                return element2.Value;
                            }
                        }
                    }
                }
            }
            return null;
        }


        private XmlDocument initDoc(String xml)
        {
            XmlDocument retval = new XmlDocument();
            retval.LoadXml(xml);
            retval.DocumentElement.Normalize();
            return retval;
        }


        private void extractPermissions(ApkInfo info, XmlDocument doc)
        {
            ExtractPermission(info, doc, "uses-permission", "android:name");
            ExtractPermission(info, doc, "permission-group", "android:name");
            ExtractPermission(info, doc, "service", "android:permission");
            ExtractPermission(info, doc, "provider", "android:permission");
            ExtractPermission(info, doc, "activity", "android:permission");
        }
        private bool readBoolean(XmlDocument doc, String tag, String attribute)
        {
            String str = FindInDocument(doc, tag, attribute);
            bool ret = false;
            try
            {
                ret = Convert.ToBoolean(str);
            }
            catch (Exception e)
            {
                ret = false;
            }
            return ret;
        }
        private void extractSupportScreens(ApkInfo info, XmlDocument doc)
        {
            info.supportSmallScreens = readBoolean(doc, "supports-screens", "android:smallScreens");
            info.supportNormalScreens = readBoolean(doc, "supports-screens", "android:normalScreens");
            info.supportLargeScreens = readBoolean(doc, "supports-screens", "android:largeScreens");

            if (info.supportSmallScreens || info.supportNormalScreens || info.supportLargeScreens)
                info.supportAnyDensity = false;
        }

        public ApkInfo extractInfo(string manifestXml, byte[] resources_arsx)
        {
            ApkInfo info = new ApkInfo();
            VER_ICN[VER_ID] = "";
            VER_ICN[ICN_ID] = "";
            VER_ICN[LABEL_ID] = "";
            try
            {
                XmlDocument doc = initDoc(manifestXml);
                if (doc == null)
                    throw new Exception("Document initialize failed");
                info.resourcesFileName = "resources.arsx";
                info.resourcesFileBytes = resources_arsx;
                // Fill up the permission field
                extractPermissions(info, doc);

                // Fill up some basic fields
                info.minSdkVersion = FindInDocument(doc, "uses-sdk", "minSdkVersion");
                info.targetSdkVersion = FindInDocument(doc, "uses-sdk", "targetSdkVersion");
                info.versionCode = FindInDocument(doc, "manifest", "versionCode");
                info.versionName = FindInDocument(doc, "manifest", "versionName");
                info.packageName = FindInDocument(doc, "manifest", "package");
                info.label = FindInDocument(doc, "application", "label");
                if (info.label.StartsWith("@"))
                    VER_ICN[LABEL_ID] = info.label; //Convert.ToInt32(info.label).ToString("X4");
                else
                    VER_ICN[LABEL_ID] = String.Format("@{0}", Convert.ToInt32(info.label).ToString("X4"));

                // Fill up the support screen field
                extractSupportScreens(info, doc);

                if (info.versionCode == null)
                    info.versionCode = fuzzFindInDocument(doc, "manifest",
                                    "versionCode");

                if (info.versionName == null)
                    info.versionName = fuzzFindInDocument(doc, "manifest",
                                    "versionName");
                else if (info.versionName.StartsWith("@"))
                    VER_ICN[VER_ID] = info.versionName;

                String id = FindInDocument(doc, "application", "android:icon");
                if (null == id)
                {
                    id = fuzzFindInDocument(doc, "manifest", "icon");
                }

                if (null == id)
                {
                    Console.WriteLine("icon resId Not Found!");
                    return info;
                }

                // Find real strings
                if (!info.hasIcon && id != null)
                {
                    if (id.StartsWith("@android:"))
                        VER_ICN[ICN_ID] = "@"
                                        + (id.Substring("@android:".Length));
                    else
                        VER_ICN[ICN_ID] = String.Format("@{0}", Convert.ToInt32(id).ToString("X4"));

                    List<String> resId = new List<String>();

                    for (int i = 0; i < VER_ICN.Length; i++)
                    {
                        if (VER_ICN[i].StartsWith("@"))
                            resId.Add(VER_ICN[i]);
                    }

                    ResourceFinder finder = new ResourceFinder();
                    info.resStrings = finder.processResourceTable(info.resourcesFileBytes, resId);

                    if (!VER_ICN[VER_ID].Equals(""))
                    {
                        List<String> versions = null;
                        if (info.resStrings.ContainsKey(VER_ICN[VER_ID].ToUpper()))
                            versions = info.resStrings[VER_ICN[VER_ID].ToUpper()];
                        if (versions != null)
                        {
                            if (versions.Count > 0)
                                info.versionName = versions[0];
                        }
                        else
                        {
                            throw new Exception(
                                            "VersionName Cant Find in resource with id "
                                                            + VER_ICN[VER_ID]);
                        }
                    }

                    List<String> iconPaths = null;
                    if (info.resStrings.ContainsKey(VER_ICN[ICN_ID].ToUpper()))
                        iconPaths = info.resStrings[VER_ICN[ICN_ID].ToUpper()];
                    if (iconPaths != null && iconPaths.Count > 0)
                    {
                        info.iconFileNameToGet = new List<String>();
                        foreach (String iconFileName in iconPaths)
                        {
                            if (iconFileName != null)
                            {
                                info.iconFileNameToGet.Add(iconFileName);
                                info.hasIcon = true;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Icon Cant Find in resource with id "
                                        + VER_ICN[ICN_ID]);
                    }

                    if (!VER_ICN[LABEL_ID].Equals(""))
                    {
                        List<String> labels = null;
                        if (info.resStrings.ContainsKey(VER_ICN[LABEL_ID]))
                            labels = info.resStrings[VER_ICN[LABEL_ID]];
                        if (labels.Count > 0)
                        {
                            info.label = labels[0];
                        }
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return info;
        }




        private void ExtractPermission(ApkInfo info, XmlDocument doc, String keyName, String attribName)
        {
            XmlNodeList usesPermissions = doc.GetElementsByTagName(keyName);
            if (usesPermissions != null)
            {
                for (int s = 0; s < usesPermissions.Count; s++)
                {
                    XmlNode permissionNode = usesPermissions.Item(s);
                    if (permissionNode.NodeType == XmlNodeType.Element)
                    {
                        XmlNode node = permissionNode.Attributes.GetNamedItem(attribName);
                        if (node != null)
                            info.Permissions.Add(node.Value);
                    }
                }
            }
        }
        private String FindInDocument(XmlDocument doc, String keyName,
                String attribName)
        {
            XmlNodeList usesPermissions = doc.GetElementsByTagName(keyName);

            if (usesPermissions != null)
            {
                for (int s = 0; s < usesPermissions.Count; s++)
                {
                    XmlNode permissionNode = usesPermissions.Item(s);
                    if (permissionNode.NodeType == XmlNodeType.Element)
                    {
                        XmlNode node = permissionNode.Attributes.GetNamedItem(attribName);
                        if (node != null)
                            return node.Value;
                    }
                }
            }
            return null;
        }



    }

    public class ApkInfo
    {
        public static int FINE = 0;
        public static int NULL_VERSION_CODE = 1;
        public static int NULL_VERSION_NAME = 2;
        public static int NULL_PERMISSION = 3;
        public static int NULL_ICON = 4;
        public static int NULL_CERT_FILE = 5;
        public static int BAD_CERT = 6;
        public static int NULL_SF_FILE = 7;
        public static int BAD_SF = 8;
        public static int NULL_MANIFEST = 9;
        public static int NULL_RESOURCES = 10;
        public static int NULL_DEX = 13;
        public static int NULL_METAINFO = 14;
        public static int BAD_JAR = 11;
        public static int BAD_READ_INFO = 12;
        public static int NULL_FILE = 15;
        public static int HAS_REF = 16;

        public String rawAndroidManifest;

        public List<String> dexClassName = new List<String>();
        public List<String> dexUrls = new List<String>();

        public String label;
        public String fileHash;
        public String versionName;
        public String versionCode;
        public String minSdkVersion;
        public String targetSdkVersion;
        public String packageName;
        public List<String> Permissions;
        public List<String> iconFileName;
        public List<String> iconFileNameToGet;
        public List<String> iconHash;
        public String resourcesFileName;
        public byte[] resourcesFileBytes;
        public bool hasIcon;
        public bool supportSmallScreens;
        public bool supportNormalScreens;
        public bool supportLargeScreens;
        public bool supportAnyDensity;
        public Dictionary<String, List<String>> resStrings;
        public Dictionary<String, String> layoutStrings;

        public static bool supportSmallScreen(byte[] dpi)
        {
            if (dpi[0] == 1)
                return true;
            return false;
        }

        public static bool supportNormalScreen(byte[] dpi)
        {
            if (dpi[1] == 1)
                return true;
            return false;
        }

        public static bool supportLargeScreen(byte[] dpi)
        {
            if (dpi[2] == 1)
                return true;
            return false;
        }
        public byte[] getDPI()
        {
            byte[] dpi = new byte[3];
            if (this.supportAnyDensity)
            {
                dpi[0] = 1;
                dpi[1] = 1;
                dpi[2] = 1;
            }
            else
            {
                if (this.supportSmallScreens)
                    dpi[0] = 1;
                if (this.supportNormalScreens)
                    dpi[1] = 1;
                if (this.supportLargeScreens)
                    dpi[2] = 1;
            }
            return dpi;
        }

        public ApkInfo()
        {
            hasIcon = false;
            supportSmallScreens = false;
            supportNormalScreens = false;
            supportLargeScreens = false;
            supportAnyDensity = true;
            versionCode = null;
            versionName = null;
            iconFileName = null;
            iconFileNameToGet = null;

            Permissions = new List<String>();
        }

        private bool isReference(List<String> strs)
        {
            try
            {
                foreach (String str in strs)
                {
                    if (isReference(str))
                        return true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return false;
        }

        private bool isReference(String str)
        {
            try
            {
                if (str != null && str.StartsWith("@"))
                {
                    int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                    return true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return false;
        }

        public bool hasReference()
        {
            if (isReference(versionCode) || isReference(versionName)
                            || isReference(iconFileNameToGet))
                return true;
            else
                return false;
        }

        public int isValid()
        {
            if (hasReference())
            {
                return HAS_REF;
            }
            else if (versionCode == null)
            {
                return NULL_VERSION_CODE;
            }
            else if (versionName == null)
            {
                return NULL_VERSION_NAME;
            }
            else if (Permissions == null)
            {
                return NULL_PERMISSION;
            }
            else if (iconFileName == null)
            {
                return NULL_ICON;
            }
            else if (iconFileNameToGet == null)
            {
                return NULL_ICON;
            }
            else if (hasIcon == false)
            {
                return NULL_ICON;
            }

            return FINE;
        }




    }

    public class ResourceFinder
    {
        private const long HEADER_START = 0;
        static short RES_STRING_POOL_TYPE = 0x0001;
        static short RES_TABLE_TYPE = 0x0002;
        static short RES_TABLE_PACKAGE_TYPE = 0x0200;
        static short RES_TABLE_TYPE_TYPE = 0x0201;
        static short RES_TABLE_TYPE_SPEC_TYPE = 0x0202;

        String[] valueStringPool = null;
        String[] typeStringPool = null;
        String[] keyStringPool = null;

        private int package_id = 0;

        // Contains no data.
        static byte TYPE_NULL = 0x00;
        // The 'data' holds a ResTable_ref, a reference to another resource
        // table entry.
        static byte TYPE_REFERENCE = 0x01;
        // The 'data' holds an attribute resource identifier.
        static byte TYPE_ATTRIBUTE = 0x02;
        // The 'data' holds an index into the containing resource table's
        // global value string pool.
        static byte TYPE_STRING = 0x03;
        // The 'data' holds a single-precision floating point number.
        static byte TYPE_FLOAT = 0x04;
        // The 'data' holds a complex number encoding a dimension value,
        // such as "100in".
        static byte TYPE_DIMENSION = 0x05;
        // The 'data' holds a complex number encoding a fraction of a
        // container.
        static byte TYPE_FRACTION = 0x06;
        // The 'data' is a raw integer value of the form n..n.
        static byte TYPE_INT_DEC = 0x10;
        // The 'data' is a raw integer value of the form 0xn..n.
        static byte TYPE_INT_HEX = 0x11;
        // The 'data' is either 0 or 1, for input "false" or "true" respectively.
        static byte TYPE_INT_BOOLEAN = 0x12;
        // The 'data' is a raw integer value of the form #aarrggbb.
        static byte TYPE_INT_COLOR_ARGB8 = 0x1c;
        // The 'data' is a raw integer value of the form #rrggbb.
        static byte TYPE_INT_COLOR_RGB8 = 0x1d;
        // The 'data' is a raw integer value of the form #argb.
        static byte TYPE_INT_COLOR_ARGB4 = 0x1e;
        // The 'data' is a raw integer value of the form #rgb.
        static byte TYPE_INT_COLOR_RGB4 = 0x1f;



        private Dictionary<String, List<String>> responseMap;

        Dictionary<int, List<String>> entryMap = new Dictionary<int, List<String>>();

        public Dictionary<string, List<String>> initialize()
        {
            byte[] data = System.IO.File.ReadAllBytes("resources.arsc");
            return this.processResourceTable(data, new List<string>());
        }
        public Dictionary<string, List<String>> processResourceTable(byte[] data, List<String> resIdList)
        {
            responseMap = new Dictionary<string, List<String>>();
            long lastPosition;

            using (MemoryStream ms = new MemoryStream(data))
            {

                using (BinaryReader br = new BinaryReader(ms))
                {

                    short type = br.ReadInt16();
                    short headerSize = br.ReadInt16();
                    int size = br.ReadInt32();
                    int packageCount = br.ReadInt32();


                    if (type != RES_TABLE_TYPE)
                    {
                        throw new Exception("No RES_TABLE_TYPE found!");
                    }
                    if (size != br.BaseStream.Length)
                    {
                        throw new Exception(
                                        "The buffer size not matches to the resource table size.");
                    }

                    int realStringPoolCount = 0;
                    int realPackageCount = 0;


                    while (true)
                    {
                        long pos = br.BaseStream.Position;
                        short t = br.ReadInt16();
                        short hs = br.ReadInt16();
                        int s = br.ReadInt32();

                        if (t == RES_STRING_POOL_TYPE)
                        {
                            if (realStringPoolCount == 0)
                            {
                                // Only the first string pool is processed.
                                Console.WriteLine("Processing the string pool ...");


                                byte[] buffer = new byte[s];
                                lastPosition = br.BaseStream.Position;
                                br.BaseStream.Seek(pos, SeekOrigin.Begin);
                                buffer = br.ReadBytes(s);
                                //br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                                valueStringPool = processStringPool(buffer);

                            }
                            realStringPoolCount++;

                        }
                        else if (t == RES_TABLE_PACKAGE_TYPE)
                        {
                            // Process the package
                            Console.WriteLine("Processing package {0} ...", realPackageCount);

                            byte[] buffer = new byte[s];
                            lastPosition = br.BaseStream.Position;
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            buffer = br.ReadBytes(s);
                            //br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);
                            processPackage(buffer);

                            realPackageCount++;

                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported Type");
                        }
                        br.BaseStream.Seek(pos + (long)s, SeekOrigin.Begin);
                        if (br.BaseStream.Position == br.BaseStream.Length)
                            break;

                    }

                    if (realStringPoolCount != 1)
                    {
                        throw new Exception("More than 1 string pool found!");
                    }
                    if (realPackageCount != packageCount)
                    {
                        throw new Exception(
                                        "Real package count not equals the declared count.");
                    }

                    return responseMap;

                }
            }

        }

        private void processPackage(byte[] data)
        {
            long lastPosition = 0;
            using (MemoryStream ms = new MemoryStream(data))
            {

                using (BinaryReader br = new BinaryReader(ms))
                {
                    //HEADER
                    short type = br.ReadInt16();
                    short headerSize = br.ReadInt16();
                    int size = br.ReadInt32();

                    int id = br.ReadInt32();
                    package_id = id;

                    //PackageName
                    char[] name = new char[256];
                    for (int i = 0; i < 256; ++i)
                    {
                        name[i] = br.ReadChar();
                    }
                    int typeStrings = br.ReadInt32();
                    int lastPublicType = br.ReadInt32();
                    int keyStrings = br.ReadInt32();
                    int lastPublicKey = br.ReadInt32();

                    if (typeStrings != headerSize)
                    {
                        throw new Exception("TypeStrings must immediately follow the package structure header.");
                    }

                    Console.WriteLine("Type strings:");
                    lastPosition = br.BaseStream.Position;
                    br.BaseStream.Seek(typeStrings, SeekOrigin.Begin);
                    byte[] bbTypeStrings = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                    typeStringPool = processStringPool(bbTypeStrings);

                    Console.WriteLine("Key strings:");

                    br.BaseStream.Seek(keyStrings, SeekOrigin.Begin);
                    short key_type = br.ReadInt16();
                    short key_headerSize = br.ReadInt16();
                    int key_size = br.ReadInt32();

                    lastPosition = br.BaseStream.Position;
                    br.BaseStream.Seek(keyStrings, SeekOrigin.Begin);
                    byte[] bbKeyStrings = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                    keyStringPool = processStringPool(bbKeyStrings);



                    // Iterate through all chunks
                    //
                    int typeSpecCount = 0;
                    int typeCount = 0;

                    br.BaseStream.Seek((keyStrings + key_size), SeekOrigin.Begin);

                    while (true)
                    {
                        int pos = (int)br.BaseStream.Position;
                        short t = br.ReadInt16();
                        short hs = br.ReadInt16();
                        int s = br.ReadInt32();

                        if (t == RES_TABLE_TYPE_SPEC_TYPE)
                        {
                            // Process the string pool
                            byte[] buffer = new byte[s];
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            buffer = br.ReadBytes(s);

                            processTypeSpec(buffer);

                            typeSpecCount++;
                        }
                        else if (t == RES_TABLE_TYPE_TYPE)
                        {
                            // Process the package
                            byte[] buffer = new byte[s];
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            buffer = br.ReadBytes(s);

                            processType(buffer);

                            typeCount++;
                        }

                        br.BaseStream.Seek(pos + s, SeekOrigin.Begin);
                        if (br.BaseStream.Position == br.BaseStream.Length)
                            break;
                    }

                    return;

                }
            }

        }
        private void putIntoMap(String resId, String value)
        {
            List<String> valueList = null;
            if (responseMap.ContainsKey(resId.ToUpper()))
                valueList = responseMap[resId.ToUpper()];
            if (valueList == null)
            {
                valueList = new List<String>();
            }
            valueList.Add(value);
            if (responseMap.ContainsKey(resId.ToUpper()))
                responseMap[resId.ToUpper()] = valueList;
            else
                responseMap.Add(resId.ToUpper(), valueList);
            return;

        }

        private void processType(byte[] typeData)
        {
            using (MemoryStream ms = new MemoryStream(typeData))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    short type = br.ReadInt16();
                    short headerSize = br.ReadInt16();
                    int size = br.ReadInt32();
                    byte id = br.ReadByte();
                    byte res0 = br.ReadByte();
                    short res1 = br.ReadInt16();
                    int entryCount = br.ReadInt32();
                    int entriesStart = br.ReadInt32();

                    Dictionary<String, int> refKeys = new Dictionary<String, int>();

                    int config_size = br.ReadInt32();

                    // Skip the config data
                    br.BaseStream.Seek(headerSize, SeekOrigin.Begin);


                    if (headerSize + entryCount * 4 != entriesStart)
                    {
                        throw new Exception("HeaderSize, entryCount and entriesStart are not valid.");
                    }

                    // Start to get entry indices
                    int[] entryIndices = new int[entryCount];
                    for (int i = 0; i < entryCount; ++i)
                    {
                        entryIndices[i] = br.ReadInt32();
                    }

                    // Get entries
                    for (int i = 0; i < entryCount; ++i)
                    {
                        if (entryIndices[i] == -1)
                            continue;

                        int resource_id = (package_id << 24) | (id << 16) | i;

                        long pos = br.BaseStream.Position;
                        short entry_size = br.ReadInt16();
                        short entry_flag = br.ReadInt16();
                        int entry_key = br.ReadInt32();

                        // Get the value (simple) or map (complex)
                        int FLAG_COMPLEX = 0x0001;

                        if ((entry_flag & FLAG_COMPLEX) == 0)
                        {
                            // Simple case
                            short value_size = br.ReadInt16();
                            byte value_res0 = br.ReadByte();
                            byte value_dataType = br.ReadByte();
                            int value_data = br.ReadInt32();

                            String idStr = resource_id.ToString("X4");
                            String keyStr = keyStringPool[entry_key];
                            String data = null;

                            Console.WriteLine("Entry 0x" + idStr + ", key: " + keyStr + ", simple value type: ");

                            List<String> entryArr = null;
                            if (entryMap.ContainsKey(int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)))
                                entryArr = entryMap[int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)];

                            if (entryArr == null)
                                entryArr = new List<String>();

                            entryArr.Add(keyStr);
                            if (entryMap.ContainsKey(int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)))
                                entryMap[int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)] = entryArr;
                            else
                                entryMap.Add(int.Parse(idStr, System.Globalization.NumberStyles.HexNumber), entryArr);

                            if (value_dataType == TYPE_STRING)
                            {
                                data = valueStringPool[value_data];
                                Console.WriteLine(", data: " + valueStringPool[value_data] + "");
                            }
                            else if (value_dataType == TYPE_REFERENCE)
                            {
                                String hexIndex = value_data.ToString("X4");
                                refKeys.Add(idStr, value_data);
                            }
                            else
                            {
                                data = value_data.ToString();
                                Console.WriteLine(", data: " + value_data + "");
                            }

                            // if (inReqList("@" + idStr)) {
                            putIntoMap("@" + idStr, data);
                        }
                        else
                        {
                            int entry_parent = br.ReadInt32();
                            int entry_count = br.ReadInt32();

                            for (int j = 0; j < entry_count; ++j)
                            {
                                int ref_name = br.ReadInt32();
                                short value_size = br.ReadInt16();
                                byte value_res0 = br.ReadByte();
                                byte value_dataType = br.ReadByte();
                                int value_data = br.ReadInt32();
                            }

                            Console.WriteLine("Entry 0x"
                                                    + resource_id.ToString("X4") + ", key: "
                                                    + keyStringPool[entry_key]
                                                    + ", complex value, not printed.");
                        }

                    }
                    HashSet<String> refKs = new HashSet<String>(refKeys.Keys);

                    foreach (String refK in refKs)
                    {
                        List<String> values = null;
                        if (responseMap.ContainsKey("@" + refKeys[refK].ToString("X4").ToUpper()))
                            values = responseMap["@" + refKeys[refK].ToString("X4").ToUpper()];

                        if (values != null)
                            foreach (String value in values)
                            {
                                putIntoMap("@" + refK, value);
                            }
                    }
                    return;

                }
            }
        }



        private string[] processStringPool(byte[] data)
        {
            long lastPosition = 0;

            using (MemoryStream ms = new MemoryStream(data))
            {

                using (BinaryReader br = new BinaryReader(ms))
                {
                    short type = br.ReadInt16();
                    short headerSize = br.ReadInt16();
                    int size = br.ReadInt32();
                    int stringCount = br.ReadInt32();
                    int styleCount = br.ReadInt32();
                    int flags = br.ReadInt32();
                    int stringsStart = br.ReadInt32();
                    int stylesStart = br.ReadInt32();

                    bool isUTF_8 = (flags & 256) != 0;

                    int[] offsets = new int[stringCount];
                    for (int i = 0; i < stringCount; ++i)
                    {
                        offsets[i] = br.ReadInt32();
                    }
                    String[] strings = new String[stringCount];

                    for (int i = 0; i < stringCount; i++)
                    {
                        int pos = stringsStart + offsets[i];
                        lastPosition = br.BaseStream.Position;
                        short len = (short)br.BaseStream.Seek(pos, SeekOrigin.Begin);
                        br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                        if (len < 0)
                        {
                            short extendShort = br.ReadInt16();
                        }
                        pos += 2;
                        strings[i] = "";
                        if (isUTF_8)
                        {
                            int start = pos;
                            int length = 0;
                            lastPosition = br.BaseStream.Position;
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            while (br.ReadByte() != 0)
                            {
                                length++;
                                pos++;
                            }
                            br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                            byte[] oneData = new byte[length];
                            if (length > 0)
                            {
                                byte[] byteArray = data;
                                for (int k = 0; k < length; k++)
                                {
                                    oneData[k] = byteArray[start + k];
                                }
                            }
                            if (oneData.Length > 0)
                                strings[i] = Encoding.UTF8.GetString(oneData);
                            else
                                strings[i] = "";
                        }
                        else
                        {
                            char c;
                            lastPosition = br.BaseStream.Position;
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            while ((c = br.ReadChar()) != 0)
                            {
                                strings[i] += c;
                                pos += 2;
                            }
                            br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);
                        }
                        Console.WriteLine("Parsed value: {0}", strings[i]);


                    }
                    return strings;

                }
            }
        }

        private void processTypeSpec(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {

                using (BinaryReader br = new BinaryReader(ms))
                {
                    short type = br.ReadInt16();
                    short headerSize = br.ReadInt16();
                    int size = br.ReadInt32();
                    byte id = br.ReadByte();
                    byte res0 = br.ReadByte();
                    short res1 = br.ReadInt16();
                    int entryCount = br.ReadInt32();


                    Console.WriteLine("Processing type spec {0}", typeStringPool[id - 1]);

                    int[] flags = new int[entryCount];
                    for (int i = 0; i < entryCount; ++i)
                    {
                        flags[i] = br.ReadInt32();
                    }

                    return;
                }
            }
        }


        //public static class EnumerableExtensions
        //{
        //    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        //    {
        //        return source.ToHashSet<T>(null);
        //    }

        //    public static HashSet<T> ToHashSet<T>(
        //        this IEnumerable<T> source, IEqualityComparer<T> comparer)
        //    {
        //        if (source == null)
        //            throw new ArgumentNullException("source");

        //        return new HashSet<T>(source, comparer);
        //    }
        //}
    }
}