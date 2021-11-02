using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

using ProtoBuf;
using Ionic.Zlib;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace OSM2SHP
{
    public partial class MForm : Form
    {
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetThreadExecutionState([In] uint esFlags);

        private string[] args;
        public Config config = new Config(true);
        public bool in_progress = false;
        public byte reset = 0;
        private string status_text = "";
        private long fileSize = 0;
        private string MandatoryText = "";

        private System.Drawing.Text.PrivateFontCollection pfc = new System.Drawing.Text.PrivateFontCollection();

        private SimpleHttpProtoServer httpServer = null;
        private bool viewonly = false;
        private string machine = "";

        public MForm(string[] args, bool isValid, string MachineID)
        {
            InitializeComponent();
            if (!isValid) viewonly = true;
            machine = MachineID;

            try
            {
                pfc.AddFontFile(AppDomain.CurrentDomain.BaseDirectory + @"\fonts\PTMono.ttf");
                status.Font = new Font(pfc.Families[0], status.Font.Size);
            }
            catch { };

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(MForm_DragEnter);
            this.DragDrop += new DragEventHandler(MForm_DragDrop);

            this.args = args;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Text = MandatoryText = Text + " v" + fvi.FileVersion + (viewonly ? " [ViewOnly]" : "");            

            SetFileAss();
            LoadConfig();

            this.Text += " by milokz@gmail.com";
        }

        public delegate void MethodNoneArguments();

        private string httpServer_onInfo(byte action, out string server, out string title, out bool  isRunning)
        {
            if (action == 1) // start
            {
                cmd_silent = true;
                //IAsyncResult res = 
                    this.BeginInvoke(new MethodInvoker(Convert));
                //
                //while (!res.IsCompleted) System.Threading.Thread.Sleep(200);
                // or
                //res.AsyncWaitHandle.WaitOne();
                // or/and
                //this.EndInvoke(res);
            };
            if (action == 2) // stop
            {
                cmd_silent = true;
                if ((in_progress) && (reset == 0))
                reset = 1;
            };

            server = this.MandatoryText;
            title = this.Text;
            isRunning = this.in_progress;
            return status.Text;
        }

        private void MForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if(files != null)
                {
                    string ext = Path.GetExtension(files[0]).ToLower();
                    if (ext == ".dbf")
                    {
                        props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName = files[0]);
                    }
                    else if (ext == ".osm")
                    {
                        OSMXMLReader fr = new OSMXMLReader(files[0]);
                        if (!fr.ValidHeader)
                        {
                            fr.Close();
                            MessageBox.Show("Неверный заголовок файла", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            fr.Close();
                            config.inputFileName = files[0];
                            props.Items[0].SubItems[1].Text = Path.GetFileName(files[0]);
                            try
                            {
                                if (File.Exists(config.inputFileName))
                                    props.Items[0].SubItems[1].Text += "  [" + GetFileSize(new FileInfo(files[0]).Length) + "]";
                            }
                            catch { };
                            props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName = "");
                        };
                    }
                    else if (ext == ".pbf")
                    {
                        OSMPBFReader fr = new OSMPBFReader(files[0]);
                        if (!fr.ValidHeader)
                        {
                            fr.Close();
                            MessageBox.Show("Неверный заголовок файла", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else
                        {
                            fr.Close();
                            config.inputFileName = files[0];
                            props.Items[0].SubItems[1].Text = Path.GetFileName(files[0]);
                            try
                            {
                                if (File.Exists(config.inputFileName))
                                    props.Items[0].SubItems[1].Text += "  [" + GetFileSize(new FileInfo(files[0]).Length) + "]";
                            }
                            catch { };
                            props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName = "");
                        };
                    }
                    else if (ext == ".o2s")
                    {
                        try
                        {
                            config = Config.Load(files[0], true);
                            config.onReloadProperties += new EventHandler(config_onReloadProperties);
                            config.ReloadProperties();
                        }
                        catch { };
                    };                    
                };
            };
        }

        private void MForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files == null) 
                    e.Effect = DragDropEffects.None;
                else if (files.Length != 1)
                    e.Effect = DragDropEffects.None;
                else
                {
                    string ext = Path.GetExtension(files[0]).ToLower();
                    if ((ext == ".osm") || (ext == ".pbf") || (ext == ".o2s") || (ext == ".dbf"))
                    {

                    }
                    else e.Effect = DragDropEffects.None;
                };
            };
        }

        private void SetFileAss()
        {
            FileAss.SetFileAssociation("pbf", "PBFFile", "Convert with OSM2SHP", AppDomain.CurrentDomain.BaseDirectory + @"\OSM2SHPFC.exe");
            FileAss.SetFileAssociation("osm", "OSMFile", "Convert with OSM2SHP", AppDomain.CurrentDomain.BaseDirectory + @"\OSM2SHPFC.exe");
            FileAss.SetFileAssociation("o2m", "O2SFile", "Open Config in OSM2SHP", AppDomain.CurrentDomain.BaseDirectory + @"\OSM2SHPFC.exe");
            FileAss.UpdateExplorer();
        }

        public void LoadConfig()
        {
            config = Config.Load("OSM2SHPFC.cpb", true);
            config.onReloadProperties += new EventHandler(config_onReloadProperties);
            config.ReloadProperties();            
        }

        public void ResetConfig()
        {
            config = new Config(true);
            config.onReloadProperties += new EventHandler(config_onReloadProperties);
            config.ReloadProperties();   
        }

        public void NullProgress()
        {
            STAT2.Value = 0;
            STAT2.Visible = false;
        }

        public void config_onReloadProperties(object sender, EventArgs e)
        {
            NullProgress();

            props.Items[0].SubItems[1].Text = Path.GetFileName(config.inputFileName);
            try
            {
                if (File.Exists(config.inputFileName))
                    props.Items[0].SubItems[1].Text += "  [" + GetFileSize(new FileInfo(config.inputFileName).Length)+"]";
            }
            catch { };
            props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName);
            props.Items[2].SubItems[1].Text = Config.SELECTOR[config.selector];
            props.Items[3].SubItems[1].Text = config.onlyHasName ? "Да" : "";
            props.Items[4].SubItems[1].Text = config.onlyWithAddr ? "Да" : "";
            props.Items[12].SubItems[1].Text = Path.GetFileName(config.onlyInPolygon);
            props.Items[15].SubItems[1].Text = config.MaxFileRecords.ToString();
            props.Items[16].SubItems[1].Text = config.allTagsFormat.Replace("\r", "\\r").Replace("\n", "\\n");
            props.Items[17].SubItems[1].Text = config.useAggPrefix.Replace("\r", "\\r").Replace("\n", "\\n");
            props.Items[18].SubItems[1].Text = config.maxAggTags.ToString();
            props.Items[20].SubItems[1].Text = config.CodePages[config.dbfCodePage].codeName;
            props.Items[21].SubItems[1].Text = config.aggrRegex;
            props.Items[22].SubItems[1].Text = config.hasText;
            props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
            props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
            props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
            props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
            props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
            props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
            props.Items[29].SubItems[1].Text = config.useNotInMemoryIndexFile ? "Хранить индекс в файле на диске (медлено)" : "Хранить индекс в оперативной памяти (быстро)";
            props.Items[30].SubItems[1].Text = config.processRelations ? "Да" : "";
            props.Items[31].SubItems[1].Text = config.processRelationFilter ? "Да" : "";
            props.Items[34].SubItems[1].Text = config.relationsAsJoins ? "Да" : "";
            props.Items[35].SubItems[1].Text = config.addFirstAndLastNodesIdToLines ? "Да" : "";
            props.Items[36].SubItems[1].Text = config.addFisrtAndLastNodesLns2Memory ? "Да" : "";
            props.Items[37].SubItems[1].Text = config.saveLineNodesShape ? "Да" : "";
            props.Items[38].SubItems[1].Text = String.IsNullOrEmpty(config.sortAggTagsPriority) ? "" : config.sortAggTagsPriority;
            
            if (!String.IsNullOrEmpty(config.scriptFilter))
                props.Items[19].SubItems[1].Text = config.scriptFilter.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
            else
                props.Items[19].SubItems[1].Text = "";            

            {//5
                string tl = "";
                foreach (KeyValuePair<string, string> kvp in config.onlyWithTags)
                {
                    if (tl.Length > 0) tl += ",";
                    if ((kvp.Value == null) || (kvp.Value == ""))
                        tl += kvp.Key;
                    else
                        tl += String.Format("{0}={1}", kvp.Key, kvp.Value);
                };
                props.Items[5].SubItems[1].Text = tl;
            };
            {//6
                string tl = "";
                foreach (KeyValuePair<string, string> kvp in config.onlyOneOfTags)
                {
                    if (tl.Length > 0) tl += ",";
                    if ((kvp.Value == null) || (kvp.Value == ""))
                        tl += kvp.Key;
                    else
                        tl += String.Format("{0}={1}", kvp.Key, kvp.Value);
                };
                props.Items[6].SubItems[1].Text = tl;
            };
            {//7
                if (config.onlyMdfAfter != DateTime.MinValue)
                    props.Items[7].SubItems[1].Text = config.onlyMdfAfter.ToString("yyyy-MM-ddTHH:mm:ss");
            };
            {//8
                if (config.onlyMdfBefore != DateTime.MaxValue)
                    props.Items[8].SubItems[1].Text = config.onlyMdfBefore.ToString("yyyy-MM-ddTHH:mm:ss");
            };
            {//9
                string tl = "";
                foreach (string user in config.onlyOfUser)
                {
                    if (tl.Length > 0) tl += ",";
                    tl += user;
                };
                props.Items[9].SubItems[1].Text = tl;
            };
            {//10
                string tl = "";
                foreach (int ver in config.onlyVersion)
                {
                    if (tl.Length > 0) tl += ",";
                    tl += ver.ToString();
                };
                props.Items[10].SubItems[1].Text = tl;
            };
            {//11
                string tl = "";
                if ((config.onlyInBox != null) && (config.onlyInBox.Length == 4))
                    tl = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2},{3}", new object[] { config.onlyInBox[0], config.onlyInBox[1], config.onlyInBox[2], config.onlyInBox[3] });
                props.Items[11].SubItems[1].Text = tl;
            };
            {//13
                string tl = "";
                foreach (string dopf in config.dbfMainFields)
                {
                    if (tl.Length > 0) tl += ",";
                    tl += dopf;
                };
                props.Items[13].SubItems[1].Text = tl;
            };
            {//14
                string tl = "";
                foreach (string dopf in config.dbfDopFields)
                {
                    if (tl.Length > 0) tl += ",";
                    tl += dopf;
                };
                props.Items[14].SubItems[1].Text = tl;
            };
            {//32
                string tl = "";
                foreach (string relline in config.relationTypesAsLine)
                {
                    if (tl.Length > 0) tl += ",";
                    tl += relline;
                };
                props.Items[32].SubItems[1].Text = tl;
            };
            {//33
                string tl = "";
                foreach (string relarea in config.relationTypesAsArea)
                {
                    if (tl.Length > 0) tl += ",";
                    tl += relarea;
                };
                props.Items[33].SubItems[1].Text = tl;
            };
        }


        private void Convert()
        {
            if (viewonly)
            {
                MessageBox.Show(String.Format("Machine is not supported!", machine), this.Text);
                return;
            };

            if (in_progress) return;
            if(!File.Exists(config.inputFileName)) return;
            if(String.IsNullOrEmpty(config.outputFileName)) return;

            status.Text = "Инициализация процесса...";

            OSMConverter osmc = new OSMConverter(config);
            osmc.onProgress += new OSMConverter.OnProgress(osmc_onProgress);
            osmc.onError += new OSMConverter.OnError(osmc_onError);
            osmc.onDone += new EventHandler(osmc_onDone);
            osmc.onStart += new EventHandler(osmc_onStart);
            try
            {
                fileSize = (new FileInfo(config.inputFileName)).Length;
                osmc.MaxFileRecords = config.MaxFileRecords;
                osmc.Convert();
            }
            catch (Exception ex)
            {
                status.Text = "Ошибка при запуске процесса: \r\n\r\n" + ex.ToString();
                if (contextMenuStrip1.Visible) contextMenuStrip1.Close();
                MessageBox.Show("Ошибка при запуске процесса: " + ex.Message, this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        public void osmc_onStart(object sender, EventArgs e)
        {
            SetPropsEnables(false);
            started = DateTime.Now;
            cOL1ToolStripMenuItem.Checked = splitContainer1.Panel1Collapsed = true;
            cOL2ToolStripMenuItem.Checked = splitContainer1.Panel2Collapsed = false;


            STAT2.Value = 0;
            STAT2.Visible = true;
            STAT2.ForeColor = Color.Navy;

            try { SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED); } catch { };
        }

        public void SaveAfterLog(OSMConverter osmc)
        {
            try
            {
                string txtfn = Path.GetDirectoryName(config.outputFileName) + @"\" + Path.GetFileNameWithoutExtension(config.outputFileName) + "[_LOG_].txt";
                FileStream fs = new FileStream(txtfn, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                sw.WriteLine("Converter Log. Created by " + this.MandatoryText);
                sw.WriteLine();
                sw.WriteLine(status.Text);                
                if (osmc.tags_skipped.Count > 0)
                {
                    sw.WriteLine("\r\n---\r\nСписок и количество неагрегированных тегов ({0}):\r\n", osmc.tags_skipped.Count);
                    foreach (KeyValuePair<string, uint> sktag in DictComparer.SortDictionary(osmc.tags_skipped, false, false))
                        sw.WriteLine("  {0,-35} - {1}", sktag.Key, sktag.Value);
                    sw.WriteLine();
                };
                sw.WriteLine("\r\n---Параметры конвертации:---\r\n");
                for (int i = 3; i < props.Items.Count; i++)
                    if (!String.IsNullOrEmpty(props.Items[i].SubItems[1].Text))
                    {
                        if (i == 19)
                            sw.WriteLine("{0}:\r\n{1}", props.Items[i].SubItems[0].Text, "{\r\n" + config.scriptFilter + "\r\n}");
                        else
                            sw.WriteLine("{0}:\r\n    {1}", props.Items[i].SubItems[0].Text, props.Items[i].SubItems[1].Text);
                    };
                sw.WriteLine("\r\n---Полный список файлов:---\r\n");
                List<string> files = new List<string>();
                string dir = Path.GetDirectoryName(osmc._dbfFile).Trim('\\') + @"\";
                string prf = Path.GetFileNameWithoutExtension(osmc._dbfFile);
                if(osmc.FilesPointsWrited > 0)
                    for (int i = 1; i <= osmc.FilesPointsWrited; i++)
                    {
                        files.Add(String.Format("{0}{1}[P{2:0000}].shp", dir, prf, i));
                        files.Add(String.Format("{0}{1}[P{2:0000}].dbf", dir, prf, i));                        
                        files.Add(String.Format("{0}{1}[P{2:0000}].shx", dir, prf, i));
                        files.Add(String.Format("{0}{1}[P{2:0000}].prj", dir, prf, i));
                        files.Add(String.Format("{0}{1}[P{2:0000}].txt", dir, prf, i));
                        files.Add("");
                    };
                if (osmc.FilesLinesWrited > 0)
                    for (int i = 1; i <= osmc.FilesLinesWrited; i++)
                    {
                        files.Add(String.Format("{0}{1}[L{2:0000}].shp", dir, prf, i));
                        files.Add(String.Format("{0}{1}[L{2:0000}].dbf", dir, prf, i));                        
                        files.Add(String.Format("{0}{1}[L{2:0000}].shx", dir, prf, i));
                        files.Add(String.Format("{0}{1}[L{2:0000}].prj", dir, prf, i));
                        files.Add(String.Format("{0}{1}[L{2:0000}].txt", dir, prf, i));
                        files.Add("");
                    };
                if (osmc.FilesAreasWrited > 0)
                    for (int i = 1; i <= osmc.FilesAreasWrited; i++)
                    {
                        files.Add(String.Format("{0}{1}[A{2:0000}].shp", dir, prf, i));
                        files.Add(String.Format("{0}{1}[A{2:0000}].dbf", dir, prf, i));                        
                        files.Add(String.Format("{0}{1}[A{2:0000}].shx", dir, prf, i));
                        files.Add(String.Format("{0}{1}[A{2:0000}].prj", dir, prf, i));
                        files.Add(String.Format("{0}{1}[A{2:0000}].txt", dir, prf, i));
                        files.Add("");
                    };
                if (osmc.NodesWrited > 0)
                {
                    files.Add(String.Format("{0}{1}[NODES].shp", dir, prf));
                    files.Add(String.Format("{0}{1}[NODES].dbf", dir, prf));
                    files.Add(String.Format("{0}{1}[NODES].shx", dir, prf));
                    files.Add(String.Format("{0}{1}[NODES].prj", dir, prf));
                    files.Add(String.Format("{0}{1}[NODES].txt", dir, prf));
                    files.Add("");
                };
                if (osmc.FilesRelationsWrited > 0)
                    for (int i = 1; i <= osmc.FilesRelationsWrited; i++)
                    {
                        files.Add(String.Format("{0}{1}[R{2:0000}].dbf", dir, prf, i));
                        files.Add(String.Format("{0}{1}[R{2:0000}].txt", dir, prf, i));
                        files.Add(String.Format("{0}{1}[M{2:0000}].dbf", dir, prf, i));
                        files.Add(String.Format("{0}{1}[M{2:0000}].txt", dir, prf, i));
                        files.Add("");
                    };
                if(osmc.FilesJoinsWrited > 0)
                    for (int i = 1; i <= osmc.FilesJoinsWrited; i++)
                    {
                        files.Add(String.Format("{0}{1}[J{2:0000}].dbf", dir, prf, i));
                        files.Add(String.Format("{0}{1}[J{2:0000}].txt", dir, prf, i));
                        files.Add("");
                    };
                files.Add(String.Format("{0}{1}[_LOG_].txt", dir, prf));

                long ttl_f_len = 0;
                foreach (string file in files)
                {
                    if (file == "")
                    {
                        sw.WriteLine();
                        continue;
                    };
                    long fl = (new FileInfo(file)).Length;
                    ttl_f_len += fl;
                    string ftype = "";
                    if (file.IndexOf("[P0") > 0) ftype = " точки";
                    if (file.IndexOf("[A0") > 0) ftype = " полигоны";
                    if (file.IndexOf("[L0") > 0) ftype = " линии";
                    if (file.IndexOf("[R0") > 0) ftype = " связи";
                    if (file.IndexOf("[M0") > 0) ftype = " объекты связей";
                    if (file.IndexOf("[J0") > 0) ftype = " запреты поворотов";
                    if (file.IndexOf("[NO") > 0) ftype = " узлы линий";
                    if (file.IndexOf("[_LOG_]") > 0)
                        ftype = " протокол лога";
                    else
                    {
                        if (Path.GetExtension(file) == ".dbf") ftype += ", таблица";
                        if (Path.GetExtension(file) == ".shx") ftype += ", индекс";
                        if (Path.GetExtension(file) == ".prj") ftype += ", проекция";
                        if (Path.GetExtension(file) == ".txt") ftype += ", описание";                        
                    };
                    sw.WriteLine("  {0} - {1,-25} - {2}", file, ftype, GetFileSize(fl));
                };
                sw.WriteLine(String.Format("\r\nОбщий размер файлов: {0}", GetFileSize(ttl_f_len)));
                sw.WriteLine();
                sw.Close();
                fs.Close();
            }
            catch (Exception ex) { };
        }

        public void osmc_onError(object sender, Exception error)
        {
            Text = MandatoryText;            
             
            string text = "---\r\n";
            text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Конвертация прервана с ошибкой : {0:HH:mm:ss dd.MM.yyyy}\r\n", DateTime.Now);
            TimeSpan e = DateTime.Now.Subtract(started);
            text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Конвертация продолжалась       : {0} дн {1:00} ч {2:00} м {3:00} c\r\n", e.Days, e.Hours, e.Minutes, e.Seconds);
            if(reset == 0)
                text += "---\r\nОшибка:\r\n\r\n" + error.ToString();
            else
                text += "---\r\nОшибка: " + error.Message;
            status.Text = "Конвертация прервана\r\n---\r\n" + status_text + text;

            STAT2.ForeColor = Color.Red;
            SetPropsEnables(true);
            if (contextMenuStrip1.Visible) contextMenuStrip1.Close();

            try { SetThreadExecutionState(ES_CONTINUOUS); } catch { };

            SaveAfterLog(sender as OSMConverter);            

            if (!cmd_silent)
                MessageBox.Show("Конвертация прервана с ошибкой: " + error.Message + "\r\nПротокол лога: " + Path.GetFileNameWithoutExtension(config.outputFileName) + "[_LOG_].txt", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //NullProgress();

            cOL1ToolStripMenuItem.Checked = splitContainer1.Panel1Collapsed = false;
            cOL2ToolStripMenuItem.Checked = splitContainer1.Panel2Collapsed = false;
        }

        public void osmc_onDone(object sender, EventArgs e)
        {
            Text = MandatoryText;            
         
            string text = "\r\n---\r\n";
            text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Конвертация успешно завершена : {0:HH:mm:ss dd.MM.yyyy}\r\n", DateTime.Now);
            TimeSpan el = DateTime.Now.Subtract(started);
            text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Конвертация продолжалась      : {0} дн {1:00} ч {2:00} м {3:00} c\r\n", el.Days, el.Hours, el.Minutes, el.Seconds);
            status.Text = "Конвертация завершена\r\n---\r\n" + status_text + text;

            STAT2.ForeColor = Color.Green;
            SetPropsEnables(true);
            if (contextMenuStrip1.Visible) contextMenuStrip1.Close();

            try { SetThreadExecutionState(ES_CONTINUOUS); } catch { };

            SaveAfterLog(sender as OSMConverter);

            if(!cmd_silent)
                MessageBox.Show(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Конвертация успешно завершена\r\nза {0:dd HH:mm:ss}\r\n", DateTime.Now.Subtract(started)) + "\r\nПротокол лога: " + Path.GetFileNameWithoutExtension(config.outputFileName) + "[_LOG_].txt", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //NullProgress();
            cOL1ToolStripMenuItem.Checked = splitContainer1.Panel1Collapsed = false;
            cOL2ToolStripMenuItem.Checked = splitContainer1.Panel2Collapsed = false;
        }

        private DateTime updated = DateTime.UtcNow;
        private DateTime started = DateTime.UtcNow;
        private sbyte ps = -1;
        private void osmc_onProgress(object sender, float percentage, bool processing)
        {
            if (reset == 1)
                throw new Exception("Прервано пользователем");
            if (reset == 2)
                throw new Exception("Прервано операционной системой");

            OSMConverter osmc = (OSMConverter)sender;
            if ((ps != DateTime.UtcNow.Subtract(updated).Seconds) || (processing == false)) // each seconds
            {
                STAT2.Value = (int)(percentage * 100.0);
                ps = (sbyte)DateTime.UtcNow.Subtract(updated).Seconds;
            };
            if ((DateTime.UtcNow.Subtract(updated).TotalSeconds >= 3) || (processing == false))
            {
                Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:P} {1}", percentage, MandatoryText);

                string text = "Идет конвертация...\r\n---\r\n";
                status_text = "";
                if((httpServer != null) && (httpServer.Running))
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Онлайн статус  : http://127.0.0.1:{0}\r\n", httpServer.ServerPort);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture    , "Исходный файл  : {0} [{1}]\r\n", Path.GetFileName(config.inputFileName), GetFileSize(fileSize));
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Выходная папка : {0}\r\n", Path.GetDirectoryName(config.outputFileName));
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Селектор       : {0}\r\n", Config.SELECTOR[config.selector]);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Максимальное число записей в файле: {0}\r\n", osmc.MaxFileRecords);
                status_text += "\r\n";
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Начато         : {0:HH:mm:ss dd.MM.yyyy}\r\n", started);
                TimeSpan e = DateTime.Now.Subtract(started);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Продолжается   : {0} дн {1:00} ч {2:00} м {3:00} c\r\n", e.Days, e.Hours, e.Minutes, e.Seconds);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Прочитано      : {0:P} файла\r\n", percentage);
                status_text += "\r\n";
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Всего {0} точек: \r\n", osmc.PointsTotal);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Из узлов (nodes): {0} точек\r\n", osmc.PointsFromNodes);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Из центроидов контуров (ways): {0} точек, {1} из линий, {2} из полигонов\r\n", osmc.PointsFromWays, osmc.PointsFromLines, osmc.PointsFromAreas);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Обработано : {0}, пропущено {1} без тегов, {2} фильтром, {3} селектором\r\n", osmc.PointsProcessed, osmc.PointsEmpty, osmc.NodesSkippedByFilter, osmc.NodesSkippedBySel);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Сохранено  : {0} точек, в том числе {1} уникальных\r\n", osmc.PointsWrited, osmc.PointsUnical);

                if (osmc.ProcWaysSure)
                {
                    if (osmc._config.useNotInMemoryIndexFile)
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Размер индексного файла точек на диске (.idx): {0}\r\n", GetFileSize(osmc.NDILength));
                    else
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Размер индексов точек в памяти: {0}\r\n", GetFileSize(osmc.NDILength));
                };
                
                status_text += "\r\n";                
                if (config.processWays)
                {
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Всего {0} контуров (ways):\r\n", osmc.WaysTotal);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Обработано : {0} контуров (ways), создано {1} точек из центроидов\r\n", osmc.WaysProcessed, osmc.PointsFromWays);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    В которых  : {0} линий и {1} полигонов, где {2} из них обоих типов\r\n", osmc.WaysAsLines, osmc.WaysAsAreas, osmc.WaysAsBoth);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Из связей  : {0} контуров (relations), где {1} линий и {2} полигонов, {3} из них обоих типов\r\n", osmc.WaysByRelationsTotal, osmc.WaysByRelationsLines, osmc.WaysByRelationsAreas, osmc.WaysByRelationsLines + osmc.WaysByRelationsAreas - osmc.WaysByRelationsTotal);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Пропущено фильтром: {0} контуров (ways), в том числе {1} линий и {2} полигонов\r\n", osmc.WaysSkippedFilter, osmc.WaysSkippedFilterLines, osmc.WaysSkippedFilterAreas);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Сохранено  : {0} контуров (ways), в том числе {1} линий и {2} полигонов\r\n", osmc.WaysWrited, osmc.WaysWritedAsLines, osmc.WaysWritedAsAreas);
                    // status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Уникальных: {0} контуров (ways)\r\n", osmc.WaysUnical);
                    if(config.addFisrtAndLastNodesLns2Memory)
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Размер индексов линий в памяти: {0}\r\n", GetFileSize(osmc.LFLLength));
                }
                else
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Всего {0} контуров (ways).\r\n", osmc.WaysTotal);

                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "\r\nВсего {0} связей (relations), {1} обработано, {2} ({3} объектов) сохранено.\r\n", osmc.RelationsTotal, osmc.RelationsProcessed, osmc.RelationsWrited, osmc.RelationsMemsWrited);
                if (osmc.ProcJoins)
                {
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Из них {0} ({6} уникальных + {5} задвоений) запретов поворотов ({3} валидных, {4} нет; сохранено как {2}) в {1} линиях.\r\n", osmc.JoinsRestrictionsTV + osmc.JoinsRestrictionsCOPY, osmc.JoinsRestrictionsLNS, osmc.JoinsRestrictionsFL, osmc.JoinsRestrictionsVLD, osmc.JoinsRestrictionsERR, osmc.JoinsRestrictionsCOPY, osmc.JoinsRestrictionsTV);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Размер индексов запретов в памяти: {0}\r\n", GetFileSize(osmc.JNSLength));
                };
                
                status_text += "\r\n";
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Максимальное число тегов в объекте      : {0}, длина строки описания: {1} символов\r\n", osmc.tags_max_count, osmc.tags_string_maxl);
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Максимальное число агрегированных тегов : {0} в {1} полях\r\n", osmc.tags_aggra_count , osmc.tags_aggrf_used);
                if (((config.selector == 9) || (config.selector == 10) || (config.selector == 11) || (config.selector == 12) || (config.selector == 13)) && (!String.IsNullOrEmpty(config.aggrRegex)) && (osmc.tags_skipped.Count > 0))
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Не агрегировано тегов: {0}\r\n", osmc.tags_skipped.Count);

                status_text += "\r\n";
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Шейп файлы (shapes) - {0}:\r\n", osmc.FilesPointsWrited+osmc.FilesLinesWrited + osmc.FilesAreasWrited + (osmc.NodesWrited > 0 ? 1 : 0));
                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Точки (полей - {0}, размер заголовка {1} байт, записи {2} байт):\r\n", osmc.PointsFieldsCount, (osmc.PointsFieldsCount + 1) * 32, osmc.PointsRecordSize);
                
                long ttl_size = 0;
                
                if(true) //(osmc.FilesPointsWrited > 0) && (osmc.FilesPointsWritedPoints[0] > 0))
                    for (int i = 0; i < osmc.FilesPointsWrited; i++)
                    {
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "        - {0}: {1} + {2} + {3} + {4} - {5} точек, {6}\r\n", i + 1, osmc.FilesPointsWritedDBFs[i], osmc.FilesPointsWritedShapes[i], osmc.FilesPointsWritedShapes[i].Replace(".shp", ".shx"), osmc.FilesPointsWritedShapes[i].Replace(".shp", ".prj"), osmc.FilesPointsWritedPoints[i], GetFileSize(osmc.FilesPointsWritedSizes[i]));
                        ttl_size += osmc.FilesPointsWritedSizes[i];
                    };
                if (osmc.ProcLineSure) // && (osmc.FilesLinesWrited > 0) && (osmc.FilesLinesWritedLines[0] > 0))
                {
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Линии (полей - {0}, размер заголовка {1} байт, записи {2} байт):\r\n", osmc.LinesFieldsCount, (osmc.LinesFieldsCount + 1) * 32, osmc.LinesRecordSize);
                    for (int i = 0; i < osmc.FilesLinesWrited; i++)
                    {
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "        - {0}: {1} + {2} + {3} + {4} - {5} линий, {6}\r\n", i + 1, osmc.FilesLinesWritedDBFs[i], osmc.FilesLinesWritedShapes[i], osmc.FilesLinesWritedShapes[i].Replace(".shp", ".shx"), osmc.FilesLinesWritedShapes[i].Replace(".shp", ".prj"), osmc.FilesLinesWritedLines[i], GetFileSize(osmc.FilesLinesWritedSizes[i]));
                        ttl_size += osmc.FilesLinesWritedSizes[i];
                    };
                };
                if (osmc.ProcAreaSure)// && (osmc.FilesAreasWrited > 0) && (osmc.FilesAreasWritedAreas[0] > 0))
                {
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Полигоны (полей - {0}, размер заголовка {1} байт, записи {2} байт):\r\n", osmc.AreasFieldsCount, (osmc.AreasFieldsCount + 1) * 32, osmc.AreasRecordSize);
                    for (int i = 0; i < osmc.FilesAreasWrited; i++)
                    {
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "        - {0}: {1} + {2} + {3} + {4} - {5} полигонов, {6}\r\n", i + 1, osmc.FilesAreasWritedDBFs[i], osmc.FilesAreasWritedShapes[i], osmc.FilesAreasWritedShapes[i].Replace(".shp", ".shx"), osmc.FilesAreasWritedShapes[i].Replace(".shp", ".prj"), osmc.FilesAreasWritedAreas[i], GetFileSize(osmc.FilesAreasWritedSizes[i]));
                        ttl_size += osmc.FilesAreasWritedSizes[i];
                    };
                };
                if (osmc.NodesWrited > 0)
                {
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Узлы линий (полей - 3, размер заголовка 128 байт, записи 34 байт) - обработано {0:P}:\r\n", osmc.NodesPercentage);
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "        - {0}.dbf + {0}.shp + {0}.shx + {0}.prj - {1} узлов, {2}\r\n", osmc.NodesFName, osmc.NodesWrited, GetFileSize(osmc.NodesFSize));
                    ttl_size += osmc.NodesFSize;
                };

                status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "    Общий размер шейпов: {0}\r\n", GetFileSize(ttl_size));

                if (osmc.ProcRelsSure)// && (osmc.FilesRelationsWrited > 0) && (osmc.FilesRelationsWritedRelsCounts[0] > 0))
                {
                    long rel_size = 0;
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Таблицы связей (relations) - {6} (полей - {0}+{1}, размер заголовка {2}+{3} байт, записи {4}+{5} байт):\r\n", osmc.RelationsFieldsCount, osmc.RelationsMembersFieldsCount, osmc.RelationsRecordSize, osmc.RelationsMembersRecordSize, (osmc.RelationsFieldsCount+1)*32, (osmc.RelationsMembersFieldsCount+1)*32, osmc.FilesRelationsWrited);
                    for (int i = 0; i < osmc.FilesRelationsWrited; i++)
                    {
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "        - {0}: {1} + {2} - {3} ({4} объектов) связей, {5}\r\n", i + 1, osmc.FilesRelationsWritedMain[i], osmc.FilesRelationsWritedMems[i], osmc.FilesRelationsWritedRelsCounts[i], osmc.FilesRelationsWritedMemsCounts[i], GetFileSize(osmc.FilesRelationsWritedSizes[i]));
                        rel_size += osmc.FilesRelationsWritedSizes[i];
                    };
                    status_text += String.Format("        R-файл - таблица связей с атрибутами и тегами (relations) - Всего записей: {0}\r\n", osmc.RelationsWrited);
                    status_text += String.Format("        M-файл - таблица объектов (members) внутри связей (relations) - Всего записей: {0}\r\n", osmc.RelationsMemsWrited);
                    status_text += String.Format("    Размер R и M таблиц: {0}\r\n", GetFileSize(rel_size));                    
                };
                if (osmc.ProcJoins) // && (osmc.FilesJoinsWrited > 0) && (osmc.FilesJoinsWritedLines[0] > 0))
                {
                    long join_size = 0;
                    status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "Таблицы запертов поворотов (joins) - {3} (полей - {0}, размер заголовка {1} байт, записи {2} байт):\r\n", osmc.RelationsJoinsFieldsCount, (osmc.RelationsJoinsFieldsCount + 1) * 32, osmc.RelationsJoinsRecordSize, osmc.FilesJoinsWrited);
                    for (int i = 0; i < osmc.FilesJoinsWrited; i++)
                    {
                        status_text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "        - {0}: {1} - {2} линий; {3} запретов, в которых {5} невалидных (сохранено как {4}), {6}\r\n", i + 1, osmc.FilesJoinsWritedDBFs[i], osmc.FilesJoinsWritedLines[i], osmc.FilesJoinsWritedTV[i], osmc.FilesJoinsWritedFL[i], osmc.FilesJoinsWritedER[i], GetFileSize(osmc.FilesJoinsWritedSizes[i]));
                        join_size += osmc.FilesJoinsWritedSizes[i];
                    };
                    //status_text += String.Format("    Всего: {1} ({6} уникальных + {5} задвоений) запретов ({3} валидных, {4} нет; сохранено как {2}) в {0} линиях\r\n", osmc.JoinsRestrictionsLNS, osmc.JoinsRestrictionsTV + osmc.JoinsRestrictionsCOPY, osmc.JoinsRestrictionsFL, osmc.JoinsRestrictionsVLD, osmc.JoinsRestrictionsERR, osmc.JoinsRestrictionsCOPY, osmc.JoinsRestrictionsTV);
                    status_text += String.Format("    Размер таблиц: {0}\r\n", GetFileSize(join_size));
                };                
                
                long fCo = 
                    osmc.FilesPointsWrited * 5 + 
                    osmc.FilesLinesWrited * 5 + 
                    osmc.FilesAreasWrited * 5 +
                    (osmc.NodesWrited > 0 ? 5 : 0) + 
                    (processing ? 0 : 1) + 
                    (osmc._config.useNotInMemoryIndexFile && osmc.ProcWaysSure ? 1 : 0) + 
                    (osmc.ProcRelsSure ? osmc.FilesRelationsWrited * 4 : 0) + 
                    (osmc.ProcJoins ? osmc.FilesJoinsWrited * 2 : 0);
                
                status_text += "\r\n";
                status_text += String.Format("Всего файлов: {0}, включая шейпы, описание и протокол лога {1}\r\n", fCo, Path.GetFileNameWithoutExtension(config.outputFileName) + "[_LOG_].txt");
                status_text += String.Format("Использовано оперативной памяти: {0}\r\n", GetFileSize(System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64));     

                status.Text = text + status_text;

                updated = DateTime.UtcNow;
                Application.DoEvents();
            };
        }

        public static string GetFileSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            };
            string res = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
            return res;
        }

        public void SaveConfig()
        {
            config.Save("OSM2SHPFC.cpb");
        }

        private void props_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SetProperty(0);
        }

        private void props_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 46)
                SetProperty(2);
            if (e.KeyValue == 37)
                SetProperty(-1);
            if (e.KeyValue == 39)
                SetProperty(+1);
            if (e.KeyValue == 113)
                SetProperty(0);
        }

        private void props_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                SetProperty(0);
            if (e.KeyChar == ' ')
                SetProperty(1);
        }

        // 0 - dialog, +1 - list forward, -1 - list backward, 2 - erase
        private void SetProperty(int mode)
        {
            if (in_progress) return;
            if (props.SelectedItems.Count == 0) return;
            int index = props.SelectedIndices[0];
            if (index < 0) return;

            NullProgress();

            //////////////////////////////////
            if ((index == 0) && (mode == 2))
            {
                props.Items[0].SubItems[1].Text = config.inputFileName = "";
            };
            if ((index == 0) && (mode == 0))
            {
                string file = config.inputFileName;
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.FileName = file;
                ofd.Filter = "OSM Files (*.osm;*.pbf)|*.osm;*.pbf";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    file = ofd.FileName;
                    if (File.Exists(file))
                    {
                        if (Path.GetExtension(file).ToLower() == ".osm")
                        {
                            OSMXMLReader fr = new OSMXMLReader(file);
                            if (!fr.ValidHeader)
                            {
                                fr.Close();
                                MessageBox.Show("Неверный заголовок файла", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                fr.Close();
                                config.inputFileName = file;
                                props.Items[0].SubItems[1].Text = Path.GetFileName(file);
                                try
                                {
                                    if (File.Exists(config.inputFileName))
                                        props.Items[0].SubItems[1].Text += "  [" + GetFileSize(new FileInfo(file).Length)+"]";
                                }
                                catch { };
                                props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName = "");
                            };
                        }
                        else if (Path.GetExtension(file).ToLower() == ".pbf")
                        {
                            OSMPBFReader fr = new OSMPBFReader(file);
                            if (!fr.ValidHeader)
                            {
                                fr.Close();
                                MessageBox.Show("Неверный заголовок файла", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                fr.Close();
                                config.inputFileName = file;
                                props.Items[0].SubItems[1].Text = Path.GetFileName(file);
                                try
                                {
                                    if (File.Exists(config.inputFileName))
                                        props.Items[0].SubItems[1].Text += "  [" + GetFileSize(new FileInfo(file).Length) + "]";
                                }
                                catch { };
                                props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName = "");
                            };
                        }
                        else
                        {
                            MessageBox.Show("Неверный тип файла", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        };
                    };
                }
                ofd.Dispose();                    
            };
            //////////////////////////////////
            if ((index == 1) && (mode == 2))
            {
                props.Items[1].SubItems[1].Text = config.outputFileName = "";
            };
            if ((index == 1) && (mode == 0))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".dbf";
                sfd.FileName = config.outputFileName;
                if (String.IsNullOrEmpty(sfd.FileName) && (!String.IsNullOrEmpty(config.inputFileName)))
                    sfd.FileName = config.inputFileName.Replace(Path.GetExtension(config.inputFileName), ".dbf");
                sfd.Filter = "Date Base and Shape Files (*.dbf;*.shp)|*.dbf;*.shp";
                if(sfd.ShowDialog() == DialogResult.OK)
                    props.Items[1].SubItems[1].Text = Path.GetFileName(config.outputFileName = sfd.FileName);
                sfd.Dispose();
            };
            //////////////////////////////////
            if (index == 2)
            {
                if (mode == 2)
                    config.selector = 1;
                else if (mode == 0)
                {
                    int new_cat = config.selector;
                    System.Windows.Forms.InputBox.defWidth = 350;
                    if (InputBox.Show("Выбор селектора", "Какой селектор использовать:", Config.SELECTOR, ref new_cat) == DialogResult.OK)
                        config.selector = (byte)new_cat;
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    int new_cat = config.selector + mode;
                    if (new_cat >= (Config.SELECTOR.Length - 1)) new_cat = Config.SELECTOR.Length - 1;
                    if (new_cat < 0) new_cat = 0;
                    config.selector = (byte)new_cat;
                };
                props.Items[2].SubItems[1].Text = Config.SELECTOR[config.selector];
            };            
            //////////////////////////////////
            if (index == 3)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.onlyHasName = false;
                    props.Items[3].SubItems[1].Text = config.onlyHasName ? "Да" : "";
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.onlyHasName ? 1 : 0;
                    if (InputBox.Show("Только с именем:", "Только с тегом `name`:", options, ref val) == DialogResult.OK)
                    {
                        config.onlyHasName = val == 1;
                        props.Items[3].SubItems[1].Text = config.onlyHasName ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    config.onlyHasName = !config.onlyHasName;
                    props.Items[3].SubItems[1].Text = config.onlyHasName ? "Да" : "";
                };
            };
            //////////////////////////////////
            if (index == 4)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.onlyWithAddr = false;
                    props.Items[4].SubItems[1].Text = config.onlyWithAddr ? "Да" : "";
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.onlyWithAddr ? 1 : 0;
                    if (InputBox.Show("Только адресами:", "Только с адресными тегами:", options, ref val) == DialogResult.OK)
                    {
                        config.onlyWithAddr = val == 1;
                        props.Items[4].SubItems[1].Text = config.onlyWithAddr ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    config.onlyWithAddr = !config.onlyWithAddr;
                    props.Items[4].SubItems[1].Text = config.onlyWithAddr ? "Да" : "";
                };
            };  
            //////////////////////////////////
            if (index == 5)
            {
                if (mode == 2)
                {
                    config.onlyWithTags.Clear();
                    props.Items[5].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[5].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{tag}");
                    options.Add("{tag}={value}");
                    options.Add("{tag},{tag}");
                    options.Add("{tag},{tag}={value}");
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    System.Windows.Forms.InputBox.defWidth = 500;
                    if (InputBox.Show("Только с тегами", "Списко тегов tag=value через запятую:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.onlyWithTags.Clear();
                            props.Items[5].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.onlyWithTags.Clear();
                            foreach (string li in list)
                            {
                                try
                                {
                                    string l = li.Trim().Trim('=');
                                    if (l.IndexOf("=") < 0)
                                        config.onlyWithTags.Add(l, null);
                                    else
                                    {
                                        string[] kv = l.Split(new char[] { '=' }, 2);
                                        config.onlyWithTags.Add(kv[0].Trim(), kv[1].Trim());
                                    };
                                }
                                catch { };
                            };
                            string tl = "";
                            foreach (KeyValuePair<string, string> kvp in config.onlyWithTags)
                            {
                                if (tl.Length > 0) tl += ",";
                                if ((kvp.Value == null) || (kvp.Value == ""))
                                    tl += kvp.Key;
                                else
                                    tl += String.Format("{0}={1}", kvp.Key, kvp.Value);
                            };
                            props.Items[5].SubItems[1].Text = tl;
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 6)
            {
                if (mode == 2)
                {
                    config.onlyOneOfTags.Clear();
                    props.Items[6].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[6].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{tag}");
                    options.Add("{tag}={value}");
                    options.Add("{tag},{tag}");
                    options.Add("{tag},{tag}={value}");
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    System.Windows.Forms.InputBox.defWidth = 500;
                    if (InputBox.Show("Только с одним из тегов", "Списко тегов tag=value через запятую:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.onlyOneOfTags.Clear();
                            props.Items[6].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.onlyOneOfTags.Clear();
                            foreach (string li in list)
                            {
                                try
                                {
                                    string l = li.Trim().Trim('=');
                                    if (l.IndexOf("=") < 0)
                                        config.onlyOneOfTags.Add(l, null);
                                    else
                                    {
                                        string[] kv = l.Split(new char[] { '=' }, 2);
                                        config.onlyOneOfTags.Add(kv[0].Trim(), kv[1].Trim());
                                    };
                                }
                                catch { };
                            };
                            string tl = "";
                            foreach (KeyValuePair<string, string> kvp in config.onlyOneOfTags)
                            {
                                if (tl.Length > 0) tl += ",";
                                if ((kvp.Value == null) || (kvp.Value == ""))
                                    tl += kvp.Key;
                                else
                                    tl += String.Format("{0}={1}", kvp.Key, kvp.Value);
                            };
                            props.Items[6].SubItems[1].Text = tl;
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 7)
            {
                if (mode == 2)
                {
                    config.onlyMdfAfter = DateTime.MinValue;
                    props.Items[7].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = "";
                    List<string> options = new List<string>();
                    options.Add(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                    options.Add("yyyy-MM-ddTHH:mm:ss");
                    if (config.onlyMdfAfter != DateTime.MinValue)
                        options.Add(txt = config.onlyMdfAfter.ToString("yyyy-MM-ddTHH:mm:ss"));
                    System.Windows.Forms.InputBox.defWidth = 182;
                    if (InputBox.Show("Изменения после", "Введите дату и время:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (DateTime.TryParse(txt, out config.onlyMdfAfter))
                            props.Items[7].SubItems[1].Text = config.onlyMdfAfter.ToString("yyyy-MM-ddTHH:mm:ss");
                        else
                        {
                            config.onlyMdfAfter = DateTime.MinValue;
                            props.Items[7].SubItems[1].Text = "";
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 8)
            {
                if (mode == 2)
                {
                    config.onlyMdfBefore = DateTime.MaxValue;
                    props.Items[8].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = "";
                    List<string> options = new List<string>();
                    options.Add(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                    options.Add("yyyy-MM-ddTHH:mm:ss");
                    if (config.onlyMdfBefore != DateTime.MaxValue)
                        options.Add(txt = config.onlyMdfBefore.ToString("yyyy-MM-ddTHH:mm:ss"));
                    System.Windows.Forms.InputBox.defWidth = 182;
                    if (InputBox.Show("Только изменения до", "Введите дату и время:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (DateTime.TryParse(txt, out config.onlyMdfBefore))
                            props.Items[8].SubItems[1].Text = config.onlyMdfBefore.ToString("yyyy-MM-ddTHH:mm:ss");
                        else
                        {
                            config.onlyMdfBefore = DateTime.MaxValue;
                            props.Items[8].SubItems[1].Text = "";
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 9)
            {
                if (mode == 2)
                {
                    config.onlyOfUser.Clear();
                    props.Items[9].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[9].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{user}");
                    options.Add("{user},{user}");
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    if (InputBox.Show("Изменено пользователями", "Список пользователей через запятую:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.onlyOfUser.Clear();
                            props.Items[9].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.onlyOfUser.Clear();
                            foreach (string li in list)
                                config.onlyOfUser.Add(li.Trim());
                            string tl = "";
                            foreach (string user in config.onlyOfUser)
                            {
                                if (tl.Length > 0) tl += ",";
                                tl += user;
                            };
                            props.Items[9].SubItems[1].Text = tl;
                        };
                    };
                };
            };
            //////////////////////////////////
            if (index == 10)
            {
                if (mode == 2)
                {
                    config.onlyVersion.Clear();
                    props.Items[10].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[10].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{id}");
                    options.Add("{id},{id}");
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    if (InputBox.Show("Только с версией", "Список версий через запятую:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.onlyVersion.Clear();
                            props.Items[10].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.onlyVersion.Clear();
                            try
                            {
                                foreach (string li in list)
                                    config.onlyVersion.Add(int.Parse(li.Trim()));
                            }
                            catch { };
                            string tl = "";
                            foreach (int ver in config.onlyVersion)
                            {
                                if (tl.Length > 0) tl += ",";
                                tl += ver.ToString();
                            };
                            props.Items[10].SubItems[1].Text = tl;
                        };
                    };
                };
            };
            //////////////////////////////////
            if (index == 11)
            {
                if (mode == 2)
                {
                    config.onlyInBox = null;
                    props.Items[11].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string[] vals = new string[] { "-90.000000", "-180.000000", "90.000000", "180.000000" };
                    if (config.onlyInBox != null)
                        vals = new string[] { config.onlyInBox[0].ToString(System.Globalization.CultureInfo.InvariantCulture), config.onlyInBox[1].ToString(System.Globalization.CultureInfo.InvariantCulture), config.onlyInBox[2].ToString(System.Globalization.CultureInfo.InvariantCulture), config.onlyInBox[3].ToString(System.Globalization.CultureInfo.InvariantCulture) };
                    System.Windows.Forms.InputBox.defWidth = 300;
                    DialogResult dr = InputBox.QueryMultiple("Только в границах прямоугольника", new string[] { "Min Lat (Нижняя граница / Bottom):", "Min Lon (Левая граница / Left):", "Max Lat (Верхняя граница / Top):", "Max Lon (Правая граница / Right):" }, vals, null, false, null, MessageBoxButtons.YesNoCancel, new string[] { "OK", "Из файла...", "Отмена" });
                    if (dr == DialogResult.No)
                        LoadBoxFromShp();
                    else if(dr == DialogResult.Yes)
                    {
                        if ((vals[0].Trim().Length == 0) || (vals[1].Trim().Length == 0) || (vals[2].Trim().Length == 0) || (vals[3].Trim().Length == 0))
                        {
                            config.onlyInBox = null;
                            props.Items[11].SubItems[1].Text = "";
                        }
                        else
                        {                                                        
                            config.onlyInBox = new double[4];
                            config.onlyInBox[0] = LatLonParser.Parse(vals[0].Trim(), true);
                            config.onlyInBox[1] = LatLonParser.Parse(vals[1].Trim(), false);
                            config.onlyInBox[2] = LatLonParser.Parse(vals[2].Trim(), true);
                            config.onlyInBox[3] = LatLonParser.Parse(vals[3].Trim(), false);
                            props.Items[11].SubItems[1].Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2},{3}", new object[] { config.onlyInBox[0], config.onlyInBox[1], config.onlyInBox[2], config.onlyInBox[3] }); ;                            
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 500;
                };
            };
            //////////////////////////////////
            if ((index == 12) && (mode == 2))
            {
                config.onlyInPolygon = "";
                props.Items[12].SubItems[1].Text = "";
            };
            if ((index == 12) && (mode == 0))
            {
                string file = config.onlyInPolygon;
                System.Windows.Forms.InputBox.defWidth = 300;
                if (InputBox.QueryFileBox("Только внутри полигона", "Выберите файл с полигоном:", ref file, "KML & ESRI Shape files (*.kml;*.shp)|*.kml;*.shp") == DialogResult.OK)
                    if (File.Exists(file))
                    {
                        string ot;
                        try
                        {
                            PointF[] points = UTILS.loadpolyfull(file, out ot);
                            if ((points != null) && (points.Length > 2))
                            {
                                config.onlyInPolygon = file;
                                props.Items[12].SubItems[1].Text = Path.GetFileName(file);
                            }
                            else
                            {
                                MessageBox.Show("Не удается загрузить полигон", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            };
                        }
                        catch
                        {
                            MessageBox.Show("Не удается загрузить полигон", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        };
                    };
            };
            //////////////////////////////////
            if (index == 13)
            {
                if (mode == 2)
                {
                    config.dbfMainFields.Clear();
                    config.dbfMainFields.AddRange(Config.defMainFields);
                    string tl = "";
                    foreach (string dopf in config.dbfMainFields)
                    {
                        if (tl.Length > 0) tl += ",";
                        tl += dopf;
                    };
                    props.Items[13].SubItems[1].Text = tl;
                }
                else if (mode == 0)
                {
                    string txt = props.Items[13].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.AddRange(Config.defMainFields);
                    {
                        string tl = "";
                        foreach (string dopf in Config.defMainFields)
                        {
                            if (tl.Length > 0) tl += ",";
                            tl += dopf;
                        };
                        options.Add(tl);
                    };
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    System.Windows.Forms.InputBox.defWidth = 500;
                    if (InputBox.Show("Основные поля DBF", "Список включаемых основных полей через запятую:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.dbfMainFields.Clear();
                            props.Items[13].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.dbfMainFields.Clear();
                            foreach (string li in list)
                                for (int i = 0; i < Config.defMainFields.Length; i++)
                                    if (Config.defMainFields[i] == li.Trim())
                                        if (!config.dbfMainFields.Contains(li.Trim()))
                                            config.dbfMainFields.Add(li.Trim());
                            string tl = "";
                            foreach (string user in config.dbfMainFields)
                            {
                                if (tl.Length > 0) tl += ",";
                                tl += user;
                            };
                            props.Items[13].SubItems[1].Text = tl;
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };   
            //////////////////////////////////
            if (index == 14)
            {
                if (mode == 2)
                {
                    config.dbfDopFields.Clear();
                    props.Items[14].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[14].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{field_name}");
                    options.Add("{field_name[length]}");
                    options.Add("{field_name},{field_name[length]}");
                    options.Add("{tag:name}");
                    options.Add("{tag:name},{tag:name}");
                    options.Add("{field_name},{tag:name[length]}");
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    System.Windows.Forms.InputBox.defWidth = 500;
                    if (InputBox.Show("Дополнительные поля DBF", "Список полей через запятую (максимум 200):", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.dbfDopFields.Clear();
                            props.Items[14].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.dbfDopFields.Clear();
                            int fc = 0;
                            foreach (string lil in list)
                            {
                                if (++fc >= 200) continue;
                                string li = lil.Trim().Replace(" ", "");
                                if ((li.IndexOf("[") > 0) && (li.IndexOf("]") > 0))
                                {
                                    string fn = li.Substring(0, li.IndexOf("["));
                                    string ln = li.Substring(li.IndexOf("[") + 1, li.IndexOf("]") - li.IndexOf("[") - 1);
                                    byte flen = 200;
                                    byte.TryParse(ln, out flen);
                                    if (flen < 2) flen = 2;
                                    if (flen > 254) flen = 254;
                                    config.dbfDopFields.Add(fn + "[" + flen.ToString() + "]");
                                }
                                else if ((li.IndexOf("[") > 0) || (li.IndexOf("]") > 0))
                                    config.dbfDopFields.Add(li.Substring(0,Math.Max(li.IndexOf("["), li.IndexOf("]"))));
                                else
                                    config.dbfDopFields.Add(li);
                            };
                            string tl = "";
                            foreach (string user in config.dbfDopFields)
                            {
                                if (tl.Length > 0) tl += ",";
                                tl += user;
                            };
                            props.Items[14].SubItems[1].Text = tl;
                        };
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 15)
            {
                if (mode == 2)
                {
                    config.MaxFileRecords = 500000;
                    props.Items[15].SubItems[1].Text = "500000";
                }
                else if (mode == 0)
                {
                    int val = config.MaxFileRecords;
                    System.Windows.Forms.InputBox.defWidth = 182;
                    if (InputBox.Show("Максимальное число записей", "Максимальное число записей в файле:", ref val, 1000, 5000000) == DialogResult.OK)
                    {
                        config.MaxFileRecords = val;
                        if (config.MaxFileRecords > 5000000) config.MaxFileRecords = 5000000;
                        if (config.MaxFileRecords < 1000) config.MaxFileRecords = 1000;
                        props.Items[15].SubItems[1].Text = config.MaxFileRecords.ToString();
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    config.MaxFileRecords = ((config.MaxFileRecords / 1000) + mode) * 1000;
                    if (config.MaxFileRecords > 5000000) config.MaxFileRecords = 5000000;
                    if (config.MaxFileRecords < 1000) config.MaxFileRecords = 1000;
                    props.Items[15].SubItems[1].Text = config.MaxFileRecords.ToString();
                };
            };
            //////////////////////////////////
            if (index == 16)
            {
                if (mode == 2)
                {
                    props.Items[16].SubItems[1].Text = (config.allTagsFormat = "{k}={v};");
                };
                if ((mode == 1) || (mode == -1))
                {
                    List<string> options = new List<string>();
                    options.Add("{k}={v}\\r\\n");
                    options.Add("{k}={v}\\r");
                    options.Add("{k}={v}\\n");
                    options.Add("{k}={v};\\r\\n");
                    options.Add("{k}={v};");
                    options.Add("{k}={v}; ");
                    options.Add("{k}={v},");
                    options.Add("{k}={v}, ");
                    options.Add("{k}:{v}\\r\\n");
                    options.Add("{k}:{v}\\r");
                    options.Add("{k}:{v}\\n");
                    options.Add("{k}:{v};\\r\\n");
                    options.Add("{k}:{v};");
                    options.Add("{k}:{v}; ");
                    options.Add("{k}:{v},");
                    options.Add("{k}:{v}, ");

                    int si = options.IndexOf(config.allTagsFormat);

                    si += mode;
                    if (si < 0) si = 0;
                    if (si >= (options.Count - 1)) si = options.Count - 1;
                    props.Items[16].SubItems[1].Text = (config.allTagsFormat = options[si]);
                };
                if (mode == 0)
                {
                    string txt = props.Items[16].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{k}={v}\\r\\n");                    
                    options.Add("{k}={v}\\r");
                    options.Add("{k}={v}\\n");
                    options.Add("{k}={v};\\r\\n");
                    options.Add("{k}={v};");
                    options.Add("{k}={v}; ");
                    options.Add("{k}={v},");
                    options.Add("{k}={v}, ");
                    options.Add("{k}:{v}\\r\\n");                    
                    options.Add("{k}:{v}\\r");
                    options.Add("{k}:{v}\\n");
                    options.Add("{k}:{v};\\r\\n");
                    options.Add("{k}:{v};");
                    options.Add("{k}:{v}; ");
                    options.Add("{k}:{v},");
                    options.Add("{k}:{v}, ");
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    if (InputBox.Show("Формат записи тегов", "Формат записи тегов {k}={v}:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        txt = txt.Replace("\\r", "\r").Replace("\\n", "\n");
                        if (txt.Length != 0)
                            props.Items[16].SubItems[1].Text = (config.allTagsFormat = txt).Replace("\r", "\\r").Replace("\n", "\\n");
                    };
                };
            };            
            //////////////////////////////////
            if (index == 17)
            {
                if (mode == 2)
                {
                    props.Items[17].SubItems[1].Text = (config.useAggPrefix = "");
                }
                else if (mode == 0)
                {
                    InputBox.defWidth = 182;
                    string txt = props.Items[17].SubItems[1].Text;
                    if (InputBox.Show("Префикс в поле агрегаторе", "Вставлять префикc в начало поля:", ref txt) == DialogResult.OK)
                    {
                        txt = txt.Replace("\\r", "\r").Replace("\\n", "\n");
                        if (txt.Length != 0)
                            props.Items[17].SubItems[1].Text = (config.useAggPrefix = txt).Replace("\r", "\\r").Replace("\n", "\\n");
                        else
                            props.Items[17].SubItems[1].Text = (config.useAggPrefix = "");
                    };
                    InputBox.defWidth = 300;
                }
                else
                {
                    List<string> ava = new List<string>(new string[] { "", ";", ",", ".", "@", "$", "!", "#", "%", "&", "*", "~", "?", "_", "-", "+" });
                    int avi = ava.IndexOf(config.useAggPrefix);
                    avi += mode;
                    if (avi >= ava.Count) avi = 0;
                    if (avi < 0) avi = ava.Count - 1;
                    props.Items[17].SubItems[1].Text = config.useAggPrefix = ava[avi];
                };
            };
            //////////////////////////////////
            if (index == 18)
            {
                if (mode == 2)
                {
                    config.maxAggTags = 8;
                    props.Items[18].SubItems[1].Text = "8";
                }
                else if (mode == 0)
                {
                    int val = config.maxAggTags;
                    System.Windows.Forms.InputBox.defWidth = 182;
                    if (InputBox.Show("Число агрегирующих", "Число агрегирующих полей TAG_X:", ref val, 1, 40) == DialogResult.OK)
                    {
                        config.maxAggTags = (sbyte)val;
                        if (config.maxAggTags > 40) config.maxAggTags = 40;
                        if (config.maxAggTags < 1) config.maxAggTags = 1;
                        props.Items[18].SubItems[1].Text = config.maxAggTags.ToString();
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    config.maxAggTags += (sbyte)mode;
                    if (config.maxAggTags > 40) config.maxAggTags = 40;
                    if (config.maxAggTags < 1) config.maxAggTags = 1;
                    props.Items[18].SubItems[1].Text = config.maxAggTags.ToString();
                };
            };
            //////////////////////////////////
            if (index == 19)
            {
                if (mode == 2)
                {
                    props.Items[19].SubItems[1].Text = config.scriptFilter = "";
                };
                if (mode == 0)
                {
                    ScriptEditor se = new ScriptEditor();                    
                    se.Script = String.IsNullOrEmpty(config.scriptFilter) ? "return true;" : config.scriptFilter;
                    DialogResult dr = DialogResult.None;
                    while (true)
                    {
                        if (se.ShowDialog() == DialogResult.Cancel) 
                            break;
                        string txt = se.Script.Trim();
                        if (txt == "return true;") txt = "";
                        if(String.IsNullOrEmpty(txt))
                        {
                            props.Items[19].SubItems[1].Text = config.scriptFilter = "";
                            break;
                        }
                        else
                        {
                            try
                            {
                                string code = ApplyFilterScript.GetCode(txt);
                                System.Reflection.Assembly asm = CSScriptLibrary.CSScript.LoadCode(code, null);
                                CSScriptLibrary.AsmHelper script = new CSScriptLibrary.AsmHelper(asm);
                                ApplyFilterScript afs = (ApplyFilterScript)script.CreateObject("OSM2SHP.Script");
                                props.Items[19].SubItems[1].Text = (config.scriptFilter = txt).Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                                break;
                            }
                            catch (Exception ex)
                            {
                                dr = MessageBox.Show("Ошибка в скрипте: \r\n" + ex.Message, this.MandatoryText, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                if (dr == DialogResult.Cancel)
                                    break;
                            };
                        };
                    };
                    se.Dispose();
                };
            };
            //////////////////////////////////
            if (index == 20)
            {
                if (mode == 2)
                {
                    config.dbfCodePage = 201; // 0xC9 // Windows-1251
                    props.Items[20].SubItems[1].Text = config.CodePages[config.dbfCodePage].codeName;
                };
                if ((mode == 1) || (mode == -1))
                {                 
                    int si = -1;
                    for (int i = 0; i < config.CodePages.Count; i++)
                        if (config.CodePages[i].headerCode == config.dbfCodePage)
                            si = i;

                    si += mode;
                    if (si >= (config.CodePages.Count - 1)) si = config.CodePages.Count - 1;
                    if (si < 0) si = 0;
                    config.dbfCodePage = config.CodePages[si].headerCode;
                    props.Items[20].SubItems[1].Text = config.CodePages[config.dbfCodePage].codeName;
                };
                if (mode == 0)
                {
                    string[] options = new string[config.CodePages.Count];
                    int idx = -1;
                    for (int i = 0; i < options.Length; i++)
                    {
                        if ((config.CodePages[i].headerCode == 201) && (idx == -1)) idx = i;
                        if ((index == -1) && (config.CodePages[i].headerCode == 201)) idx = i;
                        if (config.dbfCodePage == config.CodePages[i].headerCode) idx = i;
                        options[i] = config.CodePages[i].codeName;
                    };
                    System.Windows.Forms.InputBox.defWidth = 500;                    
                    if (InputBox.Show("Кодировка текста", "Выберите кодовую страницу DBF:", options, ref idx) == DialogResult.OK)
                    {
                        config.dbfCodePage = config.CodePages[idx].headerCode;
                        props.Items[20].SubItems[1].Text = config.CodePages[config.dbfCodePage].codeName;
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 21)
            {
                if (mode == 2)
                {
                    config.aggrRegex = @"";
                    props.Items[21].SubItems[1].Text = config.aggrRegex;
                };
                if((mode == 1) || (mode == -1))
                {
                    List<string> options = new List<string>();
                    options.Add(@"");
                    options.Add(@"(:ru)$");
                    options.Add(@"(:ru|:en)$");
                    options.Add(@"(.*(?<!:en)$)");
                    options.Add(@"(.*(?<!:en|:kz)$)");
                    options.Add(@"(.*(?<!:\w{2}|:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!:\w{2,3}|:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!name:\w{2}|name:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!name:\w{2,3}|name:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2}|addr:[\w-]+:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2,3}|addr:[\w-]+:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2}|addr:[\w-]+:\w{2}-[\w-]+|name:\w{2}|name:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2,3}|addr:[\w-]+:\w{2,3}-[\w-]+|name:\w{2,3}|name:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");

                    int si = options.IndexOf(config.aggrRegex);

                    si += mode;
                    if (si < 0) si = 0;
                    if (si >= (options.Count - 1)) si = options.Count - 1;
                    config.aggrRegex = options[si];
                    props.Items[21].SubItems[1].Text = config.aggrRegex;
                };
                if (mode == 0)
                {
                    string txt = props.Items[21].SubItems[1].Text;      
                    System.Windows.Forms.InputBox.defWidth = 600;
                    List<string> options = new List<string>();
                    options.Add(@"(:ru)$");
                    options.Add(@"(:ru|:en)$");
                    options.Add(@"(.*(?<!:en)$)");
                    options.Add(@"(.*(?<!:en|:kz)$)");
                    options.Add(@"(.*(?<!:\w{2}|:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!:\w{2,3}|:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!name:\w{2}|name:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!name:\w{2,3}|name:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2}|addr:[\w-]+:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2,3}|addr:[\w-]+:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2}|addr:[\w-]+:\w{2}-[\w-]+|name:\w{2}|name:\w{2}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    options.Add(@"(.*(?<!addr:[\w-]+:\w{2,3}|addr:[\w-]+:\w{2,3}-[\w-]+|name:\w{2,3}|name:\w{2,3}-[\w-]+)$)|((:ru|:ru-[\w-]+|:en|:en-[\w-]+)$)");
                    if ((txt != "") && (options.IndexOf(txt) < 0))
                        options.Add(txt);
                    if (InputBox.QueryRegexBox("Агрегировать теги", "Регулярное выражение для выборки тегов (проверка: regexstorm.net):", "Поле для проверки тэгов:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        config.aggrRegex = txt;
                        try { Regex rx = new Regex(txt); } catch { config.aggrRegex = ""; };
                        props.Items[21].SubItems[1].Text = config.aggrRegex;
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                };
            };
            //////////////////////////////
            if (index == 22)
            {
                if (mode == 2)
                {
                    config.hasText = "";
                    props.Items[22].SubItems[1].Text = config.hasText;
                }
                else if (mode == 0)
                {
                    string txt = config.hasText;
                    List<string> options = new List<string>();
                    options.Add("\\bметро\\b");
                    options.Add("\\bметро(политен)*\\b");
                    if (!String.IsNullOrEmpty(txt)) options.Add(txt);
                    InputBox.defWidth = 500;
                    if (InputBox.QueryRegexBox("Node contains text", "Node contains tag or tag with text:", "You can test here:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        config.hasText = txt == "" ? null : txt.ToLower();
                        props.Items[22].SubItems[1].Text = config.hasText;
                    };
                    InputBox.defWidth = 300;
                };
            };
            //////////////////////////////////
            if (index == 23)
            {
                string[] options = new string[] { "Обрабатывать контуры", "Не обрабатывать контуры" };
                if (mode == 2)
                    config.ignoreWays = false;                    
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.ignoreWays ? 1 : 0;
                    if (InputBox.Show("Обработка контуров:", "Производить обработку контуров (Ways):", options, ref val) == DialogResult.OK)
                        config.ignoreWays = val == 1;                        
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                    config.ignoreWays = !config.ignoreWays;
                props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
                props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
                props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
                props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
                if (config.ignoreWays)
                {
                    config.addFirstAndLastNodesIdToLines = false;
                    props.Items[35].SubItems[1].Text = config.addFirstAndLastNodesIdToLines ? "Да" : "";
                    config.addFisrtAndLastNodesLns2Memory = false;
                    props.Items[36].SubItems[1].Text = config.addFisrtAndLastNodesLns2Memory ? "Да" : "";
                    config.saveLineNodesShape = false;
                    props.Items[37].SubItems[1].Text = config.saveLineNodesShape ? "Да" : "";
                };
            };////////////////////
            if (index == 24)
            {
                string[] options = new string[] { "Не обрабатывать и не сохранять линии", "Сохранять линии" };
                if (mode == 2)
                    config.processLines = false;                    
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processLines ? 1 : 0;
                    if (InputBox.Show("Линии:", "Обрабатывать и сохранять линии:", options, ref val) == DialogResult.OK)
                        config.processLines = val == 1;
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                    config.processLines = !config.processLines;
                props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
                props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
                props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
                props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
                if (!config.processLines)
                {
                    config.addFirstAndLastNodesIdToLines = false;
                    props.Items[35].SubItems[1].Text = config.addFirstAndLastNodesIdToLines ? "Да" : "";
                    config.addFisrtAndLastNodesLns2Memory = false;
                    props.Items[36].SubItems[1].Text = config.addFisrtAndLastNodesLns2Memory ? "Да" : "";
                    config.saveLineNodesShape = false;
                    props.Items[37].SubItems[1].Text = config.saveLineNodesShape ? "Да" : "";
                };
            };
            //////////////////////////////////
            if (index == 25)
            {
                string[] options = new string[] { "Не обрабатывать и не сохранять полигоны", "Сохранять полигоны" };
                if (mode == 2)
                    config.processAreas = false;                    
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processAreas ? 1 : 0;
                    if (InputBox.Show("Полигоны:", "Обрабатывать и сохранять полигоны:", options, ref val) == DialogResult.OK)
                        config.processAreas = val == 1;
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                    config.processAreas = !config.processAreas;
                props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
                props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
                props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
                props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
            };
            //////////////////////////////////
            if (index == 26)
            {
                if (mode == 2)
                    config.processCentroids = 0;
                else if (mode == 0)
                {
                    int new_prc = config.processCentroids;
                    System.Windows.Forms.InputBox.defWidth = 350;
                    if (InputBox.Show("Центроиды", "Обрабатывать центроиды контуров как узлы (Nodes):", Config.PROCCENTROID, ref new_prc) == DialogResult.OK)
                        config.processCentroids = (byte)new_prc;
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    int new_prc = config.processCentroids + mode;
                    if (new_prc >= (Config.PROCCENTROID.Length - 1)) new_prc = Config.PROCCENTROID.Length - 1;
                    if (new_prc < 0) new_prc = 0;
                    config.processCentroids = (byte)new_prc;
                };
                props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
                props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
                props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
                props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
            };            
            //////////////////////////////////
            if (index == 27)
            {
                string[] options = new string[] { "[Нет] Не применять фильтр к линиям", "[Да] Применять фильтр к линиям" };                
                if (mode == 2)
                    config.processLineFilter = false;
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processLineFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Применять фильтр при обработке линий:", options, ref val) == DialogResult.OK)
                        config.processLineFilter = val == 1;
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                    config.processLineFilter = !config.processLineFilter;
                props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
                props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
                props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
                props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
            };
            //////////////////////////////////
            if (index == 28)
            {
                string[] options = new string[] { "[Нет] Не применять фильтр к полигонам", "[Да] Применять фильтр к полигонам" };
                if (mode == 2)
                    config.processAreaFilter = false;
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processAreaFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Применять фильтр при обработке полигонов:", options, ref val) == DialogResult.OK)
                        config.processAreaFilter = val == 1;
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                    config.processAreaFilter = !config.processAreaFilter;
                props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                props.Items[25].SubItems[1].Text = config.processAreas ? "[Да] Сохранять полигоны" : "[Нет] Не обрабатывать и не сохранять полигоны";
                props.Items[26].SubItems[1].Text = Config.PROCCENTROID[config.processCentroids];
                props.Items[27].SubItems[1].Text = config.processLineFilter ? "[Да] Применять фильтр к линиям" : "[Нет] Не применять фильтр к линиям";
                props.Items[28].SubItems[1].Text = config.processAreaFilter ? "[Да] Применять фильтр к полигонам" : "[Нет] Не применять фильтр к полигонам";
            };
            //////////////////////////////////
            if (index == 29)
            {
                string[] options = new string[] { "Хранить индекс в оперативной памяти (быстро)", "Хранить индекс в файле на диске (медленно)" };
                if (mode == 2)
                {
                    config.useNotInMemoryIndexFile = false;
                    props.Items[29].SubItems[1].Text = config.useNotInMemoryIndexFile ? "Хранить индекс в файле на диске (медленно)" : "Хранить индекс в оперативной памяти (быстро)";
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.useNotInMemoryIndexFile ? 1 : 0;
                    if (InputBox.Show("Индекс точек:", "Использовать внешний индексный файл для точек (медленно):", options, ref val) == DialogResult.OK)
                    {
                        config.useNotInMemoryIndexFile = val == 1;
                        props.Items[29].SubItems[1].Text = config.useNotInMemoryIndexFile ? "Хранить индекс в файле на диске (медленно)" : "Хранить индекс в оперативной памяти (быстро)";
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    config.useNotInMemoryIndexFile = !config.useNotInMemoryIndexFile;
                    props.Items[29].SubItems[1].Text = config.useNotInMemoryIndexFile ? "Хранить индекс в файле на диске (медленно)" : "Хранить индекс в оперативной памяти (быстро)";
                };
            };
            //////////////////////////////////
            if (index == 30)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.processRelations = false;
                    props.Items[30].SubItems[1].Text = config.processRelations ? "Да" : "";
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processRelations ? 1 : 0;
                    if (InputBox.Show("Сохранять связи:", "Обрабатывать связи (Relations) и сохранять в таблицу:", options, ref val) == DialogResult.OK)
                    {
                        config.processRelations = val == 1;
                        props.Items[30].SubItems[1].Text = config.processRelations ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 300;
                }
                else
                {
                    config.processRelations = !config.processRelations;
                    props.Items[30].SubItems[1].Text = config.processRelations ? "Да" : "";
                };
                if (!config.processRelations)
                {
                    props.Items[31].SubItems[1].Text = (config.processRelationFilter = false) == true ? "Да" : "";
                    props.Items[34].SubItems[1].Text = (config.relationsAsJoins = false) == true  ? "Да" : "";
                    props.Items[36].SubItems[1].Text = (config.addFisrtAndLastNodesLns2Memory = false) == true ? "Да" : "";
                };
            };
            //////////////////////////////////
            if (index == 31)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.processRelationFilter = false;
                    props.Items[31].SubItems[1].Text = config.processRelationFilter ? "Да" : "";
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processRelationFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Применять фильтры при обработке связей (Relations):", options, ref val) == DialogResult.OK)
                    {
                        config.processRelationFilter = val == 1;
                        props.Items[31].SubItems[1].Text = config.processRelationFilter ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 310;
                }
                else
                {
                    config.processRelationFilter = !config.processRelationFilter;
                    props.Items[31].SubItems[1].Text = config.processRelationFilter ? "Да" : "";
                };
                if(config.processRelationFilter)
                    props.Items[30].SubItems[1].Text = (config.processRelations = true) == true ? "Да" : "";
            };
            //////////////////////////////////
            if (index == 32)
            {
                if (mode == 2)
                {
                    config.relationTypesAsLine.Clear();
                    props.Items[32].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[32].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{type}");
                    options.Add("{type},{type}");
                    options.Add("route,waterway,network,pipeline,power"); // site
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    if (InputBox.Show("Связь как линия", "Типы связей через запятую, обрабатываемых как линия:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.relationTypesAsLine.Clear();
                            props.Items[32].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.relationTypesAsLine.Clear();
                            foreach (string li in list)
                                config.relationTypesAsLine.Add(li.Trim());
                            string tl = "";
                            foreach (string user in config.relationTypesAsLine)
                            {
                                if (tl.Length > 0) tl += ",";
                                tl += user;
                            };
                            props.Items[32].SubItems[1].Text = tl;
                        };
                    };
                };
            };
            //////////////////////////////////
            if (index == 33)
            {
                if (mode == 2)
                {
                    config.relationTypesAsArea.Clear();
                    props.Items[33].SubItems[1].Text = "";
                };
                if (mode == 0)
                {
                    string txt = props.Items[33].SubItems[1].Text;
                    List<string> options = new List<string>();
                    options.Add("{type}");
                    options.Add("{type},{type}");
                    //options.Add("boundary,site"); // site
                    if (txt.Length > 0)
                        if (options.IndexOf(txt) < 0)
                            options.Add(txt);
                    if (InputBox.Show("Связь как линия", "Типы связей через запятую, обрабатываемых как линия:", options.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if (txt.Trim().Length == 0)
                        {
                            config.relationTypesAsArea.Clear();
                            props.Items[33].SubItems[1].Text = "";
                        }
                        else
                        {
                            string[] list = txt.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            config.relationTypesAsArea.Clear();
                            foreach (string li in list)
                                config.relationTypesAsArea.Add(li.Trim());
                            string tl = "";
                            foreach (string user in config.relationTypesAsArea)
                            {
                                if (tl.Length > 0) tl += ",";
                                tl += user;
                            };
                            props.Items[33].SubItems[1].Text = tl;
                        };
                    };
                };
            };
            //////////////////////////////////
            if (index == 34)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.relationsAsJoins = false;                    
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processRelationFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Сохранять Relations как Joins (Запреты поворотов):", options, ref val) == DialogResult.OK)
                    {
                        config.relationsAsJoins = val == 1;
                        props.Items[34].SubItems[1].Text = config.relationsAsJoins ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 310;
                }
                else
                {
                    config.relationsAsJoins = !config.relationsAsJoins;                    
                };
                props.Items[34].SubItems[1].Text = config.relationsAsJoins ? "Да" : "";
                if (config.relationsAsJoins)
                    props.Items[30].SubItems[1].Text = (config.processRelations = true) == true ? "Да" : "";
            };
            //////////////////////////////////
            if (index == 35)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.addFirstAndLastNodesIdToLines = false;
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processRelationFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Добавлять ID первой и последней точки к линиям:", options, ref val) == DialogResult.OK)
                    {
                        config.addFirstAndLastNodesIdToLines = val == 1;
                        props.Items[35].SubItems[1].Text = config.addFirstAndLastNodesIdToLines ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 310;
                }
                else
                {
                    config.addFirstAndLastNodesIdToLines = !config.addFirstAndLastNodesIdToLines;                    
                };
                props.Items[35].SubItems[1].Text = config.addFirstAndLastNodesIdToLines ? "Да" : "";
                if(config.addFirstAndLastNodesIdToLines)
                {
                    config.processWays = true;
                    config.processLines = true;
                    props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                    props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                };                
            };
            //////////////////////////////////
            if (index == 36)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.addFisrtAndLastNodesLns2Memory = false;
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processRelationFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Хранить для обработки запретов в памяти ID первой и последней точек для каждой линии:", options, ref val) == DialogResult.OK)
                    {
                        config.addFisrtAndLastNodesLns2Memory = val == 1;
                        props.Items[36].SubItems[1].Text = config.addFisrtAndLastNodesLns2Memory ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 310;
                }
                else
                {
                    config.addFisrtAndLastNodesLns2Memory = !config.addFisrtAndLastNodesLns2Memory;                    
                };
                props.Items[36].SubItems[1].Text = config.addFisrtAndLastNodesLns2Memory ? "Да" : "";
                if (config.addFisrtAndLastNodesLns2Memory)
                {                    
                    config.processWays = true;
                    config.processLines = true;
                    props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                    props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";
                    config.relationsAsJoins = true;
                    config.processRelations = true;
                    props.Items[30].SubItems[1].Text = config.processRelations ? "Да" : "";
                    props.Items[34].SubItems[1].Text = config.relationsAsJoins ? "Да" : "";                    
                };
            };
            //////////////////////////////////
            if (index == 37)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.saveLineNodesShape = false;
                }
                else if (mode == 0)
                {
                    System.Windows.Forms.InputBox.defWidth = 182;
                    int val = config.processRelationFilter ? 1 : 0;
                    if (InputBox.Show("Фильтр:", "Хранить для обработки запретов в памяти ID первой и последней точек для каждой линии:", options, ref val) == DialogResult.OK)
                    {
                        config.saveLineNodesShape = val == 1;
                        props.Items[37].SubItems[1].Text = config.saveLineNodesShape ? "Да" : "";
                    };
                    System.Windows.Forms.InputBox.defWidth = 310;
                }
                else
                {
                    config.saveLineNodesShape = !config.saveLineNodesShape;                    
                };
                props.Items[37].SubItems[1].Text = config.saveLineNodesShape ? "Да" : "";
                if (config.saveLineNodesShape)
                {
                    config.processWays = true;
                    config.processLines = true;
                    props.Items[23].SubItems[1].Text = config.processWays ? "[Да] Обрабатывать контуры" : "[Нет] Не обрабатывать контуры";
                    props.Items[24].SubItems[1].Text = config.processLines ? "[Да] Сохранять линии" : "[Нет] Не обрабатывать и не сохранять линии";                    
                };
            };
            //////////////////////////////////
            if (index == 38)
            {
                string[] options = new string[] { "Нет", "Да" };
                if (mode == 2)
                {
                    config.sortAggTagsPriority = "";
                }
                else if (mode == 0)
                {
                    string txt = props.Items[38].SubItems[1].Text;
                    List<string> opts = new List<string>();
                    opts.Add("нет");
                    opts.Add("да");
                    opts.Add("addr,addr:ru,name,name:ru,addr:*,name:*");
                    opts.Add("area,building,highway,amenity,nature,place,addr,addr:ru,name,name:ru,addr:*,name:*");
                    if (txt.Length > 0)
                        if (opts.IndexOf(txt) < 0)
                            opts.Add(txt);
                    if (InputBox.Show("Сортировать агрер. теги", "Сортировать или нет агрегированные теги и в каком порядке приоритета:", opts.ToArray(), ref txt, true) == DialogResult.OK)
                    {
                        if ((txt.Trim().Length == 0) || (txt.Trim() == "нет"))
                            config.sortAggTagsPriority = "";
                        else
                            config.sortAggTagsPriority = txt.Trim();
                    };
                }
                else
                {
                    if ((config.sortAggTagsPriority == null) || (config.sortAggTagsPriority == ""))
                        config.sortAggTagsPriority = "да";
                    else
                        config.sortAggTagsPriority = "";
                };
                props.Items[38].SubItems[1].Text = config.sortAggTagsPriority;                
            };
            //////////////////////////////
            //////////////////////////////
            //////////////////////////////
        }

        private void LoadBoxFromShp()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Shape Files (*.shp)|*.shp";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            };
            try
            {
                int tof = 0;
                PointF[] poly = UTILS.loadPoly(ofd.FileName, out tof);
                if ((poly != null) && (poly.Length > 0))
                {
                    config.onlyInBox = new double[] { double.MaxValue, double.MaxValue, double.MinValue, double.MinValue };
                    for (int i = 0; i < poly.Length; i++)
                    {
                        if (poly[i].Y < config.onlyInBox[0]) config.onlyInBox[0] = poly[i].Y;
                        if (poly[i].X < config.onlyInBox[1]) config.onlyInBox[1] = poly[i].X;
                        if (poly[i].Y > config.onlyInBox[2]) config.onlyInBox[2] = poly[i].Y;
                        if (poly[i].X > config.onlyInBox[3]) config.onlyInBox[3] = poly[i].X;
                    };
                    props.Items[11].SubItems[1].Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}", new object[] { config.onlyInBox[0], config.onlyInBox[1], config.onlyInBox[2], config.onlyInBox[3] }); ;
                };
            }
            catch { };
            ofd.Dispose();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Config files (*.o2s)|*.o2s";
            if (ofd.ShowDialog() == DialogResult.OK)
                OpenFile(ofd.FileName);                
            ofd.Dispose();
        }

        private void OpenFile(string fileName)
        {
            config = Config.Load(fileName, true);
            config.onReloadProperties += new EventHandler(config_onReloadProperties);
            config.ReloadProperties();
            mrul.AddFile(fileName);
            recentToolStripMenuItem.Enabled = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfdd = new SaveFileDialog();
            sfdd.DefaultExt = ".o2s";
            sfdd.Filter = "Config files (*.o2s)|*.o2s";
            if (sfdd.ShowDialog() == DialogResult.OK)
            {
                config.Save(sfdd.FileName);
                mrul.AddFile(sfdd.FileName);
                recentToolStripMenuItem.Enabled = true;
            };
            sfdd.Dispose();
        }

        private void SetPropsEnables(bool enabled)
        {
            reset = 0;
            in_progress = !enabled;
            nEWToolStripMenuItem.Enabled = enabled;
            loadToolStripMenuItem.Enabled = enabled;
            saveToolStripMenuItem.Enabled = enabled;
            defToolStripMenuItem.Enabled = enabled;
            chvToolStripMenuItem.Enabled = enabled;
            exitToolStripMenuItem.Enabled = enabled;
            contextMenuStrip1_Opening(contextMenuStrip1, new CancelEventArgs());
        }

        private void MForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (in_progress)
            {
                if (e.CloseReason == CloseReason.WindowsShutDown)
                    reset = 2;
                else
                {
                    e.Cancel = true;
                    if (contextMenuStrip1.Visible) contextMenuStrip1.Close();
                    MessageBox.Show("Подождите пока не закончится ковертация", this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                };
            };
        }

        private void MForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if ((httpServer != null) && (httpServer.Running))
                    httpServer.Stop();
            }
            catch { };
            SaveConfig();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            xmlcToolStripMenuItem.Enabled = (in_progress == false);
            cHECKCURRToolStripMenuItem.Enabled = (in_progress == false);
            dOToolStripMenuItem.Enabled = (in_progress == false) && (File.Exists(config.inputFileName)) && (!String.IsNullOrEmpty(config.outputFileName));
            dOToolStripMenuItem.Visible = !in_progress;
            helpToolStripMenuItem.Enabled = !in_progress;
            stopToolStripMenuItem.Visible = in_progress;
            sTTSToolStripMenuItem.Enabled = (!String.IsNullOrEmpty(status.Text)) && (!in_progress);            
        }

        private void dOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Convert();
        }

        private void MForm_Load(object sender, EventArgs e)
        {
            if ((this.args != null) && (this.args.Length > 0))
            {
                foreach (string arg in this.args)
                {
                    if (File.Exists(arg))
                    {
                        string file = arg;
                        if (Path.GetExtension(file).ToLower() == ".osm")
                        {
                            try
                            {
                                OSMXMLReader fr = new OSMXMLReader(file);
                                if (fr.ValidHeader)
                                {
                                    config.inputFileName = file;
                                    props.Items[0].SubItems[1].Text = Path.GetFileName(file);
                                };
                                fr.Close();
                            }
                            catch { };
                        }
                        else if (Path.GetExtension(file).ToLower() == ".pbf")
                        {
                            try
                            {
                                OSMPBFReader fr = new OSMPBFReader(file);
                                if (fr.ValidHeader)
                                {
                                    config.inputFileName = file;
                                    props.Items[0].SubItems[1].Text = Path.GetFileName(file);
                                };
                                fr.Close();
                            }
                            catch { };
                        }
                        else if (Path.GetExtension(file).ToLower() == ".dbf")
                        {
                            try
                            {
                                config.outputFileName = file;
                                props.Items[1].SubItems[1].Text = Path.GetFileName(file);
                            }
                            catch { };
                        }
                        else if (Path.GetExtension(file).ToLower() == ".o2s")
                        {
                            try
                            {
                                config = Config.Load(file, true);
                                config.onReloadProperties += new EventHandler(config_onReloadProperties);
                                config.ReloadProperties();
                            }
                            catch { };
                        };
                    };
                };
            };
            
            mrul = new MruList(AppDomain.CurrentDomain.BaseDirectory+@"\OSM2SHPFC.mru", recentToolStripMenuItem, 10);
            mrul.FileSelected += new MruList.FileSelectedEventHandler(OpenFile);
            recentToolStripMenuItem.Enabled = mrul.Count > 0;
        }        

        private MruList mrul;

        private void dBFEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\sergdbf.exe");
            }
            catch { };
        }

        private void shapeViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\ShapeViewer.exe");
            }
            catch { };
        }

        private void dBFExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\DBFExplorer.exe");
            }
            catch { };
        }

        private void dBFNavigatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\DBFNavigator.exe");
            }
            catch { };
        }

        private void osmconvertexeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\osmconvert.exe");
            }
            catch { };
        }

        private void osmchelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptEditor se = new ScriptEditor();
            se.FullView = false;
            se.Text = "Справка по osmconvert.exe";
            se.MainText = "Справка по osmconvert.exe";
            se.SubText = "Отредактировать описание вы можете в файле osmconvert_help.txt";
            se.LoadText(AppDomain.CurrentDomain.BaseDirectory + @"\osmconvert_help.txt");
            se.ReadOnly = true;
            se.ShowDialog();
            se.Dispose();            
        }

        private void fDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptEditor se = new ScriptEditor();
            se.Width = 700;
            se.FullView = false;
            se.Text = "Описание полей DBF файла";
            se.MainText = "Описание полей DBF файла";
            se.SubText = "Отредактировать описание вы можете в файле FieldsDescriptions.txt";            
            se.LoadText(AppDomain.CurrentDomain.BaseDirectory + @"\FieldsDescriptions.txt");
            se.ReadOnly = true;
            se.ShowDialog();
            se.Dispose();            
        }

        private void nEWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetConfig();
        }

        private void defToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetProperty(2);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!in_progress) return;
            if (reset > 0) return;
            if (MessageBox.Show("Вы действительно хотите прервать конвертацию?", this.MandatoryText, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                reset = 1;
        }

        private void MForm_Resize(object sender, EventArgs e)
        {
            STAT2.Width = this.Width - 28;
        }

        private void sTTSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(status.Text)) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt|All Types (*.*)|*.*";
            sfd.DefaultExt = ".txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                    sw.Write(status.Text);
                    sw.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
            sfd.Dispose();
        }

        private void dBFCommanderToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            try
            {
                DateTime firstRun = DateTime.Today;
                using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\OSM2SHP\DBFCommander_v4.4.0.101"))
                {
                    string val = rk.GetValue("firstRun", firstRun).ToString();
                    firstRun = DateTime.Parse(val);
                    rk.SetValue("firstRun", firstRun.ToString("yyyy-MM-ddTHH:mm:ss"));
                };                
            }
            catch { };
            
            
            if(false)
            {
                try
                {
                    string proc = AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\TimeStopper.exe";
                    string args = String.Format(@"{1:dd\\MM\\yyyy} 00:00:00 ""{0}""", AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\DBFCommander_v4.4.0.101.exe", started);
                    System.Diagnostics.Process.Start(proc, args);
                }
                catch { };
            };
            if (true) // NEW
            {                
                // HKCU\Software\Enigma Protector\3EC4384858086285-333D486DFA3C01DB
                // C:\WINDOWS\Prefetch\DBF*COMMANDER*
                // C:\WINDOWS\Prefetch\DBFCOMMANDER*
                // C:\Documents and Settings\artem_karimov\Local Settings\Temp\29AA7A44

                try
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Enigma Protector", true);
                    if (rk != null) 
                        rk.DeleteSubKeyTree("3EC4384858086285-333D486DFA3C01DB");
                    rk.Close();
                }
                catch { };
                try
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Enigma Protector", true);
                    if (rk != null) 
                        rk.DeleteSubKeyTree("3EC4384858086285-333D486DFA3C01DB");
                    rk.Close();
                }
                catch { };

                List<string> dirs = new List<string>();
                if (Directory.Exists(@"C:\WINDOWS\Prefetch")) dirs.Add(@"C:\WINDOWS\Prefetch");
                List<string> files = new List<string>();
                try
                {
                    string wd = Environment.GetEnvironmentVariable("windir");
                    if (Directory.Exists(wd))
                        if (Directory.Exists(wd + @"\Prefetch"))
                            dirs.Add(wd + @"\Prefetch");
                }
                catch { };
                foreach (string dir in dirs)
                {
                    try
                    {
                        files.AddRange(Directory.GetFiles(dir, "DBF*COMMANDER*"));
                        files.AddRange(Directory.GetFiles(dir, "DBF*COMMANDER*.*"));
                        files.AddRange(Directory.GetFiles(dir, "DBFCOMMANDER*"));
                        files.AddRange(Directory.GetFiles(dir, "DBFCOMMANDER*.*"));
                    }
                    catch { };
                };
                foreach (string file in files) { try { File.Delete(file); } catch { }; };
                string lad = System.IO.Path.GetTempPath() + @"\29AA7A44";
                try { File.Delete(lad); }
                catch { };
                try
                {
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\DBFCommander_v4.4.0.101.exe");
                }
                catch { };
            };
        }

        public static bool IsAdministrator()
        {
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            contextMenuStrip1_Opening(sender, new CancelEventArgs());
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string outText = "";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            outText += "Программа-конвертер файлов данных OSM (*.osm) и (*.pbf) в ESRI Shapes (*.shp)\r\n\r\n";
            outText += "Автор: milokz@gmail.com\r\n";
            outText += "https://github.com/dkxce/OSM2SHP\r\n\r\n";
            outText += String.Format("Текущая версия {3} от {2:00}.{1:00}.{0:00} \r\n", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
            outText += "\r\n";
            outText += "При индексации точек в памяти рекомендуется запускать программу на 64-битных системах с оперативной памятью более 10 ГБ. Работа и индексным файлом на диске значителько увеличивает время обработки данных и создание shape-файлов.\r\n";
            outText += "\r\n";
            outText += "Для управления через http-сервер используйте логин `web`, пароль `osm2shp`\r\n";
            outText += "\r\n";
            outText += "MachineID: "+machine+"\r\n";
            InputBox.QueryInfoBox("О Программе", this.MandatoryText, null, outText);   
        }

        private void chvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetProperty(0);
        }

        private void sHapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\shapefile.pdf");
            }
            catch { };
        }

        private void dsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\ds.exe");
            }
            catch { };
        }

        private bool cmd_convert = true;
        private bool cmd_silent = false;
        private void MForm_Shown(object sender, EventArgs e)
        {
            if (cmd_convert)
            {
                cmd_convert = false;
                if ((this.args != null) && (this.args.Length > 0))
                    foreach (string arg in this.args)
                        if (arg == "/convert") cmd_convert = true; 
                        else if (arg == "/close") cmd_silent = true;
                        else if (arg == "/silent") cmd_silent = true;
                if (cmd_convert)
                {
                    Convert();
                    if (cmd_silent) Close();
                }; 
                cmd_silent = false;
            };

        }

        private void cMDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptEditor se = new ScriptEditor();
            se.FullView = false;
            se.Text = "Параметры командной строки";
            se.MainText = "Параметры командной строки";
            se.SubText = "";
            se.LoadText(AppDomain.CurrentDomain.BaseDirectory + @"\osm2shp_cmd_line.txt");
            se.ReadOnly = true;
            se.ShowDialog();
            se.Dispose();    
        }

        private void dSToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".cmd";
            sfd.Filter = "Command Files (*.cmd;*.bat)|*.cmd;*.bat";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                    sw.Write(String.Format("call \"{0}ds.exe\" -t005 \"{0}OSM2SHPFC.exe\" /convert /close \"{1}\" \"{2}\"", AppDomain.CurrentDomain.BaseDirectory + @"\", config.inputFileName, config.outputFileName));
                    sw.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
            sfd.Dispose();
        }

        private void tTGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptEditor se = new ScriptEditor();
            se.FullView = false;
            se.Text = "Информация для собственного селектора";
            se.MainText = "Информация для собственного селектора";
            se.SubText = "Отредактировать описание вы можете в файле tags_to_garmin.txt";
            se.LoadText(AppDomain.CurrentDomain.BaseDirectory + @"\tags_to_garmin.txt");
            se.ReadOnly = true;
            se.ShowDialog();
            se.Dispose(); 
        }

        private void xmlcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (in_progress) return;
            status.Text = "Проверка файлов XML_SELECTOR:\r\n";

            string xml_selector = AppDomain.CurrentDomain.BaseDirectory + @"\xml_selector\";
            string[] files = Directory.GetFiles(xml_selector, "*.xml");

            status.Text += String.Format("Найдено файлов: {0}\r\n\r\n", files.Length);
            status.Refresh();
            status.SelectionStart = status.Text.Length;
            status.ScrollToCaret();
            
            if (files.Length > 0)
            {
                TYPE_MAP type_map = new TYPE_MAP();
                int ttl_counter = 0;
                int ttl_good = 0;
                int ttl_bad = 0;
                List<int> ttl_types = new List<int>();
                foreach (string file in files)
                {
                    status.Text += String.Format("--- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---\r\n");
                    status.Text += String.Format("Анализ файла {0}:\r\n", Path.GetFileName(file));
                    status.Refresh();
                    status.SelectionStart = status.Text.Length;
                    status.ScrollToCaret();

                    try
                    {
                        FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                        XmlReader _xmlr = XmlReader.Create(fs);
                        XmlDocument xd = new XmlDocument();

                        _xmlr.Read();
                        if (_xmlr.NodeType != XmlNodeType.XmlDeclaration) throw new Exception("Ivalid XML File");
                        do { _xmlr.Read(); } while (_xmlr.NodeType == XmlNodeType.Whitespace);
                        if (_xmlr.Name != "TYPE_MAP") throw new Exception("Ivalid XML File");

                        do { _xmlr.Read(); }
                        while ((!_xmlr.EOF) && ((_xmlr.NodeType != XmlNodeType.Element) || (_xmlr.Name != "Elements")));
                        if (_xmlr.NodeType != XmlNodeType.Element) throw new Exception("Ivalid XML File");
                        if (_xmlr.Name != "Elements") throw new Exception("Ivalid XML File");

                        int t_counter = 0;
                        int t_good = 0;
                        int t_bad = 0;
                        List<int> t_types = new List<int>();
                        while (!_xmlr.EOF)
                        {
                            do { _xmlr.Read(); }
                            while ((!_xmlr.EOF) && ((_xmlr.NodeType != XmlNodeType.Element) || (_xmlr.Name != "t")));
                            if (!_xmlr.EOF)
                            {
                                t_counter++;
                                ttl_counter++;
                                try
                                {
                                    XmlElement xn = (XmlElement)xd.ReadNode(_xmlr);

                                    TYPE_MAP_Element mel = new TYPE_MAP_Element();
                                    mel._____xml_id = xn.Attributes["id"].Value;
                                    if (t_types.IndexOf(mel.GARMIN_TYPE) < 0) t_types.Add(mel.GARMIN_TYPE);
                                    if (ttl_types.IndexOf(mel.GARMIN_TYPE) < 0) ttl_types.Add(mel.GARMIN_TYPE);

                                    Exception ex = TYPE_MAP.CheckScript(xn.ChildNodes[0].Value);

                                    if (ex == null)
                                    {
                                        status.Text += String.Format("  {0} - {1}: {2} - Ok\r\n", t_counter, xn.Attributes["id"].Value, xn.Attributes["label"].Value);
                                        t_good++;
                                        ttl_good++;
                                    }
                                    else
                                    {
                                        status.Text += String.Format("  {0} - {1}: {2} - {3}\r\n", t_counter, xn.Attributes["id"].Value, xn.Attributes["label"].Value, ex.Message);
                                        t_bad++;
                                        ttl_bad++;
                                    };
                                    status.Refresh();                                    
                                    status.SelectionStart = status.Text.Length;
                                    status.ScrollToCaret();
                                                                        
                                }
                                catch (Exception ex)
                                {
                                    status.Text += String.Format("  {0} - --ERROR: {1}\r\n", t_counter, ex.Message);
                                    status.Refresh();
                                    status.SelectionStart = status.Text.Length;
                                    status.ScrollToCaret();
                                    
                                    t_bad++;
                                    ttl_bad++;
                                };
                            };
                        };
                        status.Text += String.Format("  ---\r\n", t_counter);
                        status.Text += String.Format("  Всего GARMIN_TYPE: {0}\r\n", t_types.Count);
                        status.Text += String.Format("  Всего записей: {0}, {1} из них хороших, {2} - плохих\r\n", t_counter, t_good, t_bad);

                        _xmlr.Close();
                        fs.Close();
                    }
                    catch (Exception ex)
                    {
                        status.Text += String.Format("  Ошибка чтения файла:{0}\r\n", ex.Message);
                    };

                    try { type_map.Import(file); } catch { };                    
                };
                status.Text += String.Format("--- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---\r\n");
                status.Text += String.Format("Всего GARMIN_TYPE: {0}\r\n", ttl_types.Count);
                status.Text += String.Format("Всего записей: {0}, {1} из них хороших, {2} - плохих в {3} файлах\r\n", ttl_counter, ttl_good, ttl_bad, files.Length);
                status.Refresh();
                status.SelectionStart = status.Text.Length;
                status.ScrollToCaret();
                type_map.Init();

                status.Text += String.Format("TYPE_MAP Elements: {0} - {1}\r\n", type_map.Count, type_map.IsValid ? "Script OK" : "No Script" );
                status.Refresh();
                status.SelectionStart = status.Text.Length;
                status.ScrollToCaret();
            };            
        }

        private void cHECKCURRToolStripMenuItem_Check(object sender, ref string infoText, ref string btnText, ref bool close)
        {
            if((sender is Button))
            {
                if (((Button)sender).Text != "Проверить")
                {
                    close = true;
                    return;
                };
                
                Exception ex = TYPE_MAP.CheckScript(infoText);
                if (ex == null)
                    btnText = DateTime.Now.ToString("HH:mm:ss") + ": Ошибок в скрипте не найдено";
                else
                {
                    btnText = DateTime.Now.ToString("HH:mm:ss") + ": Обнаружена ошибка в скрипте";
                    string msg = (new Regex(@"(c:[\\\w\d_\-\s\.]+\()")).Replace(ex.Message, "ERR(");
                    try
                    {
                        MatchCollection mc = (new Regex(@"(ERR\(\d+,\d+\))")).Matches(msg);
                        for (int i = mc.Count - 1; i >= 0; i--)
                        {
                            Match rm = mc[i];
                            string[] da = rm.Value.Substring(4, rm.Value.Length - 5).Split(new char[] { ',' });
                            int ln = int.Parse(da[0]) - 11;
                            int rw = int.Parse(da[1]) - (ln == 1 ? 9 : 0);
                            msg = msg.Remove(rm.Index, rm.Length);
                            msg = msg.Insert(rm.Index, "(" + ln.ToString() + "," + rw.ToString() + ")");
                        };
                    }
                    catch { };
                    MessageBox.Show("Ошибка в скрипте:\r\n" + msg, this.MandatoryText, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
        }
        private void cHECKCURRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (in_progress) return;
            string text = "({name}!=null)";
            InputBox.QueryInfoBox("XML SELECTOR", "Проверка скрипта для селектора XML:", "Нажмите проверить для проверки", ref text, false, false, MessageBoxButtons.YesNo, true, new string[] { "Проверить", "Закрыть" }, cHECKCURRToolStripMenuItem_Check);
        }

        private void cOL1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cOL1ToolStripMenuItem.Checked = splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
            cOL2ToolStripMenuItem.Checked = splitContainer1.Panel2Collapsed;
        }

        private void cOL2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cOL2ToolStripMenuItem.Checked = splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
            cOL1ToolStripMenuItem.Checked = splitContainer1.Panel1Collapsed;
        }

        private void akelPadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\dbf_editor\akelpad.exe");
            }
            catch { };            
        }       

        private void httpServerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int _port = 8080;
                if ((httpServer != null) && (httpServer.Running))
                    httpServer.Stop();
                else
                {                    
                    if (InputBox.Show("HTTP-сервер", "Номер порта:", ref _port, 80, 9999) != DialogResult.OK)
                        return;
                    
                    httpServer = new SimpleHttpProtoServer();
                    httpServer.ServerPort = _port;
                    httpServer.onInfo += new SimpleHttpProtoServer.OnInfo(httpServer_onInfo);
                    try
                    {
                        httpServer.Start();
                    }
                    catch { };
                };
                httpServerLogToolStripMenuItem.Text = "Выводить протокол лога через HTTP-сервер (" + _port.ToString() + ")";
                httpServerLogToolStripMenuItem.Checked = httpServer.Running;
            }
            catch { };            
        }

        private void StdConfigItem_Click(object sender, EventArgs e)
        {
            config = new Config(true);

            config.selector = 10;
            config.maxAggTags = 4;
            config.waysImplementation = 0x1D;
            config.processRelations = true;
            config.relationsAsJoins = true;
            config.addFirstAndLastNodesIdToLines = true;
            config.addFisrtAndLastNodesLns2Memory = true;

            config.dbfMainFields.Clear();
            config.dbfMainFields.AddRange(new string[] { "TIMESTAMP", "TAGS_COUNT", "TAGS_ADDRC" });

            config.onReloadProperties += new EventHandler(config_onReloadProperties);
            config.ReloadProperties(); 
        }
    }

    public class DictComparer : IComparer<KeyValuePair<string, uint>>
    {
        private bool byIndex = false;
        private bool ASC = true;
        public DictComparer(bool byIndex, bool ASC)
        {
            this.byIndex = byIndex;
            this.ASC = ASC;
        }

        public int Compare(KeyValuePair<string, uint> a, KeyValuePair<string, uint> b)
        {
            if (this.byIndex || (a.Value == b.Value))
            {
                if (this.ASC)
                    return a.Key.CompareTo(b.Key);
                else
                    return b.Key.CompareTo(a.Key);
            }
            else
            {
                if (this.ASC)
                    return a.Value.CompareTo(b.Value);
                else
                    return b.Value.CompareTo(a.Value);
            };
        }

        public static KeyValuePair<string, uint>[] SortDictionary(Dictionary<string, uint> dict, bool byIndex, bool ASC)
        {
            List<KeyValuePair<string, uint>> res = new List<KeyValuePair<string, uint>>(dict.Count);
            foreach (KeyValuePair<string, uint> kvp in dict)
                res.Add(kvp);
            res.Sort(new DictComparer(byIndex, ASC));
            return res.ToArray();
        }
    }

    public class SimpleHttpProtoServer : SimpleServers.SingledHttpServer
    {
        public delegate string OnInfo(byte action, out string Server, out string title, out bool isRunning);

        public event OnInfo onInfo;

        public void HttpClientSendText(TcpClient Client, int Code, string Html, Dictionary<string, string> dopHeaders)
        {
            string Str = "HTTP/1.1 " + Code.ToString() + "\r\n";

            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();

            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);

            Str += "Content-type: text/html\r\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;

            byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.Client.Close();
            Client.Close();
        }

        protected override void GetClientRequest(TcpClient Client, ulong clientID, string Request)
        {
            byte action = 0;
            if (Request.StartsWith("GET /start HTTP"))
                action = 1;
            if (Request.StartsWith("GET /stop HTTP"))
                action = 2;

            if (action > 0)
            {
                bool accept = false;
                string sa = "Authorization:";
                if (Request.IndexOf(sa) > 0)
                {
                    int iofcl = Request.IndexOf(sa);
                    sa = Request.Substring(iofcl + sa.Length, Request.IndexOf("\r", iofcl + sa.Length) - iofcl - sa.Length).Trim();
                    if (sa.StartsWith("Basic"))
                    {
                        sa = Base64Decode(sa.Substring(6));
                        string[] up = sa.Split(new char[] { ':' }, 2);
                        if ((up[0] == "web") && (up[1] == "osm2shp"))
                            accept = true;
                    };
                };
                if (!accept)
                {
                    Dictionary<string, string> dh = new Dictionary<string, string>();
                    dh.Add("WWW-Authenticate", "Basic realm=\"Authentification required\"");
                    HttpClientSendError(Client, 401, dh); // 401 Unauthorized
                    return;
                };
            };

            string server;
            string title;
            bool isRunning;
            string res = onInfo(action, out server, out title, out isRunning);

            res = res.Replace("\r\n", "\r\n<br/>").Replace(" ", "&nbsp;");
            res = "<h3>" + server + "</h3>" + res + "<hr/>" + (isRunning ? "" : "| <a href=\"/start\">старт</a> ") + "|" + (isRunning ? " <a href=\"/stop\">стоп</a> |" : "");
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Server", server);
            if (action > 0)
                headers.Add("Location", "/");
            else
                if (isRunning)
                    headers.Add("Refresh", "5");

            string Html = "<html><header><title>" + title + "</title></header><body style=\"background:black;color:white;font-family:PT Mono;font-size:12px;\">" + res + "</body></html>";

            HttpClientSendText(Client, action == 0 ? 200 : 301, Html, headers);
        }
    }
}