using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using ProtoBuf;

namespace OSM2SHP
{
    [ProtoContract]
    public class Config
    {
        [ProtoMember(1)]
        public string inputFileName = "";
        [ProtoMember(2)]
        public string outputFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\SHAPES\default.dbf";
        [ProtoMember(3)]
        public byte selector = 1;
        public static string[] SELECTOR = new string[] { 
            /* 00 */ "01 - OSM 2 MP", 
            /* 01 */ "02 - OSM 2 MP для Garmin", 
            /* 02 */ "03 - Список POI от OSM.RU", 
            /* 03 */ "04 - OSM2MP + POI", 
            /* 04 */ "05 - OSM2MP Garmin + POI", 

            /* 05 */ "06 - Адресная информация", 
            /* 06 */ "07 - Адресная информация [ru]", 
            /* 07 */ "08 - Адресная информация [en]", 
            /* 08 */ "09 - Адресная информация [custom]",

            /* 09 */ "10 - Агрегация тегов",
            /* 10 */ "11 - Агрегация тегов + адреса",
            /* 11 */ "12 - Агрегация тегов + адреса [ru]",
            /* 12 */ "13 - Агрегация тегов + адреса [en]",
            /* 13 */ "14 - Агрегация тегов + адреса [custom]",

            /* 14 */ "15 - GARMIN_TYPE by XML SELECTOR"
        };
        
        [ProtoMember(4)]
        public bool onlyHasName = false;
        [ProtoMember(5)]
        public Dictionary<string, string> onlyWithTags = new Dictionary<string, string>();
        [ProtoMember(6)]
        public Dictionary<string, string> onlyOneOfTags = new Dictionary<string, string>();
        [ProtoMember(7)]
        public DateTime onlyMdfAfter = DateTime.MinValue;
        [ProtoMember(8)]
        public DateTime onlyMdfBefore = DateTime.MaxValue;
        [ProtoMember(9)]
        public List<string> onlyOfUser = new List<string>();
        [ProtoMember(10)]
        public List<int> onlyVersion = new List<int>();
        [ProtoMember(11)]
        public double[] onlyInBox = null; // min lat, min Lon, max lat, lax lon
        [ProtoMember(12)]
        public string onlyInPolygon = null;
        [ProtoMember(13)]
        public List<string> dbfDopFields = new List<string>();
        [ProtoMember(14)]
        public int MaxFileRecords = 500000; // default = 500000
        [ProtoMember(15)]
        public string allTagsFormat = "{k}={v};";
        [ProtoMember(16)]
        public bool onlyWithAddr = false;
        [ProtoMember(17)]
        public sbyte maxAggTags = 8;
        [ProtoMember(18)]
        public List<string> dbfMainFields = new List<string>();
        public static string[] defMainFields = new string[] { "VERSION", "TIMESTAMP", "CHANGESET", "UID", "USER", "TAGS_COUNT", "TAGS_ADDRC", "LABEL" };
        [ProtoMember(19)]
        public string useAggPrefix = "";
        [ProtoMember(20)]
        public string scriptFilter = "";
        [ProtoMember(21)]
        public byte dbfCodePage = 0201; // 0xC9 // Windows-1251
        public CodePageList CodePages = new CodePageList();
        [ProtoMember(22)]
        public string aggrRegex = @"(.*(?<!addr:[\w-]+:\w{2,3}|addr:[\w-]+:\w{2,3}-[\w-]+|name:\w{2,3}|name:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)";
        [ProtoMember(23)]
        public string hasText = null;
        [ProtoMember(24)]
        public byte waysImplementation = 0;
        public bool ignoreWays   { get { return (waysImplementation & 0x01) == 0x00; } set { if (value) waysImplementation = 0; else waysImplementation = (byte)((waysImplementation & 0xFE) + 1); } }
        public bool processWays  { get { return (waysImplementation & 0x01) == 0x01; } set { if (value) waysImplementation = (byte)((waysImplementation & 0xFE) + 0x01); else waysImplementation = 0; } }
        public bool processLines { get { return (waysImplementation & 0x08) == 0x08; } set { if (value) waysImplementation = (byte)((waysImplementation & 0xF6) + 0x09); else waysImplementation = (byte)(waysImplementation & 0xF7) ; } }
        public bool processAreas { get { return (waysImplementation & 0x10) == 0x10; } set { if (value) waysImplementation = (byte)((waysImplementation & 0xEE) + 0x11); else waysImplementation = (byte)(waysImplementation & 0xEF); } }
        public byte processCentroids { get { return (byte)((waysImplementation & 0x06) >> 1); } set { if(value > 0) waysImplementation = (byte)((waysImplementation & 0xF8) + (value << 1) + 1); else waysImplementation = (byte)((waysImplementation & 0xF9) + (value << 1));  } }
        public bool processLineFilter { get { return (waysImplementation & 0x20) == 0x20; } set { if (value) waysImplementation = (byte)((waysImplementation & 0xD6) + 0x29); else waysImplementation = (byte)(waysImplementation & 0xDF); } }
        public bool processAreaFilter { get { return (waysImplementation & 0x40) == 0x40; } set { if (value) waysImplementation = (byte)((waysImplementation & 0xAE) + 0x51); else waysImplementation = (byte)(waysImplementation & 0xBF); } }
        public static string[] PROCCENTROID = new string[] { "[Нет] Не обрабатывать центроиды", "[Да] C адресами + здания", "[Да] C адресами + здания + замкнутые с тегами" };
        [ProtoMember(25)]
        public bool useNotInMemoryIndexFile = false;
        [ProtoMember(26)]
        public bool processRelations = false;
        [ProtoMember(27)]
        public bool processRelationFilter = false;
        [ProtoMember(28)]
        public List<string> relationTypesAsLine = new List<string>();
        [ProtoMember(29)]
        public List<string> relationTypesAsArea = new List<string>();
        [ProtoMember(30)]
        public bool relationsAsJoins = false;
        [ProtoMember(31)]
        public bool addFirstAndLastNodesIdToLines = false;
        [ProtoMember(32)]
        public bool addFisrtAndLastNodesLns2Memory = false;
        [ProtoMember(33)]
        public bool saveLineNodesShape = false;
        [ProtoMember(34)]
        public string sortAggTagsPriority = null;
        [ProtoMember(35)]
        public byte dbfMoreCompatible = 0; // 0,1 - yes, 2 - no
        [ProtoMember(36)]
        public string afterScript = null;

