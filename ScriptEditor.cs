using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OSM2SHP
{
    public partial class ScriptEditor : Form
    {
        private bool fullview = true;

        public ScriptEditor()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
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
    }
}