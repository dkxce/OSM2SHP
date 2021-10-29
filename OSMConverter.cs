using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace OSM2SHP
{
    public class OSMConverter
    {
        public string _osmFile = null;
        public string _dbfFile = null;
        public Config _config = null;
        public bool IsXML { get { return Path.GetExtension(_osmFile).ToLower() == ".osm"; } }
        public bool IsPBF { get { return Path.GetExtension(_osmFile).ToLower() == ".pbf"; } }

        public Regex AggrRegex = null;
        public Regex hasTextRegex = null;

        public Dictionary<long, FLID> linesFLID = new Dictionary<long, FLID>();
        private NodesXYIndex nodes_ndi = null;
        public long NDILength { get { return nodes_ndi == null ? 0 : nodes_ndi.Length; } }
        public long LFLLength { get { return 48 * linesFLID.Count + 100; } }
        public long JNSLength { get { return 16 * relaJJ.Count + 100; } }

        private DBFWriter points_dbf = null;
        private SHPWriter points_shp = null;
        private SHXWriter points_shx = null;

        private DBFWriter lines_dbf = null;
        private SHPWriter lines_shp = null;
        private SHXWriter lines_shx = null;

        private DBFWriter areas_dbf = null;
        private SHPWriter areas_shp = null;
        private SHXWriter areas_shx = null;

        private DBFWriter relat_dbf = null;
        private DBFWriter relam_dbf = null;
        private DBFWriter relaj_dbf = null;
        private Dictionary<long, long> relaJJ = new Dictionary<long, long>();        

        private JSONDict jsd = new JSONDict();
        private TYPE_MAP tmap = new TYPE_MAP();

        private long points_total = 0;
        private long points_procc = 0;
        private long points_writd = 0;
        private long points_unicl = 0;
        private long points_empty = 0;

        private long points_from_nodes = 0;
        private long points_from_ways  = 0;
        private long points_form_lines = 0;
        private long points_from_areas = 0;

        private long points_skb_flt = 0;
        private long points_skb_sel = 0;        

        private long ways_total = 0;
        private long ways_procc = 0;
        private long ways_writd = 0;

        private long ways_as_both = 0;
        private long ways_as_lines = 0;
        private long ways_as_areas = 0;
        private long ways_writd_lines = 0;
        private long ways_writd_areas = 0;
        private long ways_skipd_lines = 0;
        private long ways_skipd_areas = 0;
        private long ways_skipd = 0;
        private long ways_by_rels_ttl = 0;
        private long ways_by_rels_lns = 0;
        private long ways_by_rels_ars = 0;

        private long relations_total = 0;
        private long relations_procc = 0;
        private long relations_writd = 0;
        private long relations_mwrtd = 0;

        private long relations_joins_LNS = 0;
        private long relations_joins_TV = 0;
        private long relations_joins_FL = 0;
        private long relations_joins_ERR = 0;
        private long relations_joins_VLD = 0;
        private long relations_joins_COPY = 0;

        private long nodes_writed = 0;
        public long NodesWrited { get { return nodes_writed; } }
        private long nodes_fsize = 0;
        public long NodesFSize { get { return nodes_fsize; } }
        private string nodes_fname = "";
        public string NodesFName { get { return nodes_fname; } }
        private float nodes_percent = 0;
        public float NodesPercentage { get { return nodes_percent; } }

        private long files_points_wrtd = 0;        
        private List<string> files_points_wrts = new List<string>();
        private List<string> files_points_wrtf = new List<string>();
        private List<long> files_points_wrtc = new List<long>();
        private List<long> files_points_sizes = new List<long>();

        public long PointsTotal { get { return points_total; } }        
        public long PointsProcessed { get { return points_procc; } }        
        public long PointsWrited { get { return points_writd; } }
        public long PointsUnical { get { return points_unicl; } }
        public long PointsEmpty { get { return points_empty; } }

        public long PointsFromNodes { get { return points_from_nodes; } }
        public long PointsFromWays { get { return points_from_ways; } }
        public long PointsFromLines { get { return points_form_lines; } }
        public long PointsFromAreas { get { return points_from_areas; } }

        private long files_lines_wrtd = 0;
        private List<string> files_lines_wrts = new List<string>();
        private List<string> files_lines_wrtf = new List<string>();
        private List<long> files_lines_wrtc = new List<long>();
        private List<long> files_lines_sizes = new List<long>();

        private long files_areas_wrtd = 0;
        private List<string> files_areas_wrts = new List<string>();
        private List<string> files_areas_wrtf = new List<string>();
        private List<long> files_areas_wrtc = new List<long>();
        private List<long> files_areas_sizes = new List<long>();

        private long files_relations_wrtd = 0;        
        private List<string> files_relations_wrtt = new List<string>();
        private List<string> files_relations_wrtm = new List<string>();
        private List<long> files_relations_wrt_rc = new List<long>();
        private List<long> files_relations_wrt_mc = new List<long>();
        private List<long> files_relations_sizes = new List<long>();

        private long files_joins_wrtd = 0;
        private List<string> files_joins_wrtf = new List<string>();
        private List<long> files_joins_wrtl = new List<long>();
        private List<long> files_joins_wrtTV = new List<long>();
        private List<long> files_joins_wrtFL = new List<long>();
        private List<long> files_joins_wrtER = new List<long>();
        private List<long> files_joins_sizes = new List<long>();

        public long WaysTotal { get { return ways_total; } }
        public long WaysProcessed { get { return ways_procc; } }
        public long WaysWrited { get { return ways_writd; } }

        public long WaysAsBoth { get { return ways_as_both; } }
        public long WaysAsLines { get { return ways_as_lines; } }
        public long WaysAsAreas { get { return ways_as_areas; } }
        public long WaysWritedAsLines { get { return ways_writd_lines; } }
        public long WaysWritedAsAreas { get { return ways_writd_areas; } }
        public long WaysSkippedFilterLines { get { return ways_skipd_lines; } }
        public long WaysSkippedFilterAreas { get { return ways_skipd_areas; } }
        public long WaysSkippedFilter { get { return ways_skipd; } }
        public long WaysByRelationsTotal { get { return ways_by_rels_ttl; } }
        public long WaysByRelationsLines { get { return ways_by_rels_lns; } }
        public long WaysByRelationsAreas { get { return ways_by_rels_ars; } }

        public long RelationsTotal { get { return relations_total; } }
        public long RelationsProcessed { get { return relations_procc; } }
        public long RelationsWrited { get { return relations_writd; } }
        public long RelationsMemsWrited { get { return relations_mwrtd; } }

        public long NodesSkippedByFilter { get { return points_skb_flt; } }
        public long NodesSkippedBySel { get { return points_skb_sel; } }

        public long FilesPointsWrited { get { return files_points_wrtd; } }
        public string[] FilesPointsWritedShapes { get { return files_points_wrts.ToArray(); } }
        public string[] FilesPointsWritedDBFs { get { return files_points_wrtf.ToArray(); } }
        public long[] FilesPointsWritedPoints { get { return files_points_wrtc.ToArray(); } }
        public long[] FilesPointsWritedSizes { get { return files_points_sizes.ToArray(); } }

        public long FilesLinesWrited { get { return files_lines_wrtd; } }
        public string[] FilesLinesWritedShapes { get { return files_lines_wrts.ToArray(); } }
        public string[] FilesLinesWritedDBFs { get { return files_lines_wrtf.ToArray(); } }
        public long[] FilesLinesWritedLines { get { return files_lines_wrtc.ToArray(); } }
        public long[] FilesLinesWritedSizes { get { return files_lines_sizes.ToArray(); } }

        public long FilesAreasWrited { get { return files_areas_wrtd; } }
        public string[] FilesAreasWritedShapes { get { return files_areas_wrts.ToArray(); } }
        public string[] FilesAreasWritedDBFs { get { return files_areas_wrtf.ToArray(); } }
        public long[] FilesAreasWritedAreas { get { return files_areas_wrtc.ToArray(); } }
        public long[] FilesAreasWritedSizes { get { return files_areas_sizes.ToArray(); } }

        public long FilesRelationsWrited { get { return files_relations_wrtd; } }        
        public string[] FilesRelationsWritedMain { get { return files_relations_wrtt.ToArray(); } }
        public string[] FilesRelationsWritedMems { get { return files_relations_wrtm.ToArray(); } }
        public long[] FilesRelationsWritedRelsCounts { get { return files_relations_wrt_rc.ToArray(); } }
        public long[] FilesRelationsWritedMemsCounts { get { return files_relations_wrt_mc.ToArray(); } }
        public long[] FilesRelationsWritedSizes { get { return files_relations_sizes.ToArray(); } }

        public long FilesJoinsWrited { get { return files_joins_wrtd; } }
        public string[] FilesJoinsWritedDBFs { get { return files_joins_wrtf.ToArray(); } }
        public long[] FilesJoinsWritedLines { get { return files_joins_wrtl.ToArray(); } }
        public long[] FilesJoinsWritedTV { get { return files_joins_wrtTV.ToArray(); } }
        public long[] FilesJoinsWritedFL { get { return files_joins_wrtFL.ToArray(); } }
        public long[] FilesJoinsWritedER { get { return files_joins_wrtER.ToArray(); } }
        public long[] FilesJoinsWritedSizes { get { return files_joins_sizes.ToArray(); } }
        public long JoinsRestrictionsLNS { get { return relations_joins_LNS; } }
        public long JoinsRestrictionsTV { get { return relations_joins_TV; } }
        public long JoinsRestrictionsFL { get { return relations_joins_FL; } }
        public long JoinsRestrictionsVLD { get { return relations_joins_VLD; } }
        public long JoinsRestrictionsERR { get { return relations_joins_ERR; } }
        public long JoinsRestrictionsCOPY { get { return relations_joins_COPY; } }

        public Dictionary<string, uint> tags_skipped = new Dictionary<string, uint>();
        public int tags_max_count = 0;
        public int tags_aggrf_used = 0;
        public int tags_aggra_count = 0;
        public int tags_string_maxl = 0;

        public int MaxFileRecords = 500000; // default = 500000

        public delegate void OnProgress(object sender, float percentage, bool processing);
        public delegate void OnError(object sender, Exception error);
        public event OnProgress onProgress;
        public event OnError onError;
        public event EventHandler onDone;
        public event EventHandler onStart;

        private ApplyFilterScript apf = null;

        public byte PointsFieldsCount
        {
            get
            {
                if (points_dbf == null) return 0;
                return points_dbf.FieldsCount;
            }
        }

        public ushort PointsRecordSize
        {
            get
            {
                if (points_dbf == null) return 0;
                return points_dbf.RecordSize;
            }
        }

        public byte LinesFieldsCount
        {
            get
            {
                if (lines_dbf == null) return 0;
                return lines_dbf.FieldsCount;
            }
        }

        public byte AreasFieldsCount
        {
            get
            {
                if (areas_dbf == null) return 0;
                return areas_dbf.FieldsCount;
            }
        }

        public byte RelationsFieldsCount
        {
            get
            {
                if (relat_dbf == null) return 0;
                return relat_dbf.FieldsCount;
            }
        }

        public byte RelationsMembersFieldsCount
        {
            get
            {
                if (relam_dbf == null) return 0;
                return relam_dbf.FieldsCount;
            }
        }

        public byte RelationsJoinsFieldsCount
        {
            get
            {
                if (relaj_dbf == null) return 0;
                return relaj_dbf.FieldsCount;
            }
        }

        public ushort LinesRecordSize
        {
            get
            {
                if (lines_dbf == null) return 0;
                return lines_dbf.RecordSize;
            }
        }

        public ushort AreasRecordSize
        {
            get
            {
                if (areas_dbf == null) return 0;
                return areas_dbf.RecordSize;
            }
        }

        public ushort RelationsRecordSize
        {
            get
            {
                if (relat_dbf == null) return 0;
                return relat_dbf.RecordSize;
            }
        }

        public ushort RelationsMembersRecordSize
        {
            get
            {
                if (relam_dbf == null) return 0;
                return relam_dbf.RecordSize;
            }
        }

        public ushort RelationsJoinsRecordSize
        {
            get
            {
                if (relaj_dbf == null) return 0;
                return relaj_dbf.RecordSize;
            }
        }

        private void PrepareForConvert()
        {
            this.AggrRegex = null;
            this.hasTextRegex = null;

            if(!String.IsNullOrEmpty(_config.aggrRegex))
                try { this.AggrRegex = new Regex(_config.aggrRegex, RegexOptions.IgnoreCase); } catch (Exception ex) { this.AggrRegex = null; };
            if (!String.IsNullOrEmpty(_config.hasText))
                try { this.hasTextRegex = new Regex(_config.hasText, RegexOptions.IgnoreCase); } catch (Exception ex) { this.hasTextRegex = null; };

            _config.PrepareForRead();

            points_total = 0;
            points_procc = 0;
            points_writd = 0;
            points_unicl = 0;
            points_empty = 0;
            points_skb_flt = 0;
            points_skb_sel = 0;
            files_points_wrtd = 0;
            files_points_wrts.Clear();
            files_points_wrtf.Clear();
            files_points_wrtc.Clear();
            files_lines_wrtd = 0;
            files_lines_wrts.Clear();
            files_lines_wrtf.Clear();
            files_lines_wrtc.Clear();
            files_areas_wrtd = 0;
            files_areas_wrts.Clear();
            files_areas_wrtf.Clear();
            files_areas_wrtc.Clear();
            files_relations_wrtd = 0;            
            files_relations_wrtt.Clear();
            files_relations_wrtm.Clear();
            files_relations_wrt_rc.Clear();
            files_relations_wrt_mc.Clear();
            files_joins_wrtd = 0;
            files_joins_wrtf.Clear();
            files_joins_wrtl.Clear();
            files_joins_wrtTV.Clear();
            files_joins_wrtFL.Clear();
            files_joins_wrtER.Clear();
            tags_skipped.Clear();
            tags_aggrf_used = 0;
            tags_max_count = 0;
            tags_aggra_count = 0;
            tags_string_maxl = 0;
            nodes_writed = 0;
            nodes_fsize = 0;
            nodes_fname = "";
            nodes_percent = 0;

            apf = null;
            if(!String.IsNullOrEmpty(_config.scriptFilter))
            try
            {
                string code = "using System;\r\nusing System.Drawing;\r\nusing System.IO;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing System.Text.RegularExpressions;using System.Windows;\r\nusing System.Windows.Forms;\r\n\r\n";
                code += "namespace OSM2SHP {\r\n";
                code += "public class Script: ApplyFilterScript {\r\n";
                code += "public override bool ApplyFilters(OSMPBFReader.NodeInfo ni) { " + _config.scriptFilter;
                code += "\r\n}\r\n}}\r\n";

                System.Reflection.Assembly asm = CSScriptLibrary.CSScript.LoadCode(code, null);
                CSScriptLibrary.AsmHelper script = new CSScriptLibrary.AsmHelper(asm);
                apf = (ApplyFilterScript)script.CreateObject("OSM2SHPFC.Script");
                apf.Converter = this;
                apf.args = Program.exeargs;
            }
            catch { };
        }
        
        public OSMConverter(Config config)
        {            
            this._config = config;
            this._osmFile = config.inputFileName;
            this._dbfFile = config.outputFileName;
        }

        public bool ProcWaysSure
        {
            get
            {
                return (_config.processWays);//(_config.processWays) && ((_config.processLines && (_config.selector != 2)) || (_config.processAreas && (_config.selector != 2)) || (_config.processCentroids > 0));
            }
        }

        public bool ProcLineSure
        {
            get
            {
                return _config.processLines && (_config.selector != 2); // (_config.processWays) && _config.processLines && (_config.selector != 2);
            }
        }

        public bool ProcAreaSure
        {
            get
            {
                return _config.processAreas && (_config.selector != 2); // (_config.processWays) && _config.processAreas && (_config.selector != 2);
            }
        }

        public bool ProcRelsSure
        {
            get
            {
                return _config.processRelations;
            }
        }

        public bool ProcJoins
        {
            get
            {
                return _config.processRelations && _config.relationsAsJoins;
            }
        }


        public void Convert()
        {
            PrepareForConvert();

            if (!String.IsNullOrEmpty(_config.sortAggTagsPriority))
            {
                if (_config.sortAggTagsPriority == "нет")
                {
                }
                else if (_config.sortAggTagsPriority == "да")
                    tagsComparer = new TagsComparer("");
                else
                    tagsComparer = new TagsComparer(_config.sortAggTagsPriority);
            };

            files_points_wrtd++;
            if (this.ProcLineSure) files_lines_wrtd++;
            if (this.ProcAreaSure) files_areas_wrtd++;
            if (this.ProcRelsSure) files_relations_wrtd++;
            if (this.ProcJoins) files_joins_wrtd++;
            
            string _ndsi = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + "[nodes].idx";
            string _ndss = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + "[NODES]";
            
            string _dbfp = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".dbf";
            string _shpp = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".shp";
            string _shxp = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".shx";
            string _prjp = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".prj";            

            string _dbfl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".dbf";
            string _shpl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".shp";
            string _shxl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".shx";
            string _prjl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".prj";

            string _dbfa = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".dbf";
            string _shpa = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".shp";
            string _shxa = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".shx";
            string _prja = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".prj";            

            string _relat = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[R{0:0000}]", files_relations_wrtd) + ".dbf";
            string _relam = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[M{0:0000}]", files_relations_wrtd) + ".dbf";
            string _relaj = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[J{0:0000}]", files_joins_wrtd) + ".dbf";
            
            files_points_wrts.Add(Path.GetFileName(_shpp));
            files_points_wrtf.Add(Path.GetFileName(_dbfp));
            files_points_wrtc.Add(0);
            files_points_sizes.Add(0);

            if (this.ProcLineSure)
            {
                files_lines_wrts.Add(Path.GetFileName(_shpl));
                files_lines_wrtf.Add(Path.GetFileName(_dbfl));
                files_lines_wrtc.Add(0);
                files_lines_sizes.Add(0);
            };

            if (this.ProcAreaSure)
            {
                files_areas_wrts.Add(Path.GetFileName(_shpa));
                files_areas_wrtf.Add(Path.GetFileName(_dbfa));
                files_areas_wrtc.Add(0);
                files_areas_sizes.Add(0);
            };

            if (this.ProcRelsSure)
            {
                files_relations_wrtt.Add(Path.GetFileName(_relat));
                files_relations_wrtm.Add(Path.GetFileName(_relam));
                files_relations_wrt_rc.Add(0);
                files_relations_wrt_mc.Add(0);
                files_relations_sizes.Add(0);
            }
            if(this.ProcJoins)
            {
                files_joins_wrtf.Add(Path.GetFileName(_relaj));
                files_joins_wrtl.Add(0);
                files_joins_wrtTV.Add(0);
                files_joins_wrtFL.Add(0);
                files_joins_wrtER.Add(0);
                files_joins_sizes.Add(0);
            };

            try
            {
                string dir = Path.GetDirectoryName(_dbfp);
                Directory.CreateDirectory(dir);
                if (this.ProcWaysSure)
                    nodes_ndi = _config.useNotInMemoryIndexFile ? new NodesXYIndex(_ndsi) : new NodesXYIndex();
                linesFLID.Clear();

                points_dbf = new DBFWriter(_dbfp, FileMode.Create, _config.dbfCodePage);
                points_shp = SHPWriter.CreatePointsFile(_shpp);
                points_shx = SHXWriter.CreatePointsIndex(_shxp);
                PRJWriter.CreateProjFile(_prjp);

                if (this.ProcLineSure)
                {
                    lines_dbf = new DBFWriter(_dbfl, FileMode.Create, _config.dbfCodePage);
                    lines_shp = SHPWriter.CreateLinesFile(_shpl);
                    lines_shx = SHXWriter.CreateLinesIndex(_shxl);
                    PRJWriter.CreateProjFile(_prjl);

                };

                if (this.ProcAreaSure)
                {
                    areas_dbf = new DBFWriter(_dbfa, FileMode.Create, _config.dbfCodePage);
                    areas_shp = SHPWriter.CreateAreasFile(_shpa);
                    areas_shx = SHXWriter.CreateAreasIndex(_shxa);
                    PRJWriter.CreateProjFile(_prja);
                };

                if (this.ProcRelsSure)
                {
                    relat_dbf = new DBFWriter(_relat, FileMode.Create, _config.dbfCodePage);
                    relam_dbf = new DBFWriter(_relam, FileMode.Create, _config.dbfCodePage);
                };
                if(this.ProcJoins)
                {
                    relaj_dbf = new DBFWriter(_relaj, FileMode.Create, _config.dbfCodePage);
                };
            }
            catch (Exception ex)
            {
                if (nodes_ndi != null) nodes_ndi.Close();
                linesFLID.Clear();

                if (points_dbf != null) points_dbf.Close();
                if (points_shp != null) points_shp.Close();
                if (points_shx != null) points_shx.Close();

                if (this.ProcLineSure)
                {
                    if (lines_dbf != null) lines_dbf.Close();
                    if (lines_shp != null) lines_shp.Close();
                    if (lines_shx != null) lines_shx.Close();
                };

                if (this.ProcAreaSure)
                {
                    if (areas_dbf != null) areas_dbf.Close();
                    if (areas_shp != null) areas_shp.Close();
                    if (areas_shx != null) areas_shx.Close();
                };

                if (this.ProcRelsSure)
                {
                    if (relat_dbf != null) relat_dbf.Close();
                    if (relam_dbf != null) relam_dbf.Close();                    
                };
                if (this.ProcJoins)
                {
                    if (relaj_dbf != null) relaj_dbf.Close();
                };

                throw ex;
            };

            WritePointsHeaders(_config.dbfDopFields.ToArray());
            files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
            if (this.ProcLineSure)
            {
                WriteLinesHeaders(_config.dbfDopFields.ToArray(), _config.addFirstAndLastNodesIdToLines);
                files_lines_sizes[files_lines_wrtc.Count - 1] = lines_dbf.Length + lines_shx.Length + lines_shp.Length + 204;
            };
            if (this.ProcAreaSure)
            {
                WriteAreasHeaders(_config.dbfDopFields.ToArray());
                files_areas_sizes[files_areas_wrtc.Count - 1] = areas_dbf.Length + areas_shx.Length + areas_shp.Length + 204;
            };
            if (this.ProcRelsSure)
            {
                WriteRelationsHeaders(_config.dbfDopFields.ToArray());
                WriteRelMemsHeaders(new string[0]);
                files_relations_sizes[files_relations_wrt_rc.Count - 1] = relat_dbf.Length + relam_dbf.Length;                
            };
            if (this.ProcJoins)
            {
                WriteJoinsHeaders();
                files_joins_sizes[files_joins_wrtl.Count - 1] = relaj_dbf.Length;
            };
            
            if ((_config.selector == 0) || (_config.selector == 3))
            {
                jsd = new JSONDict();
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\json_mp_points", "*.json");
                foreach (string file in files) jsd.AddFile(file);
            };
            if ((_config.selector == 1) || (_config.selector == 4))
            {
                jsd = new JSONDict();
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\json_garmin_points", "*.json");
                foreach (string file in files) jsd.AddFile(file);
            };
            if (_config.selector == 14)
            {
                tmap = new TYPE_MAP();
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\xml_selector", "*.xml");
                foreach (string file in files) tmap.Import(file);
                tmap.Init();
            };

            if (onStart != null)
                onStart(this, new EventArgs());

            Exception err = null;
            try
            {
                if (IsPBF) ReadPBF();
                if (IsXML) ReadXML();
            }
            catch (Exception ex)
            {
                err = ex;               
            };

            if (this.ProcWaysSure)
            {
                if(_config.saveLineNodesShape)
                    try
                    {
                        WriteNodesShape(_ndss);
                    }
                    catch { };
                nodes_ndi.Close();
            };
            linesFLID.Clear();

            points_dbf.Close();
            points_shp.Close();
            points_shx.Close();

            if (this.ProcLineSure)
            {
                lines_dbf.Close();
                lines_shp.Close();
                lines_shx.Close();
            };

            if (this.ProcAreaSure)
            {
                areas_dbf.Close();
                areas_shp.Close();
                areas_shx.Close();
            };

            if (this.ProcRelsSure)
            {
                relat_dbf.Close();
                relam_dbf.Close();                
            };
            if (this.ProcJoins)
            {
                relaj_dbf.Close();
            };

            try
            {
                Progress(1, false);
            }
            catch { };

            if (err == null)
                Done();
            else
                Error(err);
        }

        private void WriteNodesShape(string fileName)
        {           
            if (nodes_ndi.Length == 0) return;

            DBFWriter nodes_dbf = new DBFWriter(fileName + ".dbf", FileMode.Create, _config.dbfCodePage);
            SHPWriter nodes_shp = SHPWriter.CreatePointsFile(fileName + ".shp");
            SHXWriter nodes_shx = SHXWriter.CreatePointsIndex(fileName + ".shx");
            PRJWriter.CreateProjFile(fileName + ".prj");

            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX",   012, 'N');
            fields.Add("NODE_ID", 018, 'N');
            fields.Add("L_COUNT", 002, 'N');
            nodes_dbf.WriteHeader(fields);

            long nodes_listed = 0;
            float nodes_ttl = (float)nodes_ndi.Dict.Count;

            nodes_writed = 0;
            nodes_fsize = 0;
            nodes_percent = 0;
            nodes_fname = Path.GetFileNameWithoutExtension(fileName + ".dbf");

            if(nodes_ndi.InMemory)
            {
                foreach(KeyValuePair<long,NodesXYIndex.LatLonLCo> el in nodes_ndi.Dict)
                {
                    nodes_listed++;
                    nodes_percent = ((float)nodes_listed) / nodes_ttl;

                    // в lco храним число линий, пересекающих точку
                    // 00001111 - если точка является началом или концом линии (до 15 линий)
                    // 11110000 - если точка не является началом или концом линии (до 15 линий)
                    byte se = (byte)(el.Value.lco & 0x0F);
                    byte md = (byte)(el.Value.lco & 0xF0);
                    // если число проходящих точек через линию в середине > 1
                    // или если одна линия проходит через середину, а другие нет
                    if ((md > 0x10) || ((md == 0x10) && (se > 0x00)))
                    {
                        byte lco = (byte)(se + (md >> 4));

                        Dictionary<string, object> rec = new Dictionary<string, object>();
                        rec.Add("INDEX", nodes_writed);
                        rec.Add("NODE_ID", el.Key);
                        rec.Add("L_COUNT", lco);

                        nodes_dbf.WriteRecord(rec, 0, 0, 0, 0);
                        nodes_shx.WritePointIndex((int)nodes_shp.Position);
                        nodes_shp.WritePoint(el.Value.lon, el.Value.lat);

                        nodes_writed++;
                        nodes_fsize = nodes_dbf.Length + nodes_shx.Length + nodes_shp.Length + 204;

                        Progress(0.99f, true);
                    };
                };
            }
            else
            {
                nodes_ndi.STR.Position = 0;
                while (nodes_ndi.STR.Position < nodes_ndi.STR.Length)
                {
                    nodes_listed++;                    
                    byte[] buff = new byte[25];
                    nodes_ndi.STR.Read(buff, 0, buff.Length);
                    nodes_percent = (float)nodes_listed / nodes_ttl;

                    // в lco храним число линий, пересекающих точку
                    // 00001111 - если точка является началом или концом линии (до 15 линий)
                    // 11110000 - если точка не является началом или концом линии (до 15 линий)
                    byte se = (byte)(buff[24] & 0x0F);
                    byte md = (byte)(buff[24] & 0xF0);
                    if ((md > 0x10) || ((md == 0x10) && (se > 0x00)))
                    {
                        byte lco = (byte)(se + (md >> 4));

                        NodesXYIndex.IdLatLon res = new NodesXYIndex.IdLatLon(BitConverter.ToInt64(buff, 0), BitConverter.ToDouble(buff, 8), BitConverter.ToDouble(buff, 16), buff[24]);

                        Dictionary<string, object> rec = new Dictionary<string, object>();
                        rec.Add("INDEX", nodes_writed);
                        rec.Add("NODE_ID", res.id);
                        rec.Add("L_COUNT", lco);

                        nodes_dbf.WriteRecord(rec, 0, 0, 0, 0);
                        nodes_shx.WritePointIndex((int)nodes_shp.Position);
                        nodes_shp.WritePoint(res.lon, res.lat);

                        nodes_writed++;
                        nodes_fsize = nodes_dbf.Length + nodes_shx.Length + nodes_shp.Length + 204;

                        Progress(0.99f, true);
                    };
                };
            };

            nodes_dbf.Close();
            nodes_shp.Close();
            nodes_shx.Close();
        }

        private void CheckFilesFull()
        {
            if ((points_dbf.WritedRecords >= MaxFileRecords) || (points_shp.Position >= (2147483648 - 20 * 1024 * 1024)) || (points_dbf.Position >= (2147483648 - 20 * 1024 * 1024))) // 20 MB
            {
                points_dbf.Close();
                points_shp.Close();
                points_shx.Close();

                files_points_wrtd++;

                string _dbff = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".dbf";
                string _shpf = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".shp";
                string _prjf = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".prj";
                string _shxf = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[P{0:0000}]", files_points_wrtd) + ".shx";
                files_points_wrts.Add(Path.GetFileName(_shpf));
                files_points_wrtf.Add(Path.GetFileName(_dbff));
                files_points_wrtc.Add(0);
                files_points_sizes.Add(0);
                points_dbf = new DBFWriter(_dbff, FileMode.Create, _config.dbfCodePage);
                points_shp = SHPWriter.CreatePointsFile(_shpf);
                points_shx = SHXWriter.CreatePointsIndex(_shxf);
                PRJWriter.CreateProjFile(_prjf);
                WritePointsHeaders(_config.dbfDopFields.ToArray());
                files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
            };
            if(lines_dbf != null)
                if ((lines_dbf.WritedRecords >= MaxFileRecords) || (lines_shp.Position >= (2147483648 - 20 * 1024 * 1024)) || (lines_dbf.Position >= (2147483648 - 20 * 1024 * 1024))) // 20 MB
                {
                    lines_dbf.Close();
                    lines_shp.Close();
                    lines_shx.Close();

                    files_lines_wrtd++;

                    string _dbfl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".dbf";
                    string _shpl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".shp";
                    string _prjl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".prj";
                    string _shxl = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[L{0:0000}]", files_lines_wrtd) + ".shx";
                    files_lines_wrts.Add(Path.GetFileName(_shpl));
                    files_lines_wrtf.Add(Path.GetFileName(_dbfl));
                    files_lines_wrtc.Add(0);
                    files_lines_sizes.Add(0);
                    lines_dbf = new DBFWriter(_dbfl, FileMode.Create, _config.dbfCodePage);
                    lines_shp = SHPWriter.CreateLinesFile(_shpl);
                    lines_shx = SHXWriter.CreateLinesIndex(_shxl);
                    PRJWriter.CreateProjFile(_prjl);
                    WriteLinesHeaders(_config.dbfDopFields.ToArray(), _config.addFirstAndLastNodesIdToLines);
                    files_lines_sizes[files_lines_wrtc.Count - 1] = lines_dbf.Length + lines_shx.Length + lines_shp.Length + 204;
                };
            if (areas_dbf != null)
                if ((areas_dbf.WritedRecords >= MaxFileRecords) || (areas_shp.Position >= (2147483648 - 20 * 1024 * 1024)) || (areas_dbf.Position >= (2147483648 - 20 * 1024 * 1024))) // 20 MB
                {
                    areas_dbf.Close();
                    areas_shp.Close();
                    areas_shx.Close();

                    files_areas_wrtd++;

                    string _dbfa = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".dbf";
                    string _shpa = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".shp";
                    string _prja = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".prj";
                    string _shxa = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[A{0:0000}]", files_areas_wrtd) + ".shx";
                    files_areas_wrts.Add(Path.GetFileName(_shpa));
                    files_areas_wrtf.Add(Path.GetFileName(_dbfa));
                    files_areas_wrtc.Add(0);
                    files_areas_sizes.Add(0);
                    areas_dbf = new DBFWriter(_dbfa, FileMode.Create, _config.dbfCodePage);
                    areas_shp = SHPWriter.CreateAreasFile(_shpa);
                    areas_shx = SHXWriter.CreateAreasIndex(_shxa);
                    PRJWriter.CreateProjFile(_prja);
                    WriteAreasHeaders(_config.dbfDopFields.ToArray());
                    files_areas_sizes[files_areas_wrtc.Count - 1] = areas_dbf.Length + areas_shx.Length + areas_shp.Length + 204;
                };
            if (relat_dbf != null)
            {
                if ((relat_dbf.WritedRecords >= MaxFileRecords) || (relat_dbf.Position >= (2147483648 - 20 * 1024 * 1024)) || (relam_dbf.Position >= (2147483648 - 10 * relam_dbf.RecordSize))) // 20 MB
                {
                    relat_dbf.Close();
                    relam_dbf.Close();
                    
                    files_relations_wrtd++;

                    string _relat = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[R{0:0000}]", files_relations_wrtd) + ".dbf";
                    string _relam = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[M{0:0000}]", files_relations_wrtd) + ".dbf";
                    files_relations_wrtt.Add(Path.GetFileName(_relat));
                    files_relations_wrtm.Add(Path.GetFileName(_relam));
                    files_relations_wrt_rc.Add(0);
                    files_relations_wrt_mc.Add(0);
                    files_relations_sizes.Add(0);
                    relat_dbf = new DBFWriter(_relat, FileMode.Create, _config.dbfCodePage);
                    relam_dbf = new DBFWriter(_relam, FileMode.Create, _config.dbfCodePage);
                    WriteRelationsHeaders(_config.dbfDopFields.ToArray());
                    WriteRelMemsHeaders(_config.dbfDopFields.ToArray());
                    files_relations_sizes[files_relations_wrt_rc.Count - 1] = relat_dbf.Length + relam_dbf.Length;
                };                
            };
            if (relaj_dbf != null)
            {
                if (relaj_dbf.Position >= (2147483648 - 10 * relaj_dbf.RecordSize))
                {
                    relaj_dbf.Close();
                    relaJJ.Clear(); // reset lines list

                    files_joins_wrtd++;

                    string _relaj = Path.GetDirectoryName(this._dbfFile) + @"\" + Path.GetFileNameWithoutExtension(this._dbfFile) + String.Format("[J{0:0000}]", files_joins_wrtd) + ".dbf";
                    files_joins_wrtf.Add(Path.GetFileName(_relaj));
                    files_joins_wrtl.Add(0);
                    files_joins_wrtTV.Add(0);
                    files_joins_wrtFL.Add(0);
                    files_joins_wrtER.Add(0);
                    files_joins_sizes.Add(0);
                    relaj_dbf = new DBFWriter(_relaj, FileMode.Create, _config.dbfCodePage);
                    WriteJoinsHeaders();
                    files_joins_sizes[files_joins_wrtl.Count - 1] = relaj_dbf.Length;
                };
            };
        }

        private void WritePointsHeaders(string[] dop_fields)
        {
            // Fields
            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX",     012, 'N');
            fields.Add("NODE_ID",   018, 'N');
            fields.Add("CRTD_FROM", 001, 'C');
            if ((_config.selector == 0) || (_config.selector == 1) || (_config.selector == 3) || (_config.selector == 4) || (_config.selector == 14))
            {
                fields.Add("TYPES", 012, 'C');
                fields.Add("TYPEI", 012, 'N');
            };
            if(_config.dbfMainFields.Contains("VERSION"))
                fields.Add("VERSION",   012, 'N');
            if (_config.dbfMainFields.Contains("TIMESTAMP"))
                fields.Add("TIMESTAMP", 018, 'N');
            if (_config.dbfMainFields.Contains("CHANGESET"))
                fields.Add("CHANGESET", 018, 'N');
            if (_config.dbfMainFields.Contains("UID"))
                fields.Add("UID",       012, 'N');
            if (_config.dbfMainFields.Contains("USER"))
                fields.Add("USER",      050, 'C');

            if (_config.dbfMainFields.Contains("TAGS_COUNT"))
                fields.Add("TAGS_COUNT",005, 'N');
            if (_config.dbfMainFields.Contains("TAGS_ADDRC"))
                fields.Add("TAGS_ADDRC",005, 'N');
            if ((_config.selector == 0) || (_config.selector == 1)) // OSM 2 MP
                fields.Add("SELECTOR", 25, 'C');
            else if ((_config.selector == 5) || (_config.selector == 6) || (_config.selector == 7) || (_config.selector == 8)) // addresses
            {                
                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };                
            }
            else if (_config.selector == 9) // all with tags
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');                
            }
            else if ((_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');   

                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };                
            }
            else // default
            {
                fields.Add("SELECTOR", 200, 'C');
            };

            if (_config.dbfMainFields.Contains("LABEL"))
                fields.Add("LABEL", 200, 'C');

            if ((_config.selector == 9) || (_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                for (int i = 1; i <= _config.maxAggTags; i++)
                    fields.Add("TAGS_" + i.ToString(), 250, 'C');
            };
            
            if((dop_fields != null) && (dop_fields.Length > 0))
                foreach(string str in dop_fields)
                {
                    if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                    {
                        string fn = str.Substring(0, str.IndexOf("["));
                        string ln = str.Substring(str.IndexOf("[")+1, str.IndexOf("]") - str.IndexOf("[") - 1);
                        fields.Add(fn, byte.Parse(ln), 'C');
                    }
                    else
                        fields.Add(str, 250, 'C');
                };

            // Write Header Info
            points_dbf.WriteHeader(fields);            
        }

        private void WriteLinesHeaders(string[] dop_fields, bool first_and_last)
        {
            // Fields
            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX", 012, 'N');
            fields.Add("LINE_ID", 018, 'N');
            fields.Add("CRTD_FROM", 001, 'C');
            if (_config.dbfMainFields.Contains("VERSION"))
                fields.Add("VERSION", 012, 'N');
            if (_config.dbfMainFields.Contains("TIMESTAMP"))
                fields.Add("TIMESTAMP", 018, 'N');
            if (_config.dbfMainFields.Contains("CHANGESET"))
                fields.Add("CHANGESET", 018, 'N');
            if (_config.dbfMainFields.Contains("UID"))
                fields.Add("UID", 012, 'N');
            if (_config.dbfMainFields.Contains("USER"))
                fields.Add("USER", 050, 'C');

            fields.Add("IS_ROAD", 001, 'L');
            fields.Add("IS_CLOSED", 001, 'L');
            fields.Add("PNTS_COUNT", 007, 'N');
            if (first_and_last)
            {
                fields.Add("PNT_FIRST", 018, 'N');
                fields.Add("PNT_LAST", 018, 'N');
            };

            if (_config.dbfMainFields.Contains("TAGS_COUNT"))
                fields.Add("TAGS_COUNT", 005, 'N');
            if (_config.dbfMainFields.Contains("TAGS_ADDRC"))
                fields.Add("TAGS_ADDRC", 005, 'N');            
            if ((_config.selector == 0) || (_config.selector == 1)) // OSM 2 MP
                fields.Add("SELECTOR", 25, 'C');
            else if ((_config.selector == 5) || (_config.selector == 6) || (_config.selector == 7) || (_config.selector == 8)) // addresses
            {
                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };
            }
            else if (_config.selector == 9) // all with tags
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');
            }
            else if ((_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');

                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };
            }
            else // default
            {
                fields.Add("SELECTOR", 200, 'C');
            };

            if (_config.dbfMainFields.Contains("LABEL"))
                fields.Add("LABEL", 200, 'C');

            if ((_config.selector == 9) || (_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                for (int i = 1; i <= _config.maxAggTags; i++)
                    fields.Add("TAGS_" + i.ToString(), 250, 'C');
            };

            if ((dop_fields != null) && (dop_fields.Length > 0))
                foreach (string str in dop_fields)
                {
                    if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                    {
                        string fn = str.Substring(0, str.IndexOf("["));
                        string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                        fields.Add(fn, byte.Parse(ln), 'C');
                    }
                    else
                        fields.Add(str, 250, 'C');
                };

            // Write Header Info
            lines_dbf.WriteHeader(fields);            
        }

        private void WriteAreasHeaders(string[] dop_fields)
        {
            // Fields
            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX", 012, 'N');
            fields.Add("AREA_ID", 018, 'N');
            fields.Add("CRTD_FROM", 001, 'C');
            if (_config.dbfMainFields.Contains("VERSION"))
                fields.Add("VERSION", 012, 'N');
            if (_config.dbfMainFields.Contains("TIMESTAMP"))
                fields.Add("TIMESTAMP", 018, 'N');
            if (_config.dbfMainFields.Contains("CHANGESET"))
                fields.Add("CHANGESET", 018, 'N');
            if (_config.dbfMainFields.Contains("UID"))
                fields.Add("UID", 012, 'N');
            if (_config.dbfMainFields.Contains("USER"))
                fields.Add("USER", 050, 'C');

            fields.Add("PNTS_COUNT", 007, 'N');
            
            if (_config.dbfMainFields.Contains("TAGS_COUNT"))
                fields.Add("TAGS_COUNT", 005, 'N');
            if (_config.dbfMainFields.Contains("TAGS_ADDRC"))
                fields.Add("TAGS_ADDRC", 005, 'N');
            if ((_config.selector == 0) || (_config.selector == 1)) // OSM 2 MP
                fields.Add("SELECTOR", 25, 'C');
            else if ((_config.selector == 5) || (_config.selector == 6) || (_config.selector == 7) || (_config.selector == 8)) // addresses
            {
                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };
            }
            else if (_config.selector == 9) // all with tags
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');
            }
            else if ((_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');

                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };
            }
            else // default
            {
                fields.Add("SELECTOR", 200, 'C');
            };

            if (_config.dbfMainFields.Contains("LABEL"))
                fields.Add("LABEL", 200, 'C');

            if ((_config.selector == 9) || (_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                for (int i = 1; i <= _config.maxAggTags; i++)
                    fields.Add("TAGS_" + i.ToString(), 250, 'C');
            };

            if ((dop_fields != null) && (dop_fields.Length > 0))
                foreach (string str in dop_fields)
                {
                    if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                    {
                        string fn = str.Substring(0, str.IndexOf("["));
                        string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                        fields.Add(fn, byte.Parse(ln), 'C');
                    }
                    else
                        fields.Add(str, 250, 'C');
                };

            // Write Header Info
            areas_dbf.WriteHeader(fields);            
        }

        private void WriteRelationsHeaders(string[] dop_fields)
        {
            // Fields
            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX", 012, 'N');
            fields.Add("REL_ID", 018, 'N');
            if (_config.dbfMainFields.Contains("VERSION"))
                fields.Add("VERSION", 012, 'N');
            if (_config.dbfMainFields.Contains("TIMESTAMP"))
                fields.Add("TIMESTAMP", 018, 'N');
            if (_config.dbfMainFields.Contains("CHANGESET"))
                fields.Add("CHANGESET", 018, 'N');
            if (_config.dbfMainFields.Contains("UID"))
                fields.Add("UID", 012, 'N');
            if (_config.dbfMainFields.Contains("USER"))
                fields.Add("USER", 050, 'C');

            fields.Add("REL_TYPE", 050, 'C');
            fields.Add("MEMS_COUNT", 007, 'N');

            if (_config.dbfMainFields.Contains("TAGS_COUNT"))
                fields.Add("TAGS_COUNT", 005, 'N');
            if (_config.dbfMainFields.Contains("TAGS_ADDRC"))
                fields.Add("TAGS_ADDRC", 005, 'N');
            if ((_config.selector == 0) || (_config.selector == 1)) // OSM 2 MP
                fields.Add("SELECTOR", 25, 'C');
            else if ((_config.selector == 5) || (_config.selector == 6) || (_config.selector == 7) || (_config.selector == 8)) // addresses
            {
                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };
            }
            else if (_config.selector == 9) // all with tags
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');
            }
            else if ((_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                fields.Add("TAGS_F_USED", 3, 'N');
                fields.Add("TAGS_A_CNT", 5, 'N');
                fields.Add("TAGS_LENGTH", 7, 'N');

                if (_config.addrFields.Count > 0)
                    foreach (string str in _config.addrFields)
                    {
                        if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                        {
                            string fn = str.Substring(0, str.IndexOf("["));
                            string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                            fields.Add(fn, byte.Parse(ln), 'C');
                        }
                        else
                            fields.Add(str, 200, 'C');
                    };
            }
            else // default
            {
                fields.Add("SELECTOR", 200, 'C');
            };

            if (_config.dbfMainFields.Contains("LABEL"))
                fields.Add("LABEL", 200, 'C');

            if ((_config.selector == 9) || (_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
            {
                for (int i = 1; i <= _config.maxAggTags; i++)
                    fields.Add("TAGS_" + i.ToString(), 250, 'C');
            };

            if ((dop_fields != null) && (dop_fields.Length > 0))
                foreach (string str in dop_fields)
                {
                    if ((str.IndexOf("[") > 0) && (str.IndexOf("]") > 0))
                    {
                        string fn = str.Substring(0, str.IndexOf("["));
                        string ln = str.Substring(str.IndexOf("[") + 1, str.IndexOf("]") - str.IndexOf("[") - 1);
                        fields.Add(fn, byte.Parse(ln), 'C');
                    }
                    else
                        fields.Add(str, 250, 'C');
                };

            // Write Header Info
            relat_dbf.WriteHeader(fields);            
        }

        private void WriteRelMemsHeaders(string[] dop_fields)
        {
            // Fields
            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX",  012, 'N');
            fields.Add("REL_ID", 018, 'N');
            fields.Add("MEM_ID", 018, 'N');
            fields.Add("MEM_TYPE",   010, 'C');
            fields.Add("MEM_ROLE",   050, 'C');            

            // Write Header Info
            relam_dbf.WriteHeader(fields);            
        }

        private void WriteJoinsHeaders()
        {
            // Fields
            FieldInfos fields = new FieldInfos();
            fields.Add("INDEX", 012, 'N');
            fields.Add("LINE_ID", 018, 'N');
            fields.Add("R_COUNT", 002, 'N'); // 2
            fields.Add("R_SAVED", 002, 'N'); // 2
            if (_config.addFisrtAndLastNodesLns2Memory)
                fields.Add("R_VALID", 001, 'L'); // 1
            fields.Add("FL", 250, 'C');
            fields.Add("TV", 250, 'C'); // 535 bytes // 4 012 000 records

            relaj_dbf.WriteHeader(fields);
        }        

        private void ReadPBF()
        {
            OSMPBFReader pbfr = new OSMPBFReader(_osmFile);
            if (!pbfr.ValidHeader)
            {
                pbfr.Close();
                throw new Exception("Invalid File Header");
            };            

            while (!pbfr.EndOfFile)
            {
                pbfr.ReadNext();

                ProcessNode(pbfr.Position, pbfr.Length, null, true);

                if (pbfr.HasOSMPrimitiveBlock)
                {
                    double[] bbox = new double[4];
                    if (pbfr.HasOSMHeader)
                        bbox = pbfr.OSMHeaderBlock.GetBBox;
                    if (pbfr.OSMPrimitiveBlock.primitivegroup.Count > 0)
                        for (int z = 0; z < pbfr.OSMPrimitiveBlock.primitivegroup.Count; z++)
                        {
                            if (pbfr.OSMPrimitiveBlock.primitivegroup[z].dense != null)
                            {
                                OSMPBFReader.DenseNodes dns = pbfr.OSMPrimitiveBlock.primitivegroup[z].dense;
                                if ((dns.id != null) && (dns.Count > 0))
                                {
                                    points_total += dns.Count;
                                    points_from_nodes += dns.Count;
                                    KeyValuePair<int, OSMPBFReader.NodeInfo> kvp;
                                    while ((kvp = dns.Next).Key != -1)
                                        ProcessNode(pbfr.Position, pbfr.Length, kvp.Value, true);
                                };
                            };
                            if ((pbfr.OSMPrimitiveBlock.primitivegroup[z].nodes != null) && (pbfr.OSMPrimitiveBlock.primitivegroup[z].nodes.Count > 0))
                            {
                                List<OSMPBFReader.Node> nodes = pbfr.OSMPrimitiveBlock.primitivegroup[z].nodes;
                                points_total += nodes.Count;
                                points_from_nodes += nodes.Count;
                                for (int i = 0; i < nodes.Count; i++)
                                    ProcessNode(pbfr.Position, pbfr.Length, nodes[i].GetNode(pbfr.OSMPrimitiveBlock), true);
                            };
                            if((pbfr.OSMPrimitiveBlock.primitivegroup[z].ways != null) && (pbfr.OSMPrimitiveBlock.primitivegroup[z].ways.Count > 0))
                            {
                                ways_total += pbfr.OSMPrimitiveBlock.primitivegroup[z].ways.Count;
                                if (this.ProcWaysSure)
                                {
                                    for (int i = 0; i < pbfr.OSMPrimitiveBlock.primitivegroup[z].ways.Count; i++)
                                    {
                                        pbfr.OSMPrimitiveBlock.primitivegroup[z].ways[i].tags = pbfr.OSMPrimitiveBlock.primitivegroup[z].ways[i].GetTags(pbfr.OSMPrimitiveBlock);
                                        if (pbfr.OSMPrimitiveBlock.primitivegroup[z].ways[i].info != null)
                                            pbfr.OSMPrimitiveBlock.primitivegroup[z].ways[i].info.user = pbfr.OSMPrimitiveBlock.stringtable.strings[(int)pbfr.OSMPrimitiveBlock.primitivegroup[z].ways[i].info.user_sid];
                                        ProcessWay(pbfr.Position, pbfr.Length, pbfr.OSMPrimitiveBlock.primitivegroup[z].ways[i], true);
                                    };
                                };
                            };
                            if ((pbfr.OSMPrimitiveBlock.primitivegroup[z].relations != null) && (pbfr.OSMPrimitiveBlock.primitivegroup[z].relations.Count > 0))
                            {
                                relations_total += pbfr.OSMPrimitiveBlock.primitivegroup[z].relations.Count;
                                if (this.ProcRelsSure)
                                {
                                    for (int i = 0; i < pbfr.OSMPrimitiveBlock.primitivegroup[z].relations.Count; i++)
                                    {
                                        pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].tags = pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].GetTags(pbfr.OSMPrimitiveBlock);
                                        pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].roles = pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].GetRoles(pbfr.OSMPrimitiveBlock);
                                        if (pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].info != null)
                                            pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].info.user = pbfr.OSMPrimitiveBlock.stringtable.strings[(int)pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i].info.user_sid];
                                        ProcessRelation(pbfr.Position, pbfr.Length, pbfr.OSMPrimitiveBlock.primitivegroup[z].relations[i], true);
                                    };
                                };
                            };
                        };
                };
            };
            ProcessNode(pbfr.Position, pbfr.Length, null, false);
            pbfr.Close();            
        }

        private void ReadXML()
        {
            OSMXMLReader pbfr = new OSMXMLReader(_osmFile);
            if (!pbfr.ValidHeader)
            {
                pbfr.Close();
                throw new Exception ("Invalid File Header");
            };

            ProcessNode(0, pbfr.Length, null, true);
            while (!pbfr.EndOfFile)
            {
                pbfr.ReadNext();
                if (pbfr.NodeInfo != null)
                {
                    points_total++;
                    points_from_nodes++;
                    ProcessNode(pbfr.Position, pbfr.Length, pbfr.NodeInfo, true);
                };
                if(pbfr.Way != null)
                {
                    ways_total++;
                    if (this.ProcWaysSure)
                        ProcessWay(pbfr.Position, pbfr.Length, pbfr.Way, true);
                };
                if (pbfr.Relation != null)
                {
                    relations_total++;
                    if (this.ProcRelsSure)
                        ProcessRelation(pbfr.Position, pbfr.Length, pbfr.Relation, true);
                };
            };
            ProcessNode(pbfr.Position, pbfr.Length, null, false);
            
            pbfr.Close();          
        }

        private void ProcessRelation(long pos, long total, OSMPBFReader.Relation rel, bool processing)
        {
            if (rel != null)
            {
                ProcessRelation(rel);
                relations_procc++;
            };
            Progress((float)pos / (float)total - 0.01f, processing);
        }

        private void ProcessRelation(OSMPBFReader.Relation rel)
        {
            if (rel.tags.ContainsKey("type") && (rel.Refs.Length > 0))
            {
                string typ = rel.tags["type"];
                if((typ == "street") || (typ == "associatedStreet"))
                {

                };
                byte proc = 0;
                if (this.ProcLineSure && (_config.relationTypesAsLine.Count > 0) && (_config.relationTypesAsLine.Contains(typ)))
                    proc += 1;
                if (this.ProcAreaSure && (_config.relationTypesAsArea.Count > 0) && (_config.relationTypesAsArea.Contains(typ)))
                    proc += 2;
                if (proc > 0)
                {
                    List<long> nodes = new List<long>();
                    for (int i = 0; i < rel.Refs.Length; i++)
                        if (rel.types[i] == OSMPBFReader.Relation.MemberType.NODE)
                            nodes.Add(rel.Refs[i]);
                    if ((((proc & 1) == 0x01) && (nodes.Count > 1)) || (((proc & 2) == 0x02) && (nodes.Count > 2)))
                    {
                        NodesXYIndex.IdLatLon[] vector = null;
                        try { vector = nodes_ndi.GetNodes(nodes.ToArray(), false); } catch { };
                        if (vector != null)
                        {   
                            OSMPBFReader.Way way = new OSMPBFReader.Way();
                            way.id = rel.id;
                            way.info = rel.info;
                            way.tags = rel.tags;
                            way.tags.Add("CRTD_FROM", "R");
                            bool skipped_by_filter = false;
                            ways_by_rels_ttl++;
                            if ((((proc & 1) == 0x01) && (nodes.Count > 1)))
                            {
                                ways_by_rels_lns++;
                                bool isClosed = (vector[vector.Length - 1].id == vector[0].id);
                                bool isRoad = rel.tags.ContainsKey("highway") || rel.tags.ContainsKey("cycleway") || rel.tags.ContainsKey("busway");

                                if (_config.addFisrtAndLastNodesLns2Memory)
                                    if (!linesFLID.ContainsKey(way.id))
                                        linesFLID.Add(way.id, new FLID(vector[0].id, vector[vector.Length - 1].id));

                                if (_config.processLineFilter)
                                {
                                    if (ApplyWayFilters(way, vector))
                                        WriteLine(way, vector, isRoad, isClosed);
                                    else
                                    {
                                        skipped_by_filter = true;
                                        ways_skipd_lines++;
                                    };
                                }
                                else
                                    WriteLine(way, vector, isRoad, isClosed);
                            };
                            if (((proc & 2) == 0x02) && (nodes.Count > 2))
                            {
                                ways_by_rels_ars++;
                                if (_config.processAreaFilter)
                                {
                                    if (ApplyWayFilters(way, vector))
                                        WriteArea(way, vector);
                                    else
                                    {
                                        skipped_by_filter = true;
                                        ways_skipd_areas++;
                                    };
                                }
                                else
                                    WriteArea(way, vector);
                            };
                            if (skipped_by_filter) ways_skipd++;
                        };
                    };                    
                };
            };

            if (_config.processRelationFilter)
            {                
                if (ApplyFilters(rel))
                    WriteRelation(rel);
            }
            else
                WriteRelation(rel);
        }

        private void ProcessWay(long pos, long total, OSMPBFReader.Way way, bool processing)
        {
            if (way != null)
            {
                ProcessWay(way);
                ways_procc++;
            };
            Progress((float)pos / (float)total - 0.01f, processing);
        }

        private void ProcessWay(OSMPBFReader.Way way)
        {
            // Way Types
            const byte justLine = 0;  
            const byte justArea = 1;
            const byte alwsLine = 2; 
            const byte alwsArea = 3; 
            const byte justBoth = 4;
            byte wayType = justLine; 
            bool isClosed  = false;

            if (way.NodeDefs[way.NodeDefs.Length - 1] == way.NodeDefs[0]) // Closed Way (default is area)
            {
                isClosed = true;    // closed way
                wayType = justArea; // area by default
            };

            bool isRoad      = false; // Is Road Tagged
            bool addCentroid = false; // Add Node as Centroid

            // Check Lines
            foreach (KeyValuePair<string, string> tag in way.tags)
            {
                if ((tag.Key == "area") && (tag.Value == "no")) wayType = alwsLine;
                if (wayType == justArea)
                {
                    if ((tag.Key == "leisure") && (tag.Value == "track")) wayType = justLine;
                    if ((tag.Key == "sport") && (tag.Value == "running")) wayType = justLine;
                    if ((tag.Key == "barrier")) wayType = justLine;
                    if ((tag.Key == "aeroway") && (tag.Value == "taxiway")) wayType = justLine;
                };
                if (!isRoad)
                {
                    if (tag.Key == "highway") { wayType = justLine; isRoad = true; };
                    if (tag.Key == "cycleway") { wayType = justLine; isRoad = true; };
                    if (tag.Key == "busway") { wayType = justLine; isRoad = true; };
                };
            };

            // Check Areas
            foreach (KeyValuePair<string, string> tag in way.tags)
            {
                if ((tag.Key == "area") && (tag.Value == "yes")) wayType = alwsArea;                
                
                // Check buildings
                if (tag.Key == "building")
                {
                    if (wayType == justLine) wayType = justArea;
                    if (way.tags.Count > 1) addCentroid = true;
                };

                // Check if Polygon
                if ((wayType == justLine) && (isClosed))
                {
                    // Both
                    if ((tag.Key == "landuse")) wayType = justBoth;
                    if ((tag.Key == "amenity") && (tag.Value == "school")) wayType = justBoth;
                    if ((tag.Key == "amenity") && (tag.Value == "kindergarten")) wayType = justBoth;
                    // Area
                    if ((tag.Key == "leisure")) wayType = justArea;
                    if ((tag.Key == "natural")) wayType = justArea;                    
                    if ((tag.Key == "indoor") && (tag.Value == "corridor")) wayType = justArea;
                    if ((tag.Key == "aeroway") && (tag.Value != "taxiway")) wayType = justArea;
                };                                               
            };

            if ((!addCentroid) && isClosed && (_config.processCentroids == 2) && (way.tags.Count > 1))
                addCentroid = true;

            if ((_config.processCentroids > 0) && (!addCentroid) && (way.tags.Count > 0))
                foreach (KeyValuePair<string, string> kvp in way.tags)
                    if (kvp.Key.StartsWith("addr:housenumber"))
                        addCentroid = true;

            bool isPolygon = (wayType == justArea) || (wayType == alwsArea) || (wayType == justBoth);
            bool isLine = (wayType == justLine) || (wayType == alwsLine) || (wayType == justBoth);
            if ((wayType == justBoth)) ways_as_both++;
            if (isPolygon) ways_as_areas++; 
            if (isLine) ways_as_lines++;

            NodesXYIndex.IdLatLon[] vector = null;

            bool skipped_by_filter = false;
            if (isLine && this.ProcLineSure)
            {
                if (vector == null)
                {
                    try { vector = nodes_ndi.GetNodes(way.NodeDefs, true); }
                    catch { };
                };

                if (vector != null)
                {
                    if (_config.addFisrtAndLastNodesLns2Memory)
                        if (!linesFLID.ContainsKey(way.id))
                            linesFLID.Add(way.id, new FLID(vector[0].id, vector[vector.Length - 1].id));

                    if (_config.processLineFilter)
                    {
                        if (ApplyWayFilters(way, vector))
                            WriteLine(way, vector, isRoad, isClosed);
                        else
                        {
                            skipped_by_filter = true;
                            ways_skipd_lines++;
                        };
                    }
                    else
                        WriteLine(way, vector, isRoad, isClosed);
                };
            };
            if (isPolygon && this.ProcAreaSure)
            {
                if (vector == null)
                {
                    try { vector = nodes_ndi.GetNodes(way.NodeDefs, false); } catch { };
                };

                if (vector != null)
                {
                    if (_config.processAreaFilter)
                    {
                        if (ApplyWayFilters(way, vector))
                            WriteArea(way, vector);
                        else
                        {
                            skipped_by_filter = true;
                            ways_skipd_areas++;
                        };
                    }
                    else
                        WriteArea(way, vector);
                };
            };
            if (skipped_by_filter) ways_skipd++;

            if (addCentroid && (_config.processCentroids > 0)) ///// Process Node as Centroid From Way (Line/Area)
            {
                List<PointF> points = new List<PointF>();
                try
                {
                    if(vector == null)
                        vector = nodes_ndi.GetNodes(way.NodeDefs, false);
                    for (int i = 0; i < vector.Length; i++)
                        points.Add(new PointF((float)vector[i].lon, (float)vector[i].lat));
                }
                catch (Exception ex) { points = null; };

                if (points != null)
                {
                    PointF centroid = GeoUtils.GetCentroid(points, isPolygon, isClosed); // Node From Polygon Centroid
                    OSMPBFReader.NodeInfo ni = new OSMPBFReader.NodeInfo();
                    ni.id = way.id;
                    ni.lat = centroid.Y;
                    ni.lon = centroid.X;
                    if (way.info != null)
                    {
                        ni.version = way.info.version;
                        ni.timestamp = way.info.timestamp;
                        ni.changeset = way.info.changeset;
                        ni.uid = way.info.uid;
                        ni.user = way.info.user;
                    };
                    ni.tags = way.tags;
                    if (isPolygon)
                    {
                        ni.tags.Add("CRTD_FROM", "A");
                        points_from_areas++;
                    }
                    else
                    {
                        ni.tags.Add("CRTD_FROM", "L");
                        points_form_lines++;
                    };
                    points_from_ways++;

                    points_total++;
                    ProcessNode(ni);
                    points_procc++;
                };
            }; ///// Node From Way
        }

        private TagsComparer tagsComparer = null;
        public IEnumerable<KeyValuePair<string, string>> SortTags(Dictionary<string, string> tags, TagsComparer tagsComparer)
        {
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> kvp in tags) vals.Add(kvp);
            if (tagsComparer != null) vals.Sort(tagsComparer);
            return vals;
        }

        public class TagsComparer : IComparer<KeyValuePair<string, string>>
        {
            private List<string> priority = new List<string>();
            public TagsComparer(string priority)
            {
                if (String.IsNullOrEmpty(priority)) return;
                if (priority == "да") return;
                string[] ss = priority.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if ((ss != null) && (ss.Length > 0))
                    for (int i = 0; i < ss.Length; i++) 
                        this.priority.Add(ss[i].Trim().ToLower());
            }

            public int Compare(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
            {
                string fal = String.IsNullOrEmpty(a.Key) ? "" : a.Key.ToLower();
                string fbl = String.IsNullOrEmpty(b.Key) ? "" : b.Key.ToLower();
                if ((this.priority.Count) > 0 && (fal != "") && (fbl != ""))
                {                    
                    int ia = 9999;
                    int ib = 9999;
                    for (int i = this.priority.Count - 1; i >= 0; i--)
                    {
                        if (this.priority[i].Contains(":*"))
                        {
                            if (fal.StartsWith(this.priority[i].Split(new char[] { ';' }, StringSplitOptions.None)[0])) ia = i;
                        }
                        else if (fal == this.priority[i])  ia = i;
                        if (this.priority[i].Contains(":*"))
                        {
                            if (fbl.StartsWith(this.priority[i].Split(new char[] { ';' }, StringSplitOptions.None)[0])) ib = i;
                        }
                        else if (fbl == this.priority[i]) ib = i;
                    };                     
                    fal = ia.ToString("0000") + fal;
                    fbl = ib.ToString("0000") + fbl;
                };
                return fal.CompareTo(fbl);
            }         
        }

        private void CheckInfo(ref OSMPBFReader.Info info)
        {
            if (info != null) return;
            info = new OSMPBFReader.Info();
            info.version = 0;
            info.timestamp = 0;
            info.changeset = 0;
            info.uid = 0;
            info.user = "";
            info.user_sid = 0;
            info.visible = true;
        }


        private void WriteLine(OSMPBFReader.Way way, NodesXYIndex.IdLatLon[] nodes, bool isRoad, bool isClosed)
        {
            CheckInfo(ref way.info);

            Dictionary<string, object> rec = new Dictionary<string, object>();
            rec.Add("INDEX", lines_dbf.WritedRecords + 1);
            rec.Add("LINE_ID", way.id);
            rec.Add("VERSION", way.info.version);
            rec.Add("TIMESTAMP", way.info.timestamp);
            rec.Add("CHANGESET", way.info.changeset);
            rec.Add("UID", way.info.uid);
            rec.Add("USER", way.info.user);
            rec.Add("TAGS_COUNT", way.tags.Count);
            rec.Add("IS_ROAD", isRoad);
            rec.Add("IS_CLOSED", isClosed);
            rec.Add("PNTS_COUNT", nodes.Length);                        
            rec.Add("PNT_FIRST", nodes[0].id);
            rec.Add("PNT_LAST", nodes[nodes.Length - 1].id);

            if (!way.tags.ContainsKey("CRTD_FROM")) rec.Add("CRTD_FROM", "W");

            string wName = "";
            foreach (KeyValuePair<string, string> kvp in way.tags)
                if (kvp.Key == "name")
                    wName = kvp.Value;

            int wAddrTagsCount = 0;
            foreach (KeyValuePair<string, string> kvp in way.tags)
                if (kvp.Key.IndexOf("addr:street") == 0)
                    wAddrTagsCount++;
                    
            rec.Add("TAGS_ADDRC", wAddrTagsCount); 
            rec.Add("LABEL", wName);

            string[] tags = new string[_config.maxAggTags + 1];
            for (int i = 0; i < tags.Length; i++) tags[i] = "";
            int tag_i = 1;
            int tags_l = 0;
            int tags_a = 0;
            if (way.tags.Count > 0)
                tags[1] = _config.useAggPrefix;
            
            IEnumerable<KeyValuePair<string, string>> sorted = (tagsComparer == null ? way.tags : SortTags(way.tags, tagsComparer));
            foreach (KeyValuePair<string, string> tag in sorted)
            {
                rec.Add(tag.Key, tag.Value);

                if (tag.Key == "CRTD_FROM") continue;
                if (AggrRegex != null)
                    if (!AggrRegex.IsMatch(tag.Key))
                    {
                        if (tags_skipped.ContainsKey(tag.Key))
                            tags_skipped[tag.Key]++;
                        else
                            tags_skipped.Add(tag.Key, 1);
                        continue;
                    };
                
                string tnv = _config.allTagsFormat.Replace("{k}", tag.Key).Replace("{v}", tag.Value);
                tags_l += tnv.Length;

                if (tag_i > _config.maxAggTags) continue;

                if ((tags[tag_i].Length + tnv.Length) > 250)
                {
                    tag_i++;
                    if (tag_i > _config.maxAggTags)
                        continue;
                    else
                    {
                        tags[tag_i] += _config.useAggPrefix + tnv;
                        tags_a++;
                    };
                }
                else
                {
                    tags[tag_i] += tnv;
                    tags_a++;
                };
            };

            sbyte tags_f_used = tag_i > _config.maxAggTags ? _config.maxAggTags : (sbyte)tag_i;
            rec.Add("TAGS_F_USED", tags_f_used);
            rec.Add("TAGS_A_CNT", tags_a);
            rec.Add("TAGS_LENGTH", tags_l);
            for (int i = 1; i <= _config.maxAggTags; i++)
                rec.Add("TAGS_" + i.ToString(), tags[i]);

            UpdateTagsCounters(tags_f_used, way.tags.Count, tags_a, tags_l);
            lines_dbf.WriteRecord(rec, tags_f_used, way.tags.Count, tags_a, tags_l);
            lines_shx.WriteLineIndex((int)lines_shp.Position, 48 + nodes.Length * 2 * 8);
            lines_shp.WriteSingleLine(nodes);
            files_lines_wrtc[files_lines_wrtc.Count - 1]++;
            files_lines_sizes[files_lines_wrtc.Count - 1] = lines_dbf.Length + lines_shx.Length + lines_shp.Length + 204;
            CheckFilesFull();

            ways_writd++;
            ways_writd_lines++;
        }

        private void WriteArea(OSMPBFReader.Way area, NodesXYIndex.IdLatLon[] nodes)
        {
            CheckInfo(ref area.info);

            Dictionary<string, object> rec = new Dictionary<string, object>();
            rec.Add("INDEX", areas_dbf.WritedRecords + 1);
            rec.Add("AREA_ID", area.id);
            rec.Add("VERSION", area.info.version);
            rec.Add("TIMESTAMP", area.info.timestamp);
            rec.Add("CHANGESET", area.info.changeset);
            rec.Add("UID", area.info.uid);
            rec.Add("USER", area.info.user);
            rec.Add("TAGS_COUNT", area.tags.Count);
            rec.Add("PNTS_COUNT", nodes.Length);
            // rec.Add("PNT_FIRST", nodes[0].id);
            // rec.Add("PNT_LAST", nodes[nodes.Length - 1].id);

            if (!area.tags.ContainsKey("CRTD_FROM")) rec.Add("CRTD_FROM", "W");

            string wName = "";
            foreach (KeyValuePair<string, string> kvp in area.tags)
                if (kvp.Key == "name")
                    wName = kvp.Value;

            int wAddrTagsCount = 0;
            foreach (KeyValuePair<string, string> kvp in area.tags)
                if (kvp.Key.IndexOf("addr:street") == 0)
                    wAddrTagsCount++;

            rec.Add("TAGS_ADDRC", wAddrTagsCount);
            rec.Add("LABEL", wName);

            string[] tags = new string[_config.maxAggTags + 1];
            for (int i = 0; i < tags.Length; i++) tags[i] = "";
            int tag_i = 1;
            int tags_l = 0;
            int tags_a = 0;
            if (area.tags.Count > 0)
                tags[1] = _config.useAggPrefix;
            IEnumerable<KeyValuePair<string, string>> sorted = (tagsComparer == null ? area.tags : SortTags(area.tags, tagsComparer));
            foreach (KeyValuePair<string, string> tag in sorted)
            {
                rec.Add(tag.Key, tag.Value);

                if (tag.Key == "CRTD_FROM") continue;
                if (AggrRegex != null)
                    if (!AggrRegex.IsMatch(tag.Key))
                    {
                        if (tags_skipped.ContainsKey(tag.Key))
                            tags_skipped[tag.Key]++;
                        else
                            tags_skipped.Add(tag.Key, 1);
                        continue;
                    };

                string tnv = _config.allTagsFormat.Replace("{k}", tag.Key).Replace("{v}", tag.Value);
                tags_l += tnv.Length;

                if (tag_i > _config.maxAggTags) continue;

                if ((tags[tag_i].Length + tnv.Length) > 250)
                {
                    tag_i++;
                    if (tag_i > _config.maxAggTags)
                        continue;
                    else
                    {
                        tags[tag_i] += _config.useAggPrefix + tnv;
                        tags_a++;
                    };
                }
                else
                {
                    tags[tag_i] += tnv;
                    tags_a++;
                };
            };

            sbyte tags_f_used = tag_i > _config.maxAggTags ? _config.maxAggTags : (sbyte)tag_i;
            rec.Add("TAGS_F_USED", tags_f_used);
            rec.Add("TAGS_A_CNT", tags_a);
            rec.Add("TAGS_LENGTH", tags_l);
            for (int i = 1; i <= _config.maxAggTags; i++)
                rec.Add("TAGS_" + i.ToString(), tags[i]);


            UpdateTagsCounters(tags_f_used, area.tags.Count, tags_a, tags_l);
            areas_dbf.WriteRecord(rec, tags_f_used, area.tags.Count, tags_a, tags_l);
            areas_shx.WriteAreaIndex((int)areas_shp.Position, 48 + nodes.Length * 2 * 8);
            areas_shp.WriteSingleArea(nodes);
            files_areas_wrtc[files_areas_wrtc.Count - 1]++;
            files_areas_sizes[files_areas_wrtc.Count - 1] = areas_dbf.Length + areas_shx.Length + areas_shp.Length + 204;
            CheckFilesFull();

            ways_writd++;
            ways_writd_areas++;
        }

        private void WriteRelation(OSMPBFReader.Relation rel)
        {
            {
                CheckInfo(ref rel.info);

                Dictionary<string, object> rec = new Dictionary<string, object>();
                rec.Add("INDEX", relat_dbf.WritedRecords + 1);
                rec.Add("REL_ID", rel.id);
                rec.Add("VERSION", rel.info.version);
                rec.Add("TIMESTAMP", rel.info.timestamp);
                rec.Add("CHANGESET", rel.info.changeset);
                rec.Add("UID", rel.info.uid);
                rec.Add("USER", rel.info.user);
                rec.Add("REL_TYPE", rel.tags.ContainsKey("type") ? rel.tags["type"] : "");
                rec.Add("MEMS_COUNT", rel.Refs.Length);
                rec.Add("TAGS_COUNT", rel.tags.Count);

                string wName = "";
                foreach (KeyValuePair<string, string> kvp in rel.tags)
                    if (kvp.Key == "name")
                        wName = kvp.Value;

                int wAddrTagsCount = 0;
                foreach (KeyValuePair<string, string> kvp in rel.tags)
                    if (kvp.Key.IndexOf("addr:street") == 0)
                        wAddrTagsCount++;

                rec.Add("TAGS_ADDRC", wAddrTagsCount);
                rec.Add("LABEL", wName);

                string[] tags = new string[_config.maxAggTags + 1];
                for (int i = 0; i < tags.Length; i++) tags[i] = "";
                int tag_i = 1;
                int tags_l = 0;
                int tags_a = 0;
                if (rel.tags.Count > 0)
                    tags[1] = _config.useAggPrefix;
                IEnumerable<KeyValuePair<string, string>> sorted = (tagsComparer == null ? rel.tags : SortTags(rel.tags, tagsComparer));
                foreach (KeyValuePair<string, string> tag in sorted)
                {
                    rec.Add(tag.Key, tag.Value);

                    if (tag.Key == "CRTD_FROM") continue;
                    if (AggrRegex != null)
                        if (!AggrRegex.IsMatch(tag.Key))
                        {
                            if (tags_skipped.ContainsKey(tag.Key))
                                tags_skipped[tag.Key]++;
                            else
                                tags_skipped.Add(tag.Key, 1);
                            continue;
                        };

                    string tnv = _config.allTagsFormat.Replace("{k}", tag.Key).Replace("{v}", tag.Value);
                    tags_l += tnv.Length;

                    if (tag_i > _config.maxAggTags) continue;

                    if ((tags[tag_i].Length + tnv.Length) > 250)
                    {
                        tag_i++;
                        if (tag_i > _config.maxAggTags)
                            continue;
                        else
                        {
                            tags[tag_i] += _config.useAggPrefix + tnv;
                            tags_a++;
                        };
                    }
                    else
                    {
                        tags[tag_i] += tnv;
                        tags_a++;
                    };
                };

                sbyte tags_f_used = tag_i > _config.maxAggTags ? _config.maxAggTags : (sbyte)tag_i;
                rec.Add("TAGS_F_USED", tags_f_used);
                rec.Add("TAGS_A_CNT", tags_a);
                rec.Add("TAGS_LENGTH", tags_l);
                for (int i = 1; i <= _config.maxAggTags; i++)
                    rec.Add("TAGS_" + i.ToString(), tags[i]);


                UpdateTagsCounters(tags_f_used, rel.tags.Count, tags_a, tags_l);
                relat_dbf.WriteRecord(rec, tags_f_used, rel.tags.Count, tags_a, tags_l);
                files_relations_wrt_rc[files_relations_wrt_rc.Count - 1]++;
            };

            for (int i = 0; i < rel.Refs.Length; i++)
            {
                Dictionary<string, object> rec = new Dictionary<string, object>();
                rec.Add("INDEX", relam_dbf.WritedRecords + 1);
                rec.Add("REL_ID", rel.id);
                rec.Add("MEM_ID", rel.Refs[i]);
                rec.Add("MEM_TYPE", rel.types[i].ToString().ToLower());
                rec.Add("MEM_ROLE", rel.roles[i]);
                relam_dbf.WriteRecord(rec, 0, 0, 0 ,0);
                files_relations_wrt_mc[files_relations_wrt_mc.Count - 1]++;
                relations_mwrtd++;                
            };            
            files_relations_sizes[files_relations_wrt_rc.Count - 1] = relat_dbf.Length + relam_dbf.Length;            
           
            relations_writd++;

            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////// ОБРАБОТКА ЗАПРЕТОВ ПОВОРОТОВ ////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            if (this.ProcJoins)
            {
                //
                // #1 restriction: from `way` via `*` to `way`
                //
                if (rel.tags.ContainsKey("type") && (rel.tags["type"] == "restriction"))
                {
                    long rf = 0, rv = 0, rt = 0;
                    for (int i = 0; i < rel.Refs.Length; i++)
                    {
                        if ((rel.roles[i] == "from") && (rel.types[i] == OSMPBFReader.Relation.MemberType.WAY)) rf = rel.Refs[i];
                        if (rel.roles[i] == "via") rv = rel.Refs[i];
                        if ((rel.roles[i] == "to") && (rel.types[i] == OSMPBFReader.Relation.MemberType.WAY)) rt = rel.Refs[i];
                    };
                    if ((rf > 0) && (rv > 0) && (rt > 0))
                        WriteJoin(rf, rv, rt);
                };
                // // //
                files_joins_sizes[files_joins_sizes.Count - 1] = relaj_dbf.Length;
            };
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////

            CheckFilesFull();
        }

        private void WriteJoin(long fline, long via, long tline)
        {
            Dictionary<string, object> rec = new Dictionary<string, object>();
            rec.Add("INDEX", files_joins_wrtl[files_joins_wrtl.Count - 1] + 1);
            rec.Add("LINE_ID", fline);
            rec.Add("R_COUNT", 1);            
            rec.Add("TV", "T" + tline.ToString() + "V" + via.ToString());
            bool valid = false;
            int toFLAdd = 0;
            if (_config.addFisrtAndLastNodesLns2Memory && linesFLID.ContainsKey(fline))
            {
                if (linesFLID[fline].f == via)
                {
                    rec.Add("FL", "F" + tline.ToString());
                    toFLAdd = 1;
                    valid = true;
                };
                if (linesFLID[fline].l == via)
                {
                    rec.Add("FL", "L" + tline.ToString());
                    toFLAdd = 1;
                    valid = true;
                };
                if (toFLAdd == 0)
                {
                    valid = false;
                };
            }
            else
            {
                valid = false;
                toFLAdd = 2;
                rec.Add("FL", "F" + tline.ToString() + ";L" + tline.ToString());
            };            
            rec.Add("R_SAVED", toFLAdd);
            rec.Add("R_VALID", valid);

            if (relaJJ.ContainsKey(fline)) // records with line_id already exists
            {
                bool tvadd = false;
                int added = relaj_dbf.UpdateJoinRecord(rec, relaJJ[fline], out tvadd);

                relations_joins_FL += added;
                files_joins_wrtFL[files_joins_wrtFL.Count - 1] += added;

                if (!tvadd)
                    relations_joins_COPY++;
                else
                {
                    relations_joins_TV++;
                    files_joins_wrtTV[files_joins_wrtTV.Count - 1]++;            

                    if (valid)
                    {
                        relations_joins_VLD++;
                    }
                    else
                    {
                        files_joins_wrtER[files_joins_wrtER.Count - 1]++;
                        relations_joins_ERR++;                        
                    };
                };                
            }
            else
            {
                relaJJ[fline] = relaj_dbf.WriteRecord(rec, 0, 0, 0, 0);

                relations_joins_FL += toFLAdd;
                files_joins_wrtFL[files_joins_wrtFL.Count - 1] += toFLAdd;

                relations_joins_TV++;
                files_joins_wrtTV[files_joins_wrtTV.Count - 1]++;

                relations_joins_LNS++;
                files_joins_wrtl[files_joins_wrtl.Count - 1]++;

                if (valid)
                    relations_joins_VLD++;
                else
                {
                    files_joins_wrtER[files_joins_wrtER.Count - 1]++;
                    relations_joins_ERR++;
                };
            };           
        }

        private void UpdateTagsCounters(int aggr_fields_used, int count, int aggr_tags_count, int string_len)
        {
            if (count > tags_max_count) tags_max_count = count;
            if (aggr_fields_used > tags_aggrf_used) tags_aggrf_used = aggr_fields_used;
            if (aggr_tags_count > tags_aggra_count) tags_aggra_count = aggr_tags_count;
            if (string_len > tags_string_maxl) tags_string_maxl = string_len;
        }

        private void ProcessNode(long pos, long total, OSMPBFReader.NodeInfo ni, bool processing)
        {
            if (ni != null)
            {
                if (this.ProcWaysSure)
                    nodes_ndi.WriteNode(ni.id, ni.lat, ni.lon);
                ProcessNode(ni);
                points_procc++;
            };           
            Progress((float)pos / (float)total - 0.01f, processing);
        }

        private void ProcessNode(OSMPBFReader.NodeInfo ni)
        {
            if (ni.tags.Count == 0)
            {
                points_empty++;
                return;
            };

            if (!ApplyFilters(ni))
            {
                points_skb_flt++;
                return;
            };

            int was_added = 0;

            if ((_config.selector == 0) || (_config.selector == 1) || (_config.selector == 3) || (_config.selector == 4))
            {
                if (jsd.records.Count > 0)
                    was_added += ProcessNode01(ni);
            };

            if ((_config.selector == 2) || ((was_added == 0) && ((_config.selector == 3) || (_config.selector == 4))))
                if ((_config.osmCat.records != null) && (_config.osmCat.Count > 0))
                    was_added += ProcessNode2(ni);

            if ((_config.selector == 5) || (_config.selector == 6) || (_config.selector == 7) || (_config.selector == 8))
            {
                if(_config.addrFields.Count > 0)
                    was_added += ProcessNode5678(ni);
            };

            if ((_config.selector == 9) || (_config.selector == 10) || (_config.selector == 11) || (_config.selector == 12) || (_config.selector == 13))
                was_added += ProcessNode9_13(ni);

            if (_config.selector == 14)
            {
                if((tmap != null) && (tmap.IsValid))
                    was_added += ProcessNode14(ni);
            };

            if (was_added > 0)
                points_unicl++;
            else
                points_skb_sel++;
        }

        private int ProcessNode01(OSMPBFReader.NodeInfo ni)
        {
            int was_added = 0;

            foreach (JSONDictElement el in jsd.records)
            {
                if (el.ProcessNode(ni.tags))
                {
                    foreach (JSONDictElement.EAction ea in el.actions)
                    {
                        if (ea.name == "write_poi")
                        {
                            long typi = ea.GetTypeL(ni.tags);
                            string typs = String.Format("0x{0:X4}", typi);
                            string nam = ea.GetName(ni.tags, "ru");

                            Dictionary<string, object> rec = new Dictionary<string, object>();
                            rec.Add("INDEX", points_dbf.WritedRecords + 1);
                            rec.Add("NODE_ID", ni.id);
                            rec.Add("VERSION", ni.version);
                            rec.Add("TIMESTAMP", ni.timestamp);
                            rec.Add("CHANGESET", ni.changeset);
                            rec.Add("UID", ni.uid);
                            rec.Add("USER", ni.user);
                            rec.Add("TAGS_COUNT", ni.tags.Count);
                            rec.Add("TAGS_ADDRC", ni.AddrTagsCount);
                            rec.Add("LABEL", nam == "NoName" ? null : nam);                            
                            rec.Add("TYPES", typs);
                            rec.Add("TYPEI", typi);
                            rec.Add("SELECTOR", Config.SELECTOR[_config.selector]);

                            foreach (KeyValuePair<string, string> tag in ni.tags)
                                rec.Add(tag.Key, tag.Value);

                            string[] poi_types = new string[0];
                            if((_config.selector == 3) || (_config.selector == 4))
                                poi_types = GetNodePOITypes(ni);
                            if (poi_types.Length == 0)
                            {
                                UpdateTagsCounters(0, ni.tags.Count, 0, 0);
                                points_dbf.WriteRecord(rec, 0, ni.tags.Count, 0, 0);                                
                                points_shx.WritePointIndex((int)points_shp.Position);
                                points_shp.WritePoint(ni.lon, ni.lat);                                
                                points_writd++;
                                files_points_wrtc[files_points_wrtc.Count - 1]++;
                                files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
                                was_added++;
                                CheckFilesFull();
                            }
                            else foreach (string pt in poi_types)
                                {
                                    rec["SELECTOR"] = pt;
                                    UpdateTagsCounters(0, ni.tags.Count, 0, 0);
                                    points_dbf.WriteRecord(rec, 0, ni.tags.Count, 0, 0);
                                    points_shx.WritePointIndex((int)points_shp.Position);
                                    points_shp.WritePoint(ni.lon, ni.lat);                                    
                                    points_writd++;
                                    files_points_wrtc[files_points_wrtc.Count - 1]++;
                                    files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
                                    was_added++;
                                    CheckFilesFull();
                                };
                        };
                    };
                };
            };
            return was_added;
        }

        private int ProcessNode2(OSMPBFReader.NodeInfo ni)
        {
            int was_added = 0;
            string[] selectors = GetNodePOITypes(ni);
            if(selectors.Length > 0)
                foreach (string sel in selectors)
                {
                    WriteNodeWithSelector(ni, sel);
                    was_added++;
                };
            return was_added;
        }

        private int ProcessNode5678(OSMPBFReader.NodeInfo ni)
        {
            int add_fields_count = ni.AddrTagsCount;
            if (add_fields_count > 0)
            {
                Dictionary<string, object> rec = new Dictionary<string, object>();
                rec.Add("INDEX", points_dbf.WritedRecords + 1);
                rec.Add("NODE_ID", ni.id);
                rec.Add("VERSION", ni.version);
                rec.Add("TIMESTAMP", ni.timestamp);
                rec.Add("CHANGESET", ni.changeset);
                rec.Add("UID", ni.uid);
                rec.Add("USER", ni.user);
                rec.Add("TAGS_COUNT", ni.tags.Count);
                rec.Add("TAGS_ADDRC", add_fields_count);
                rec.Add("LABEL", ni.Name);                

                foreach (KeyValuePair<string, string> tag in ni.tags)
                    rec.Add(tag.Key, tag.Value);

                UpdateTagsCounters(0, ni.tags.Count, 0 ,0);
                points_dbf.WriteRecord(rec, 0, ni.tags.Count, 0, 0);
                points_shx.WritePointIndex((int)points_shp.Position);
                points_shp.WritePoint(ni.lon, ni.lat);                
                points_writd++;
                files_points_wrtc[files_points_wrtc.Count - 1]++;
                files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
                CheckFilesFull();
                return 1;
            };
            
            return 0;
        }

        private int ProcessNode9_13(OSMPBFReader.NodeInfo ni)
        {
            Dictionary<string, object> rec = new Dictionary<string, object>();
            rec.Add("INDEX", points_dbf.WritedRecords + 1);
            rec.Add("NODE_ID", ni.id);
            rec.Add("VERSION", ni.version);
            rec.Add("TIMESTAMP", ni.timestamp);
            rec.Add("CHANGESET", ni.changeset);
            rec.Add("UID", ni.uid);
            rec.Add("USER", ni.user);
            rec.Add("TAGS_COUNT", ni.tags.Count);
            rec.Add("TAGS_ADDRC", ni.AddrTagsCount);
            rec.Add("LABEL", ni.Name);

            string[] tags = new string[_config.maxAggTags + 1];
            for (int i = 0; i < tags.Length; i++) tags[i] = "";
            int tag_i = 1;
            int tags_l = 0;
            int tags_a = 0;
            if (ni.tags.Count > 0)
                tags[1] = _config.useAggPrefix;
            IEnumerable<KeyValuePair<string, string>> sorted = (tagsComparer == null ? ni.tags : SortTags(ni.tags, tagsComparer));
            foreach (KeyValuePair<string, string> tag in sorted)
            {
                rec.Add(tag.Key, tag.Value);

                if (tag.Key == "CRTD_FROM") continue;
                if (AggrRegex != null)
                    if (!AggrRegex.IsMatch(tag.Key))
                    {
                        if (tags_skipped.ContainsKey(tag.Key))
                            tags_skipped[tag.Key]++;
                        else
                            tags_skipped.Add(tag.Key, 1);
                        continue;
                    };
                
                string tnv = _config.allTagsFormat.Replace("{k}", tag.Key).Replace("{v}", tag.Value);
                tags_l += tnv.Length;

                if (tag_i > _config.maxAggTags) continue;

                if ((tags[tag_i].Length + tnv.Length) > 250)
                {
                    tag_i++;
                    if (tag_i > _config.maxAggTags)
                        continue;
                    else
                    {
                        tags[tag_i] += _config.useAggPrefix + tnv;
                        tags_a++;
                    };
                }
                else
                {
                    tags[tag_i] += tnv;
                    tags_a++;
                };
            };
            sbyte tags_f_used = tag_i > _config.maxAggTags ? _config.maxAggTags : (sbyte)tag_i;

            rec.Add("TAGS_F_USED", tags_f_used);
            rec.Add("TAGS_A_CNT", tags_a);
            rec.Add("TAGS_LENGTH", tags_l);
            for (int i = 1; i <= _config.maxAggTags; i++)
                rec.Add("TAGS_" + i.ToString(), tags[i]);


            UpdateTagsCounters(tags_f_used, ni.tags.Count, tags_a, tags_l);
            points_dbf.WriteRecord(rec, tags_f_used, ni.tags.Count, tags_a, tags_l);            
            points_shx.WritePointIndex((int)points_shp.Position);
            points_shp.WritePoint(ni.lon, ni.lat);            
            points_writd++;
            files_points_wrtc[files_points_wrtc.Count - 1]++;
            files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
            CheckFilesFull();

            return 1;
        }

        private int ProcessNode14(OSMPBFReader.NodeInfo ni)
        {
            int was_added = 0;

            TYPE_MAP_Element[] els = tmap.ProcessNode(ni);
            foreach (TYPE_MAP_Element el in els)
            {
                long typi = el.GARMIN_TYPE;
                string typs = String.Format("0x{0:X4}", typi);
                string nam = el.GetName(ni);

                Dictionary<string, object> rec = new Dictionary<string, object>();
                rec.Add("INDEX", points_dbf.WritedRecords + 1);
                rec.Add("NODE_ID", ni.id);
                rec.Add("VERSION", ni.version);
                rec.Add("TIMESTAMP", ni.timestamp);
                rec.Add("CHANGESET", ni.changeset);
                rec.Add("UID", ni.uid);
                rec.Add("USER", ni.user);
                rec.Add("TAGS_COUNT", ni.tags.Count);
                rec.Add("TAGS_ADDRC", ni.AddrTagsCount);
                rec.Add("LABEL", nam == "NoName" ? null : nam);
                rec.Add("TYPES", typs);
                rec.Add("TYPEI", typi);
                rec.Add("SELECTOR", Config.SELECTOR[_config.selector]);

                foreach (KeyValuePair<string, string> tag in ni.tags)
                    rec.Add(tag.Key, tag.Value);

                UpdateTagsCounters(0, ni.tags.Count, 0, 0);
                points_dbf.WriteRecord(rec, 0, ni.tags.Count, 0, 0);
                points_shx.WritePointIndex((int)points_shp.Position);
                points_shp.WritePoint(ni.lon, ni.lat);
                points_writd++;
                files_points_wrtc[files_points_wrtc.Count - 1]++;
                files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
                was_added++;
                CheckFilesFull();
            };
            
            return was_added;
        }

        private string[] GetNodePOITypes(OSMPBFReader.NodeInfo ni)
        {
            OSMCatalogHierarchyList in_cats_by_one = new OSMCatalogHierarchyList();
            OSMCatalogHierarchyList in_cats_by_all = new OSMCatalogHierarchyList();
            List<string> no_cats = new List<string>();

            for (int i = 0; i < _config.osmCat.Count; i++)
            {
                if (_config.osmCat[i].tags.Count == 0) continue;

                string all_tags = "";
                int different_tags = _config.osmCat[i].tags.Count;
                foreach (KeyValuePair<string, string> nitag in ni.tags)
                {
                    // FIND CATEGORIES
                    foreach (KeyValuePair<string, string> catag in _config.osmCat[i].tags)
                    {
                        if ((nitag.Key == catag.Key) && (Regex.IsMatch(nitag.Value, @"^([\w\s\:\=]+)$")))
                        {
                            string no_in_cats = String.Format("{0}:{1}", nitag.Key, nitag.Value);
                            if (no_cats.IndexOf(no_in_cats) < 0) no_cats.Add(no_in_cats);
                        };
                        if ((nitag.Key == catag.Key) && (nitag.Value == catag.Value))
                        {
                            if (all_tags.Length > 0) all_tags += ",";
                            all_tags += String.Format("{0}:{1}", nitag.Key, nitag.Value);
                            different_tags--;
                        };
                    };                    
                };
                if (different_tags < _config.osmCat[i].tags.Count)
                {
                    if (different_tags > 0)
                    {
                        if (in_cats_by_one.IndexOf(all_tags) < 0)
                            in_cats_by_one.Add(new OSMCatalogHierarchy(_config.osmCat[i].id, all_tags));
                    }
                    else
                    {
                        if (in_cats_by_all.IndexOf(all_tags) < 0)
                            in_cats_by_all.Add(new OSMCatalogHierarchy(_config.osmCat[i].id, all_tags));
                    };
                };
            }; // END foreach osmCat
            
            if (in_cats_by_all.Count > 0)
                foreach (OSMCatalogHierarchy hel in in_cats_by_all) if (in_cats_by_one.IndexOf(hel.id) < 0) in_cats_by_one.Add(hel);

            if (in_cats_by_one.Count == 0)
                return no_cats.ToArray();
            else
            {
                List<string> types = new List<string>();
                foreach (OSMCatalogHierarchy hel in in_cats_by_one)
                    types.Add(hel.name);
                return types.ToArray();
            };            
        }

        private void WriteNodeWithSelector(OSMPBFReader.NodeInfo ni, string selector)
        {
            Dictionary<string, object> rec = new Dictionary<string, object>();
            rec.Add("INDEX", points_dbf.WritedRecords + 1);
            rec.Add("NODE_ID", ni.id);
            rec.Add("VERSION", ni.version);
            rec.Add("TIMESTAMP", ni.timestamp);
            rec.Add("CHANGESET", ni.changeset);
            rec.Add("UID", ni.uid);
            rec.Add("USER", ni.user);
            rec.Add("TAGS_COUNT", ni.tags.Count);
            rec.Add("TAGS_ADDRC", ni.AddrTagsCount);
            rec.Add("LABEL", ni.Name);            
            rec.Add("SELECTOR", selector);

            foreach (KeyValuePair<string, string> tag in ni.tags)
                rec.Add(tag.Key, tag.Value);

            UpdateTagsCounters(0, ni.tags.Count, 0, 0);
            points_dbf.WriteRecord(rec, 0, ni.tags.Count, 0, 0);
            points_shx.WritePointIndex((int)points_shp.Position);
            points_shp.WritePoint(ni.lon, ni.lat);            
            points_writd++;
            files_points_wrtc[files_points_wrtc.Count - 1]++;
            files_points_sizes[files_points_wrtc.Count - 1] = points_dbf.Length + points_shx.Length + points_shp.Length + 204;
            CheckFilesFull();
        }

        private bool ApplyFilters(OSMPBFReader.NodeInfo ni)
        {
            if (_config.onlyHasName && (!ni.HasName)) return false;
            if (_config.onlyWithTags.Count > 0)
            {
                if (ni.tags.Count == 0) return false;
                foreach (KeyValuePair<string, string> _kvp in _config.onlyWithTags)
                {
                    if (!ni.tags.ContainsKey(_kvp.Key)) return false;
                    if ((_kvp.Value != null) && (_kvp.Value != "") && (ni.tags[_kvp.Key] != _kvp.Value)) return false;
                };
            };
            if (_config.onlyOneOfTags.Count > 0)
            {
                if (ni.tags.Count == 0) return false;
                bool skip = true;
                foreach (KeyValuePair<string, string> _kvp in _config.onlyOneOfTags)
                {
                    if (ni.tags.ContainsKey(_kvp.Key))
                    {
                        if ((_kvp.Value == null) || (_kvp.Value == ""))
                            skip = false;
                        else if ((ni.tags[_kvp.Key] == _kvp.Value))
                            skip = false;
                    };
                };
                if (skip)
                    return false;
            };
            if (_config.onlyMdfAfter != DateTime.MinValue) if (ni.datetime < _config.onlyMdfAfter) return false;
            if (_config.onlyMdfBefore != DateTime.MaxValue) if (ni.datetime > _config.onlyMdfBefore) return false;
            if (_config.onlyOfUser.Count > 0)
            {
                bool skip = true;
                foreach (string user in _config.onlyOfUser)
                    if (ni.user == user)
                        skip = false;
                if (skip)
                    return false;
            };
            if (_config.onlyVersion.Count > 0)
            {
                bool skip = true;
                foreach (int ver in _config.onlyVersion)
                    if (ni.version == ver)
                        skip = false;
                if (skip)
                    return false;
            };
            if ((_config.onlyInBox != null) && (_config.onlyInBox.Length == 4))
            {
                if (ni.lat < _config.onlyInBox[0]) return false;
                if (ni.lon < _config.onlyInBox[1]) return false;
                if (ni.lat > _config.onlyInBox[2]) return false;
                if (ni.lon > _config.onlyInBox[3]) return false;
            };
            if ((_config._in_polygon != null) && (_config._in_polygon.Length > 2))
            {
                if (!UTILS.PointInPolygon(new PointF((float)ni.lon, (float)ni.lat), _config._in_polygon))
                    return false;
            };
            if (_config.onlyWithAddr)
                if (ni.AddrTagsCount == 0)
                    return false;

            if (this.hasTextRegex != null)
            {
                if (ni.tags == null) return false;
                if (ni.tags.Count == 0) return false;
                bool isok = false;
                foreach (KeyValuePair<string, string> kvp in ni.tags)
                {
                    if (this.hasTextRegex.IsMatch(kvp.Key)) isok = true;
                    if (this.hasTextRegex.IsMatch(kvp.Value)) isok = true;
                };
                if (!isok)
                    return false;
            };

            if (!ApplyScriptFilters(ni))
                return false;

            return true;
        }

        private bool ApplyWayFilters(OSMPBFReader.Way way, NodesXYIndex.IdLatLon[] points)
        {
            if (_config.onlyHasName && (!way.tags.ContainsKey("name"))) return false;
            if (_config.onlyWithTags.Count > 0)
            {
                if (way.tags.Count == 0) return false;
                foreach (KeyValuePair<string, string> _kvp in _config.onlyWithTags)
                {
                    if (!way.tags.ContainsKey(_kvp.Key)) return false;
                    if ((_kvp.Value != null) && (_kvp.Value != "") && (way.tags[_kvp.Key] != _kvp.Value)) return false;
                };
            };
            if (_config.onlyOneOfTags.Count > 0)
            {
                if (way.tags.Count == 0) return false;
                bool skip = true;
                foreach (KeyValuePair<string, string> _kvp in _config.onlyOneOfTags)
                {
                    if (way.tags.ContainsKey(_kvp.Key))
                    {
                        if ((_kvp.Value == null) || (_kvp.Value == ""))
                            skip = false;
                        else if ((way.tags[_kvp.Key] == _kvp.Value))
                            skip = false;
                    };
                };
                if (skip)
                    return false;
            };
            if (way.info != null)
            {
                if (_config.onlyMdfAfter != DateTime.MinValue) if (way.info.datetime < _config.onlyMdfAfter) return false;
                if (_config.onlyMdfBefore != DateTime.MaxValue) if (way.info.datetime > _config.onlyMdfBefore) return false;
                if (_config.onlyOfUser.Count > 0)
                {
                    bool skip = true;
                    foreach (string user in _config.onlyOfUser)
                        if (way.info.user == user)
                            skip = false;
                    if (skip)
                        return false;
                };
                if (_config.onlyVersion.Count > 0)
                {
                    bool skip = true;
                    foreach (int ver in _config.onlyVersion)
                        if (way.info.version == ver)
                            skip = false;
                    if (skip)
                        return false;
                };
            };
            
            if ((_config.onlyInBox != null) && (_config.onlyInBox.Length == 4))
            {
                int pCo = points.Length;
                foreach (NodesXYIndex.IdLatLon ni in points)
                {
                    if (ni.lat < _config.onlyInBox[0]) { pCo--; continue; };
                    if (ni.lon < _config.onlyInBox[1]) { pCo--; continue; };
                    if (ni.lat > _config.onlyInBox[2]) { pCo--; continue; };
                    if (ni.lon > _config.onlyInBox[3]) { pCo--; continue; };
                };
                if(pCo == 0) return false;
            };
            if ((_config._in_polygon != null) && (_config._in_polygon.Length > 2))
            {
                PointF[] _points = new PointF[points.Length];
                for (int i = 0; i < _points.Length; i++)
                    _points[i] = new PointF((float)points[i].lon, (float)points[i].lat);
                if(!UTILS.IntersectPolygons(_points, _config._in_polygon))
                    return false;                
            };
            if (_config.onlyWithAddr)
            {
                bool had = false;
                foreach(KeyValuePair<string,string> kvp in way.tags)
                    if(kvp.Key.StartsWith("addr"))
                    {
                        had = true;
                        break;
                    };
                if(!had) return false;
                
            };

            if (this.hasTextRegex != null)
            {
                if (way.tags == null) return false;
                if (way.tags.Count == 0) return false;
                bool isok = false;
                foreach (KeyValuePair<string, string> kvp in way.tags)
                {
                    if (this.hasTextRegex.IsMatch(kvp.Key)) isok = true;
                    if (this.hasTextRegex.IsMatch(kvp.Value)) isok = true;
                };
                if (!isok)
                    return false;
            };

            return true;
        }

        private bool ApplyFilters(OSMPBFReader.Relation rel)
        {
            if (_config.onlyHasName && (!rel.tags.ContainsKey("name"))) return false;
            if (_config.onlyWithTags.Count > 0)
            {
                if (rel.tags.Count == 0) return false;
                foreach (KeyValuePair<string, string> _kvp in _config.onlyWithTags)
                {
                    if (!rel.tags.ContainsKey(_kvp.Key)) return false;
                    if ((_kvp.Value != null) && (_kvp.Value != "") && (rel.tags[_kvp.Key] != _kvp.Value)) return false;
                };
            };
            if (_config.onlyOneOfTags.Count > 0)
            {
                if (rel.tags.Count == 0) return false;
                bool skip = true;
                foreach (KeyValuePair<string, string> _kvp in _config.onlyOneOfTags)
                {
                    if (rel.tags.ContainsKey(_kvp.Key))
                    {
                        if ((_kvp.Value == null) || (_kvp.Value == ""))
                            skip = false;
                        else if ((rel.tags[_kvp.Key] == _kvp.Value))
                            skip = false;
                    };
                };
                if (skip)
                    return false;
            };
            if (rel.info != null)
            {
                if (_config.onlyMdfAfter != DateTime.MinValue) if (rel.info.datetime < _config.onlyMdfAfter) return false;
                if (_config.onlyMdfBefore != DateTime.MaxValue) if (rel.info.datetime > _config.onlyMdfBefore) return false;
                if (_config.onlyOfUser.Count > 0)
                {
                    bool skip = true;
                    foreach (string user in _config.onlyOfUser)
                        if (rel.info.user == user)
                            skip = false;
                    if (skip)
                        return false;
                };
                if (_config.onlyVersion.Count > 0)
                {
                    bool skip = true;
                    foreach (int ver in _config.onlyVersion)
                        if (rel.info.version == ver)
                            skip = false;
                    if (skip)
                        return false;
                };
            };
            if (_config.onlyWithAddr)
            {
                bool had = false;
                foreach (KeyValuePair<string, string> kvp in rel.tags)
                    if (kvp.Key.StartsWith("addr"))
                    {
                        had = true;
                        break;
                    };
                if (!had) return false;

            };

            if (this.hasTextRegex != null)
            {
                if (rel.tags == null) return false;
                if (rel.tags.Count == 0) return false;
                bool isok = false;
                foreach (KeyValuePair<string, string> kvp in rel.tags)
                {
                    if (this.hasTextRegex.IsMatch(kvp.Key)) isok = true;
                    if (this.hasTextRegex.IsMatch(kvp.Value)) isok = true;
                };
                if (!isok)
                    return false;
            };

            return true;
        }

        public bool ApplyScriptFilters(OSMPBFReader.NodeInfo ni)
        {
            try { if (apf != null) if (!apf.ApplyFilters(ni)) return false; } catch { };
            return true;
        }

        private void Progress(float percentage, bool processing)
        {
            if (onProgress != null)
                onProgress(this, percentage, processing);
        }        

        private void Done()
        {
            if(onDone != null)
                onDone(this, new EventArgs());
        }

        private void Error(Exception ex)
        {
            if (onError != null)
                onError(this, ex);
        }
    }

    public struct FLID
    {
        public long f;
        public long l;
        public FLID(long f, long l)
        {
            this.f = f;
            this.l = l;
        }
    }

    // https://programmingwithmosh.com/net/csharp-collections/
    // List<T> - Represents a list of objects that can be accessed by an index
    // Dictionary<TKey, TValue> - Dictionary is a collection type that is useful when you need fast lookups by keys
    // HashSet<T> - Unical list
    // Stack<T> - LIFO buffer
    // Queue<T> - FIFO buffer
    public class NodesXYIndex
    {
        private FileStream str = null;
        private Dictionary<long, LatLonLCo> dict = new Dictionary<long, LatLonLCo>();
        private bool inMemory = true;

        public FileStream STR { get { return STR; } }
        public Dictionary<long, LatLonLCo> Dict { get { return dict; } }
        public bool InMemory { get { return inMemory; } }        

        public NodesXYIndex()
        {
            inMemory = true;
        }

        public NodesXYIndex(string fileName)
        {
            inMemory = false;
            str = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
        }

        public long Length
        {
            get
            {
                if (inMemory)
                {
                    return 49 * dict.Count + 100; // 20 oversize + 8 ID + 8 Lat + 8 Lon + 1 Lines + 4 addit
                }
                else
                {
                    if (str == null) return 0;
                    return str.Length;
                };
            }
        }

        public void WriteNode(long id, double lat, double lon)
        {
            if (inMemory)
            {
                dict.Add(id, new LatLonLCo(lat, lon, 0));
            }
            else
            {
                str.Write(BitConverter.GetBytes(id), 0, 8);
                str.Write(BitConverter.GetBytes(lat), 0, 8);
                str.Write(BitConverter.GetBytes(lon), 0, 8);
                str.WriteByte(0);
            };
        }        

        private IdLatLon ReadNode()
        {
            if (inMemory)
            {
                return null;
            }
            else
            {
                if (str.Position == str.Length) return null;
                byte[] buff = new byte[25];
                str.Read(buff, 0, buff.Length);                
                IdLatLon res = new IdLatLon(BitConverter.ToInt64(buff, 0),BitConverter.ToDouble(buff, 8), BitConverter.ToDouble(buff, 16), buff[24] );
                return res;
            };
        }

        
        public struct LatLonLCo
        {
            public double lat;
            public double lon;
            public byte lco;
            public LatLonLCo(double lat, double lon)
            {
                this.lat = lat;
                this.lon = lon;
                this.lco = 0;
            }
            public LatLonLCo(double lat, double lon, byte lco)
            {
                this.lat = lat;
                this.lon = lon;
                this.lco = lco;
            }
        }

        public class IdLatLon
        {
            public long id;
            public double lat;
            public double lon;
            public byte lco;
            public IdLatLon(long id, double lat, double lon)
            {
                this.id = id;
                this.lat = lat;
                this.lon = lon;
                this.lco = 0;
            }
            public IdLatLon(long id, double lat, double lon, byte lines_count)
            {
                this.id = id;
                this.lat = lat;
                this.lon = lon;
                this.lco = lines_count;
            }
            public IdLatLon(long id, LatLonLCo latlonlco)
            {
                this.id = id;
                this.lat = latlonlco.lat;
                this.lon = latlonlco.lon;
                this.lco = latlonlco.lco;
            }
        }

        public IdLatLon[] GetNodes(long[] nodes, bool update_lco)
        {
            if (inMemory)
            {
                IdLatLon[] res = new IdLatLon[nodes.Length];
                try
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        LatLonLCo lll = dict[nodes[i]];
                        if (update_lco)
                        {
                            byte se = (byte)(lll.lco & 0x0F);
                            byte md = (byte)(lll.lco & 0xF0);
                            if ((i == 0) || (i == nodes.Length - 1)) // start or end
                            {
                                if (se < 0x0F) lll.lco += 0x01;
                            }
                            else // middle
                            {
                                if (md < 0xF0) lll.lco += 0x10;
                            };
                            dict[nodes[i]] = lll;
                        };
                        res[i] = new IdLatLon(nodes[i], lll);
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception("Node for vector not found\r\n" + ex.ToString());
                };
                return res;
            }
            else
            {
                if (nodes == null) return new IdLatLon[0];
                if (nodes.Length == 0) return new IdLatLon[0];
                if (str.Length == 0) return new IdLatLon[0];

                long pos = str.Position;
                str.Position = 0;
                IdLatLon[] res = new IdLatLon[nodes.Length];
                int ntl = res.Length;
                while (true)
                {
                    IdLatLon node = ReadNode();
                    if (node == null) break;
                    for (int i = 0; i < nodes.Length; i++)
                        if (node.id == nodes[i])
                        {
                            if (update_lco)
                            {
                                byte se = (byte)(node.lco & 0x0F);
                                byte md = (byte)(node.lco & 0xF0);
                                if ((i == 0) || (i == nodes.Length - 1)) // start or end
                                {
                                    if (se < 0x0F) node.lco += 0x01;
                                }
                                else // middle
                                {
                                    if (md < 0xF0) node.lco += 0x10;
                                };
                                str.Position -= 1;
                                str.WriteByte(node.lco);
                            };
                            res[i] = node;
                            ntl--;
                        };
                    if (ntl == 0) break;
                };
                str.Position = pos;
                return res;
            };
        }

        public void Close()
        {
            if (inMemory)
            {
                dict.Clear();
                dict = null;
                dict = new Dictionary<long, LatLonLCo>();
            }
            else
            {
                str.Close();
                str = null;
            };
        }
    }

    public class GeoUtils
    {
        public static PointF GetCentroid( List<PointF> vector, bool isPolygon, bool isClosed)
        {
            PointF centroid = isPolygon ? GetCentroid(vector) : new PointF(0, 0);
            if (isPolygon ? PointInPolygon(centroid, vector, MaxError) == false : true)
            {
                PointF newCenter = centroid = GetCenter(vector);
                PointF closest; int side;
                double maxD = isClosed ? DistanceFromPointToLine(centroid, vector[vector.Count - 1], vector[0], out newCenter, out side) : double.MaxValue;
                for (int i = 1; i < vector.Count; i++)
                {
                    double curD = DistanceFromPointToLine(centroid, vector[i - 1], vector[i], out closest, out side);
                    if (curD < maxD) { maxD = curD; newCenter = closest; };
                };
                centroid = newCenter;
            };
            return centroid;
        }

        private static PointF GetCentroid(List<PointF> poly)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                float temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
                accumulatedArea += temp;
                centerX += (poly[i].X + poly[j].X) * temp;
                centerY += (poly[i].Y + poly[j].Y) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return PointF.Empty;  // Avoid division by zero

            accumulatedArea *= 3f;
            return new PointF(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        public static PointF GetCenter(List<PointF> poly)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int i = 0; i < poly.Count; i++)
            {
                if (poly[i].X < minX) minX = poly[i].X;
                if (poly[i].X > maxX) maxX = poly[i].X;
                if (poly[i].Y < minY) minY = poly[i].Y;
                if (poly[i].Y > maxY) maxY = poly[i].Y;
            };
            return new PointF((float)((maxX + minX) / 2), (float)((maxY + minY) / 2));
        }        

        #region Проверка на вхождение в полигон
        private static double MaxError = 1E-09;

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

        private static bool PointInPolygon(PointF point, List<PointF> polygon, double EPS)
        {
            int count, up;
            count = 0;
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                up = CRS(point, polygon[i], polygon[i + 1], EPS);
                if (up >= 0)
                    count += up;
                else
                    break;
            };
            up = CRS(point, polygon[polygon.Count - 1], polygon[0], EPS);
            if (up >= 0)
                return Convert.ToBoolean((count + up) & 1);
            else
                return false;
        }
        #endregion

        public static float DistanceFromPointToLine(PointF pt, PointF lineStart, PointF lineEnd, out PointF pointOnLine, out int side)
        {
            /////////////////////////
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

    public abstract class ApplyFilterScript
    {
        public OSMConverter Converter = null;
        public OSMConverter converter { get { return Converter; } }
        public OSMConverter Convertor { get { return Converter; } }
        public OSMConverter convertor { get { return Converter; } }        
        public OSMConverter Conv { get { return Converter; } }
        public OSMConverter conv { get { return Converter; } }
        public OSMConverter Parent { get { return Converter; } }
        public OSMConverter parent { get { return Converter; } }
        public OSMConverter Sender { get { return Converter; } }
        public OSMConverter sender { get { return Converter; } }

        public object userdata = null;
        public object UserData { get { return userdata; } set { userdata = value; } }
        public object Userdata { get { return userdata; } set { userdata = value; } }
        public object userData { get { return userdata; } set { userdata = value; } }

        public string[] args = new string[0];

        public virtual bool ApplyFilters(OSMPBFReader.NodeInfo ni) { return true; }
        public virtual bool ApplyFilters(OSMPBFReader.NodeInfo ni, int filter) { return true; }
    }
}