        public PointF[] _in_polygon = null;
        public OSMDictionary osmDict = new OSMDictionary();
        public OSMCatalog osmCat = new OSMCatalog();
        public List<string> addrFields = new List<string>();

        private Config() {} // for Protobuf deserializer

        public Config(bool setDefaults) 
        {
            if (setDefaults)
            {
                dbfMainFields.AddRange(defMainFields);
            };
        }
        
        public bool Save(string fileName)
        {
            string prefix1 = "";
            string prefix2 = "";
            string gdn = Path.GetDirectoryName(fileName);
            if ((gdn == "") || (gdn == fileName) || (!fileName.Contains("\\")))
            {
                prefix1 = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\";
                prefix2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Trim('\\') + @"\OSM2SHP\";
            };
            try
            {
                FileStream fs = new FileStream(prefix1 + fileName, FileMode.Create, FileAccess.Write);
                ProtoBuf.Serializer.Serialize<Config>(fs, this);
                fs.Close();
                return true;
            }
            catch { };            
            try
            {
                Directory.CreateDirectory(prefix2);
                FileStream fs = new FileStream(prefix2 + fileName, FileMode.Create, FileAccess.Write);
                ProtoBuf.Serializer.Serialize<Config>(fs, this);
                fs.Close();
                return true;
            }
            catch {};
            return false;
        }

