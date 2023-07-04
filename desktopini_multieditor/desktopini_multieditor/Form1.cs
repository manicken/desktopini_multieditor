using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace desktopini_multieditor
{
    public partial class Form1 : Form
    {
        RichTextBox rtxt;
        public Form1()
        {
            InitializeComponent();
            Button btn = new Button();
            btn.Text = "select folder";
            btn.Click += Btn_Click;
            btn.Left = 8;
            btn.Top = 8;
            btn.Height = 32;
            this.Controls.Add(btn);
            //this.Width = btn.Width + 64;
            //this.Height = btn.Height + 64;

            rtxt = new RichTextBox();
            rtxt.Top = btn.Bottom + 8;
            rtxt.Left = 8;
            rtxt.Width = this.Width -32 ;
            rtxt.Height = this.Height - ( btn.Height + btn.Top + 64) ;
            rtxt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            this.Controls.Add(rtxt);
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = "C:\\";
            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            string path = fbd.SelectedPath;

            string[] folders = System.IO.Directory.GetDirectories(path);
            for (int i=0;i<folders.Length;i++)
            {
                string filePath = folders[i] + "\\desktop.ini";
                if (!System.IO.File.Exists(filePath)) continue;

                string[] contents = System.IO.File.ReadAllLines(filePath);
                string name = "";
                if (!getIconName(ref contents, out name)) continue;
                rtxt.AppendText(folders[i] + "    " + name + "\r\n");
                System.IO.FileAttributes attributes = System.IO.File.GetAttributes(filePath);

                System.IO.File.SetAttributes(filePath, System.IO.FileAttributes.Normal);
                System.IO.File.WriteAllLines(filePath, contents);
                System.IO.File.SetAttributes(filePath, attributes);
            }
        }

        private bool getIconName(ref string[] contents, out string name)
        {
            for (int i=0;i<contents.Length;i++)
            {
                if (contents[i].StartsWith("IconResource"))
                {
                    name = contents[i].Substring("IconResource".Length + 1);
                    if (name.Contains("\\"))
                    {
                        name = name.Substring(name.LastIndexOf("\\")+1);
                    }
                    contents[i] = "IconResource=" + name;
                    return true;
                }
            }
            name = "";
            return false;
        }
    }
}
