﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Xml.Linq;

namespace desktopini_multieditor
{
    public partial class MainForm : Form
    {
        RichTextBox rtxt;
        System.Windows.Forms.CheckBox chkAutogenerate;
        System.Windows.Forms.CheckBox chkMinimalIni;
        public MainForm()
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

            chkAutogenerate = new System.Windows.Forms.CheckBox();
            chkAutogenerate.Text = "autogenerate";
            setToolTip(chkAutogenerate, "Autogenerate from first found ico file in each folder");
            chkAutogenerate.Left = btn.Right + 8;
            chkAutogenerate.Top = 8;
            this.Controls.Add(chkAutogenerate);

            chkMinimalIni = new System.Windows.Forms.CheckBox();
            chkMinimalIni.Text = "minimal desktop.ini:s";
            setToolTip(chkMinimalIni, "used together with autogenerate, this will generate minimal desktop.ini files");
            chkMinimalIni.Left = chkAutogenerate.Right + 8;
            chkMinimalIni.Width = 256;
            chkMinimalIni.Top = 8;
            this.Controls.Add(chkMinimalIni);

            this.Text = "multi Desktop.ini fixer";
        }
        private void setToolTip(Control ctrl, string text)
        {
            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 10000;
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(ctrl, text);
        }

        bool autogenerate = false;
        bool minimal_inis = false;

        private void Btn_Click(object sender, EventArgs e)
        {
            rtxt.Clear();
            autogenerate = chkAutogenerate.Checked;
            minimal_inis = chkMinimalIni.Checked;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = "C:\\";
            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            string path = fbd.SelectedPath;

            bool skip = false;

            string[] folders = Directory.GetDirectories(path);
            for (int i=0;i<folders.Length;i++)
            {
                skip = false;

                string filePath = folders[i] + "\\desktop.ini";
                if (!File.Exists(filePath)) skip = true;

                string[] contents = File.ReadAllLines(filePath);
                string name = "";
                if (autogenerate)
                {
                    if (!getFirstIconName(folders[i], out name)) skip = true;

                    if (minimal_inis == false)
                    {
                        int lineIndex = getIconResourceLineIndex(contents);
                        if (lineIndex == -1) skip = true;
                        contents[lineIndex] = "IconResource=" + name;
                    }
                    else
                    {
                        contents = new string[2];
                        contents[0] = "[.ShellClassInfo]";
                        contents[1] = "IconResource=" + name;
                    }
                }
                else
                {
                    int lineIndex = getIconResourceLineIndex(contents);
                    if (lineIndex == -1) skip = true;

                    name = contents[lineIndex].Substring("IconResource".Length + 1);
                    if (name.Contains("\\"))
                        name = name.Substring(name.LastIndexOf("\\") + 1);
                    contents[lineIndex] = "IconResource=" + name;
                }

                rtxt.AppendText(folders[i] + "    " + name);

                if (skip == false)
                {
                    rtxt.AppendText(" [OK]\r\n", Color.Green);
                    
                    FileAttributes attributes = File.GetAttributes(filePath);
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.WriteAllLines(filePath, contents);
                    File.SetAttributes(filePath, attributes);
                }
                else
                {
                    rtxt.AppendText(" [SKIPPING]\r\n", Color.Yellow);
                }
            }
            rtxt.AppendText("\r\nFinished!!!\r\n");
        }

        private bool getFirstIconName(string path, out string iconName)
        {
            string[] files = Directory.GetFiles(path, "*.ico");
            if (files.Length == 0) { iconName = ""; return false; }
            iconName = files[0].Substring(files[0].LastIndexOf("\\") + 1);
            return true;
        }

        private int getIconResourceLineIndex(string[] contents)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i].StartsWith("IconResource"))
                {
                    return i;
                }
            }
            return -1;
        }
    }
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