        public static Config Load(string fileName, bool emptyIfNoFile)
        {
            string prefix1 = "";
            string prefix2 = "";
            string gdn = Path.GetDirectoryName(fileName);
            if ((gdn == "") || (gdn == fileName) || (!fileName.Contains("\\")))
            {
                prefix1 = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\";
                prefix2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Trim('\\') + @"\OSM2SHP\";
            };
            if (File.Exists(prefix1 + fileName))
            {
                try
                {
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                    Config result = ProtoBuf.Serializer.Deserialize<Config>(fs);
                    fs.Close();

                    if (!File.Exists(result.inputFileName)) result.inputFileName = "";
                    if (!File.Exists(result.onlyInPolygon)) result.onlyInPolygon = "";

                    result.LoadJSONConfigs();
                    return result;
                }
                catch { };
            }
            else if(File.Exists(prefix2+fileName))
            {
                try
                {
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                    Config result = ProtoBuf.Serializer.Deserialize<Config>(fs);
                    fs.Close();

                    if (!File.Exists(result.inputFileName)) result.inputFileName = "";
                    if (!File.Exists(result.onlyInPolygon)) result.onlyInPolygon = "";

                    result.LoadJSONConfigs();
                    return result;
                }
                catch { };
            };
            if (emptyIfNoFile)
                return new Config();
            else
                return null;
        }

        public void LoadJSONConfigs()
        {
            string dictionaryFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\json_poi_catalog\dictionary.json";
            string catalogFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\json_poi_catalog\catalog.json";            

            osmDict = new OSMDictionary(); 
            if ((!String.IsNullOrEmpty(dictionaryFileName)) && (File.Exists(dictionaryFileName)))
            { try { osmDict = OSMDictionary.ReadFromFile(dictionaryFileName); } catch { }; };

            osmCat = new OSMCatalog();
            if ((!String.IsNullOrEmpty(catalogFileName)) && (File.Exists(catalogFileName)))
            { try { osmCat = OSMCatalog.ReadFromFile(catalogFileName); } catch { }; };

            osmCat.dict = osmDict;            
             
            _in_polygon = null;
        }

        public event EventHandler onReloadProperties;
        public void ReloadProperties()
        {
            if (onReloadProperties != null)
                onReloadProperties(this, null);
        }

        public void PrepareForRead()
        {
            LoadPolygonsAndRoutes();
            LoadAddrConfig();
        }

        private void LoadPolygonsAndRoutes()
        {
            if ((!String.IsNullOrEmpty(this.onlyInPolygon)) && (File.Exists(this.onlyInPolygon)))
            {
                try
                {
                    string tof = "";
                    this._in_polygon = UTILS.loadpolyfull(this.onlyInPolygon, out tof);
                }
                catch
                {
                    this._in_polygon = null;
                };
            };            
        }

        private void LoadAddrConfig()
        {
            addrFields.Clear();
            {
                string addrFieldsFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\json_addr\fields.json";
                if((this.selector == 6) || (this.selector == 11))
                    addrFieldsFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\json_addr\fields_ru.json";
                if ((this.selector == 7) || (this.selector == 12))
                    addrFieldsFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\json_addr\fields_en.json";
                if ((this.selector == 8) || (this.selector == 13))
                    addrFieldsFileName = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\json_addr\fields_custom.json";

                string text = "";
                string[] addrf = new string[0];
                if ((!String.IsNullOrEmpty(addrFieldsFileName)) && (File.Exists(addrFieldsFileName)))
                {
                    try
                    {
                        FileStream fs = new FileStream(addrFieldsFileName, FileMode.Open, FileAccess.Read);
                        StreamReader sw = new StreamReader(fs, System.Text.Encoding.UTF8);
                        text = sw.ReadToEnd();
                        sw.Close();
                        fs.Close();
                        addrf = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(text);
                    }
                    catch { };
                };
                if (addrf.Length > 0)
                    addrFields.AddRange(addrf);
            };
        }
    }
    public class UTILS
    {
        public static PointF[] loadPoly(string filename, out int tof)
        {
            List<PointF> result = new List<PointF>();

            tof = 0;

            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);

            if (System.IO.Path.GetExtension(filename).ToLower() == ".shp")
            {
                fs.Position = 32;
                tof = fs.ReadByte();
                if ((tof == 3) || (tof == 5))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(ba);
                    int len = BitConverter.ToInt32(ba, 0) * 2;
                    fs.Read(ba, 0, ba.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                    tof = BitConverter.ToInt32(ba, 0);
                    if ((tof == 3) || (tof == 5))
                    {
                        fs.Position += 32;
                        fs.Read(ba, 0, ba.Length);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                        if (BitConverter.ToInt32(ba, 0) == 1)
                        {
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            int pCo = BitConverter.ToInt32(ba, 0);
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            if (BitConverter.ToInt32(ba, 0) == 0)
                            {
                                ba = new byte[8];
                                for (int i = 0; i < pCo; i++)
                                {
                                    PointF ap = new PointF();
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.X = (float)BitConverter.ToDouble(ba, 0);
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.Y = (float)BitConverter.ToDouble(ba, 0);
                                    result.Add(ap);
                                };
                            };
                        };
                    };
                };
            };
            sr.Close();
            fs.Close();
            return result.ToArray();
        }

