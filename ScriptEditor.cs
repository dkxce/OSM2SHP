using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OSM2SHP 
{
    public partial class ScriptEditor : Form
    {
        public class TbInfo
        {
            public FastColoredTextBoxNS.AutocompleteMenu popupMenu;
        }

        public class Methods2AutoComplete : FastColoredTextBoxNS.MethodAutocompleteItem
        {
            string firstPart;
            string lastPart;

            public Methods2AutoComplete(string text)
                : base(text)
            {
                int i = text.LastIndexOf('.');
                if (i < 0)
                    firstPart = text;
                else
                {
                    firstPart = text.Substring(0, i);
                    lastPart = text.Substring(i + 1);
                }
            }

            public override FastColoredTextBoxNS.CompareResult Compare(string fragmentText)
            {
                int i = fragmentText.LastIndexOf('.');

                if (i < 0)
                {
                    if (firstPart.StartsWith(fragmentText) && string.IsNullOrEmpty(lastPart))
                        return FastColoredTextBoxNS.CompareResult.VisibleAndSelected;
                    //if (firstPart.ToLower().Contains(fragmentText.ToLower()))
                    //  return CompareResult.Visible;
                }
                else
                {
                    string fragmentFirstPart = fragmentText.Substring(0, i);
                    string fragmentLastPart = fragmentText.Substring(i + 1);


                    if (firstPart != fragmentFirstPart)
                        return FastColoredTextBoxNS.CompareResult.Hidden;

                    if (lastPart != null && lastPart.StartsWith(fragmentLastPart))
                        return FastColoredTextBoxNS.CompareResult.VisibleAndSelected;

                    if (lastPart != null && lastPart.ToLower().Contains(fragmentLastPart.ToLower()))
                        return FastColoredTextBoxNS.CompareResult.Visible;

                }

                return FastColoredTextBoxNS.CompareResult.Hidden;
            }

            public override string GetTextForReplace()
            {
                if (lastPart == null)
                    return firstPart;

                return firstPart + "." + lastPart;
            }

            public override string ToString()
            {
                if (lastPart == null)
                    return firstPart;

                return lastPart;
            }
        }

        public class Underlined : FastColoredTextBoxNS.Style
        {
            private Pen pen = new Pen(Brushes.Maroon, 1);

            public Underlined(Color color, int width)
            {
                this.pen = new Pen(new SolidBrush(color), width);
            }

            public override void Draw(Graphics gr, Point position, FastColoredTextBoxNS.Range range)
            {
                //get size of rectangle
                Size size = GetSizeOfRange(range);
                //create rectangle
                Rectangle rect = new Rectangle(position, size);
                //inflate it
                //rect.Inflate(2, 2);
                //get rounded rectangle
                System.Drawing.Drawing2D.GraphicsPath path = GetRoundedRectangle(rect, 7);                
                //draw rounded rectangle
                //gr.DrawPath(Pens.Red, path);
                gr.DrawLine(pen, new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));
            }
        }        

        #region C# snippets
        private string[] keywords = { 
            "as", "base", "bool", "break", "byte", "case", "catch", "char", "const", "continue", "decimal", "default", "do", "double", "else", "false", "finally", "float", "for", "foreach", "goto", "if", "in", "int", "is", "lock", "long", "new", "null", "object", "operator", "out", "params", "ref", "return", "sbyte", "short", "sizeof", "string", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "while" 
        };
        private string[] snippets = { 
            "if(^)\n{\n;\n}", "if(^)\n{\n;\n}\nelse\n{\n;\n}", "for(^;;)\n{\n;\n}", "while(^)\n{\n;\n}", "do\n{\n^;\n}while();", "switch(^)\n{\ncase : break;\n}" 
        };
        #endregion

        #region Script Snippets
        private string[] properties = { 
            // properties
            "converter", "args", "userdata", "nodesCounter", "waysCounter", "relsCounter", "ni", "ti", "infoType" 
        };
        private string[] objects_and_methods = { 
            // universal
            "MessageBox", "MessageBox.Show(\"\")", "Abort()", "Abort(Exception ex)", "Abort(string message)", "AbortAfterReturn()", "AbortAfterReturn(Exception ex)", "AbortAfterReturn(string message)",
            // this.converter
            "this.converter", "this.converter.IsXML", "this.converter.IsPBF", "this.converter.PointsTotal", "this.converter.PointsProcessed", "this.converter.PointsWrited", "this.converter.PointsUnical", "this.converter.PointsEmpty", "this.converter.PointsFromNodes", "this.converter.PointsFromWays", "this.converter.PointsFromLines", "this.converter.PointsFromAreas",
            "this.converter.WaysTotal","this.converter.WaysProcessed","this.converter.WaysWrited",
            "this.converter.WaysAsBoth","this.converter.WaysAsLines","this.converter.WaysAsAreas","this.converter.WaysWritedAsLines","this.converter.WaysWritedAsAreas","this.converter.WaysSkippedFilterLines","this.converter.WaysSkippedFilterAreas","this.converter.WaysSkippedFilter","this.converter.WaysByRelationsTotal","this.converter.WaysByRelationsLines","this.converter.WaysByRelationsAreas",
            "this.converter.RelationsTotal","this.converter.RelationsProcessed","this.converter.RelationsWrited","this.converter.RelationsMemsWrited",
            "this.converter.NodesSkippedByFilter","this.converter.NodesSkippedBySel",
            "this.converter.FilesPointsWrited","this.converter.FilesLinesWrited","this.converter.FilesAreasWrited","this.converter.FilesRelationsWrited","this.converter.FilesJoinsWrited",
            "this.converter.MaxFileRecords",
            "this.converter.PointsFieldsCount","this.converter.PointsRecordSize",
            "this.converter.LinesFieldsCount","this.converter.LinesRecordSize",
            "this.converter.AreasFieldsCount","this.converter.AreasRecordSize",
            "this.converter.RelationsFieldsCount","this.converter.RelationsRecordSize",
            "this.converter.RelationsMembersFieldsCount","this.converter.RelationsMembersRecordSize",
            "this.converter.RelationsJoinsFieldsCount","this.converter.RelationsJoinsRecordSize",
            // this
            "this.args", "this.args.Length", "this.userdata" , "this.nodesCounter", "this.waysCounter", "this.relsCounter", "this.nodesCounter.ToString()", "this.waysCounter.ToString()", "this.relsCounter.ToString()",
            // OSMPBFReader.TagsInfo ti
            "ti.infoType", "ti.infoType.ToString()", "ti.PointsCount", "ti.PointsCount.ToString()", "ti.tags", "ti.HasTags", "ti.TagsCount", "ti.TagsCount.ToString()", "ti.TagsAll", "ti.HasName", "ti.Name", "ti.AddrTagsCount", "ti.AddrTagsCount.ToString()", "ti.Addr", "ti.HasTag(string k)", "ti.GetTag(string k)", "ti.GetPoint(int index)",
            // OSMPBFReader.NodesInfo ni
            "ni.infoType", "ni.infoType.ToString()", "ni.PointsCount", "ni.PointsCount.ToString()", "ni.tags", "ni.HasTags", "ni.TagsCount", "ni.TagsCount.ToString()", "ni.TagsAll", "ni.HasName", "ni.Name", "ni.AddrTagsCount", "ni.AddrTagsCount.ToString()", "ni.Addr", "ni.HasTag(string k)", "ni.GetTag(string k)", "ni.GetPoint(int index)",
            "ni.id", "ni.id.ToString()", "ni.lat", "ni.lat.ToString()", "ni.lon", "ni.lon.ToString()", "ni.version", "ni.version.ToString()", "ni.timestamp", "ni.timestamp.ToString()", "ni.changeset", "ni.changeset.ToString()", "ni.uid", "ni.uid.ToString()", "ni.user", "ni.point", "ni.point.X", "ni.point.X.ToString()", "ni.point.Y", "ni.point.Y.ToString()", "ni.datetime", "ni.datetime.ToString()"
        };                
        #endregion

        private bool fullview = true;

        public ScriptEditor()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;

            textBox1.Font = new Font("Consolas", 9.75f);
            textBox1.Language = FastColoredTextBoxNS.Language.CSharp;
            FastColoredTextBoxNS.AutocompleteMenu popupMenu = new FastColoredTextBoxNS.AutocompleteMenu(textBox1);
            BuildAutocompleteMenu(popupMenu);
            textBox1.Tag = new TbInfo();
            (textBox1.Tag as TbInfo).popupMenu = popupMenu;

            textBox1.TextChanged += new EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(textBox1_TextChanged);
        }        

        private void BuildAutocompleteMenu(FastColoredTextBoxNS.AutocompleteMenu popupMenu)
        {
            List<FastColoredTextBoxNS.AutocompleteItem> items = new List<FastColoredTextBoxNS.AutocompleteItem>();

            foreach (string item in snippets)
                items.Add(new FastColoredTextBoxNS.SnippetAutocompleteItem(item));
            foreach (string item in objects_and_methods)
                items.Add(new Methods2AutoComplete(item));
            foreach (string item in properties)
                items.Add(new FastColoredTextBoxNS.AutocompleteItem(item));
            foreach (string item in keywords)
                items.Add(new FastColoredTextBoxNS.AutocompleteItem(item));            
            popupMenu.Items.SetAutocompleteItems(items);
            popupMenu.SearchPattern = @"[\w\.:=!<>]";
        }

        Underlined u_userdata = new Underlined(Color.Maroon, 1);
        Underlined u_infoType = new Underlined(Color.Blue, 1);
        Underlined u_counter = new Underlined(Color.Pink, 1);
        FastColoredTextBoxNS.TextStyle u_tini = new FastColoredTextBoxNS.TextStyle(Brushes.Navy, null, FontStyle.Bold);
        FastColoredTextBoxNS.TextStyle u_redd = new FastColoredTextBoxNS.TextStyle(Brushes.Red, null, FontStyle.Bold);

        private void textBox1_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            e.ChangedRange.ClearStyle(new FastColoredTextBoxNS.Style[] { u_userdata, u_infoType, u_counter, u_tini });
            e.ChangedRange.SetStyle(u_userdata, @"\bargs\b|\bconverter\b|\buserdata\b", RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(u_infoType, @"\binfoType\b|\bPointsCount\b", RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(u_counter, @"\bnodesCounter\b|\bwaysCounter\b|\brelsCounter\b", RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(u_tini, @"\bthis\b|\bti\b|\bni\b", RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(u_redd, @"\bAbort\b|\bAbortAfterReturn\b", RegexOptions.IgnoreCase);
        }

        public string Script
        {
            get
            {
                return textBox1.Text;
            }
            set
            {                
                textBox1.Text = value;
            }
        }

        public void LoadText(string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                Script = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            }
            catch { };
        }

        public bool FullView
        {
            get
            {
                return fullview;
            }
            set
            {
                fullview = value;
                panel1.Visible = fullview;
                panel5.Visible = fullview;
                panel6.Visible = fullview;
                button1.Text   = fullview ? "Отмена" : "ОК";
                button2.Visible = fullview;
                button3.Visible = fullview;
                button4.Visible = fullview;
                button5.Visible = fullview;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return textBox1.ReadOnly;
            }
            set
            {
                textBox1.ReadOnly = value;
                textBox1.BackColor = SystemColors.Window;
            }
        }

        public string MainText
        {
            get
            {
                return label2.Text;
            }
            set
            {
                label2.Text = value;
            }
        }

        public string SubText
        {
            get
            {
                return label3.Text;
            }
            set
            {
                label3.Text = value;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt|All Types (*.*)|*.*";
            sfd.DefaultExt = ".txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                    sw.Write(Script);
                    sw.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
            sfd.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|All Types (*.*)|*.*";
            ofd.DefaultExt = ".txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                    Script = sr.ReadToEnd();
                    sr.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
            ofd.Dispose();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Script = "return true;";
        }

        private void ScriptEditor_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                try
                {
                    textBox1.Select();
                    textBox1.Focus();
                    textBox1.SelectionStart = textBox1.Text.Length;
                }
                catch { };
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}