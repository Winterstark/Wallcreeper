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
using GenericForms;

namespace Wallcreeper
{
    public partial class formExport : Form
    {
        public formMain main;
        public List<Theme> themes;
        string saveDir;
        long size = 0;
        int nWalls = 0;
        bool chkEnMasse = false;


        void calcDiff(int ind, bool prefix)
        {
            int nWallsDiff = Directory.GetFiles(themes[ind].wallDir).Length;

            long sizeDiff = 0;
            foreach (string wall in Directory.GetFiles(themes[ind].wallDir))
                sizeDiff += new FileInfo(wall).Length;

            if (prefix)
            {
                nWalls += nWallsDiff;
                size += sizeDiff;
            }
            else
            {
                nWalls -= nWallsDiff;
                size -= sizeDiff;
            }
        }

        void dispInfo()
        {
            lblNumber.Text = "Wallpaper pack will contain " + nWalls + " wallpapers.";
            lblSize.Text = "Total size = " + (size / 1024 / 1024) + " MB";
        }


        public formExport()
        {
            InitializeComponent();
        }

        private void formExport_Load(object sender, EventArgs e)
        {
            if (main.Archiver != "")
                textArchiver.Text = main.Archiver;
            else
                textArchiver.Text = Misc.GetArchiver(); //try to locate 7zip or winrar

            //populate list with themes
            chklistThemes.Items.Clear();

            foreach (var theme in themes)
                chklistThemes.Items.Add(theme.name);

            new Tutorial(Application.StartupPath + "\\tutorials\\export.txt", this);
        }

        private void buttSelAll_Click(object sender, EventArgs e)
        {
            chkEnMasse = true;

            for (int i = 0; i < chklistThemes.Items.Count; i++)
            {
                if (!chklistThemes.GetItemChecked(i))
                    calcDiff(i, true);
                chklistThemes.SetItemChecked(i, true);
            }

            dispInfo();
            chkEnMasse = false;
        }

        private void buttSelInvert_Click(object sender, EventArgs e)
        {
            chkEnMasse = true;

            for (int i = 0; i < chklistThemes.Items.Count; i++)
            {
                calcDiff(i, !chklistThemes.GetItemChecked(i));
                chklistThemes.SetItemChecked(i, !chklistThemes.GetItemChecked(i));
            }

            dispInfo();
            chkEnMasse = false;
        }

        private void buttSelNone_Click(object sender, EventArgs e)
        {
            chkEnMasse = true;

            for (int i = 0; i < chklistThemes.Items.Count; i++)
                chklistThemes.SetItemChecked(i, false);

            nWalls = 0;
            size = 0;
            dispInfo();

            chkEnMasse = false;
        }

        private void buttBrowse_Click(object sender, EventArgs e)
        {
            string prevPath = textArchiver.Text;

            if (prevPath == "")
                openDiag.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            else
                openDiag.InitialDirectory = prevPath;

            if (openDiag.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            textArchiver.Text = openDiag.FileName;

            if (textArchiver.Text != prevPath)
            {
                //save new archiver path
                main.Archiver = textArchiver.Text;
                main.SaveOptions();
            }
        }

        private void chklistThemes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (chklistThemes.SelectedIndex != -1 && !chkEnMasse)
            {
                calcDiff(chklistThemes.SelectedIndex, !chklistThemes.GetItemChecked(chklistThemes.SelectedIndex));
                dispInfo();
            }
        }

        private void buttExport_Click(object sender, EventArgs e)
        {
            if (chklistThemes.CheckedItems.Count == 0)
            {
                MessageBox.Show("You haven't selected any themes to export.");
                return;
            }

            if (textArchiver.Text == "" && MessageBox.Show("Instead of one file, the wallpaper pack will be saved as a folder. Proceed?", "File archiver path not set", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
                return;

            //select save path
            if (textArchiver.Text == "")
                saveDiag.Filter = "";
            else
                saveDiag.Filter = "Wallcreeper pack|*.wcp";

            saveDiag.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveDiag.FileName = "";
            saveDiag.ShowDialog();
            if (saveDiag.FileName == "")
                return;

            //create dir
            saveDir = saveDiag.FileName;
            if (saveDir.Contains('.'))
                saveDir = saveDir.Substring(0, saveDir.LastIndexOf('.'));

            Directory.CreateDirectory(saveDir);
            StreamWriter fWrt = new StreamWriter(saveDir + "\\" + Misc.GetFilename(saveDir) + ".txt");

            foreach (var item in chklistThemes.CheckedItems)
            {
                //copy dir
                Theme theme = themes[chklistThemes.Items.IndexOf(item)];
                string srcDir = theme.wallDir;
                string destDir = saveDir + "\\" + Misc.GetFilename(srcDir) + "\\";

                Directory.CreateDirectory(destDir);

                foreach (string file in Directory.GetFiles(srcDir))
                    File.Copy(file, destDir + Misc.GetFilename(file));

                //write theme
                fWrt.Write(theme.SaveTxt());
            }

            fWrt.Close();

            if (textArchiver.Text == "")
            {
                MessageBox.Show("Wallpaper pack successfully created.");
                this.Close();
            }
            else
            {
                //archive pack
                ProcessStartInfo szInfo = new ProcessStartInfo(textArchiver.Text, " a \"" + saveDiag.FileName + "\" \"" + saveDir + '"');
                Process sZip = Process.Start(szInfo);

                sZip.EnableRaisingEvents = true;
                sZip.Exited += new EventHandler(sZip_Exited);
            }
        }

        private void sZip_Exited(object sender, EventArgs e)
        {
            Directory.Delete(saveDir, true);
            MessageBox.Show("Wallpaper pack successfully created.");
        }
    }
}