        public static PointF[] loadpolyfull(string filename, out string ftype)
        {
            ftype = "shp";

            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            List<PointF> points = new List<PointF>();

            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (Path.GetExtension(filename).ToLower() == ".kml")
            {
                ftype = "kml";
                System.IO.StreamReader sr = new StreamReader(fs);
                {
                    string file = sr.ReadToEnd();
                    int si = file.IndexOf("<coordinates>");
                    int ei = file.IndexOf("</coordinates>");
                    string co = file.Substring(si + 13, ei - si - 13).Trim().Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                    string[] arr = co.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if ((arr != null) && (arr.Length > 0))
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string[] xyz = arr[i].Split(new string[] { "," }, StringSplitOptions.None);
                            points.Add(new PointF(float.Parse(xyz[0], ni), float.Parse(xyz[1], ni)));
                        };
                };
                sr.Close();
            };
            if (Path.GetExtension(filename).ToLower() == ".shp")
            {
                ftype = "shp";
                fs.Position = 32;
                int tof = fs.ReadByte();
                if ((tof == 5))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(ba);
                    int len = BitConverter.ToInt32(ba, 0) * 2;
                    fs.Read(ba, 0, ba.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                    tof = BitConverter.ToInt32(ba, 0);
                    if ((tof == 5))
                    {
                        fs.Position += 32;
                        fs.Read(ba, 0, ba.Length);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                        if (BitConverter.ToInt32(ba, 0) == 1)
                        {
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            int pCo = BitConverter.ToInt32(ba, 0);
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            if (BitConverter.ToInt32(ba, 0) == 0)
                            {
                                ba = new byte[8];
                                for (int i = 0; i < pCo; i++)
                                {
                                    PointF ap = new PointF();
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.X = (float)BitConverter.ToDouble(ba, 0);
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.Y = (float)BitConverter.ToDouble(ba, 0);
                                    points.Add(ap);
                                };
                            };
                        };
                    };
                };
            };
            fs.Close();

