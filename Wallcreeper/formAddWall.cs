using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Shell;

namespace Wallcreeper
{
    public partial class formAddWall : Form
    {
        Action<string> banWallpaper;
        string[] themeNames, themeDirs;
        string imgPath;


        void populateThemes()
        {
            listThemes.Items.Clear();
            
            string[] search = textFilter.Text.ToLower().Split(' ');
            bool add;

            foreach (string theme in themeNames)
            {
                add = true;

                foreach (string req in search)
                    if (!theme.ToLower().Contains(req))
                    {
                        add = false;
                        break;
                    }

                if (add)
                    listThemes.Items.Add(theme);
            }
        }

        bool canSave()
        {
            return listThemes.SelectedIndex != -1 || listThemes.Items.Count == 1;
        }

        public void Init(string imgPath, List<Theme> themes, Action<string> banWallpaper)
        {
            this.imgPath = imgPath;
            this.banWallpaper = banWallpaper;

            //load thumbnail and get img dimensions
            ShellFile shFile = ShellFile.FromFilePath(imgPath);
            
            picThumbnail.Image = shFile.Thumbnail.LargeBitmap;
            lblFilename.Text = Path.GetFileName(imgPath) + Environment.NewLine + (int)shFile.Properties.System.Image.HorizontalSize.Value + " x " + (int)shFile.Properties.System.Image.VerticalSize.Value;

            //grab theme names & folders
            themeNames = new string[themes.Count];
            themeDirs = new string[themes.Count];

            for (int i = 0; i < themes.Count; i++)
            {
                themeNames[i] = themes[i].name;
                themeDirs[i] = themes[i].wallDir;
                
                //remove last backlash if any
                if (themeDirs[i][themeDirs[i].Length - 1] == '\\')
                    themeDirs[i] = themeDirs[i].Substring(0, themeDirs[i].Length - 1);
            }

            populateThemes();
        }

        void saveWall()
        {
            string theme;
            if (listThemes.SelectedIndex != -1)
                theme = listThemes.Text;
            else
                theme = listThemes.Items[0].ToString();

            //get wallDir of selected theme
            string wallDir = "";

            for (int i = 0; i < themeNames.Length; i++)
                if (themeNames[i] == theme)
                {
                    wallDir = themeDirs[i];
                    break;
                }

            //copy wall to available destination
            string dest = wallDir + "\\" + Path.GetFileName(imgPath);
            int j = 1;
            while (File.Exists(dest))
                dest = wallDir + "\\" + Path.GetFileName(imgPath) + "_" + (j++);

            File.Copy(imgPath, dest);

            banWallpaper("<current>"); //since wallcreeper is saving the wallpaper locally, the user probably doesn't want to get this wallpaper again as an online source
            this.Close();
        }


        public formAddWall()
        {
            InitializeComponent();
        }

        private void formAddWall_Load(object sender, EventArgs e)
        {
            picTooltip.SetToolTip(picThumbnail, "Click to open picture in Paint");
        }

        private void picThumbnail_MouseEnter(object sender, EventArgs e)
        {
            picFrame.Visible = true;
        }

        private void picThumbnail_MouseLeave(object sender, EventArgs e)
        {
            picFrame.Visible = false;
        }

        private void picThumbnail_Click(object sender, EventArgs e)
        {
            Process.Start("c:\\windows\\system32\\mspaint.exe", "\"" + imgPath + "\"");
        }

        private void textFilter_TextChanged(object sender, EventArgs e)
        {
            populateThemes();
            buttSave.Enabled = canSave();
        }

        private void listThemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttSave.Enabled = canSave();
        }

        private void buttSave_Click(object sender, EventArgs e)
        {
            saveWall();
        }

        private void textFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && canSave())
                saveWall();
        }

        private void listThemes_DoubleClick(object sender, EventArgs e)
        {
            if (canSave())
                saveWall();
        }
    }
}
