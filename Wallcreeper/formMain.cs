using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GenericForms;

namespace Wallcreeper
{
    public partial class formMain : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private static readonly UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x01;
        private static readonly UInt32 SPIF_SENDWININICHANGE = 0x02;


        class WinTheme
        {
            public string name, style, color, sounds, ssaver, date;

            public WinTheme(string name, string style, string color, string sounds, string ssaver, string date)
            {
                this.name = name;
                this.style = style;
                this.color = color;
                this.sounds = sounds;
                this.ssaver = ssaver;
                this.date = date;
            }
        }

        enum WallpaperSource { Local, Dropbox, Flickr };

        const double MOON_PERIOD = 29.530588853;
        //readonly DateTime REFERENCE_FULL_MOON = new DateTime(2012, 1, 9, 8, 33, 0);

        formAddWall addWall;
        BackgroundWorker worker;
        List<Theme> themes = new List<Theme>();
        List<WinTheme> winThemes = new List<WinTheme>();
        List<string> bannedWalls;
        Random rand;
        ContextMenu trayMenu;
        DateTime sunrise, sunset, moonrise, moonset, nextFullMoon, nextEaster, prevWeatherCheck = new DateTime(2012, 1, 24), lastWallChange = new DateTime();
        string[] descClear, descCloudy, descRain, descSnow, win7Colors;
        public string Archiver;
        string season, moonPhase, currWallDir, currDate, currTime, currWeather, prevActiveThemes = "", activeWinTheme, tempDir;
        string currLocation;
        string currWinThemeStyle, currWinThemeColor, currWinThemeSounds, currWinThemeSSaver, currWinThemeDate, appliedWallPath = "", onlineWallSource = "";
        double currLon, currLat;
        int currTZone, currRefresh, currWCheckPeriod, currUpdateNotifs, currLocalFreq, currDropboxFreq, currFlickrFreq, currFlickrMinW, currFlickrMinH, oldWallsCount = -1;
        bool weathClear = false, weathCloudy = false, weathRain = false, weathSnow = false, forcedWeather = false, newForcedWeather = false, loadingGlobalVals = true, currOverpower, currShowChangelog, currWinManager, firstRun, disableToggle = false;


        int utcTZone()
        {
            if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
                return currTZone + 1;
            else
                return currTZone;
        }

        int getRefreshPeriod()
        {
            switch (currRefresh)
            {
                case 0:
                    return 10 * 1000;
                case 1:
                    return 30 * 1000;
                case 2:
                    return 1 * 60 * 1000;
                case 3:
                    return 3 * 60 * 1000;
                case 4:
                    return 5 * 60 * 1000;
                case 5:
                    return 10 * 60 * 1000;
                case 6:
                    return 15 * 60 * 1000;
                case 7:
                    return 20 * 60 * 1000;
                case 8:
                    return 30 * 60 * 1000;
                case 9:
                    return 1 * 60 * 60 * 1000;
                case 10:
                    return 2 * 60 * 60 * 1000;
                case 11:
                    return 3 * 60 * 60 * 1000;
                case 12:
                    return 4 * 60 * 60 * 1000;
                case 13:
                    return 6 * 60 * 60 * 1000;
                case 14:
                    return 12 * 60 * 60 * 1000;
                case 15:
                    return 24 * 60 * 60 * 1000;
                default:
                    return 1 * 60 * 60 * 1000;
            }
        }

        void refreshUpdateNotifLabel()
        {
            switch (trackUpdate.Value)
            {
                case 0:
                    lblUpdateNotifications.Text = "Always ask";
                    break;
                case 1:
                    lblUpdateNotifications.Text = "Check for update automatically";
                    break;
                case 2:
                    lblUpdateNotifications.Text = "Download update automatically";
                    break;
                case 3:
                    lblUpdateNotifications.Text = "Install update automatically";
                    break;
            }
        }

        void toggleRunAtStartup()
        {
            if (disableToggle)
                return;
            
            disableToggle = true;

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (!trayMenu.MenuItems[1].Checked)
            {
                rkApp.SetValue("Wallcreeper", Application.ExecutablePath.ToString());
                MessageBox.Show("Wallcreeper now runs at Windows startup.");
            }
            else
            {
                rkApp.DeleteValue("Wallcreeper", true);
                MessageBox.Show("Wallcreeper no longer runs at Windows startup.");
            }

            trayMenu.MenuItems[1].Checked = !trayMenu.MenuItems[1].Checked;
            checkRunAtStartup.Checked = trayMenu.MenuItems[1].Checked;

            disableToggle = false;
        }

        void importPack(string path)
        {
            if (themes.Count > 0)
                browseDiag.SelectedPath = Misc.GetDirPath(themes[0].wallDir);
            else
                browseDiag.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);
            browseDiag.Description = "Select (or create) root wallpaper directory" + Environment.NewLine + "This is the folder where you want to store the wallpapers from the pack.";

            if (browseDiag.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            if (Directory.Exists(path))
            {
                path = findThemeFile(path);

                if (path != "")
                    processThemePackContents(path);
            }
            else if (path.EndsWith(".txt"))
                processThemePackContents(path);
            else if (path.EndsWith(".wcp"))
            {
                if (Archiver == "")
                    Archiver = Misc.GetArchiver(); //try to find archiver in registry

                if (Archiver == "")
                {
                    //ask user to locate archiver
                    MessageBox.Show("The theme pack is archived, but Wallcreeper was unable to locate a suitable file archiver (e.g. 7-Zip). Please specify the full path of a file archiver in the following dialog.");

                    openDiag.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    openDiag.FileName = "";
                    openDiag.Filter = "File archiver|*.exe";

                    if (openDiag.ShowDialog() == System.Windows.Forms.DialogResult.Cancel || openDiag.FileName == "")
                    {
                        MessageBox.Show("Unable to continue without a file archiver." + Environment.NewLine + "Please extract the theme pack's contents yourself and then import either the resulting folder or the text file inside it.");
                        return;
                    }

                    //save archiver path
                    Archiver = openDiag.FileName;
                    SaveOptions();
                }

                //extract theme pack contents
                tempDir = Misc.GetDirPath(path) + "\\temp";
                //check for conflicts with existing dirs
                while (Directory.Exists(tempDir))
                    tempDir += "_1";

                ProcessStartInfo szInfo = new ProcessStartInfo(Archiver, "x \"" + path + "\" -o\"" + tempDir + '"');
                Process sZip = Process.Start(szInfo);

                sZip.EnableRaisingEvents = true;
                sZip.Exited += new EventHandler(sZip_Exited);
            }
        }