            return points.ToArray();
        }

        public static PointF[] loadroute(string filename, out string ftype)
        {
            ftype = "kml";

            List<PointF> LoadedRoute = new List<PointF>();

            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            if (System.IO.Path.GetExtension(filename).ToLower() == ".kml")
            {
                ftype = "kml";
                string file = sr.ReadToEnd();
                int si = file.IndexOf("<coordinates>");
                int ei = file.IndexOf("</coordinates>");
                string co = file.Substring(si + 13, ei - si - 13).Trim().Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                string[] arr = co.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if ((arr != null) && (arr.Length > 0))
                    for (int i = 0; i < arr.Length; i++)
                    {
                        string[] xyz = arr[i].Split(new string[] { "," }, StringSplitOptions.None);
                        LoadedRoute.Add(new PointF(float.Parse(xyz[0], ni), float.Parse(xyz[1], ni)));
                    };
            };
            if (System.IO.Path.GetExtension(filename).ToLower() == ".gpx")
            {
                ftype = "gpx";
                string file = sr.ReadToEnd();
                int si = 0;
                int ei = 0;
                si = file.IndexOf("<rtept", ei);
                ei = file.IndexOf(">", si);
                while (si > 0)
                {
                    string rtept = file.Substring(si + 7, ei - si - 7).Replace("\"", "").Replace("/", "").Trim();
                    int ssi = rtept.IndexOf("lat=");
                    int sse = rtept.IndexOf(" ", ssi);
                    if (sse < 0) sse = rtept.Length;
                    string lat = rtept.Substring(ssi + 4, sse - ssi - 4);
                    ssi = rtept.IndexOf("lon=");
                    sse = rtept.IndexOf(" ", ssi);
                    if (sse < 0) sse = rtept.Length;
                    string lon = rtept.Substring(ssi + 4, sse - ssi - 4);
                    LoadedRoute.Add(new PointF(float.Parse(lon, ni), float.Parse(lat, ni)));

                    si = file.IndexOf("<rtept", ei);
                    if (si > 0)
                        ei = file.IndexOf(">", si);
                };
            };
            if (Path.GetExtension(filename).ToLower() == ".shp")
            {
                ftype = "shp";
                fs.Position = 32;
                int tof = fs.ReadByte();
                if ((tof == 3))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(ba);
                    int len = BitConverter.ToInt32(ba, 0) * 2;
                    fs.Read(ba, 0, ba.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                    tof = BitConverter.ToInt32(ba, 0);
                    if ((tof == 3))
                    {
                        fs.Position += 32;
                        fs.Read(ba, 0, ba.Length);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                        if (BitConverter.ToInt32(ba, 0) == 1)
                        {
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            int pCo = BitConverter.ToInt32(ba, 0);
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            if (BitConverter.ToInt32(ba, 0) == 0)
                            {
                                ba = new byte[8];
                                for (int i = 0; i < pCo; i++)
                                {
                                    PointF ap = new PointF();
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.X = (float)BitConverter.ToDouble(ba, 0);
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.Y = (float)BitConverter.ToDouble(ba, 0);
                                    LoadedRoute.Add(ap);
                                };
                            };
                        };
                    };
                };
            };
            sr.Close();
            fs.Close();

            return LoadedRoute.ToArray();
        }

        public static bool PointInRoute(PointF point, PointF[] route, double MaxDistInMeter, int LeftOrRight)
        {
            bool skip = true;

            double x = point.X;
            double y = point.Y;

            double length2 = double.MaxValue;
            int side2 = 0;
            for (int i = 1; i < route.Length; i++)
            {
                PointF op;
                int side;
                double d = DistanceFromPointToLine(new PointF((float)x, (float)y), route[i - 1], route[i], out op, out side);
                if (d < length2)
                {
                    length2 = d;
                    side2 = side;
                };
            };

            if (length2 <= MaxDistInMeter)
            {
                if (LeftOrRight < 1)
                    skip = false;
                else
                    if ((LeftOrRight == 1) && (side2 <= 0))
                        skip = false;
                    else
                        if ((LeftOrRight == 2) && (side2 > 0)) skip = false;
            };

            return !skip;
        }

        public static bool PointInPolygon(PointF point, PointF[] polygon)
        {
            return PointInPolygon(point, polygon, 1E-09);
        }

        public static bool PointInPolygon(PointF point, PointF[] polygon, double EPS)
        {
            int count, up;
            count = 0;
            for (int i = 0; i < polygon.Length - 1; i++)
            {
                up = CRS(point, polygon[i], polygon[i + 1], EPS);
                if (up >= 0)
                    count += up;
                else
                    break;
            };
            up = CRS(point, polygon[polygon.Length - 1], polygon[0], EPS);
            if (up >= 0)
                return Convert.ToBoolean((count + up) & 1);
            else
                return false;
        }

        private static int CRS(PointF P, PointF A1, PointF A2, double EPS)
        {
            double x;
            int res = 0;
            if (Math.Abs(A1.Y - A2.Y) < EPS)
            {
                if ((Math.Abs(P.Y - A1.Y) < EPS) && ((P.X - A1.X) * (P.X - A2.X) < 0.0)) res = -1;
                return res;
            };
            if ((A1.Y - P.Y) * (A2.Y - P.Y) > 0.0) return res;
            x = A2.X - (A2.Y - P.Y) / (A2.Y - A1.Y) * (A2.X - A1.X);
            if (Math.Abs(x - P.X) < EPS)
            {
                res = -1;
            }
            else
            {
                if (x < P.X)
                {
                    res = 1;
                    if ((Math.Abs(A1.Y - P.Y) < EPS) && (A1.Y < A2.Y)) res = 0;
                    else
                        if ((Math.Abs(A2.Y - P.Y) < EPS) && (A2.Y < A1.Y)) res = 0;
                };
            };
            return res;
        }

        private static float DistanceFromPointToLine(PointF pt, PointF lineStart, PointF lineEnd, out PointF pointOnLine, out int side)
        {
            float dx = lineEnd.X - lineStart.X;
            float dy = lineEnd.Y - lineStart.Y;

            if ((dx == 0) && (dy == 0))
            {
                // line is a point
                // линия может быть с нулевой длиной после анализа TRA
                pointOnLine = lineStart;
                side = 0;
                //dx = pt.X - lineStart.X;
                //dy = pt.Y - lineStart.Y;                
                //return Math.Sqrt(dx * dx + dy * dy);
                return GetLengthMeters(pt.Y, pt.X, pointOnLine.Y, pointOnLine.X, false);
            };

            side = Math.Sign((lineEnd.X - lineStart.X) * (pt.Y - lineStart.Y) - (lineEnd.Y - lineStart.Y) * (pt.X - lineStart.X));

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - lineStart.X) * dx + (pt.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                pointOnLine = new PointF(lineStart.X, lineStart.Y);
                dx = pt.X - lineStart.X;
                dy = pt.Y - lineStart.Y;
            }
            else if (t > 1)
            {
                pointOnLine = new PointF(lineEnd.X, lineEnd.Y);
                dx = pt.X - lineEnd.X;
                dy = pt.Y - lineEnd.Y;
            }
            else
            {
                pointOnLine = new PointF(lineStart.X + t * dx, lineStart.Y + t * dy);
                dx = pt.X - pointOnLine.X;
                dy = pt.Y - pointOnLine.Y;
            };

            //return Math.Sqrt(dx * dx + dy * dy);
            return GetLengthMeters(pt.Y, pt.X, pointOnLine.Y, pointOnLine.X, false);
        }

        // пересечение полигонов
        public static bool IntersectPolygonsA(PointF[] pol1, PointF[] pol2, double EPS)
        {
            for (int i = 0; i < pol1.Length; i++) if (PointInPolygon(pol1[i], pol2, EPS)) return true;
            for (int i = 0; i < pol2.Length; i++) if (PointInPolygon(pol2[i], pol1, EPS)) return true;
            return false;
        }

        // 
        // CrossPolygons IntersectPolygons
        /// <summary>
        ///     Накладываются ли или пересекаются полигоны
        ///     быстрее чем B
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IntersectPolygonsA(PointF[] pol1, PointF[] pol2)
        {
            for (int i = 0; i < pol1.Length; i++) if (PointInPolygon(pol1[i], pol2)) return true;
            for (int i = 0; i < pol2.Length; i++) if (PointInPolygon(pol2[i], pol1)) return true;
            return false;
        }

        // 
        // CrossPolygons IntersectPolygons
        /// <summary>
        ///     Накладываются ли или пересекаются полигоны
        ///     быстрее чем B
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IntersectPolygons(PointF[] pol1, PointF[] pol2)
        {
            return IntersectPolygonsA(pol1, pol2);
        }    

        // Рассчет расстояния       
        #region LENGTH
        public static float GetLengthMeters(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            // use fastest
            float result = (float)GetLengthMetersD(StartLat, StartLong, EndLat, EndLong, radians);

            if (float.IsNaN(result))
            {
                result = (float)GetLengthMetersC(StartLat, StartLong, EndLat, EndLong, radians);
                if (float.IsNaN(result))
                {
                    result = (float)GetLengthMetersE(StartLat, StartLong, EndLat, EndLong, radians);
                    if (float.IsNaN(result))
                        result = 0;
                };
            };

            return result;
        }

        // Slower
        public static uint GetLengthMetersA(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;     // Преобразование градусов в радианы

            double a = 6378137.0000;     // WGS-84 Equatorial Radius (a)
            double f = 1 / 298.257223563;  // WGS-84 Flattening (f)
            double b = (1 - f) * a;      // WGS-84 Polar Radius
            double e2 = (2 - f) * f;      // WGS-84 Квадрат эксцентричности эллипсоида  // 1-(b/a)^2

            // Переменные, используемые для вычисления смещения и расстояния
            double fPhimean;                           // Средняя широта
            double fdLambda;                           // Разница между двумя значениями долготы
            double fdPhi;                           // Разница между двумя значениями широты
            double fAlpha;                           // Смещение
            double fRho;                           // Меридианский радиус кривизны
            double fNu;                           // Поперечный радиус кривизны
            double fR;                           // Радиус сферы Земли
            double fz;                           // Угловое расстояние от центра сфероида
            double fTemp;                           // Временная переменная, использующаяся в вычислениях

            // Вычисляем разницу между двумя долготами и широтами и получаем среднюю широту
            // предположительно что расстояние между точками << радиуса земли
            if (!radians)
            {
                fdLambda = (StartLong - EndLong) * D2R;
                fdPhi = (StartLat - EndLat) * D2R;
                fPhimean = ((StartLat + EndLat) / 2) * D2R;
            }
            else
            {
                fdLambda = StartLong - EndLong;
                fdPhi = StartLat - EndLat;
                fPhimean = (StartLat + EndLat) / 2;
            };

            // Вычисляем меридианные и поперечные радиусы кривизны средней широты
            fTemp = 1 - e2 * (sqr(Math.Sin(fPhimean)));
            fRho = (a * (1 - e2)) / Math.Pow(fTemp, 1.5);
            fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            // Вычисляем угловое расстояние
            if (!radians)
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * sqr(Math.Sin(fdLambda / 2.0)));
            }
            else
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat) * Math.Cos(StartLat) * sqr(Math.Sin(fdLambda / 2.0)));
            };
            fz = 2 * Math.Asin(fz);

            // Вычисляем смещение
            if (!radians)
            {
                fAlpha = Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            }
            else
            {
                fAlpha = Math.Cos(EndLat) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            };
            fAlpha = Math.Asin(fAlpha);

            // Вычисляем радиус Земли
            fR = (fRho * fNu) / (fRho * sqr(Math.Sin(fAlpha)) + fNu * sqr(Math.Cos(fAlpha)));
            // Получаем расстояние
            return (uint)Math.Round(Math.Abs(fz * fR));
        }
        // Slowest
        public static uint GetLengthMetersB(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double fPhimean, fdLambda, fdPhi, fAlpha, fRho, fNu, fR, fz, fTemp, Distance,
                D2R = Math.PI / 180,
                a = 6378137.0,
                e2 = 0.006739496742337;
            if (radians) D2R = 1;

            fdLambda = (StartLong - EndLong) * D2R;
            fdPhi = (StartLat - EndLat) * D2R;
            fPhimean = (StartLat + EndLat) / 2.0 * D2R;

            fTemp = 1 - e2 * Math.Pow(Math.Sin(fPhimean), 2);
            fRho = a * (1 - e2) / Math.Pow(fTemp, 1.5);
            fNu = a / Math.Sqrt(1 - e2 * Math.Sin(fPhimean) * Math.Sin(fPhimean));

            fz = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) +
              Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2)));
            fAlpha = Math.Asin(Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) / Math.Sin(fz));
            fR = fRho * fNu / (fRho * Math.Pow(Math.Sin(fAlpha), 2) + fNu * Math.Pow(Math.Cos(fAlpha), 2));
            Distance = fz * fR;

            return (uint)Math.Round(Distance);
        }
        // Average
        public static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;
            if (radians) D2R = 1;
            double dDistance = Double.MinValue;
            double dLat1InRad = StartLat * D2R;
            double dLong1InRad = StartLong * D2R;
            double dLat2InRad = EndLat * D2R;
            double dLong2InRad = EndLong * D2R;

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            const double kEarthRadiusKms = 6378137.0000;
            dDistance = kEarthRadiusKms * c;

            return (uint)Math.Round(dDistance);
        }
        // Fastest
        public static double GetLengthMetersD(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            return EarthRadius * (Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2)));
        }
        // Fastest
        public static double GetLengthMetersE(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            /* This algorithm is called Sinnott's Formula */
            double dlon = (lon2) - (lon1);
            double dlat = (lat2) - (lat1);
            double a = Math.Pow(Math.Sin(dlat / 2), 2.0) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2.0);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadius * c;
        }
        private static double sqr(double val)
        {
            return val * val;
        }
        public static double DegToRad(double deg)
        {
            return (deg / 180.0 * Math.PI);
        }
        #endregion LENGTH
    }
}
