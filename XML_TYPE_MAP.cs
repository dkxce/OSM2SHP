using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace OSM2SHP
{
    [Serializable]
    public class TYPE_MAP: XMLSaved<TYPE_MAP>
    {
        [XmlArray, XmlArrayItem("t")]
        public List<TYPE_MAP_Element> Elements = new List<TYPE_MAP_Element>();

        [XmlIgnore]
        private ApplyFilterScript afs = null;
        
        [XmlIgnore]
        public int Count
        {
            get
            {
                return this.Elements.Count;
            }
        }

        [XmlIgnore]
        public TYPE_MAP_Element this[int index]
        {
            get
            {
                return this.Elements[index];
            }            
        }        

        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return afs != null;
            }
        }

        public void Import(string fileName)
        {
            TYPE_MAP tmap = TYPE_MAP.LoadFile(fileName);
            if (tmap.Count > 0)
                this.Elements.AddRange(tmap.Elements);
            afs = null;
        }
        
        public void Init()
        {
            afs = null;
            if (this.Count == 0) return;
            try
            {
                Regex rx = new Regex(@"(\{[\@\w:-_]+\})");
                string func_text = "";
                for (int el = 0; el < this.Count; el++)
                {
                    if (this[el].FUNC == null) continue;
                    string text = this[el].FUNC.Trim();
                    if (String.IsNullOrEmpty(text)) continue;
                    MatchCollection mc = rx.Matches(text);
                    if (mc.Count > 0)
                        for (int i = mc.Count - 1; i >= 0; i--)
                        {
                            Match mch = mc[i];
                            text = text.Remove(mch.Index, mch.Length);
                            text = text.Insert(mch.Index, mch.Value.Replace("{", "ni[\"").Replace("}", "\"]"));
                        };
                    func_text += "\r\n  if (filter == " + el.ToString() + ") { return " + text + "; };  \r\n";
                };

                string code = "using System;\r\nusing System.Drawing;\r\nusing System.IO;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing System.Text.RegularExpressions;using System.Windows;\r\nusing System.Windows.Forms;\r\n\r\n";
                code += "namespace OSM2SHP {\r\n";
                code += "public class Script: ApplyFilterScript {\r\n";
                code += "public override bool ApplyFilters(OSMPBFReader.NodeInfo ni, int filter) {\r\n" + func_text;
                code += "\r\n  return false;\r\n}\r\n}}\r\n";

                System.Reflection.Assembly asm = CSScriptLibrary.CSScript.LoadCode(code, null);
                CSScriptLibrary.AsmHelper script = new CSScriptLibrary.AsmHelper(asm);
                this.afs = (ApplyFilterScript)script.CreateObject("OSM2SHP.Script");
            }
            catch { };
        }

        public static Exception CheckScript(string script_text)
        {
            try
            {
                Regex rx = new Regex(@"(\{[\@\w:-_]+\})");
                string text = script_text.Trim();
                MatchCollection mc = rx.Matches(text);
                if (mc.Count > 0)
                    for (int i = mc.Count - 1; i >= 0; i--)
                    {
                        Match mch = mc[i];
                        text = text.Remove(mch.Index, mch.Length);
                        text = text.Insert(mch.Index, mch.Value.Replace("{", "ni[\"").Replace("}", "\"]"));
                    };
                string func_text = " return (" + text + "); ";
                
                string code = "using System;\r\nusing System.Drawing;\r\nusing System.IO;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing System.Text.RegularExpressions;using System.Windows;\r\nusing System.Windows.Forms;\r\n\r\n";
                code += "namespace OSM2SHP {\r\n";
                code += "public class Script: ApplyFilterScript {\r\n";
                code += "public override bool ApplyFilters(OSMPBFReader.NodeInfo ni) {\r\n";
                code += func_text;
                code += "}\r\n}}\r\n";

                System.Reflection.Assembly asm = CSScriptLibrary.CSScript.LoadCode(code, null);
                CSScriptLibrary.AsmHelper script = new CSScriptLibrary.AsmHelper(asm);
                ApplyFilterScript afs = (ApplyFilterScript)script.CreateObject("OSM2SHP.Script");
                return null;
            }
            catch (Exception ex) { return ex; };
        }

        public TYPE_MAP_Element[] ProcessNode(OSMPBFReader.NodeInfo ni)
        {
            if (this.Elements == null) return new TYPE_MAP_Element[0];
            if (this.Elements.Count == 0) return new TYPE_MAP_Element[0];
            if (this.afs == null) return new TYPE_MAP_Element[0];

            List<TYPE_MAP_Element> result = new List<TYPE_MAP_Element>();
            for (int i = 0; i < this.Count; i++)
                if (afs.ApplyFilters(ni, i))
                    result.Add(this[i]);

            return result.ToArray();
        }
    }

    [Serializable]
    public class TYPE_MAP_Element
    {
        #region XML params
        [XmlAttribute("id")]
        public string _____xml_id
        {
            get
            {
                return this.ToString();
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    GARMIN_TYPE = -1;
                    return;
                }
                else
                    GARMIN_TYPE = int.Parse(value.Replace("0x", ""), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        [XmlAttribute("label")]
        public string _____xml_label = null;
        [XmlText]
        public string _____xml_cond
        {
            get
            {
                return _____proc;
            }
            set
            {
                string text = _____proc = value;                
            }
        }
        #endregion

        [XmlIgnore]
        public string LABEL
        {
            get { return _____xml_label; }
            set { _____xml_label = value; }
        }
        [XmlIgnore]
        public int    GARMIN_TYPE
        {
            get { return _____gtype; }
            set { _____gtype = value; }
        }
        [XmlIgnore]
        public string FUNC
        {
            get
            {
                return _____proc;
            }
            set
            {
                _____proc = value;
            }
        }

        [XmlIgnore]
        private int _____gtype = -1;
        [XmlIgnore]
        private string _____proc = null;        

        public string GetName(OSMPBFReader.NodeInfo ni)
        {
            string text = LABEL;
            Regex rx = new Regex(@"(\{[\@\w:-_]+\})");
            MatchCollection mc = rx.Matches(text);
            if (mc.Count > 0)
                for (int i = mc.Count - 1; i >= 0; i--)
                {
                    Match mch = mc[i];
                    text = text.Remove(mch.Index, mch.Length);
                    string new_val = ni[mch.Value.Substring(1,mch.Length-2)];
                    if(!String.IsNullOrEmpty(new_val))
                        text = text.Insert(mch.Index, new_val);
                };
            return text.Trim();
        }

        public override string ToString()
        {
            return String.Format("0x{0:X4}", GARMIN_TYPE);
        }
    }
}
