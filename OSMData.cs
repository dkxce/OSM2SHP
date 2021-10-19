using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Drawing;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace OSM2SHP
{
    public class OSMDictionary
    {
        public string language = "";
        public Dictionary<string, DictCatalog> catalog = new Dictionary<string, DictCatalog>();
        public Dictionary<string, DictCatalog> moretags = new Dictionary<string, DictCatalog>();
        public Dictionary<string, Dictionary<string, string>> clas = new Dictionary<string, Dictionary<string, string>>();

        public static OSMDictionary ReadFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string jsontext = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            OSMDictionary result = new OSMDictionary();

            Newtonsoft.Json.Linq.JToken osmd = (Newtonsoft.Json.Linq.JContainer)Newtonsoft.Json.JsonConvert.DeserializeObject(jsontext);
            foreach (Newtonsoft.Json.Linq.JProperty suntoken in osmd)
            {
                if (suntoken.Name == "language")
                    result.language = suntoken.Value.ToString();
                if (suntoken.Name == "catalog")
                {
                    foreach (Newtonsoft.Json.Linq.JProperty cat in suntoken.Value)
                    {
                        DictCatalog c = new DictCatalog();
                        foreach (Newtonsoft.Json.Linq.JProperty catv in cat.Value)
                        {
                            if (catv.Name == "name") c.name = catv.Value.ToString();
                            if (catv.Name == "description") c.description = catv.Value.ToString();
                            if (catv.Name == "link") c.link = catv.Value.ToString();
                            if (catv.Name == "keywords") c.keywords = catv.Value.ToString();
                        };
                        result.catalog.Add(cat.Name, c);
                    };
                };
                if (suntoken.Name == "moretags")
                {
                    foreach (Newtonsoft.Json.Linq.JProperty cat in suntoken.Value)
                    {
                        DictCatalog c = new DictCatalog();
                        foreach (Newtonsoft.Json.Linq.JProperty catv in cat.Value)
                        {
                            if (catv.Name == "name") c.name = catv.Value.ToString();
                            if (catv.Name == "description") c.description = catv.Value.ToString();
                            if (catv.Name == "link") c.link = catv.Value.ToString();
                            if (catv.Name == "keywords") c.keywords = catv.Value.ToString();
                        };
                        result.moretags.Add(cat.Name, c);
                    };
                };
                if (suntoken.Name == "class")
                {
                    foreach (Newtonsoft.Json.Linq.JProperty cat in suntoken.Value)
                    {
                        Dictionary<string, string> kv = new Dictionary<string, string>();
                        foreach (Newtonsoft.Json.Linq.JProperty catv in cat.Value)
                            kv.Add(catv.Name, catv.Value.ToString());
                        result.clas.Add(cat.Name, kv);
                    };
                };
            };

            return result;
        }

        public class DictCatalog
        {
            public string name;
            public string description;
            public string link;
            public string keywords;
        }

        public string Translate(string text)
        {
            string result = text;
            foreach (KeyValuePair<string, DictCatalog> kvp in catalog)
                if (kvp.Key == text) return kvp.Value.name;
            foreach (KeyValuePair<string, DictCatalog> kvp in moretags)
                if (kvp.Key == text) return kvp.Value.name;
            return result;
        }

        public string TranslateClass(string clas, string text)
        {
            string result = text;
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in this.clas)
                if (kvp.Key == clas)
                    foreach (KeyValuePair<string, string> tr in kvp.Value)
                        if (tr.Key == text)
                            return tr.Value;
            return result;
        }

        public string Translate(string key, string value)
        {
            string result = value;
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in clas)
                if (kvp.Key == key)
                    foreach (KeyValuePair<string, string> vvp in kvp.Value)
                        if (vvp.Key == value)
                            return vvp.Value;
            return result;

        }
    }

    public class OSMCatalog
    {
        public List<OSMCatalogRecord> records = new List<OSMCatalogRecord>();

        public int Count
        {
            get
            {
                return records.Count;
            }
        }

        public string GetTopParentCategory(string category)
        {
            string result = category;
            if (Count == 0) return result;
            bool ex = true;
            while (ex)
            {
                ex = false;
                for (int i = 0; i < Count; i++)
                    if (this[i].name == result)
                    {
                        ex = true;
                        if (this[i].parent == null) return result;
                        if (this[i].parent.Length == 0) return result;
                        result = this[i].parent[0];
                        break;
                    };
            };
            return result;
        }

        private string[] GetHeirarchyString(int index)
        {
            string[] result = new string[] { records[index].name, dict.Translate(records[index].name) };
            while ((records[index].parent != null) && (records[index].parent.Length > 0))
            {
                foreach (string parent in records[index].parent)
                    for (int i = 0; i < records.Count; i++)
                        if (records[i].name == parent)
                        {
                            index = i;                            
                            break;
                        };
                result[0] = records[index].name + @"\" + result[0];
                result[1] = dict.Translate(records[index].name) + @"\" + result[1];
            };
            return result;
        }

        public OSMCatalogHierarchy[] GetHierarchyElements(string category, int hierarchyIndex)
        {
            if (hierarchyIndex == 0) return new OSMCatalogHierarchy[0];
            if (records == null) return new OSMCatalogHierarchy[0];
            if (records.Count == 0) return new OSMCatalogHierarchy[0];

            for (int i = 0; i < records.Count; i++)
                if (records[i].name == category)
                    return GetHierarchyElements(records[i].id, hierarchyIndex);
            return new OSMCatalogHierarchy[0];
            
        }

        public OSMCatalogHierarchy GetHierarchyElement(string category, int hierarchyIndex)
        {
            if (hierarchyIndex == 0) return null;
            if (records == null) return null;
            if (records.Count == 0) return null;

            for (int i = 0; i < records.Count; i++)
                if (records[i].name == category)
                    return GetHierarchyElement(records[i].id, hierarchyIndex);
            return null;

        }

        public OSMCatalogHierarchy[] GetHierarchyElements(int category, int hierarchyIndex)
        {
            if (hierarchyIndex == 0) return new OSMCatalogHierarchy[0];
            if (records == null) return new OSMCatalogHierarchy[0];
            if (records.Count == 0) return new OSMCatalogHierarchy[0];
            
            List<OSMCatalogHierarchy> result = new List<OSMCatalogHierarchy>();
            List<int> rids = new List<int>();

            hierarchyIndex = Math.Abs(hierarchyIndex) - 1;
            
            for(int i=0;i<records.Count;i++)
                if (records[i].id == category)
                {
                    OSMCatalogHierarchy hel = new OSMCatalogHierarchy();
                    hel.id = records[i].id;
                    hel.dict_name = dict.Translate(hel.name = records[i].name);
                    string[] hs = GetHeirarchyString(i);
                    hel.fullname = hs[0];
                    hel.dict_fullname = hs[1];
                    hel.default_icon = records[i].name;
                    result.Add(hel);
                    rids.Add(i);
                };

            if (hierarchyIndex > 0)
                for (int h = 0; h < hierarchyIndex; h++)
                {
                    bool has_parent = false;
                    for (int el = result.Count - 1; el >= 0; el--)
                        if ((records[rids[el]].parent != null) && (records[rids[el]].parent.Length > 0))
                        {
                            bool remove = false;
                            foreach (string parent in records[rids[el]].parent)
                            {
                                for (int i = 0; i < records.Count; i++)
                                    if (records[i].name == parent)
                                    {
                                        has_parent = remove = true;
                                        OSMCatalogHierarchy hel = new OSMCatalogHierarchy();
                                        hel.id = records[i].id;
                                        hel.dict_name = dict.Translate(hel.name = records[i].name);
                                        string[] hs = GetHeirarchyString(i);
                                        hel.fullname = hs[0];
                                        hel.dict_fullname = hs[1];
                                        hel.default_icon = records[i].name;
                                        result.Add(hel);
                                        rids.Add(i);
                                    };
                            };
                            if (remove)
                            {
                                result.RemoveAt(el);
                                rids.RemoveAt(el);
                            };
                        };

                    if (!has_parent) result.ToArray();
                };

            return result.ToArray();
        }

        public OSMCatalogHierarchy GetHierarchyElement(int category, int hierarchyIndex)
        {
            if (hierarchyIndex == 0) return null;
            if (records == null) return null;
            if (records.Count == 0) return null;
            hierarchyIndex = Math.Abs(hierarchyIndex) - 1;

            OSMCatalogHierarchy hel_item = null;
            int hel_index = -1;


            for (int i = 0; i < records.Count; i++)
                if (records[i].id == category)
                {
                    hel_item = new OSMCatalogHierarchy();
                    hel_item.id = records[i].id;
                    hel_item.dict_name = dict.Translate(hel_item.name = records[i].name);
                    string[] hs = GetHeirarchyString(i);
                    hel_item.fullname = hs[0];
                    hel_item.dict_fullname = hs[1];
                    hel_item.default_icon = records[i].name;
                    hel_index = i;
                    break;
                };

            if (hel_item == null) return null;

            while ((hierarchyIndex > 0) && (records[hel_index].parent != null) && (records[hel_index].parent.Length > 0))
            {
                for (int i = 0; i < records.Count; i++)
                    if (records[i].name == records[hel_index].parent[0])
                    {
                        hel_item = new OSMCatalogHierarchy();
                        hel_item.id = records[i].id;
                        hel_item.dict_name = dict.Translate(hel_item.name = records[i].name);
                        string[] hs = GetHeirarchyString(i);
                        hel_item.fullname = hs[0];
                        hel_item.dict_fullname = hs[1];
                        hel_item.default_icon = records[i].name;
                        hel_index = i;
                        i = records.Count;
                        hierarchyIndex--;
                        break;
                    };
            };
            return hel_item;
        }

        public OSMCatalogRecord this[int index]
        {
            get
            {
                return records[index];
            }
        }

        public OSMCatalogRecord ByName(string name)
        {
            if (Count == 0) return null;
            foreach (OSMCatalogRecord rec in records)
                if (rec.name == name)
                    return rec;
            return null;
        }

        public OSMCatalogRecord ByID(int id)
        {
            if (Count == 0) return null;
            foreach (OSMCatalogRecord rec in records)
                if (rec.id == id)
                    return rec;
            return null;
        }

        public OSMDictionary dict = new OSMDictionary();

        public class OSMCatalogRecord
        {
            public string name;
            public string[] parent;
            [JsonIgnore]
            public Dictionary<string, string> tags = new Dictionary<string, string>();
            [JsonIgnore]
            public Dictionary<string, Dictionary<string, string>> moretags = new Dictionary<string, Dictionary<string, string>>();
            public bool poi;
            public string[] type;
            public int id;
        }

        public static OSMCatalog ReadFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string jsontext = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            Newtonsoft.Json.Linq.JArray src = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(jsontext);
            List<OSMCatalogRecord> res = new List<OSMCatalogRecord>();

            for (int i = 0; i < src.Count; i++)
            {
                Newtonsoft.Json.Linq.JToken t = (Newtonsoft.Json.Linq.JToken)src[i];
                OSMCatalogRecord sc = new OSMCatalogRecord();
                sc.id = (int)t["id"];
                sc.name = t["name"].ToString();
                sc.poi = (bool)t["poi"];

                Newtonsoft.Json.Linq.JArray sub_arr = (Newtonsoft.Json.Linq.JArray)t["parent"];
                if (sub_arr.Count > 0)
                {
                    sc.parent = new string[sub_arr.Count];
                    for (int a = 0; a < sub_arr.Count; a++)
                        sc.parent[a] = sub_arr[a].ToString();
                };

                sub_arr = (Newtonsoft.Json.Linq.JArray)t["type"];
                if (sub_arr.Count > 0)
                {
                    sc.type = new string[sub_arr.Count];
                    for (int a = 0; a < sub_arr.Count; a++)
                        sc.type[a] = sub_arr[a].ToString();
                };

                Newtonsoft.Json.Linq.JToken tags = (Newtonsoft.Json.Linq.JContainer)t["tags"];
                foreach (Newtonsoft.Json.Linq.JProperty suntoken in tags)
                    sc.tags.Add(suntoken.Name, suntoken.Value.ToString());

                Newtonsoft.Json.Linq.JToken moretags = (Newtonsoft.Json.Linq.JContainer)t["moretags"];
                foreach (Newtonsoft.Json.Linq.JProperty suntoken in moretags)
                {
                    Dictionary<string, string> subs = new Dictionary<string, string>();
                    Newtonsoft.Json.Linq.JToken elmt = (Newtonsoft.Json.Linq.JContainer)suntoken.Value;
                    foreach (Newtonsoft.Json.Linq.JProperty et in elmt)
                        subs.Add(et.Name, et.Value.ToString());
                    sc.moretags.Add(suntoken.Name, subs);
                };


                res.Add(sc);
            };
            OSMCatalog catalogue = new OSMCatalog();
            catalogue.records = res;
            return catalogue;
        }

        public OSMCatalogHierarchyList GetHierarchy(int hierarchyIndex)
        {
            return GetHierarchy(hierarchyIndex, false, -1);
        }
        public OSMCatalogHierarchyList GetHierarchy(int hierarchyIndex, bool onlyBasic, int saveNoCategory)
        {
            OSMCatalogHierarchyList result = new OSMCatalogHierarchyList();
            {
                if ((saveNoCategory > 0) || (!onlyBasic))
                {
                    OSMCatalogHierarchy hel = new OSMCatalogHierarchy();
                    hel.id = -1; // "NoCategory"
                    hel.name = hel.fullname = hel.dict_name = hel.dict_fullname = "NoCategory";
                    hel.iconList.Add(hel.default_icon = "nocategory");
                    result.Add(hel);
                };

                if ((saveNoCategory == 2) || (!onlyBasic))
                {
                    OSMCatalogHierarchy hel = new OSMCatalogHierarchy();
                    hel.id = -2; // "NoValuesButKeys"
                    hel.name = hel.fullname = hel.dict_name = hel.dict_fullname = "NoValuesButKeys";
                    hel.iconList.Add(hel.default_icon = "novalues");
                    result.Add(hel);
                };
            };

            if (onlyBasic) return result;

            if((records != null) && (records.Count > 0))
                for (int i = 0; i < records.Count; i++)
                {                    
                    if (hierarchyIndex == 0)
                    {
                        string all_tags = "";
                        if (records[i].tags.Count == 0)
                            all_tags = records[i].name;
                        else
                            foreach (KeyValuePair<string, string> tag in records[i].tags)
                            {
                                if (all_tags.Length > 0) all_tags += ",";
                                all_tags += String.Format("{0}:{1}", tag.Key, tag.Value);
                            };                            
                        if (result.IndexOf(all_tags) < 0)
                            result.Add(new OSMCatalogHierarchy(records[i].id, all_tags, records[i].name));
                    }
                    else
                    {
                        OSMCatalogHierarchy[] hels = this.GetHierarchyElements(records[i].id, hierarchyIndex);
                        foreach (OSMCatalogHierarchy hel in hels)
                            if (result.IndexOf(hel.id) < 0)
                                result.Add(hel);
                    };
                };
            return result;
        }
    }

    public class OSMCatalogHierarchy
    {
        public int id;
        public string name;
        public string fullname;
        public string dict_name;
        public string dict_fullname;
        public string default_icon = "noicon";
        public List<string> iconList = new List<string>(new string[] { "noicon" });
        public long nodes = 0;
        public long file_size = 0;

        public string FileSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = file_size;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                };
                string res = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
                return res;
            }
        }

        public OSMCatalogHierarchy() { }
        public OSMCatalogHierarchy(int id, string name)
        {
            this.id = id;
            this.name = this.fullname = this.dict_name = this.dict_fullname = name;
        }
        public OSMCatalogHierarchy(int id, string name, string def_icon)
        {
            this.id = id;
            this.name = this.fullname = this.dict_name = this.dict_fullname = name;
            this.default_icon = def_icon;
            if (this.iconList.IndexOf(def_icon) < 0)
                this.iconList.Add(def_icon);
        }

        public byte[] FileHeader
        {
            get
            {
                string text = dict_name;
                if (name != dict_name) text = dict_name + " (" + name + ")";
                return System.Text.Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                    "<kml>\r\n<Document>\r\n<name>" + text + "</name>\r\n<createdby>KMZRebuilder XP</createdby>\r\n");
            }
        }

        public byte[] FileHeaderFull
        {
            get
            {
                string text = dict_fullname;
                if (fullname != dict_fullname) text = dict_fullname + " (" + fullname + ")";
                return System.Text.Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                    "<kml>\r\n<Document>\r\n<name>" + text + "</name>\r\n<createdby>KMZRebuilder XP</createdby>\r\n");
            }
        }

        public byte[] FileFooter
        {
            get
            {
                return System.Text.Encoding.UTF8.GetBytes("</Document></kml>");
            }
        }

        public byte[] FolderHeader
        {
            get
            {
                string text = dict_name;
                if (name != dict_name) text = dict_name + " (" + name + ")";
                return System.Text.Encoding.UTF8.GetBytes("<Folder>\r\n<name>" + text + "</name>\r\n");
            }
        }

        public byte[] FolderHeaderFull
        {
            get
            {
                string text = dict_fullname;
                if (fullname != dict_fullname) text = dict_fullname + " (" + fullname + ")";
                return System.Text.Encoding.UTF8.GetBytes("<Folder>\r\n<name>" + text + "</name>\r\n");
            }
        }

        public byte[] FolderFooter
        {
            get
            {
                return System.Text.Encoding.UTF8.GetBytes("</Folder>\r\n");
            }
        }

        public byte[] FileStyles
        {
            get
            {
                string styles = "";
                for (int i = 0; i < iconList.Count; i++)
                    styles += "\t<Style id=\"" + iconList[i] + "\"><IconStyle><Icon><href>images/" + iconList[i] + ".png</href></Icon></IconStyle></Style>";
                return System.Text.Encoding.UTF8.GetBytes(styles);
            }
        }
    }

    public class OSMCatalogHierarchyList: List<OSMCatalogHierarchy>
    {
        public int IndexOf(int elementID)
        {
            if (this.Count == 0) return -1;
            for (int i = 0; i < this.Count; i++)
                if (this[i].id == elementID)
                    return i;
            return -1;
        }

        public int IndexOf(string name)
        {
            if (this.Count == 0) return -1;
            for (int i = 0; i < this.Count; i++)
                if (this[i].name == name)
                    return i;
            return -1;
        }

        public static Image GetImageFromZip(string zipfile, string imagefile)
        {
            try
            {
                FileStream fs = File.OpenRead(zipfile);
                ZipFile zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile) continue; // Ignore directories
                    if (zipEntry.Name.ToLower() != imagefile.ToLower()) continue;

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Position = 0;
                        Image im = new Bitmap(ms);
                        ms.Dispose();
                        zf.Close();
                        fs.Close();
                        return im;
                    }
                    catch
                    {
                    };
                };
                zf.Close();
                fs.Close();
            }
            catch { };
            return null;
        }

        public static Stream GetImageFileFromZip(string zipfile, string imagefile)
        {
            try
            {
                FileStream fs = File.OpenRead(zipfile);
                ZipFile zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile) continue; // Ignore directories
                    if (zipEntry.Name.ToLower() != imagefile.ToLower()) continue;

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Position = 0;                        
                        zf.Close();
                        fs.Close();
                        return ms;
                    }
                    catch
                    {
                    };
                };
                zf.Close();
                fs.Close();
            }
            catch { };
            return null;
        }

        public byte[] GetFileHeader(string name)
        {
            return System.Text.Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                    "<kml>\r\n<Document>\r\n<name>" + name + "</name>\r\n<createdby>KMZRebuilder XP</createdby>\r\n");
        }

        public byte[] GetFileFooter()
        {
            return System.Text.Encoding.UTF8.GetBytes("</Document></kml>");
        }
        

        public byte[] GetFileStyles(string[] iconList)
        {
            string styles = "";
            for (int i = 0; i < iconList.Length; i++)
                styles += "\t<Style id=\"" + iconList[i] + "\"><IconStyle><Icon><href>images/" + iconList[i] + ".png</href></Icon></IconStyle></Style>";
            return System.Text.Encoding.UTF8.GetBytes(styles);
        }
    }
}