        string findThemeFile(string path)
        {
            string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                MessageBox.Show("Can't find text file with themes info. Import failed.");
                return "";
            }
            else
                return files[0];
        }

        void processThemePackContents(string path)
        {
            string installed = "";

            //add themes
            int prevThemeCount = themes.Count;
            loadThemes(path);

            if (themes.Count == prevThemeCount)
                return; //no new themes were added

            //copy wallpapers
            foreach (string dir in Directory.GetDirectories(Misc.GetDirPath(path)))
            {
                //find theme (and check if added)
                Theme newTheme = null;

                foreach (Theme theme in themes.Skip(prevThemeCount))
                    if (Misc.GetFilename(theme.wallDir) == Misc.GetFilename(dir))
                    {
                        newTheme = theme;
                        break;
                    }

                if (newTheme == null)
                    continue;

                installed += newTheme.name + Environment.NewLine;

                //copy wallpapers
                string destDir = browseDiag.SelectedPath + "\\" + Misc.GetFilename(dir);
                //check for conflicts with existing dirs
                while (Directory.Exists(destDir))
                    destDir += "_1";

                Directory.CreateDirectory(destDir);

                foreach (string file in Directory.GetFiles(dir))
                    File.Copy(file, destDir + "\\" + Misc.GetFilename(file));

                //edit walldir in theme
                newTheme.wallDir = destDir;
            }

            saveThemes();
            MessageBox.Show("Import success!" + Environment.NewLine + "The following themes have been copied:" + Environment.NewLine + installed + Environment.NewLine + "You can delete the theme pack archive if you wish");
        }

        void saveThemes()
        {
            StreamWriter file = new StreamWriter(Application.StartupPath + "\\themes.txt");

            foreach (Theme theme in themes)
                file.Write(theme.SaveTxt());

            file.Close();
        }

        void saveWinThemes()
        {
            StreamWriter file = new StreamWriter(Application.StartupPath + "\\winThemes.txt");

            foreach (WinTheme winTheme in winThemes)
                file.WriteLine(winTheme.name + Environment.NewLine + winTheme.style + Environment.NewLine + winTheme.color + Environment.NewLine + winTheme.sounds + Environment.NewLine + winTheme.ssaver + Environment.NewLine + winTheme.date);

            file.Close();
        }

        void showTheme(int ind)
        {
            noteCurrThemeVals();

            textWallDir.Text = themes[comboTheme.SelectedIndex].wallDir;
            comboDate.Text = themes[comboTheme.SelectedIndex].date;
            comboTime.Text = themes[comboTheme.SelectedIndex].time;
            comboWeather.Text = themes[comboTheme.SelectedIndex].weather;
            checkOverpower.Checked = themes[comboTheme.SelectedIndex].overpower;
        }

        void noteCurrThemeVals()
        {
            currWallDir = themes[comboTheme.SelectedIndex].wallDir;
            currDate = themes[comboTheme.SelectedIndex].date;
            currTime = themes[comboTheme.SelectedIndex].time;
            currWeather = themes[comboTheme.SelectedIndex].weather;
            currOverpower = themes[comboTheme.SelectedIndex].overpower;
        }

        void noteCurrWinThemeVals()
        {
            currWinThemeStyle = winThemes[comboWinTheme.SelectedIndex].style;
            currWinThemeColor = winThemes[comboWinTheme.SelectedIndex].color;
            currWinThemeSounds = winThemes[comboWinTheme.SelectedIndex].sounds;
            currWinThemeSSaver = winThemes[comboWinTheme.SelectedIndex].ssaver;
            currWinThemeDate = winThemes[comboWinTheme.SelectedIndex].date;
        }

        void noteCurrGlobalVals()
        {
            currRefresh = comboRefresh.SelectedIndex;
            currWCheckPeriod = (int)numWCheck.Value;
            currLocation = textLocation.Text;

            double.TryParse(textLatitude.Text, out currLat);
            double.TryParse(textLongitude.Text, out currLon);
            int.TryParse(textTimezone.Text, out currTZone);

            currUpdateNotifs = trackUpdate.Value;
            currShowChangelog = checkShowChangelog.Checked;
            currWinManager = checkWinManager.Checked;

            currLocalFreq = trackLocal.Value;
            currDropboxFreq = trackDropbox.Value;
            currFlickrFreq = trackFlickr.Value;
            currFlickrMinW = int.Parse(textFlickrMinW.Text);
            currFlickrMinH = int.Parse(textFlickrMinH.Text);
        }

        void checkIfThemeValsChanged()
        {
            if (comboTheme.SelectedIndex != -1)
                buttSaveThemeChanges.Visible = themeValsChanged();
        }

        void checkIfWinThemeValsChanged()
        {
            if (comboWinTheme.SelectedIndex != -1)
                buttSaveWinTheme.Visible = winThemeValsChanged();
        }

        void checkIfGlobalValsChanged()
        {
            if (!loadingGlobalVals)
            {
                bool changesMade = globalValsChanged();
                buttSaveOptions.Visible = changesMade;
                buttWallSourcesSaveChanges.Visible = changesMade;
            }
        }

        bool themeValsChanged()
        {
            if (currWallDir != textWallDir.Text)
                return true;
            if (currDate != comboDate.Text)
                return true;
            if (currTime != comboTime.Text)
                return true;
            if (currWeather != comboWeather.Text)
                return true;
            if (currOverpower != checkOverpower.Checked)
                return true;

            return false;
        }

        bool winThemeValsChanged()
        {
            if (currWinThemeStyle != comboWinThemeStyle.Text)
                return true;
            if (currWinThemeColor != comboWinThemeColor.Text)
                return true;
            if (currWinThemeSounds != comboWinThemeSounds.Text)
                return true;
            if (currWinThemeSSaver != comboWinThemeSSaver.Text)
                return true;
            if (currWinThemeDate != comboWinThemeDate.Text)
                return true;

            return false;
        }

        bool globalValsChanged()
        {
            if (comboRefresh.SelectedIndex != currRefresh)
                return true;
            if ((int)numWCheck.Value != currWCheckPeriod)
                return true;
            if (textLocation.Text != currLocation)
                return true;
            if (textLatitude.Text != currLat.ToString())
                return true;
            if (textLongitude.Text != currLon.ToString())
                return true;
            if (textTimezone.Text.Replace("+", "") != currTZone.ToString())
                return true;
            if (trackUpdate.Value != currUpdateNotifs)
                return true;
            if (checkShowChangelog.Checked != currShowChangelog)
                return true;
            if (checkWinManager.Checked != currWinManager)
                return true;
            if (trackLocal.Value != currLocalFreq)
                return true;
            if (trackDropbox.Value != currDropboxFreq)
                return true;
            if (trackFlickr.Value != currFlickrFreq)
                return true;
            if (textFlickrMinW.Text != currFlickrMinW.ToString())
                return true;
            if (textFlickrMinH.Text != currFlickrMinH.ToString())
                return true;

            return false;
        }

        void getActiveWinTheme()
        {
            string path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes", "CurrentTheme", "not_found").ToString();
            path = Misc.GetFilename(path);
            activeWinTheme = path.Substring(0, path.LastIndexOf('.'));

            if (activeWinTheme.Contains('('))
                activeWinTheme = activeWinTheme.Substring(0, activeWinTheme.LastIndexOf(" ("));
        }

        void applyWinTheme(WinTheme winTheme, bool alwaysCreateNewTheme)
        {
            if (!alwaysCreateNewTheme)
            {
                //does theme already exist?
                string themePath = findWinThemeFile(winTheme.name);

                if (themePath != "not_found")
                {
                    runTheme(new FileInfo(themePath), false);
                    return;
                }
            }

            //otherwise, create new theme file
            FileInfo themeFile = new FileInfo(Application.StartupPath + "\\" + winTheme.name + ".theme");
            
            if (themeFile.Exists)
                themeFile.Delete();

            StreamReader fRdr = new StreamReader(Application.StartupPath + "\\template.theme");
            string themeContents = fRdr.ReadToEnd();
            fRdr.Close();

            //convert to win7 color format
            string color = "";
            string[] temp;

            foreach (string win7Color in win7Colors)
            {
                temp = win7Color.Split('=');

                if (temp[0] == winTheme.color)
                {
                    color = temp[1];
                    break;
                }
            }

            if (color == "" && winTheme.color.Contains('-'))
            {
                //convert from rgb
                temp = winTheme.color.Split('-');
                color = "0X77" + int.Parse(temp[0]).ToString("X").PadRight(2, '0') + int.Parse(temp[1]).ToString("X").PadRight(2, '0') + int.Parse(temp[2]).ToString("X").PadRight(2, '0');
            }

            themeContents = themeContents.Replace("%WALL_DIR%", Application.StartupPath + "\\walls_current").Replace("%REFRESHRATE%", getRefreshPeriod().ToString()).Replace("%NAME%", winTheme.name).Replace("%STYLE%", @"%SystemRoot%\resources\Themes\" + winTheme.style).Replace("%COLOR%", color).Replace("%SOUNDS%", winTheme.sounds).Replace("%SSAVER%", winTheme.ssaver);

            StreamWriter fWrt = themeFile.CreateText();
            fWrt.Write(themeContents);
            fWrt.Close();

            runTheme(themeFile, true);
        }

        void updateWinTheme(WinTheme winTheme)
        {
            //find old file
            string themePath = findWinThemeFile(winTheme.name);;

            //delete old file
            if (themePath != "not_found")
                File.Delete(themePath);

            //apply theme (creating a new file)
            applyWinTheme(winTheme, true);

            ////delete old file
            //if (themePath != "not_found")
            //    File.Delete(themePath);
        }

        string findWinThemeFile(string name)
        {
            //find all theme files that match this name (including copies)
            //for example, "Winter" should match "Winter", "Winter (1)", "Winter (2)", but not "Winter Holidays"
            List<string> files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows\\Themes\\", "*" + name + "*").ToList();

            for (int i = 0; i < files.Count; i++)
            {
                string fname = Path.GetFileNameWithoutExtension(files[i]);

                int lb = fname.IndexOf('(');
                int ub = fname.LastIndexOf(')');

                if (ub != -1 && lb != -1)
                    fname = fname.Remove(lb, ub + 1 - lb);

                while (fname.Length > 0 && fname[fname.Length - 1] == ' ')
                    fname = fname.Substring(0, fname.Length - 1);

                if (fname.ToLower() != name.ToLower())
                    files.RemoveAt(i--);
            }

            if (files.Count != 1)
                return "not_found";

            return files[0];
        }

        void runTheme(FileInfo themeFile, bool delAfter)
        {
            //apply theme file to windows
            Process proc = Process.Start(themeFile.FullName);
            
            Thread.Sleep(1000);

            proc.WaitForExit();
            //Personalization.Hide();

            if (delAfter)
            {
                themeFile = new FileInfo(Application.StartupPath + "\\" + themeFile.Name);

                //wait until finished applying
                while (themeFile.IsReadOnly)
                    Thread.Sleep(1500);

                themeFile.Delete(); //cleanup
            }
        }

        void calcTwilights()
        {
            if (currLon == 0 && currLat == 0)
            {
                sunrise = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
                sunset = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
                return;
            }

            //calc julian day
            int a = (14 - DateTime.Now.Month) / 12;
            int y = DateTime.Now.Year + 4800 - a;
            int m = DateTime.Now.Month + 12 * a - 3;
            int JD = DateTime.Now.Day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;

            //calc julian cycle
            double lw = -currLon;
            double nAsterisk = JD - 2451545.0009 - lw / 360;
            double n = Math.Round(nAsterisk);

            //calc approximate solar noon
            double JAsterisk = 2451545.0009 + lw / 360 + n;

            //calc solar mean anomaly
            double M = (357.5291 + 0.98560028 * (JAsterisk - 2451545)) % 360;

            //calc equation of center
            double C = 1.9148 * sin(M) + 0.02 * sin(2 * M) + 0.0003 * sin(3 * M);

            //calc ecliptic longitude
            double λ = (M + 102.9372 + C + 180) % 360;

            //calc solar transit
            double JTransit = JAsterisk + 0.0053 * sin(M) - 0.0069 * sin(2 * λ);

            //calc sun declination
            double δ = Math.Asin(sin(λ) * sin(23.45)) * 180.0 / Math.PI;

            //calc hour angle
            double Φ = currLat;
            double ω0 = Math.Acos((sin(-0.83) - sin(Φ) * sin(δ)) / (cos(Φ) * cos(δ))) * 180.0 / Math.PI;

            //calc sunrise & sunset
            double JSet = 2451545.0009 + (ω0 + lw) / 360 + n + 0.0053 * sin(M) - 0.0069 * sin(2 * λ);
            double JRise = JTransit - (JSet - JTransit);

            //convert to time of day
            double JRiseDiff = 1.0 - JRise % 1.0f;
            double JSetDiff = JSet % 1.0f;

            sunrise = DateTime.Now.Date.AddHours(12).Subtract(new TimeSpan(0, 0, (int)(JRiseDiff * 86400)));
            sunset = DateTime.Now.Date.AddHours(12).AddSeconds(JSetDiff * 86400);

            sunrise = sunrise.AddHours(utcTZone());
            sunset = sunset.AddHours(utcTZone());
        }

        void calcFullMoon()
        {
            nextFullMoon = Moon.NextFullMoon(DateTime.Now);

            //set moon phase icon
            TimeSpan untilNextFMoon = nextFullMoon.Subtract(DateTime.Now);
            int icon = (12 - (int)Math.Round(untilNextFMoon.Ticks / (double)new TimeSpan(3, 16, 35, 30, 360).Ticks)) % 8;
            moonPhase = Application.StartupPath + "\\lunar phases\\" + icon + ".png";
        }

        void calcMoonRiseSet()
        {
            //approximation algorithm from: http://www.moonstick.com/moonriseset.htm

            TimeSpan untilNextFMoon = nextFullMoon.Subtract(DateTime.Now);
            double moonPhaseProgress = 1 - untilNextFMoon.TotalDays / 29.530588853;

            DateTime mRiseApprox = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0).AddHours(moonPhaseProgress * 24);
            DateTime mSetApprox = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0).AddHours(moonPhaseProgress * 24);

            DateTime utcTime = DateTime.Now.AddHours(-utcTZone());
            DateTime localTime = utcTime.AddMinutes((int)(60 * currLon / 15));
            TimeSpan localTimeAdjust = DateTime.Now.Subtract(localTime);

            mRiseApprox = mRiseApprox.Add(localTimeAdjust);
            mSetApprox = mSetApprox.Add(localTimeAdjust);

            DateTime nextWinSol = new DateTime(DateTime.Now.Year, 12, 21);
            if (nextWinSol < DateTime.Now)
                nextWinSol = nextWinSol.AddYears(1);
            TimeSpan untilNextWinSol = nextWinSol.Subtract(DateTime.Now);

            double moonMeanLocAngle = (moonPhaseProgress * 360 + 180) % 360;
            moonMeanLocAngle += 360 * (1 - untilNextWinSol.TotalDays / (365 + (DateTime.IsLeapYear(nextWinSol.Year) ? 1 : 0)));
            moonMeanLocAngle %= 360;

            //apply corrections
            double correction = calcCorrection(moonMeanLocAngle, currLat);
            if (correction == double.NaN)
                return;
            moonrise = mRiseApprox.AddHours(correction);

            correction = calcCorrection(moonMeanLocAngle, -currLat);
            if (correction == double.NaN)
                return;
            moonset = mSetApprox.AddHours(correction);

            if (moonrise > moonset)
                moonrise = moonrise.AddDays(-1);
        }

        void calcEaster()
        {
            DateTime nextVernalEquinox = new DateTime(DateTime.Now.Year + (DateTime.Now.Month > 3 || (DateTime.Now.Month == 3 && DateTime.Now.Day > 21) ? 1 : 0), 3, 21);
            DateTime ecclFullMoon = Moon.NextFullMoon(nextVernalEquinox);

            for (nextEaster = ecclFullMoon; nextEaster.DayOfWeek != DayOfWeek.Sunday; nextEaster = nextEaster.AddDays(1)) ;
        }

        double calcCorrection(double moonMeanLocAngle, double lat)
        {
            #region correction table
            string[,] corrections = {
{
"down",
"down",
"0",
"up",
"up",
"up",
"0",
"down",
"down",
}
,
{
"down",
"down",
"0",
"up",
"up",
"up",
"0",
"down",
"down",
}
,
{
"down",
"3:44",
"0",
"-3:44",
"up",
"-3:25",
"0",
"3:25",
"down",
}
,
{
"3:15",
"2:12",
"0",
"-2:12",
"-3:15",
"-1:52",
"0",
"1:52",
"3:15",
}
,
{
"2:04",
"1:32",
"0",
"-1:32",
"-2:04",
"-1:12",
"0",
"1:12",
"2:04",
}
,
{
"1:25",
"1:07",
"0",
"-1:07",
"-1:25",
"-0:47",
"0",
"0:47",
"1:25",
}
,
{
"0:58",
"0:49",
"0",
"-0:49",
"-0:58",
"-0:29",
"0",
"0:29",
"0:58",
}
,
{
"0:36",
"0:34",
"0",
"-0:34",
"-0:36",
"-0:15",
"0",
"0:15",
"0:36",
}
,
{
"0:18",
"0:22",
"0",
"-0:22",
"-0:18",
"-0:02",
"0",
"0:02",
"0:18",
}
,
{
"0",
"0:10",
"0",
"-0:10",
"0",
"0:10",
"0",
"-0:10",
"0",
}
,
{
"-0:18",
"-0:02",
"0",
"0:02",
"0:18",
"0:22",
"0",
"-0:22",
"-0:18",
}
,
{
"-0:36",
"-0:15",
"0",
"0:15",
"0:36",
"0:34",
"0",
"-0:34",
"-0:36",
}
,
{
"-0:58",
"-0:29",
"0",
"0:29",
"0:58",
"0:49",
"0",
"-0:49",
"-0:58",
}
,
{
"-1:25",
"-0:47",
"0",
"0:47",
"1:25",
"1:07",
"0",
"-1:07",
"-1:25",
}
,
{
"-2:04",
"-1:12",
"0",
"1:12",
"2:04",
"1:32",
"0",
"-1:32",
"-2:04",
}
,
{
"-3:15",
"-1:52",
"0",
"1:52",
"3:15",
"2:12",
"0",
"-2:12",
"-3:15",
}
,
{
"up",
"-3:25",
"0",
"3:25",
"down",
"3:44",
"0",
"-3:44",
"up",
}
,
{
"up",
"up",
"0",
"down",
"down",
"down",
"0",
"up",
"up",
}
,
{
"up",
"up",
"0",
"down",
"down",
"down",
"0",
"up",
"up",
}
};
            #endregion

            double y = 9 - currLat / 10;
            int interpRowA = (int)y, interpRowB = (int)Math.Ceiling(y);

            double x = moonMeanLocAngle / 45;
            int interpColA = (int)x, interpColB = (int)Math.Ceiling(x);
            if (interpColB == 8)
                interpColB = 0;

            //check for down/up
            int closestRow = y - interpRowA < 0.5 ? interpRowA : interpRowB;
            int closestCol = x - interpColA < 0.5 ? interpColA : interpColB;

            if (corrections[closestRow, closestCol] == "up")
            {
                moonrise = DateTime.MinValue;
                moonset = DateTime.MaxValue;
                return double.NaN;
            }
            else if (corrections[closestRow, closestCol] == "down")
            {
                moonrise = DateTime.MaxValue;
                moonset = DateTime.MinValue;
                return double.NaN;
            }

            double aa = convertTime(corrections[interpRowA, interpColA]), ab = convertTime(corrections[interpRowA, interpColB]);
            double ba = convertTime(corrections[interpRowB, interpColA]), bb = convertTime(corrections[interpRowB, interpColB]);

            //interpolate
            double valLeftCol = interpolate(aa, ba, y - interpRowA);
            double valRightCol = interpolate(ab, bb, y - interpRowA);

            return interpolate(valLeftCol, valRightCol, x - interpColA);
        }

        double convertTime(string s)
        {
            if (s == "up" || s == "down")
                return double.NaN;
            else if (s == "0")
                return 0;
            else
            {
                int prefix = 1;
                if (s[0] == '-')
                {
                    prefix = -1;
                    s = s.Substring(1);
                }

                return prefix * TimeSpan.ParseExact(s, @"h\:mm", System.Globalization.CultureInfo.InvariantCulture).TotalHours;
            }
        }

        double interpolate(double val1, double val2, double p)
        {
            if (val1 == double.NaN)
                return val2;
            else if (val2 == double.NaN)
                return val1;
            else
                return val1 + (val2 - val1) * p;
        }

        string currSeason()
        {
            if (DateTime.Now.Month == 3 && DateTime.Now.Day >= 21 || DateTime.Now.Month == 4 || DateTime.Now.Month == 5 || DateTime.Now.Month == 6 && DateTime.Now.Day < 21)
                return "Spring";
            else if (DateTime.Now.Month == 6 && DateTime.Now.Day >= 21 || DateTime.Now.Month == 7 || DateTime.Now.Month == 8 || DateTime.Now.Month == 9 && DateTime.Now.Day < 23)
                return "Summer";
            else if (DateTime.Now.Month == 9 && DateTime.Now.Day >= 23 || DateTime.Now.Month == 10 || DateTime.Now.Month == 11 || DateTime.Now.Month == 12 && DateTime.Now.Day < 21)
                return "Autumn";
            //else if (DateTime.Now.Month == 12 && DateTime.Now.Day >= 21 || DateTime.Now.Month == 1 || DateTime.Now.Month == 2 || DateTime.Now.Month == 3 && DateTime.Now.Day < 21)
            else
                return "Winter";
        }

        string dlPage(string URL)
        {
            try
            {
                WebClient web = new WebClient();
                web.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                return web.DownloadString(URL);
            }
            catch
            {
                return "error";
            }
        }

        void checkWeather()
        {
            //string report = dlPage("http://www.google.com/ig/api?weather=" + textLocation.Text);
            //google killed this service :(

            string report = dlPage("http://www.wolframalpha.com/input/?i=weather+" + currLocation);

            if (report == "error" || report.Contains("Using closest Wolfram|Alpha interpretation"))
            {
                if (!forcedWeather)
                {
                    weathClear = false;
                    weathCloudy = false;
                    weathRain = false;
                    weathSnow = false;
                }

                return;
            }

            int lb = report.IndexOf("<dd class=\"conditions\">") + 23;
            int ub = report.IndexOf('<', lb);

            setWeatherFlags(report.Substring(lb, ub - lb).ToLower());
            setWeatherIcon();
        }

        double sin(double degs)
        {
            return Math.Sin(degs * Math.PI / 180.0);
        }

        double cos(double degs)
        {
            return Math.Cos(degs * Math.PI / 180.0);
        }

        double parseCoord(string txt)
        {
            txt = txt.Replace(" ", "");

            double coord = parseCoordElement(ref txt, "&deg;");
            coord += parseCoordElement(ref txt, "\\'") / 60;
            coord += parseCoordElement(ref txt, "&quot;") / 3600;

            if (txt[txt.Length - 1] == 'S' || txt[txt.Length - 1] == 'W')
                coord *= -1;

            return coord;
        }

        double parseCoordElement(ref string txt, string element)
        {
            if (!txt.Contains(element))
                return 0;

            double coordEl = double.Parse(txt.Substring(0, txt.IndexOf(element)));
            txt = txt.Substring(txt.IndexOf(element) + element.Length);

            return coordEl;
        }

        bool isValidNow(Theme theme)
        {
            //check date
            if (!checkDate(theme.date))
                return false;

            //check time
            string[] times = theme.time.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            bool hit = false;

            foreach (string time in times)
                if (time.Contains('-'))
                {
                    if (parseTime(time.Substring(0, time.IndexOf('-'))) < DateTime.Now && DateTime.Now < parseTime(time.Substring(time.IndexOf('-') + 1)))
                    {
                        hit = true;
                        break;
                    }
                }
                else
                {
                    if (checkTime(time))
                    {
                        hit = true;
                        break;
                    }
                }

            if (!hit)
                return false;

            //check weather
            switch (theme.weather)
            {
                case "Any weather":
                    return true;
                case "Clear":
                    return weathClear;
                case "Cloudy":
                    return weathCloudy;
                case "Rain":
                    return weathRain;
                case "Snow":
                    return weathSnow;
                default:
                    return false;
            }
        }

        bool checkDate(string date)
        {
            if (date == "Never")
                return false;

            if (date != "Any date" && date != season)
                if (date == "Spring" || date == "Summer" || date == "Autumn" || date == "Winter")
                    return false;
                else
                {
                    //check for margins
                    string thDate = date;
                    int margBefore = 0, margAfter = 0;

                    foreach (string ev in new string[] { "Full moon", "Halloween", "Easter" })
                        if (thDate.Contains(ev) && thDate != ev)
                        {
                            string[] margins = thDate.Replace(ev, "").Replace("--", "-").Split('-');
                            int.TryParse(margins[0], out margBefore);
                            int.TryParse(margins[1], out margAfter);

                            thDate = ev;
                            break;
                        }

                    //check if valid date
                    switch (thDate)
                    {
                        case "Halloween":
                            if (!isDateWithinMargins(new DateTime(DateTime.Now.Year, 10, 31), margBefore, margAfter))
                                return false;
                            break;
                        case "Easter":
                            if (!isDateWithinMargins(nextEaster, margBefore, margAfter))
                                return false;
                            break;
                        case "Full moon":
                            DateTime nightOfTheFullMoon;
                            if (nextFullMoon.Hour < 12)
                                nightOfTheFullMoon = nextFullMoon.AddDays(-1);
                            else
                                nightOfTheFullMoon = nextFullMoon;

                            if (!isDateWithinMargins(nightOfTheFullMoon, margBefore, margAfter))
                                return false;
                            break;
                        default:
                            string[] dates = date.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string dateExpr in dates)
                                if (dateExpr.Contains('-'))
                                {
                                    if (!(DateTime.Parse(dateExpr.Substring(0, dateExpr.IndexOf('-'))) > DateTime.Now || DateTime.Now.AddDays(-1) > DateTime.Parse(dateExpr.Substring(dateExpr.IndexOf('-') + 1))))
                                        return true;
                                }
                                else
                                {
                                    DateTime parsedDate;

                                    if (DateTime.TryParse(dateExpr, out parsedDate) && parsedDate.Date == DateTime.Now.Date)
                                        return true;
                                }

                            return false;
                    }
                }

            return true;
        }

        bool isDateWithinMargins(DateTime eventDate, int margBefore, int margAfter)
        {
            DateTime lbEvent = eventDate.AddDays(-margBefore), ubEvent = eventDate.AddDays(margAfter);

            lbEvent = new DateTime(lbEvent.Year, lbEvent.Month, lbEvent.Day, 0, 0, 0);
            ubEvent = new DateTime(ubEvent.Year, ubEvent.Month, ubEvent.Day, 23, 59, 59);

            return lbEvent < DateTime.Now && DateTime.Now < ubEvent;
        }

        bool checkTime(string time)
        {
            switch (time.ToLower())
            {
                case "anytime":
                    return true;
                case "day":
                    return isDay(30);
                case "night":
                    return !isDay(-30);
                case "twilight":
                    return (DateTime.Now > sunrise.AddMinutes(-30) && DateTime.Now < sunrise.AddMinutes(30)) || (DateTime.Now > sunset.AddMinutes(-30) && DateTime.Now < sunset.AddMinutes(30));
                default:
                    string[] times = time.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string timeExpr in times)
                        if (timeExpr.Contains('-'))
                        {
                            if (parseTime(timeExpr.Substring(0, timeExpr.IndexOf('-'))) < DateTime.Now && DateTime.Now < parseTime(timeExpr.Substring(timeExpr.IndexOf('-') + 1)))
                                return true;
                        }
                        else
                        {
                            DateTime dt;
                            if (DateTime.TryParse(timeExpr, out dt))
                            {
                                if (dt.Hour == DateTime.Now.Hour)
                                    return true;
                            }
                            else
                            {
                                int h;
                                if (timeExpr.Length <= 2 && int.TryParse(timeExpr, out h) && h == DateTime.Now.Hour)
                                    return true;
                            }
                        }

                    return false;
            }
        }

        DateTime parseTime(string time)
        {
            DateTime res;

            switch (time.ToLower())
            {
                case "prev_sunrise":
                    res = sunrise.AddDays(-1);
                    break;
                case "sunrise":
                    res = sunrise;
                    break;
                case "next_sunrise":
                    res = sunrise.AddDays(1);
                    break;
                case "prev_sunset":
                    res = sunset.AddDays(-1);
                    break;
                case "sunset":
                    res = sunset;
                    break;
                case "next_sunset":
                    res = sunset.AddDays(1);
                    break;
                default:
                    int h;
                    if (!DateTime.TryParse(time, out res))
                    {
                        if (time.Length <= 2 && int.TryParse(time, out h))
                            res = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, h, 0, 0);
                        else
                            res = new DateTime();
                    }
                    break;
            }

            if (DateTime.Now < sunrise)
                res = res.AddDays(-1);

            return res;
        }

        bool isDay(int twiMinuteMargin)
        {
            return DateTime.Now > sunrise.AddMinutes(twiMinuteMargin) && DateTime.Now < sunset.AddMinutes(-twiMinuteMargin);
        }

        void loadThemes(string path)
        {
            StreamReader file = new StreamReader(path);

            while (!file.EndOfStream)
            {
                string name = file.ReadLine();
                bool conflict = false;

                foreach (Theme theme in themes)
                    if (theme.name == name)
                    {
                        conflict = true;        
                        break;
                    }

                //when adding themes from a wallpaper pack, check for name conflicts
                if (path != Application.StartupPath + "\\themes.txt" && conflict && MessageBox.Show("Theme name: " + name + ". Add it anyway?", "You already have a theme with the same name as one being added.", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.No)
                {
                    //load the rest of the theme4
                    file.ReadLine();
                    file.ReadLine();
                    file.ReadLine();
                    file.ReadLine();
                    file.ReadLine();
                    
                    //and move on
                    continue;
                }

                themes.Add(new Theme(name, file.ReadLine(), file.ReadLine(), file.ReadLine(), file.ReadLine(), bool.Parse(file.ReadLine())));
                comboTheme.Items.Add(themes[themes.Count - 1].name);
            }

            file.Close();
        }

        void loadOptions()
        {
            try
            {
                StreamReader file = new StreamReader(Application.StartupPath + "\\options.txt");

                textLocation.Text = file.ReadLine();
                textLatitude.Text = file.ReadLine();
                textLongitude.Text = file.ReadLine();
                textTimezone.Text = file.ReadLine();
                comboRefresh.SelectedIndex = int.Parse(file.ReadLine());
                numWCheck.Value = int.Parse(file.ReadLine());
                firstRun = bool.Parse(file.ReadLine());
                Archiver = file.ReadLine();
                trackUpdate.Value = int.Parse(file.ReadLine());
                checkShowChangelog.Checked = bool.Parse(file.ReadLine());
                checkWinManager.Checked = bool.Parse(file.ReadLine());
                trackLocal.Value = int.Parse(file.ReadLine());
                trackDropbox.Value = int.Parse(file.ReadLine());
                trackFlickr.Value = int.Parse(file.ReadLine());
                textFlickrMinW.Text = file.ReadLine();
                textFlickrMinH.Text = file.ReadLine();
                file.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Corrupted options file!");
            }

            refreshUpdateNotifLabel();
            noteCurrGlobalVals();
            checkSourcesForDisables();

            //load weather descriptors
            StreamReader descFile = new StreamReader(Application.StartupPath + "\\weather.txt");
            descClear = descFile.ReadLine().Split('/');
            descCloudy = descFile.ReadLine().Split('/');
            descRain = descFile.ReadLine().Split('/');
            descSnow = descFile.ReadLine().Split('/');
            descFile.Close();
        }

        public void SaveOptions()
        {
            StreamWriter file = new StreamWriter(Application.StartupPath + "\\options.txt");

            file.WriteLine(textLocation.Text);
            file.WriteLine(textLatitude.Text);
            file.WriteLine(textLongitude.Text);
            file.WriteLine(textTimezone.Text);
            file.WriteLine(comboRefresh.SelectedIndex);
            file.WriteLine(numWCheck.Value);
            file.WriteLine(firstRun);
            file.WriteLine(Archiver);
            file.WriteLine(trackUpdate.Value);
            file.WriteLine(checkShowChangelog.Checked);
            file.WriteLine(checkWinManager.Checked);

            file.WriteLine(trackLocal.Value);
            file.WriteLine(trackDropbox.Value);
            file.WriteLine(trackFlickr.Value);
            file.WriteLine(textFlickrMinW.Text);
            file.WriteLine(textFlickrMinH.Text);

            file.Close();

            noteCurrGlobalVals();
        }

        void setWeatherFlags(string weather)
        {
            if (descClear.Contains(weather))
            {
                if (forcedWeather && !newForcedWeather)
                {
                    if (weathClear && !weathCloudy && !weathRain && !weathSnow)
                    {
                        forcedWeather = false;
                        picWIcon.BackColor = Color.FromKnownColor(KnownColor.Control);
                    }

                    return;
                }

                weathClear = true;
                weathCloudy = false;
                weathRain = false;
                weathSnow = false;
            }
            else if (descCloudy.Contains(weather))
            {
                if (forcedWeather && !newForcedWeather)
                {
                    if (!weathClear && weathCloudy && !weathRain && !weathSnow)
                    {
                        forcedWeather = false;
                        picWIcon.BackColor = Color.FromKnownColor(KnownColor.Control);
                    }

                    return;
                }

                weathClear = false;
                weathCloudy = true;
                weathRain = false;
                weathSnow = false;
            }
            else if (descRain.Contains(weather))
            {
                if (forcedWeather && !newForcedWeather)
                {
                    if (!weathClear && weathCloudy && weathRain && !weathSnow)
                    {
                        forcedWeather = false;
                        picWIcon.BackColor = Color.FromKnownColor(KnownColor.Control);
                    }

                    return;
                }

                weathClear = false;
                weathCloudy = true;
                weathRain = true;
                weathSnow = false;
            }
            else if (descSnow.Contains(weather))
            {
                if (forcedWeather && !newForcedWeather)
                {
                    if (!weathClear && weathCloudy && !weathRain && weathSnow)
                    {
                        forcedWeather = false;
                        picWIcon.BackColor = Color.FromKnownColor(KnownColor.Control);
                    }

                    return;
                }

                weathClear = false;
                weathCloudy = true;
                weathRain = false;
                weathSnow = true;
            }
            else if (weather != "" && weather != "lass=\"\">")
                MessageBox.Show("Unrecognized weather condition: [" + weather + "]");
        }

        void setWeatherIcon()
        {
            if (isDay(0))
            {
                if (weathClear)
                    picWIcon.ImageLocation = Application.StartupPath + "\\weather icons\\day - clear.png";
                else if (weathRain)
                    picWIcon.ImageLocation = Application.StartupPath + "\\weather icons\\day - rain.png";
                else if (weathSnow)
                    picWIcon.ImageLocation = Application.StartupPath + "\\weather icons\\day - snow.png";
                else 
                    picWIcon.ImageLocation = Application.StartupPath + "\\weather icons\\day - cloudy.png";
            }
            else
            {
                if (weathRain)
                    picWIcon.ImageLocation = Application.StartupPath + "\\weather icons\\night - rain.png";
                else if (weathSnow)
                    picWIcon.ImageLocation = Application.StartupPath + "\\weather icons\\night - snow.png";
                else
                    picWIcon.ImageLocation = moonPhase;
            }

            saveWeatherStatus();
        }

        void saveWeatherStatus()
        {
            int lb = picWIcon.ImageLocation.LastIndexOf('\\') + 1;
            int ub = picWIcon.ImageLocation.LastIndexOf('.');
            string wIconFilename = picWIcon.ImageLocation.Substring(lb, ub - lb);

            int wStatus;
            if (!int.TryParse(wIconFilename, out wStatus))
                switch (wIconFilename)
                {
                    case "day - clear":
                        wStatus = 8;
                        break;
                    case "day - cloudy":
                        wStatus = 9;
                        break;
                    case "day - rain":
                        wStatus = 10;
                        break;
                    case "day - snow":
                        wStatus = 11;
                        break;
                    case "night - rain":
                        wStatus = 12;
                        break;
                    case "night - snow":
                        wStatus = 13;
                        break;
                }

            StreamWriter file = new StreamWriter(Application.StartupPath + "\\weather_status.txt");
            file.WriteLine(wStatus);
            file.Close();
        }

        bool winThemeNameTaken(string name)
        {
            foreach (WinTheme winTheme in winThemes)
                if (winTheme.name == name)
                    return true;

            return false;
        }

        string getOutputFName(string path)
        {
            return path.Substring(path.LastIndexOf("\\", path.LastIndexOf("\\") - 1) + 1).Replace("\\", "_");
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string mainWallDir = createSubdir("walls_current", false);

            if (currWinManager)
                getActiveWinTheme();

            string status = "Status:" + Environment.NewLine + "Date: " + DateTime.Now.ToString("d. M. yyyy") + " - " + season + Environment.NewLine + "Time: " + DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine + "Sunrise/sunset: " + sunrise.ToString("HH:mm") + "/" + sunset.ToString("HH:mm") + Environment.NewLine;

            if (DateTime.Now.Date != sunrise.Date)
                calcTwilights(); //recalculate sunrise/sunset

            //get weather conditions
            if (DateTime.Now - prevWeatherCheck > TimeSpan.FromMinutes(currWCheckPeriod))
            {
                status += "Checking weather..." + Environment.NewLine;

                prevWeatherCheck = DateTime.Now;
                checkWeather();
            }
            else
                status += "Weather: " + (weathClear ? "Clear" : "") + (weathCloudy ? "Cloudy" : "") + (weathRain ? " & Rain" : "") + (weathSnow ? " & Snow" : "") + (forcedWeather ? " (forced)" : "") + Environment.NewLine;

            status += "Next full moon: " + nextFullMoon.ToString("dd. MM. yyyy @ HH:mm") + Environment.NewLine;

            if (currWinManager)
            {
                //check for win theme change
                WinTheme validWinTheme = null;

                foreach (WinTheme winTheme in winThemes)
                    if (checkDate(winTheme.date))
                        validWinTheme = winTheme;

                if (validWinTheme != null && validWinTheme.name != activeWinTheme)
                {
                    applyWinTheme(validWinTheme, false);
                    getActiveWinTheme();
                }

                status += "Active windows theme: " + activeWinTheme + Environment.NewLine;
            }

            //list new walls
            List<string> newWalls = new List<string>();
            List<string> dropboxThemes = new List<string>();
            string activeThemes = "";

            bool overpoweredTheme = false;
            foreach (Theme theme in themes)
                if (isValidNow(theme) && theme.overpower)
                {
                    overpoweredTheme = true;
                    break;
                }

            foreach (Theme theme in themes)
                if (isValidNow(theme) && theme.overpower == overpoweredTheme && Directory.Exists(theme.wallDir))
                {
                    newWalls.AddRange(Directory.GetFiles(theme.wallDir));
                    activeThemes += theme.name + ", ";

                    //add to dropbox themes (themes used to look for wallpaper themes on dropbox (AND flickr))
                    //accept only general themes (themes using terms like Autumn or Day) and holiday themes
                    if (theme.name == "Full Moon" || theme.name.Contains("Holidays"))
                        dropboxThemes.Add(theme.name);
                    else if ((theme.date == "Any date" || theme.date == "Spring" || theme.date == "Summer" || theme.date == "Autumn" || theme.date == "Winter")
                        && (theme.time == "Any time" || theme.time == "Day" || theme.time == "Twilight" || theme.time == "Night"))
                    {
                        string dropboxTheme = "";

                        if (theme.date != "Any date")
                            dropboxTheme = theme.date;
                        if (theme.time != "Any time")
                            dropboxTheme += (dropboxTheme != "" ? " - " : "") + theme.time;
                        if (theme.weather != "Any weather")
                            dropboxTheme += (dropboxTheme != "" ? " - " : "") + theme.weather;

                        dropboxThemes.Add(dropboxTheme);
                    }
                }

            if (activeThemes.Length > 2)
                activeThemes = activeThemes.Substring(0, activeThemes.Length - 2);

            status += "Active wallpaper themes: " + activeThemes;

            //eliminate duplicates
            newWalls.Sort();

            for (int i = 0; i < newWalls.Count - 1; )
                if (newWalls[i] == newWalls[i + 1])
                    newWalls.RemoveAt(i);
                else
                    i++;

            //show warnings in status msg
            status += (currLat == 0 && currLon == 0 ? Environment.NewLine + "Your geolocation is not assigned!" : "");
            status += (textTimezone.Text == "" ? Environment.NewLine + "Your time zone is not assigned!" : "");

            e.Result = status;

            //theme change?
            if (activeThemes != prevActiveThemes || newWalls.Count != oldWallsCount || DateTime.Now.Subtract(lastWallChange).TotalMilliseconds >= getRefreshPeriod())
            {
                if (currWinManager)
                    switch (pickWallSource())
                    {
                        case WallpaperSource.Local:
                            //get rid of invalid walls
                            string[] oldWalls = Directory.GetFiles(mainWallDir);
                            oldWallsCount = oldWalls.Length;

                            string oldWallFName;
                            bool wallAlreadyActive;

                            foreach (string oldWall in oldWalls)
                            {
                                oldWallFName = Misc.GetFilename(oldWall);
                                wallAlreadyActive = false;

                                foreach (string newWall in newWalls)
                                    if (getOutputFName(newWall) == oldWallFName)
                                    {
                                        wallAlreadyActive = true;
                                        oldWallFName = newWall;

                                        break;
                                    }

                                if (wallAlreadyActive)
                                    newWalls.Remove(oldWallFName);
                                else
                                    File.Delete(oldWall);
                            }

                            //add new valid walls
                            foreach (string newWall in newWalls)
                                File.Copy(newWall, mainWallDir + "\\" + getOutputFName(newWall));
                            break;
                        case WallpaperSource.Dropbox:
                            //dl new wall
                            appliedWallPath = Dropbox.GetWallpaper(pickRandomTheme(dropboxThemes), createSubdir("temp dl", true), out onlineWallSource, bannedWalls);

                            //delete all current wallpapers
                            mainWallDir = createSubdir("walls_current", true);

                            //copy new wall
                            File.Copy(appliedWallPath, Application.StartupPath + "\\walls_current\\" + Path.GetFileName(appliedWallPath));
                            break;
                        case WallpaperSource.Flickr:
                            //dl new wall
                            appliedWallPath = FlickrSource.GetWallpaper(pickRandomTheme(dropboxThemes), currFlickrMinW, currFlickrMinH, createSubdir("temp dl", true), out onlineWallSource, bannedWalls, banWallpaper);

                            //delete all current wallpapers
                            mainWallDir = createSubdir("walls_current", true);

                            //copy new wall
                            File.Copy(appliedWallPath, Application.StartupPath + "\\walls_current\\" + Path.GetFileName(appliedWallPath));
                            break;
                    }
                else
                    switch (pickWallSource())
                    {
                        case WallpaperSource.Local:
                            if (newWalls.Count > 0)
                                applyWallpaper(newWalls[rand.Next(newWalls.Count)]);
                            break;
                        case WallpaperSource.Dropbox:
                            applyWallpaper(Dropbox.GetWallpaper(pickRandomTheme(dropboxThemes), createSubdir("temp dl", true), out onlineWallSource, bannedWalls));
                            break;
                        case WallpaperSource.Flickr:
                            applyWallpaper(FlickrSource.GetWallpaper(pickRandomTheme(dropboxThemes), currFlickrMinW, currFlickrMinH, createSubdir("temp dl", true), out onlineWallSource, bannedWalls, banWallpaper));
                            break;
                    }

                lastWallChange = DateTime.Now;
                prevActiveThemes = activeThemes;
                oldWallsCount = newWalls.Count;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblStatus.Text = e.Result.ToString();
        }

        void checkSourcesForDisables()
        {
            lblLocalDisabled.Visible = trackLocal.Value == 0;
            lblDropboxDisabled.Visible = trackDropbox.Value == 0;
            lblFlickrDisabled.Visible = trackFlickr.Value == 0;
        }

        string createSubdir(string name, bool purgeFiles)
        {
            string dir = Application.StartupPath + "\\" + name + "\\";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                if (purgeFiles)
                    foreach (string file in Directory.GetFiles(dir))
                        File.Delete(file);
            }
            catch
            {
                //file currently in use; probably because the user is saving it right now
                //it'll get deleted eventually
            }

            return dir;
        }

        void applyWallpaper(string path)
        {
            if (path == "")
                return;

            //convert to bmp
            if (File.Exists(Application.StartupPath + "\\curr_wallpaper.bmp"))
                File.Delete(Application.StartupPath + "\\curr_wallpaper.bmp");

            Image img = Image.FromFile(path);
            img.Save(Application.StartupPath + "\\curr_wallpaper.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            img.Dispose();

            //apply wallpaper
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, Application.StartupPath + "\\curr_wallpaper.bmp", SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            
            appliedWallPath = path;
        }

        string pickRandomTheme(List<string> themes)
        {
            //more specific themes have a greater chance to be picked
            int[] odds = new int[themes.Count];
            int total = 0;

            for (int i = 0; i < themes.Count; i++)
            {
                odds[i] = 1 + themes[i].Count(c => c == '-') * 4;
                total += odds[i];
            }

            int pick = rand.Next(total), prevTotal = 0;

            for (int i = 0; i < themes.Count; i++)
            {
                if (pick < prevTotal + odds[i])
                    return themes[i];

                prevTotal += odds[i];
            }

            return "";
        }

        void loadBannedList()
        {
            bannedWalls = new List<string>();

            StreamReader file = new StreamReader(Application.StartupPath + "\\banned_walls.txt");
            while (!file.EndOfStream)
                bannedWalls.Add(file.ReadLine());
            file.Close();
        }

        void banWallpaper(string url)
        {
            if (url == "<current>")
            {
                if (onlineWallSource != "")
                    url = onlineWallSource;
                else
                    return;
            }

            StreamWriter file = new StreamWriter(Application.StartupPath + "\\banned_walls.txt", true);
            file.WriteLine(url);
            file.Close();
        }

        WallpaperSource pickWallSource()
        {
            int randomPick = rand.Next(100);

            if (randomPick < currLocalFreq)
                return WallpaperSource.Local;
            else if (randomPick < currLocalFreq + currDropboxFreq)
                return WallpaperSource.Dropbox;
            else
                return WallpaperSource.Flickr;
        }


        public formMain()
        {
            InitializeComponent();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            
            loadOptions();
            loadingGlobalVals = false;

            loadBannedList();

            //check win version
            if (Environment.OSVersion.Version.Major <= 5)
            {
                //xp or lower
                currWinManager = false;
                checkWinManager.Checked = false;
                checkWinManager.Enabled = false;

                lblXPWinThemes.Visible = true;
            }
            else
                getActiveWinTheme();

            //installed update -> delete old version
            string[] files = Directory.GetFiles(Application.StartupPath);

            foreach (string fPath in files)
                if (fPath.Contains("Wallcreeper-OBSOLETE"))
                    File.Delete(Application.StartupPath + "\\Wallcreeper-OBSOLETE.exe");

            //precalculations
            rand = new Random((int)DateTime.Now.Ticks);

            season = currSeason();
            calcTwilights();
            calcFullMoon();
            //calcMoonRiseSet();
            calcEaster();

            //load themes
            loadThemes(Application.StartupPath + "\\themes.txt");

            //load win themes
            StreamReader file = new StreamReader(Application.StartupPath + "\\winThemes.txt");

            while (!file.EndOfStream)
            {
                winThemes.Add(new WinTheme(file.ReadLine(), file.ReadLine(), file.ReadLine(), file.ReadLine(), file.ReadLine(), file.ReadLine()));
                comboWinTheme.Items.Add(winThemes[winThemes.Count - 1].name);
            }

            file.Close();

            //prepare choices for win themes
            //load visual styles
            foreach (string vstyle in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Resources), "*.msstyles", SearchOption.AllDirectories))
                comboWinThemeStyle.Items.Add(vstyle.Substring(vstyle.IndexOf("\\Themes\\") + 8));

            //load win7 colors
            file = new StreamReader(Application.StartupPath + "\\win7 colors.txt");
            win7Colors = file.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            file.Close();

            foreach (string color in win7Colors)
                comboWinThemeColor.Items.Add(color.Substring(0, color.IndexOf('=')));

            //load sounds
            comboWinThemeSounds.Items.Add("Windows Default");
            foreach (string soundTheme in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Media"))
                comboWinThemeSounds.Items.Add(Misc.GetFilename(soundTheme));

            //load ssavers
            comboWinThemeSSaver.Items.Add("None");

            foreach (string ssaver in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "*.scr"))
                comboWinThemeSSaver.Items.Add(Misc.GetFilename(ssaver));

            if (comboWinThemeSSaver.Items.Count == 1)
                foreach (string ssaver in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.System), "*.scr"))
                    comboWinThemeSSaver.Items.Add(Misc.GetFilename(ssaver));

            if (trayMenu == null)
            {
                //initialize all menu items
                trayMenu = new ContextMenu();

                trayMenu.MenuItems.Add(0, new MenuItem("Change wallpaper", new System.EventHandler(Tray_ChangeWall_Click)));
                trayMenu.MenuItems.Add(1, new MenuItem("-"));
                trayMenu.MenuItems.Add(2, new MenuItem("Locate current wallpaper", new System.EventHandler(Tray_LocCurrWall_Click)));
                trayMenu.MenuItems.Add(3, new MenuItem("Open wallpaper webpage", new System.EventHandler(Tray_OpenWallWebPage_Click)));
                trayMenu.MenuItems.Add(4, new MenuItem("Save wallpaper to local theme", new System.EventHandler(Tray_SaveWall_Click)));
                trayMenu.MenuItems.Add(5, new MenuItem("-"));
                trayMenu.MenuItems.Add(6, new MenuItem("Options", new System.EventHandler(Tray_Options_Click)));
                trayMenu.MenuItems.Add(7, new MenuItem("Run at startup", new System.EventHandler(Tray_RunAtStartup_Click)));
                trayMenu.MenuItems.Add(8, new MenuItem("-"));
                trayMenu.MenuItems.Add(9, new MenuItem("Exit", new System.EventHandler(Tray_Exit_Click)));

                trayIcon.ContextMenu = trayMenu;
            }

            //check registry
            disableToggle = true;

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rkApp.GetValue("Wallcreeper") != null)
            {
                trayMenu.MenuItems[7].Checked = true;
                checkRunAtStartup.Checked = true;
            }

            disableToggle = false;

            //prepare background worker
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            //refresh
            worker.RunWorkerAsync();

            //update check
            bool[] askPermissions = new bool[3] { true, true, true };
            for (int i = 0; i < currUpdateNotifs; i++)
                askPermissions[i] = false;

            Updater.Update(0.97, "https://docs.google.com/document/pub?id=12XxGDFe_dEF8XawWLhQPIeMwOKBf-uI6C1dnW8MO9vA", askPermissions, currShowChangelog);

            //first run
            if (firstRun)
                trayIcon.ShowBalloonTip(10000, "Wallcreeper", "First time running Wallcreeper? Double click this icon to view the main window.", ToolTipIcon.Info);
        }

        private void formMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.Visible = false;
            else if (this.WindowState == FormWindowState.Normal)
            {
                if (firstRun)
                {
                    firstRun = false;
                    SaveOptions();

                    //add sample wallpaper theme?
                    if (themes.Count == 0 && Directory.Exists(Application.StartupPath + "\\sample theme") && MessageBox.Show("Would you like to create a sample wallpaper theme?", "Wallcreeper has no wallpaper themes.", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        themes.Add(new Theme("Sample theme", Application.StartupPath + "\\sample theme", "Any date", "Any time", "Any weather", false));

                        comboTheme.Items.Add("Sample theme");
                        comboTheme.SelectedIndex = themes.Count - 1;

                        saveThemes();
                    }

                    //add sample win theme?
                    if (currWinManager && winThemes.Count == 0 && MessageBox.Show("Would you like to add a sample windows theme?" + Environment.NewLine + "If you want to use Windows 7's theme manager you need to add a sample theme or create one yourself.", "Wallcreeper has no windows themes.", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        winThemes.Add(new WinTheme("Sample windows theme", comboWinThemeStyle.Items[0].ToString(), "Sky", "Windows Default", "None", "Any date"));

                        comboWinTheme.Items.Add("Sample windows theme");
                        comboWinTheme.SelectedIndex = winThemes.Count - 1;

                        saveWinThemes();
                    }
                }

                //tutorial
                new Tutorial(Application.StartupPath + "\\tutorials\\wallpapers.txt", this);
            }
        }

        private void comboTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTheme.SelectedIndex != -1)
                showTheme(comboTheme.SelectedIndex);

            buttCloneTheme.Visible = comboTheme.SelectedIndex != -1;
            buttRenameTheme.Visible = comboTheme.SelectedIndex != -1;
            buttDeleteTheme.Visible = comboTheme.SelectedIndex != -1;
        }

        private void buttNewTheme_Click(object sender, EventArgs e)
        {
            string themeName = "";
            classInputBox.Show("New Theme", "Theme name?", ref themeName);

            themes.Add(new Theme(themeName, "", "", "", "", false));

            comboTheme.Items.Add(themeName);
            comboTheme.SelectedIndex = themes.Count - 1;

            saveThemes();
        }

        private void buttCloneTheme_Click(object sender, EventArgs e)
        {
            if (comboTheme.SelectedIndex == -1)
                return;

            string themeName = themes[comboTheme.SelectedIndex].name + " - Clone";

            if (classInputBox.Show("New Theme", "Theme name?", ref themeName) == System.Windows.Forms.DialogResult.OK)
            {
                themes.Add(new Theme(themeName, themes[comboTheme.SelectedIndex].wallDir, themes[comboTheme.SelectedIndex].date, themes[comboTheme.SelectedIndex].time, themes[comboTheme.SelectedIndex].weather, themes[comboTheme.SelectedIndex].overpower));

                comboTheme.Items.Add(themeName);
                comboTheme.SelectedIndex = themes.Count - 1;

                saveThemes();
            }
        }

        private void buttRenameTheme_Click(object sender, EventArgs e)
        {
            string theme = comboTheme.Text;
            classInputBox.Show("Rename theme", "New theme name?", ref theme);

            if (theme != comboTheme.Text)
            {
                themes[comboTheme.SelectedIndex].name = theme;
                comboTheme.Items[comboTheme.SelectedIndex] = theme;

                saveThemes();
            }
        }

        private void buttDeleteTheme_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete theme?", comboTheme.Text, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                themes.RemoveAt(comboTheme.SelectedIndex);
                comboTheme.Items.RemoveAt(comboTheme.SelectedIndex);

                buttCloneTheme.Visible = false;
                buttRenameTheme.Visible = false;
                buttDeleteTheme.Visible = false;

                saveThemes();
            }
        }

        private void textWallDir_TextChanged(object sender, EventArgs e)
        {
            checkIfThemeValsChanged();
        }

        private void comboDate_TextChanged(object sender, EventArgs e)
        {
            checkIfThemeValsChanged();
        }

        private void comboTime_TextChanged(object sender, EventArgs e)
        {
            checkIfThemeValsChanged();
        }

        private void comboWeather_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkIfThemeValsChanged();
        }

        private void checkOverpower_CheckedChanged(object sender, EventArgs e)
        {
            checkIfThemeValsChanged();
        }

        private void buttSaveThemeChanges_Click(object sender, EventArgs e)
        {
            themes[comboTheme.SelectedIndex].wallDir = textWallDir.Text;
            themes[comboTheme.SelectedIndex].date = comboDate.Text;
            themes[comboTheme.SelectedIndex].time = comboTime.Text;
            themes[comboTheme.SelectedIndex].weather = comboWeather.Text;
            themes[comboTheme.SelectedIndex].overpower = checkOverpower.Checked;

            saveThemes();

            noteCurrThemeVals();
            buttSaveThemeChanges.Visible = false;
        }

        private void buttBrowse_Click(object sender, EventArgs e)
        {
            browseDiag.Description = "Select wallpaper directory" + Environment.NewLine + "This is the directory which contains wallpaper for this theme.";
            browseDiag.SelectedPath = textWallDir.Text;
            browseDiag.ShowDialog();

            textWallDir.Text = browseDiag.SelectedPath;
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void Tray_ChangeWall_Click(object sender, EventArgs e)
        {
            lastWallChange = DateTime.Now.AddDays(-2);
            //if (currWinManager)
            //    MessageBox.Show("If the wallpaper doesn't change that means you are currently using Windows's built-in wallpaper manager and local wallpaper sources." + Environment.NewLine + "Right-click on the Desktop and select \"Next desktop background\".");
        }

        private void Tray_Options_Click(object sender, EventArgs e)
        {
            tabs.SelectedIndex = 3;

            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void Tray_RunAtStartup_Click(object sender, EventArgs e)
        {
            toggleRunAtStartup();
        }

        private void Tray_LocCurrWall_Click(object sender, EventArgs e)
        {
            if (currWinManager)
            {
                string path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Desktop\General", "WallpaperSource", "").ToString();

                if (!File.Exists(path))
                {
                    if (appliedWallPath != "")
                        Process.Start("explorer.exe", " /select, " + appliedWallPath);

                    return;
                }

                int delimit = Misc.GetFilename(path).IndexOf('_');
                if (delimit == -1)
                {
                    Process.Start("explorer.exe", " /select, " + path);
                    return;
                }

                string themeName = Misc.GetFilename(path).Substring(0, delimit);
                string themeDir = "";

                foreach (Theme theme in themes)
                    if (theme.name == themeName)
                    {
                        themeDir = theme.wallDir;
                        break;
                    }

                if (themeDir == "")
                    Process.Start("explorer.exe", " /select, " + path);
                else
                    Process.Start("explorer.exe", " /select, " + themeDir + "\\" + Misc.GetFilename(path).Substring(delimit + 1));
            }
            else if (appliedWallPath != "")
                Process.Start("explorer.exe", " /select, " + appliedWallPath);
            else
                MessageBox.Show("Wallcreeper hasn't set a wallpaper yet.");
        }

        private void Tray_OpenWallWebPage_Click(object sender, EventArgs e)
        {
            if (onlineWallSource != "")
                Process.Start(onlineWallSource);
            else
                MessageBox.Show("The current wallpaper has no online source.");
        }

        private void Tray_SaveWall_Click(object sender, EventArgs e)
        {
            if (appliedWallPath == "")
                MessageBox.Show("The current wallpaper is not new.");
            else if (addWall == null || addWall.IsDisposed)
            {
                addWall = new formAddWall();
                addWall.Show();
                addWall.Init(appliedWallPath, themes, banWallpaper);
            }
        }

        private void Tray_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
            this.WindowState = FormWindowState.Normal;
        }

        private void buttGetData_Click(object sender, EventArgs e)
        {
            //geocoordinates

            try
            {
                lblWAStatus.Text = "Downloading geocoordinates...";
                this.Refresh();
                
                string page = dlPage("http://www.wolframalpha.com/input/?i=" + textLocation.Text + "+coordinates");
                
                if (page == "error")
                    throw new Exception();

                int lb = page.LastIndexOf('"', page.IndexOf("&deg;")) + 1;
                int ub = page.IndexOf('"', lb);
                string[] coords = page.Substring(lb, ub - lb).Split(',');

                textLatitude.Text = parseCoord(coords[0]).ToString();
                textLongitude.Text = parseCoord(coords[1]).ToString();
            }
            catch (Exception exc)
            {
                if (MessageBox.Show("Would you like to open Wolfram|Alpha webpage?", "Error while downloading geocoordinates.", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Yes)
                    Process.Start("http://www.wolframalpha.com/input/?i=" + textLocation.Text + "+coordinates");
            }

            try
            {
                //timezone
                lblWAStatus.Text = "Downloading time zone data...";
                this.Refresh();

                string page = dlPage("http://www.wolframalpha.com/input/?i=" + textLocation.Text + "+timezone");
                
                if (page == "error")
                    throw new Exception();

                int lb = page.IndexOf("UTC+") + 3;
                if (lb == 2)
                    lb = page.IndexOf("UTC-") + 3;
                if (lb == 2)
                    throw new Exception();

                int ub = page.IndexOf('"', lb);
                if (page[ub - 1] == '.')
                    ub--;

                textTimezone.Text = page.Substring(lb, ub - lb);

                //dst
                bool plusPrefix = textTimezone.Text[0] == '+';

                if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
                    textTimezone.Text = (plusPrefix ? "+" : "") + (int.Parse(textTimezone.Text) - 1).ToString();
            }
            catch (Exception exc)
            {
                if (MessageBox.Show("Would you like to open Wolfram|Alpha webpage?", "Error while downloading timezone information.", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Yes)
                    Process.Start("http://www.wolframalpha.com/input/?i=" + textLocation.Text + "+timezone");
            }

            lblWAStatus.Text = "";
        }

        private void buttSaveOptions_Click(object sender, EventArgs e)
        {
            bool recalcTwilights = textLatitude.Text != currLat.ToString() || textLongitude.Text != currLon.ToString() || textTimezone.Text != currTZone.ToString();
            bool changedRefreshRate = comboRefresh.SelectedIndex != currRefresh;
            bool changedWinTheme = checkWinManager.Checked != currWinManager;

            SaveOptions();
            buttSaveOptions.Visible = false;

            if (recalcTwilights)
                calcTwilights();

            if (changedRefreshRate && checkWinManager.Checked)
            {
                WinTheme currWinTheme = null;

                foreach (WinTheme winTheme in winThemes)
                    if (winTheme.name == activeWinTheme)
                    {
                        currWinTheme = winTheme;
                        break;
                    }

                if (currWinTheme != null)
                    updateWinTheme(currWinTheme);
            }

            if (changedWinTheme)
                getActiveWinTheme();
        }

        private void picWIcon_MouseEnter(object sender, EventArgs e)
        {
            picWIcon.BackColor = isDay(0) ? Color.White : Color.Black;
        }

        private void picWIcon_MouseLeave(object sender, EventArgs e)
        {
            if (!forcedWeather)
                picWIcon.BackColor = Color.FromKnownColor(KnownColor.Control);
        }

        private void picWIcon_Click(object sender, EventArgs e)
        {
            List<string> possibleIcons = new List<string>();

            if (isDay(0))
            {
                possibleIcons.Add(Application.StartupPath + "\\weather icons\\day - clear.png");
                possibleIcons.Add(Application.StartupPath + "\\weather icons\\day - cloudy.png");
                possibleIcons.Add(Application.StartupPath + "\\weather icons\\day - rain.png");

                //if (season == "Winter")
                possibleIcons.Add(Application.StartupPath + "\\weather icons\\day - snow.png");
            }
            else
            {
                possibleIcons.Add(moonPhase);
                possibleIcons.Add(Application.StartupPath + "\\weather icons\\night - rain.png");

                //if (season == "Winter")
                possibleIcons.Add(Application.StartupPath + "\\weather icons\\night - snow.png");
            }

            picWIcon.ImageLocation = possibleIcons[(possibleIcons.IndexOf(picWIcon.ImageLocation) + 1) % possibleIcons.Count];

            string flag = "";
            if (picWIcon.ImageLocation.Contains("phase"))
                flag = "clear";
            else
                flag = picWIcon.ImageLocation.Substring(picWIcon.ImageLocation.LastIndexOf(" - ") + 3, picWIcon.ImageLocation.Length - 4 - picWIcon.ImageLocation.LastIndexOf(" - ") - 3);

            newForcedWeather = true;
            setWeatherFlags(flag);
            saveWeatherStatus();
            newForcedWeather = false;
            forcedWeather = true;
        }

        private void picWIcon_DoubleClick(object sender, EventArgs e)
        {
            picWIcon.BackColor = Color.FromKnownColor(KnownColor.Control);
            this.Refresh();

            forcedWeather = false;
            checkWeather();
        }

        private void numWCheck_ValueChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void textLocation_TextChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void textLatitude_TextChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void textLongitude_TextChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void textTimezone_TextChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void comboShowMoon_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void buttColorPicker_Click(object sender, EventArgs e)
        {
            colorDiag.ShowDialog();

            comboWinThemeColor.Text = colorDiag.Color.R.ToString() + '-' + colorDiag.Color.G.ToString() + '-' + colorDiag.Color.B.ToString();
        }

        private void comboWinTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboWinTheme.SelectedIndex != -1)
            {
                //show win theme
                noteCurrWinThemeVals();

                comboWinThemeStyle.Text = winThemes[comboWinTheme.SelectedIndex].style;
                comboWinThemeColor.Text = winThemes[comboWinTheme.SelectedIndex].color;
                comboWinThemeSounds.Text = winThemes[comboWinTheme.SelectedIndex].sounds;
                comboWinThemeSSaver.Text = winThemes[comboWinTheme.SelectedIndex].ssaver;
                comboWinThemeDate.Text = winThemes[comboWinTheme.SelectedIndex].date;
            }

            buttRenameWinTheme.Visible = comboWinTheme.SelectedIndex != -1;
            buttDelWinTheme.Visible = comboWinTheme.SelectedIndex != -1;
        }

        private void buttNewWinTheme_Click(object sender, EventArgs e)
        {
            string winThemeName = "";
            classInputBox.Show("New Windows Theme", "Theme name?", ref winThemeName);

            winThemes.Add(new WinTheme(winThemeName, comboWinThemeStyle.Items[0].ToString(), "Sky", "Windows Default", "None", "Never"));

            comboWinTheme.Items.Add(winThemeName);
            comboWinTheme.SelectedIndex = winThemes.Count - 1;

            saveWinThemes();
        }

        private void comboWinThemeStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkIfWinThemeValsChanged();
        }

        private void comboWinThemeColor_TextChanged(object sender, EventArgs e)
        {
            checkIfWinThemeValsChanged();
        }

        private void comboWinThemeSounds_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkIfWinThemeValsChanged();
        }

        private void comboWinThemeSSaver_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkIfWinThemeValsChanged();
        }

        private void comboWinThemeDate_TextChanged(object sender, EventArgs e)
        {
            checkIfWinThemeValsChanged();
        }

        private void buttSaveWinTheme_Click(object sender, EventArgs e)
        {
            winThemes[comboWinTheme.SelectedIndex].style = comboWinThemeStyle.Text;
            winThemes[comboWinTheme.SelectedIndex].color = comboWinThemeColor.Text;
            winThemes[comboWinTheme.SelectedIndex].sounds = comboWinThemeSounds.Text;
            winThemes[comboWinTheme.SelectedIndex].ssaver = comboWinThemeSSaver.Text;
            winThemes[comboWinTheme.SelectedIndex].date = comboWinThemeDate.Text;

            saveWinThemes();

            if (activeWinTheme == comboWinTheme.Text && checkDate(winThemes[comboWinTheme.SelectedIndex].date))
                updateWinTheme(winThemes[comboWinTheme.SelectedIndex]);

            noteCurrWinThemeVals();
            buttSaveWinTheme.Visible = false;
        }

        private void buttCloneWinTheme_Click(object sender, EventArgs e)
        {
            if (comboWinTheme.SelectedIndex == -1)
                return;

            string winThemeName = winThemes[comboWinTheme.SelectedIndex].name + " - Clone";

            if (classInputBox.Show("New Windows Theme", "Theme name?", ref winThemeName) == System.Windows.Forms.DialogResult.OK)
            {
                while (winThemeNameTaken(winThemeName))
                    if (classInputBox.Show("Theme Name Already Taken", "Enter new name?", ref winThemeName) == System.Windows.Forms.DialogResult.Cancel)
                        return;


                winThemes.Add(new WinTheme(winThemeName, winThemes[comboWinTheme.SelectedIndex].style, winThemes[comboWinTheme.SelectedIndex].color, winThemes[comboWinTheme.SelectedIndex].sounds, winThemes[comboWinTheme.SelectedIndex].ssaver, winThemes[comboWinTheme.SelectedIndex].date));

                comboWinTheme.Items.Add(winThemeName);
                comboWinTheme.SelectedIndex = winThemes.Count - 1;

                saveWinThemes();
            }
        }

        private void buttRenameWinTheme_Click(object sender, EventArgs e)
        {
            string winTheme = comboWinTheme.Text;
            classInputBox.Show("Rename windows theme", "New theme name?", ref winTheme);

            if (winTheme != comboWinTheme.Text)
            {
                winThemes[comboWinTheme.SelectedIndex].name = winTheme;
                comboWinTheme.Items[comboWinTheme.SelectedIndex] = winTheme;

                saveWinThemes();
            }
        }

        private void buttDelWinTheme_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete windows theme?", comboWinTheme.Text, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                winThemes.RemoveAt(comboWinTheme.SelectedIndex);
                comboWinTheme.Items.RemoveAt(comboWinTheme.SelectedIndex);

                buttRenameWinTheme.Visible = false;
                buttDelWinTheme.Visible = false;

                saveWinThemes();
            }
        }

        private void buttExportPack_Click(object sender, EventArgs e)
        {
            formExport export = new formExport();

            export.main = this;
            export.themes = themes;

            export.Show();
        }

        private void buttImportPack_Click(object sender, EventArgs e)
        {
            openDiag.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openDiag.FileName = "";
            openDiag.Filter = "Wallcreeper pack|*.wcp|Unarchived pack info|*.txt";

            if (openDiag.ShowDialog() != System.Windows.Forms.DialogResult.Cancel && openDiag.FileName != "")
                importPack(openDiag.FileName);
        }

        private void formMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                string ext = path.Substring(path.LastIndexOf('.') + 1);

                if (Directory.Exists(path) || ext == "wcp" || ext == "txt")
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }

            e.Effect = DragDropEffects.None;
        }

        private void formMain_DragDrop(object sender, DragEventArgs e)
        {
            importPack(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
        }

        private void sZip_Exited(object sender, EventArgs e)
        {
            string path = findThemeFile(tempDir);

            if (path != "")
                processThemePackContents(path);

            //del temp dir
            Directory.Delete(tempDir, true);
        }

        private void checkRunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            toggleRunAtStartup();
        }

        private void checkWinManager_CheckedChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabs.SelectedIndex)
            {
                case 1:
                    if (!lblXPWinThemes.Visible)
                        new Tutorial(Application.StartupPath + "\\tutorials\\winthemes.txt", this);
                    break;
                case 2:
                    new Tutorial(Application.StartupPath + "\\tutorials\\options.txt", this);
                    break;
            }
        }

        private void trackUpdate_Scroll(object sender, EventArgs e)
        {
            refreshUpdateNotifLabel();
            checkIfGlobalValsChanged();
        }

        private void checkShowChangelog_CheckedChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void comboRefresh_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void buttDLWallPacks_Click(object sender, EventArgs e)
        {
            Process.Start("https://sourceforge.net/projects/wallcreeper/files/Wallpaper%20packs/");
        }

        private void trackLocal_Scroll(object sender, EventArgs e)
        {
            //adjust the other two slides to ensure their sum is equal to 100
            trackDropbox.Value = Math.Max(100 - trackLocal.Value - trackFlickr.Value, 0);
            trackFlickr.Value = 100 - trackLocal.Value - trackDropbox.Value;

            checkSourcesForDisables();
            checkIfGlobalValsChanged();
        }

        private void trackDropbox_Scroll(object sender, EventArgs e)
        {
            //adjust the other two slides to ensure their sum is equal to 100
            trackLocal.Value = Math.Max(100 - trackDropbox.Value - trackFlickr.Value, 0);
            trackFlickr.Value = 100 - trackLocal.Value - trackDropbox.Value;

            checkSourcesForDisables();
            checkIfGlobalValsChanged();
        }

        private void trackFlickr_Scroll(object sender, EventArgs e)
        {
            //adjust the other two slides to ensure their sum is equal to 100
            trackLocal.Value = Math.Max(100 - trackDropbox.Value - trackFlickr.Value, 0);
            trackDropbox.Value = 100 - trackLocal.Value - trackFlickr.Value;

            checkSourcesForDisables();
            checkIfGlobalValsChanged();
        }

        private void textFlickrMinW_TextChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void textFlickrMinH_TextChanged(object sender, EventArgs e)
        {
            checkIfGlobalValsChanged();
        }

        private void buttWallSourcesSaveChanges_Click(object sender, EventArgs e)
        {
            SaveOptions();
            buttWallSourcesSaveChanges.Visible = false;
        }
    }
}
