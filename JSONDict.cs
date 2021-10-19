using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OSM2SHP
{
    public class JSONDict
    {        
        public static JSONDict Read(string fileName)
        {
            string val = "";
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            val = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            JSONDict result = new JSONDict();
            JArray obj = JsonConvert.DeserializeObject<JArray>(val);
            foreach (JObject jo in obj)
            {
                JSONDictElement el = new JSONDictElement();
                if (jo["condition"] != null)
                {
                    foreach (JToken jcel in jo["condition"].Values<JToken>())
                    {
                        if (jcel.Type == JTokenType.String)
                        {
                            string text = jcel.ToString();
                            if (text == "only_node") { el.only_node = true; continue; };
                            if (text == "named") { el.named = true; continue; };
                            string[] kv = text.Split(new char[] { '=' }, 2);
                            if (kv.Length == 1)
                                el.AddAnd(kv[0].Trim(), new JSONDictElement.ECondition.YNValue(false, null));
                            else
                            {
                                bool yn = true;
                                string kt = kv[0].Trim();
                                string vt = kv[1].Trim();
                                if (kt.Substring(kt.Length - 1) == "!")
                                {
                                    yn = false;
                                    kt = kt.Remove(kt.Length - 1).Trim();
                                };
                                if(vt == "*")
                                    el.AddAnd(kt,new JSONDictElement.ECondition.YNValue(!yn, null));
                                else
                                    el.AddAnd(kt, new JSONDictElement.ECondition.YNValue(yn, vt));
                            };
                        };
                        if ((jcel.Type == JTokenType.Object) && (jcel["and"] == null) && (jcel["or"] == null))
                            throw new Exception("ERROR X1A1*5");
                        if ((jcel.Type == JTokenType.Object) && (jcel["and"] != null))
                        {
                            foreach (JToken jand in jcel["and"].Values<JToken>())
                            {
                                string text = jand.ToString();
                                if (text == "only_node") { el.only_node = true; continue; };
                                string[] kv = text.Split(new char[] { '=' }, 2);
                                if (kv.Length == 1)
                                    el.AddAnd(kv[0].Trim(), new JSONDictElement.ECondition.YNValue(false, null));
                                else
                                {
                                    bool yn = true;
                                    string kt = kv[0].Trim();
                                    string vt = kv[1].Trim();
                                    if (kt.Substring(kt.Length - 1) == "!")
                                    {
                                        yn = false;
                                        kt = kt.Remove(kt.Length - 1).Trim();
                                    };
                                    if (vt == "*")
                                        el.AddAnd(kt, new JSONDictElement.ECondition.YNValue(!yn, null));
                                    else
                                        el.AddAnd(kt, new JSONDictElement.ECondition.YNValue(yn, vt));
                                };
                            };
                        };
                        if((jcel.Type == JTokenType.Object) && (jcel["or"] != null))
                        {
                            List<JSONDictElement.ECondition.YNValue> vvv = new List<JSONDictElement.ECondition.YNValue>();
                            List<string> vvk = new List<string>();

                            foreach (JToken jor in jcel["or"].Values<JToken>())
                            {
                                if (jor.Type == JTokenType.String)
                                {
                                    string text = jor.ToString();
                                    if (text == "only_node") { el.only_node = true; continue; };
                                    string[] kv = text.Split(new char[] { '=' }, 2);
                                    if (kv.Length == 1)
                                    {
                                        vvv.Add(new JSONDictElement.ECondition.YNValue(false, null));
                                        vvk.Add(kv[0].Trim());
                                    }
                                    else
                                    {
                                        bool yn = true;
                                        string kt = kv[0].Trim();
                                        string vt = kv[1].Trim();
                                        if (kt.Substring(kt.Length - 1) == "!")
                                        {
                                            yn = false;
                                            kt = kt.Remove(kt.Length - 1).Trim();
                                        };
                                        if (vt == "*")
                                        {
                                            vvv.Add(new JSONDictElement.ECondition.YNValue(!yn, null));
                                            vvk.Add(kt);
                                        }
                                        else
                                        {
                                            vvv.Add(new JSONDictElement.ECondition.YNValue(yn, vt));
                                            vvk.Add(kt);
                                        };
                                    };
                                };
                                if ((jor.Type == JTokenType.Object) && (jor["and"] != null))
                                {
                                    List<JSONDictElement.ECondition> morec = new List<JSONDictElement.ECondition>();
                                    
                                    foreach (JToken jand in jor["and"].Values<JToken>())
                                    {
                                        if (jand.Type == JTokenType.String)
                                        {
                                            string text = jand.ToString();
                                            if (text == "only_node") { el.only_node = true; continue; };
                                            string[] kv = text.Split(new char[] { '=' }, 2);
                                            if (kv.Length == 1)
                                            {                                                
                                                JSONDictElement.ECondition ec = new JSONDictElement.ECondition();
                                                ec.ect = JSONDictElement.ECondition.ECType.ectAnd;
                                                ec.query.Add(new KeyValuePair<string,JSONDictElement.ECondition.YNValue>(kv[0].Trim(), new JSONDictElement.ECondition.YNValue(false, null)));
                                                morec.Add(ec);                                                
                                            }
                                            else
                                            {
                                                bool yn = true;
                                                string kt = kv[0].Trim();
                                                string vt = kv[1].Trim();
                                                if (kt.Substring(kt.Length - 1) == "!")
                                                {
                                                    yn = false;
                                                    kt = kt.Remove(kt.Length - 1).Trim();
                                                };
                                                if (vt == "*")
                                                {
                                                    JSONDictElement.ECondition ec = new JSONDictElement.ECondition();
                                                    ec.ect = JSONDictElement.ECondition.ECType.ectAnd;
                                                    ec.query.Add(new KeyValuePair<string,JSONDictElement.ECondition.YNValue>(kt, new JSONDictElement.ECondition.YNValue(!yn, null)));
                                                    morec.Add(ec);
                                                }
                                                else
                                                {
                                                    JSONDictElement.ECondition ec = new JSONDictElement.ECondition();
                                                    ec.ect = JSONDictElement.ECondition.ECType.ectAnd;
                                                    ec.query.Add(new KeyValuePair<string,JSONDictElement.ECondition.YNValue>(kt, new JSONDictElement.ECondition.YNValue(yn, vt)));
                                                    morec.Add(ec);   
                                                };
                                            };
                                        };
                                        if ((jand.Type != JTokenType.String) && (jand["or"] != null))
                                        {
                                            List<JSONDictElement.ECondition.YNValue> vvv2 = new List<JSONDictElement.ECondition.YNValue>();
                                            List<string> vvk2 = new List<string>();

                                            foreach (JToken jor2 in jand["or"].Values<JToken>())
                                            {
                                                ////
                                                if (jor2.Type == JTokenType.String)
                                                {
                                                    string text = jor2.ToString();
                                                    if (text == "only_node") { el.only_node = true; continue; };
                                                    string[] kv = text.Split(new char[] { '=' }, 2);
                                                    if (kv.Length == 1)
                                                    {
                                                        vvv2.Add(new JSONDictElement.ECondition.YNValue(false, null));
                                                        vvk2.Add(kv[0].Trim());
                                                    }
                                                    else
                                                    {
                                                        bool yn = true;
                                                        string kt = kv[0].Trim();
                                                        string vt = kv[1].Trim();
                                                        if (kt.Substring(kt.Length - 1) == "!")
                                                        {
                                                            yn = false;
                                                            kt = kt.Remove(kt.Length - 1).Trim();
                                                        };
                                                        if (vt == "*")
                                                        {
                                                            vvv2.Add(new JSONDictElement.ECondition.YNValue(!yn, null));
                                                            vvk2.Add(kt);
                                                        }
                                                        else
                                                        {
                                                            vvv2.Add(new JSONDictElement.ECondition.YNValue(yn, vt));
                                                            vvk2.Add(kt);
                                                        };
                                                    };
                                                }
                                                else
                                                    throw new Exception("ERROR X1A1*8");
                                                /////
                                            };

                                            if (vvk2.Count > 0)
                                            {
                                                JSONDictElement.ECondition ec = new JSONDictElement.ECondition();
                                                ec.ect = JSONDictElement.ECondition.ECType.ectOR;
                                                for (int i = 0; i < vvk2.Count; i++)
                                                    ec.query.Add(new KeyValuePair<string, JSONDictElement.ECondition.YNValue>(vvk2[i], vvv2[i]));
                                                morec.Add(ec);  
                                            };
                                        };
                                        if ((jand.Type != JTokenType.String) && (jand["or"] == null))
                                        {
                                            throw new Exception("ERROR X1A1*6");
                                        };
                                    };

                                    vvv.Add(new JSONDictElement.ECondition.YNValue(true, null, morec));
                                    vvk.Add(null);
                                };
                                if ((jor.Type == JTokenType.Object) && (jor["and"] == null))
                                    throw new Exception("ERROR X1A1*7");
                            };

                            if (vvk.Count > 0)
                                el.AddOr(vvk.ToArray(), vvv.ToArray());
                        };
                    };
                };
                if (jo["action"] != null)
                {
                    foreach (JToken jact in jo["action"].Values<JToken>())
                    {
                        if (jact.Type == JTokenType.String)
                        {
                            JSONDictElement.EAction ac = new JSONDictElement.EAction();
                            ac.name = jact.ToString();
                            el.actions.Add(ac);
                        };
                        if (jact.Type == JTokenType.Object)
                        {
                            JSONDictElement.EAction ac = new JSONDictElement.EAction();
                            ac.name = jact["action"].ToString();
                            if (jact["city"] != null) ac.city = jact["city"].ToString() == "yes";
                            if (jact["contacts"] != null) ac.contacts = jact["contacts"].ToString() == "yes";
                            try
                            {
                                if (jact["level_h"] != null) ac.level_h = int.Parse(jact["level_h"].ToString());
                            }
                            catch { ac.level_h = 1; };
                            if (jact["level_l"] != null) ac.level_l = int.Parse(jact["level_l"].ToString());
                            if (jact["type"]  != null)
                            {
                                JTokenType jtt = jact["type"].Type;
                                if ((jtt == JTokenType.Integer) || (jtt == JTokenType.String))
                                    ac.mp_types.Add(new string[] { null, null, jact["type"].ToString() });
                                else
                                {
                                    string sel = jact["type"]["selector"].ToString();
                                    if (sel == "tag")
                                    {
                                        string tagName = jact["type"]["tag"].ToString();
                                        if(jact["type"]["_default"] != null)
                                            ac.mp_types.Add(new string[] { tagName, null, jact["type"]["_default"].ToString() });
                                        foreach (JProperty ev in jact["type"].Values<JProperty>())
                                        {
                                            if (ev.Name == "selector") continue;
                                            if (ev.Name == "tag") continue;
                                            if (ev.Name == "_default") continue;
                                            ac.mp_types.Add(new string[] { tagName, ev.Name, ev.Value.ToString() });
                                        };
                                    }
                                    else if (sel == "if")
                                    {
                                        string cond = jact["type"]["condition"].ToString();
                                        string then = jact["type"]["then"].ToString();
                                        string[] at = cond.Split(new char[] { '=' }, 2);
                                        at[0] = at[0].Trim();
                                        if (at[0] == "named")
                                        {
                                            ac.mp_types.Add(new string[] { "name", null, then });
                                        }
                                        else
                                        {
                                            at[1] = at[1].Trim();
                                            if (at[0].Substring(at[0].Length - 1) == "!")
                                            {
                                                at[0] = at[0].Remove(at[0].Length - 1).Trim();
                                                at[1] = "!" + at[1];
                                            };
                                            ac.mp_types.Add(new string[] { at[0], at[1], then });
                                        };
                                    }
                                    else
                                        throw new Exception("ERROR X1A1*9");
                                };
                            };
                            if ((jact["name"] != null) && (jact["name"].Type == JTokenType.Object))
                            {
                                string sel = jact["name"]["selector"].ToString();
                                if(sel == "lang")
                                {
                                    foreach (JProperty ev in jact["name"].Values<JProperty>())
                                    {
                                        if (ev.Name == "selector") continue;
                                        string txt = "";
                                        if (ev.Value.Type == JTokenType.String)
                                            txt = ev.Value.ToString();
                                        else if (ev.Value.Type == JTokenType.Array)
                                        {
                                            foreach (JToken ns in ev.Value.Values<JToken>())
                                            {
                                                if (ns.Type == JTokenType.String)
                                                    txt += ns + " ";
                                                else 
                                                    throw new Exception("ERROR X1A1*11");
                                            };
                                        }
                                        else if(ev.Value.Type == JTokenType.Object)
                                        {
                                            string ccs = ev.Value["selector"].ToString();
                                            if (ccs == "tag")
                                            {
                                                string cct = ev.Value[ccs].ToString();
                                                foreach (JProperty c in ev.Value.Values<JProperty>())
                                                {
                                                    if (c.Name == "selector") continue;
                                                    if (c.Name == "tag") continue;
                                                    string xv = c.Name;
                                                    if (c.Name == "_default") xv = null;

                                                    string txt2 = "";
                                                    if (c.Value.Type == JTokenType.String)
                                                        txt2 = c.Value.ToString();
                                                    else if (c.Value.Type == JTokenType.Array)
                                                    {
                                                        foreach (JToken ns in c.Value.Values<JToken>())
                                                        {
                                                            if (ns.Type == JTokenType.String)
                                                                txt2 += ns + " ";
                                                            else
                                                                throw new Exception("ERROR X1A1*14");
                                                        };
                                                    }
                                                    else if (c.Value.Type == JTokenType.Object)
                                                    {
                                                        string cc2 = c.Value["selector"].ToString();
                                                        if (cc2 == "if")
                                                        {
                                                            string cond = c.Value["condition"].ToString();
                                                            string then = c.Value["then"].ToString();
                                                            if (c.Value["then"].Type == JTokenType.Array)
                                                            {
                                                                string txt3 = "";
                                                                foreach (string ns2 in ev.Value["then"])
                                                                    txt3 += ns2 + " ";
                                                                then = txt3.Trim();
                                                            };
                                                            string elss = c.Value["else"] == null ? null : c.Value["else"].ToString();
                                                            if ((c.Value["else"] != null) && (c.Value["else"].Type == JTokenType.Array))
                                                            {
                                                                string txt3 = "";
                                                                foreach (string ns2 in c.Value["else"])
                                                                    txt3 += ns2 + " ";
                                                                elss = txt3.Trim();
                                                            };
                                                            string[] at = cond.Split(new char[] { '=' }, 2);
                                                            at[0] = at[0].Trim();
                                                            at[1] = at[1].Trim();
                                                            if (at[0].Substring(at[0].Length - 1) == "!")
                                                            {
                                                                at[0] = at[0].Remove(at[0].Length - 1).Trim();
                                                                at[1] = "!" + at[1];
                                                            };
                                                            ac.names.Add(new string[] { ev.Name, at[0], at[1], then, elss });
                                                        }
                                                        else if (cc2 == "tag")
                                                        {
                                                            string tagName = c.Value["tag"].ToString();
                                                            if (c.Value["_default"] != null)
                                                            {
                                                                string txtv = c.Value["_default"].ToString();
                                                                if (c.Value["_default"].Type == JTokenType.Array)
                                                                {
                                                                    txtv = "";
                                                                    foreach (string ns2 in c.Value["_default"])
                                                                        txtv += ns2 + " ";
                                                                    txtv = txtv.Trim();
                                                                };
                                                                ac.mp_types.Add(new string[] { tagName, null, txtv });
                                                            };
                                                            foreach (JProperty ev2 in c.Value)
                                                            {
                                                                if (ev2.Name == "selector") continue;
                                                                if (ev2.Name == "tag") continue;
                                                                if (ev2.Name == "_default") continue;

                                                                string txtv = ev2.Value.ToString();
                                                                if (ev2.Value.Type == JTokenType.Array)
                                                                {
                                                                    txtv = "";
                                                                    foreach (string ns2 in c.Value["_default"])
                                                                        txtv += ns2 + " ";
                                                                    txtv = txtv.Trim();
                                                                };
                                                                ac.mp_types.Add(new string[] { tagName, ev2.Name, txtv });
                                                            };
                                                        }
                                                        else
                                                            throw new Exception("ERROR X1A1*16");
                                                    }
                                                    else
                                                        throw new Exception("ERROR X1A1*15");
                                                    ac.names.Add(new string[] { ev.Name, cct, xv, txt2, null });
                                                };
                                            }
                                            else if (ccs == "if")
                                            {
                                                string cond = ev.Value["condition"].ToString();
                                                string then = ev.Value["then"].ToString();
                                                if (ev.Value["then"].Type == JTokenType.Array)
                                                {
                                                    string txt2 = "";
                                                    foreach (string ns2 in ev.Value["then"])
                                                        txt2 += ns2 + " ";
                                                    then = txt2.Trim();
                                                };
                                                string elss = ev.Value["else"] == null ? null : ev.Value["else"].ToString();
                                                if ((ev.Value["else"] != null) && (ev.Value["else"].Type == JTokenType.Array))
                                                {
                                                    string txt2 = "";
                                                    foreach (string ns2 in ev.Value["else"])
                                                        txt2 += ns2 + " ";
                                                    elss = txt2.Trim();
                                                };
                                                string[] at = cond.Split(new char[] { '=' }, 2);
                                                at[0] = at[0].Trim();
                                                at[1] = at[1].Trim();
                                                if (at[0].Substring(at[0].Length - 1) == "!")
                                                {
                                                    at[0] = at[0].Remove(at[0].Length - 1).Trim();
                                                    at[1] = "!" + at[1];
                                                };
                                                ac.names.Add(new string[] { ev.Name, at[0], at[1], then, elss });
                                            }
                                            else
                                                throw new Exception("ERROR X1A1*13");
                                        }
                                        else
                                            throw new Exception("ERROR X1A1*12");
                                        txt = txt.Trim();
                                        if(!string.IsNullOrEmpty(txt))
                                            ac.names.Add(new string[] { ev.Name, null, null, txt, null });
                                    };
                                };
                                if (sel == "tag")
                                {
                                    string tag = jact["name"]["tag"].ToString();
                                    foreach (JProperty ev in jact["name"].Values<JProperty>())
                                    {
                                        if (ev.Name == "selector") continue;
                                        if (ev.Name == "tag") continue;
                                        if (ev.Name == "_default") continue;
                                        foreach (JObject ns in ev.Values<JObject>())
                                        {
                                            sel = ns["selector"].ToString();
                                            if (sel == "lang")
                                            {    
                                                foreach (JProperty ev2 in ns.Values<JProperty>())
                                                {
                                                    if (ev2.Name == "selector") continue;                                                    
                                                    if (ev2.Value.Type == JTokenType.Array)
                                                    {
                                                        string txt = "";
                                                        foreach (string ns2 in ev2.Value.Values<string>())
                                                            txt += ns2 + " ";
                                                        txt = txt.Trim();
                                                        ac.names.Add(new string[] { ev2.Name, tag, ev.Name, txt, null });
                                                    };
                                                    if(ev2.Value.Type == JTokenType.Object)
                                                    {
                                                        //////////
                                                        string sel2 = ev2.Value["selector"].ToString();
                                                        if (sel2 == "tag")
                                                        {
                                                            throw new Exception("ERROR X1A1*10");
                                                        }
                                                        else if (sel2 == "if")
                                                        {
                                                            string cond = ev2.Value["condition"].ToString();
                                                            string then = ev2.Value["then"].ToString();
                                                            if (ev2.Value["then"].Type == JTokenType.Array)
                                                            {
                                                                string txt = "";
                                                                foreach (string ns2 in ev2.Value["then"])
                                                                    txt += ns2 + " ";
                                                                then = txt.Trim();
                                                            };
                                                            string elss = ev2.Value["else"] == null ? null : ev2.Value["else"].ToString();
                                                            if ((ev2.Value["else"] != null) && (ev2.Value["else"].Type == JTokenType.Array))
                                                            {
                                                                string txt = "";
                                                                foreach (string ns2 in ev2.Value["else"])
                                                                    txt += ns2 + " ";
                                                                elss = txt.Trim();
                                                            };
                                                            string[] at = cond.Split(new char[] { '=' }, 2);
                                                            at[0] = at[0].Trim();
                                                            at[1] = at[1].Trim();
                                                            if (at[0].Substring(at[0].Length - 1) == "!")
                                                            {
                                                                at[0] = at[0].Remove(at[0].Length - 1).Trim();
                                                                at[1] = "!" + at[1];
                                                            };
                                                            ac.names.Add(new string[] { "*", at[0], at[1], then, elss });
                                                        }
                                                        else
                                                            throw new Exception("ERROR X1A1*9");
                                                        //////////
                                                    };                                                    
                                                };
                                            };
                                        };
                                    };
                                };
                            };
                            el.actions.Add(ac);
                        };
                    };
                };
                result.Add(el);
            };
            return result;
        }

        public void AddFile(string fileName)
        {
            JSONDict tmp = JSONDict.Read(fileName);
            if (tmp.records.Count > 0)
                this.records.AddRange(tmp.records);
        }

        public List<JSONDictElement> records = new List<JSONDictElement>();

        public void Add(JSONDictElement record)
        {
            records.Add(record);
        }

        public int Count
        {
            get
            {
                return this.records.Count;
            }
        }

        public JSONDictElement this[int index]
        {
            get
            {
                return this.records[index];
            }
            set
            {
                this.records[index] = value;
            }
        }

    }

    public class JSONDictElement
    {
        public bool only_node = false;
        public bool named = false;
        public List<ECondition> conditions = new List<ECondition>();
        public List<EAction> actions = new List<EAction>();

        public class EAction
        {
            public string name;
            public List<string[]> mp_types = new List<string[]>();
            public List<string[]> names = new List<string[]>();
            public int level_h = -1;
            public int level_l = -1;
            public bool contacts = false;
            public bool city = false;

            public long GetTypeL(Dictionary<string,string> tags)
            {
                long res = 0;
                if (mp_types.Count == 0) 
                    return res;
                if (mp_types[0][0] == null) 
                    return long.Parse(mp_types[0][2]);
                foreach(string[] sss in mp_types)
                    if(sss[1] == null)
                        res = long.Parse(sss[2]);
                foreach (string[] sss in mp_types)
                {
                    if ((tags.ContainsKey(sss[0]) && (sss[1] == null)))
                        res = long.Parse(sss[2]);
                    if ((tags.ContainsKey(sss[0]) && (sss[1] != null) && Compare(tags[sss[0]], sss[1])))
                        res = long.Parse(sss[2]);
                };
                return res;
            }

            public string GetTypeS(Dictionary<string, string> tags)
            {
                return String.Format("0x{0:X4}", GetTypeL(tags));
            }


            public string GetName(Dictionary<string, string> tags)
            {
                return GetName(tags, "ru");
            }

            public string GetName(Dictionary<string, string> tags, string def_lang)
            {
                string nm = "NoName";
                if (tags.ContainsKey("name")) nm = tags["name"];

                if (!String.IsNullOrEmpty(def_lang))
                {
                    string dn = "name:" + def_lang;
                    if (tags.ContainsKey(dn)) nm = tags[dn];
                };                

                if (names.Count > 0)
                {
                    string name_by_dict = null;
                    for (int i = 0; i < names.Count; i++)
                    {
                        if (names[i][0] == null) continue;
                        if ((names[i][0] != "*") && (names[i][0] != def_lang)) continue;
                        if (names[i][1] == null)
                            name_by_dict = names[i][3];
                        else 
                        {
                            if (names[i][2] == null)
                            {
                                if (tags.ContainsKey(names[i][1]))
                                    name_by_dict = names[i][4];
                                else
                                    name_by_dict = names[i][3];
                            }
                            else
                            {
                                if (tags.ContainsKey(names[i][1]))
                                {
                                    if (Compare(tags[names[i][1]], names[i][2]))
                                        name_by_dict = names[i][3];
                                    else if(names[i][4] != null)
                                        name_by_dict = names[i][4];
                                }
                                else if (names[i][4] != null)
                                    name_by_dict = names[i][4];

                            };                            
                        };
                    };
                    if (!String.IsNullOrEmpty(name_by_dict))
                    {
                        if (name_by_dict.Contains("%label"))
                            nm = name_by_dict.Replace("%label", nm);
                        else if (nm == "NoName")
                            nm = name_by_dict;
                        else
                            nm += ", " + name_by_dict;
                    };
                };

                if (tags.ContainsKey("ele"))
                    nm = nm.Replace("%ele", tags["ele"]);
                nm = nm.Replace("%ele", "1");

                if (tags.ContainsKey("addr:housenumber"))
                    nm = nm.Replace("%house", tags["addr:housenumber"]);
                nm = nm.Replace("%house", "?");

                if (tags.ContainsKey("addr:flats"))
                    nm = nm.Replace("%flats", tags["addr:flats"]);
                nm = nm.Replace("%flats", "flats");

                Regex rg = new Regex(@"(%\w+)");
                foreach (Match mc in rg.Matches(nm))
                {
                    string v = mc.Value;
                    if (tags.ContainsKey(v.Substring(1)))
                        nm = nm.Replace(v, tags[v.Substring(1)]);
                    else
                        nm = nm.Replace(v, "");
                };

                return nm;
            }
        }

        public class ECondition
        {
            public enum ECType
            {
                ectAnd = 0,
                ectOR = 1
            }
            public class YNValue
            {
                public bool yn = true;
                public string v = null;
                public object more = null;

                public YNValue(bool yn, string v)
                {
                    this.yn = yn;
                    this.v = v;
                }

                public YNValue(bool yn, string v, object more)
                {
                    this.yn = yn;
                    this.v = v;
                    this.more = more;
                }
            }

            public ECType ect = ECType.ectAnd;
            public List<KeyValuePair<string, YNValue>> query = new List<KeyValuePair<string, YNValue>>();            
        }

        public static bool ProcessNode(Dictionary<string, string> tags, List<ECondition> conditions)
        {
            bool process = true;
            foreach (ECondition ec in conditions)
            {
                if (ec.ect == ECondition.ECType.ectAnd)
                {
                    foreach (KeyValuePair<string, ECondition.YNValue> kvp in ec.query)
                    {
                        if (kvp.Key == null)
                        {
                            throw new Exception("Not implemented yet");
                        }
                        else
                        {
                            if (kvp.Value.v == null)
                            {
                                if (kvp.Value.yn)
                                    process = process && (!tags.ContainsKey(kvp.Key));
                                else
                                    process = process && (tags.ContainsKey(kvp.Key));
                            }
                            else
                            {
                                if (!tags.ContainsKey(kvp.Key))
                                    process = false;
                                else
                                {                                    
                                    if (kvp.Value.yn)
                                    {
                                        bool ok = false;
                                        string[] av = kvp.Value.v.Split(new char[] { '|' });
                                        foreach (string a in av)
                                            if(Compare(tags[kvp.Key], a)) ok = true;
                                        process = process && ok;
                                    }
                                    else
                                    {
                                        bool ok = true;
                                        string[] av = kvp.Value.v.Split(new char[] { '|' });
                                        foreach (string a in av)
                                            if (Compare(tags[kvp.Key], a)) ok = false;
                                        process = process && ok;
                                    };
                                };
                            };
                        };
                    };
                };
                if (ec.ect == ECondition.ECType.ectOR)
                {
                    bool ok = false;
                    foreach (KeyValuePair<string, ECondition.YNValue> kvp in ec.query)
                    {
                        if (ok) continue;

                        if (kvp.Key == null)
                        {
                            if (ProcessNode(tags, (List<ECondition>) kvp.Value.more))
                                ok = true;
                        }
                        else
                        {
                            if (kvp.Value.v == null)
                            {
                                if (kvp.Value.yn)
                                {
                                    if (!tags.ContainsKey(kvp.Key)) ok = true;
                                }
                                else
                                {
                                    if (tags.ContainsKey(kvp.Key)) ok = true;
                                };
                            }
                            else
                            {
                                if (tags.ContainsKey(kvp.Key))
                                {
                                    if (kvp.Value.yn)
                                    {
                                        string[] av = kvp.Value.v.Split(new char[] { '|' });
                                        foreach (string a in av)
                                            if (Compare(tags[kvp.Key], a)) ok = true;
                                    }
                                    else
                                    {
                                        ok = true;
                                        string[] av = kvp.Value.v.Split(new char[] { '|' });
                                        foreach (string a in av)
                                            if (Compare(tags[kvp.Key], a)) ok = false;
                                    };
                                };
                            };
                        };                        
                    };
                    process = process && ok;
                };
            };
            return process;
        }

        private static bool Compare(string val, string dict)
        {
            if (dict == null) return false;
            if (dict.StartsWith("(") && dict.Contains("*"))
                return (new Regex(dict.Replace(" ", ""))).IsMatch(val);
            else if (dict.StartsWith("!(") && dict.Contains("*"))
                return !(new Regex(dict.Substring(1).Replace(" ", ""))).IsMatch(val);
            else
                return val == dict;
        }

        public bool ProcessNode(Dictionary<string, string> tags)
        {
            if(this.named && (!tags.ContainsKey("name"))) return false;
            return ProcessNode(tags, conditions);            
        }

        public void AddAnd(string key, ECondition.YNValue value)
        {
            ECondition ec = new ECondition();
            ec.ect = ECondition.ECType.ectAnd;
            ec.query.Add(new KeyValuePair<string,ECondition.YNValue>(key, value));
            this.conditions.Add(ec);
        }

        public void AddOr(string[] keys, ECondition.YNValue[] values)
        {
            ECondition ec = new ECondition();
            ec.ect = ECondition.ECType.ectOR;
            for (int i = 0; i < keys.Length; i++)
                ec.query.Add(new KeyValuePair<string, ECondition.YNValue>(keys[i], values[i]));
            this.conditions.Add(ec);
        }
    }
}
