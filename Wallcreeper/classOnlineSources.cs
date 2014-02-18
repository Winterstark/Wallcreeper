using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FlickrNet;

namespace Wallcreeper
{
    class Dropbox
    {
        #region Dropbox Directory
        static readonly string[,] themes = { 
                                           {"Autumn", "https://www.dropbox.com/sh/f3e0jym0o65k7xi/Y1i3kZ1qBI"},
                                           {"Autumn - Day", "https://www.dropbox.com/sh/arh8ywi74qs4vlb/wfX2CICCcR"},
                                           {"Autumn - Day - Clear", "https://www.dropbox.com/sh/chhegtqyzmvubbl/33_unaEaZv"},
                                           {"Autumn - Day - Cloudy", "https://www.dropbox.com/sh/fu0j0en1z25nbi6/r2G5GlFtVo"},
                                           {"Autumn - Day - Rain", "https://www.dropbox.com/sh/wt95ut7qgacrcai/-WvqbTN3AY"},
                                           {"Autumn - Night", "https://www.dropbox.com/sh/e22a3qez3l6bpir/SOymSxfj95"},
                                           {"Autumn - Twilight - Clear", "https://www.dropbox.com/sh/p8n25a17k05x1g6/Qg1LkldBxS"},
                                           {"Autumn - Twilight - Cloudy", "https://www.dropbox.com/sh/bxcr07ff8yibmhz/oFpcw45vip"},
                                           {"Day - Snow", "https://www.dropbox.com/sh/6o0xpp4n7qtti0e/F2gYA1Zbj3"},
                                           {"Full Moon", "https://www.dropbox.com/sh/pstpzpz8byumasf/5CUMj3acIX"},
                                           {"Holidays - Easter", "https://www.dropbox.com/sh/o55368yfc3yc2ll/vJ0BHZsvOA"},
                                           {"Holidays - Halloween", "https://www.dropbox.com/sh/w3m4yl5hxvifh3a/gZA__zq59V"},
                                           {"Holidays - Winter", "https://www.dropbox.com/sh/xswejg3m6tz4k3z/CPwsAqKFCK"},
                                           {"Holidays - Winter - Christmas", "https://www.dropbox.com/sh/dmmsnhjwhokr8xp/CI49fKU6ui"},
                                           {"Holidays - Winter - New Year", "https://www.dropbox.com/sh/97tc7tfn8sizvqr/qOhIzAINW1"},
                                           {"Night - Rain", "https://www.dropbox.com/sh/qwwloxd7l29zo2a/5Esz1LJ4Fr"},
                                           {"Night - Snow", "https://www.dropbox.com/sh/wb7b6tgs9xl82m9/gZ4NkjrA-w"},
                                           {"Spring", "https://www.dropbox.com/sh/92zkw380q4njthc/nZAtpBPDQ3"},
                                           {"Spring - Day", "https://www.dropbox.com/sh/6lnp3c3p4e4ucxz/u3n3-hm0W1"},
                                           {"Spring - Day - Clear", "https://www.dropbox.com/sh/f1i5dj7boid9mrq/al0ikvb9_1"},
                                           {"Spring - Day - Cloudy", "https://www.dropbox.com/sh/z1g09v7eifa43r2/82S_0SeP_3"},
                                           {"Spring - Day - Rain", "https://www.dropbox.com/sh/w3ffbtb4hu81u41/0gmtTcWY3D"},
                                           {"Spring - Night", "https://www.dropbox.com/sh/9cjup0lfnxgr6pe/bAJY7XfhnD"},
                                           {"Spring - Twilight - Clear", "https://www.dropbox.com/sh/v8h27zovyipmhae/ISTz51xNvk"},
                                           {"Spring - Twilight - Cloudy", "https://www.dropbox.com/sh/x8eyvu0cvme8ea0/neQYgxj5yy"},
                                           {"Summer", "https://www.dropbox.com/sh/fl5veo1cg2astno/Xu3I0uNV_F"},
                                           {"Summer - Day", "https://www.dropbox.com/sh/5a7wnw91rbbgd2f/R14Lk7nmoD"},
                                           {"Summer - Day - Clear", "https://www.dropbox.com/sh/8g5f3z6q7afs7rj/YCiNQ92PEY"},
                                           {"Summer - Day - Cloudy", "https://www.dropbox.com/sh/5n9fesnho5uejo6/Ff6BnfcdtT"},
                                           {"Summer - Day - Rain", "https://www.dropbox.com/sh/2ri3pmry4v62gkp/O8o9hZtzPW"},
                                           {"Summer - Night", "https://www.dropbox.com/sh/m91xzau6xt89rg7/biB0-kyT4g"},
                                           {"Summer - Twilight - Clear", "https://www.dropbox.com/sh/u9glb6aiypa1s4k/8ajB7TG66W"},
                                           {"Summer - Twilight - Cloudy", "https://www.dropbox.com/sh/aqutvxnekba0kuj/HBJKfgyGXb"},
                                           {"Winter", "https://www.dropbox.com/sh/wtxep8rmre2kp2w/4G4ZDA17Js"},
                                           {"Winter - Day", "https://www.dropbox.com/sh/xb50nabrgs3uail/xK3Avdwra7"},
                                           {"Winter - Day - Clear", "https://www.dropbox.com/sh/u39punswly30h87/4VIcFEmk3H"},
                                           {"Winter - Day - Cloudy", "https://www.dropbox.com/sh/2v4t2mitej6gnt4/_bC0ul0uUh"},
                                           {"Winter - Day - Rain", "https://www.dropbox.com/sh/8beafzukgqy7ali/R_1TXYDl8J"},
                                           {"Winter - Night", "https://www.dropbox.com/sh/ewkh5ke2lc8dumz/Gr92W-bkVJ"},
                                           {"Winter - Twilight - Clear", "https://www.dropbox.com/sh/6dov0t7fzspma8t/MMYJn4WE41"},
                                           {"Winter - Twilight - Cloudy", "https://www.dropbox.com/sh/hwfjea7q1atpp2y/AqK5crJqc-"}
                                           }; 
        #endregion


        public static string GetWallpaper(string acceptableTheme, string dlFolder, out string dropboxPage, List<string> bannedWalls)
        {
            dropboxPage = "";

            try
            {
                string url = "";

                //find the dropbox theme that corresponds to this theme
                for (int i = 0; i < themes.Length / 2; i++)
                    if (themes[i, 0] == acceptableTheme)
                    {
                        url = themes[i, 1];
                        break;
                    }

                if (url == "")
                    return "";

                //get list of wallpapers in this theme
                WebClient webClient = new WebClient();
                string src = webClient.DownloadString(url);

                int lb = src.IndexOf("SharingModel.init_folder");
                //int ub = src.IndexOf("}]\"))", lb);
                int ub = src.IndexOf("}])", lb);
                string seg = src.Substring(lb, ub - lb);

                List<string> walls = new List<string>();

                //lb = seg.IndexOf("\\\"orig_url\\\": \\\"") + 16;
                lb = seg.IndexOf("\"orig_url\": \"") + 13;

                //while (lb != 15)
                while (lb != 12)
                {
                    ub = seg.IndexOf("?token_hash", lb);

                    if (!bannedWalls.Contains(seg.Substring(lb, ub - lb))) //check if wallpaper banned
                        walls.Add(seg.Substring(lb, ub - lb));

                    //lb = seg.IndexOf("\\\"orig_url\\\": \\\"", ub) + 16;
                    lb = seg.IndexOf("\"orig_url\": \"", ub) + 13;
                }

                if (walls.Count == 0)
                    return "";

                //download random wallpaper
                Random rand = new Random((int)DateTime.Now.Ticks);

                url = walls[rand.Next(walls.Count)];
                string filename = url.Substring(url.LastIndexOf('/') + 1);

                using (WebClient webClient2 = new WebClient())
                {
                    webClient2.DownloadFile(url, dlFolder + filename);
                }

                dropboxPage = url;
                return dlFolder + filename;
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }

    class FlickrSource
    {
        public static string GetWallpaper(string acceptableTheme, int minW, int minH, string dlFolder, out string flickrPage, List<string> bannedWalls, Action<string> banWallpaper)
        {
            flickrPage = "";

            acceptableTheme = acceptableTheme.Replace("Twilight", "Sunset"); //because flickr usually returns nothing when the tag combination contains "Twilight"

            try
            {
                Flickr flickr = new Flickr("928a5bfb36cc1ea3160c6d236c2c76d4");

                //search flickr
                PhotoSearchOptions opts = new PhotoSearchOptions();

                opts.Tags = acceptableTheme.Replace(" - ", ",");
                opts.TagMode = TagMode.AllTags;
                opts.SortOrder = PhotoSearchSortOrder.InterestingnessDescending;
                opts.Licenses.Add(LicenseType.AttributionCC);
                opts.Licenses.Add(LicenseType.AttributionNoDerivativesCC);
                opts.Licenses.Add(LicenseType.AttributionShareAlikeCC);
                opts.Licenses.Add(LicenseType.NoKnownCopyrightRestrictions);
                opts.SafeSearch = SafetyLevel.Safe;
                opts.ContentType = ContentTypeSearch.PhotosOnly;
                opts.MediaType = MediaType.Photos;
                opts.Extras = PhotoSearchExtras.OwnerName;

                PhotoCollection flickrResults = flickr.PhotosSearch(opts);

                if (flickrResults.Count == 0) //no suitable wallpapers found
                    return "";

                //choose random images for one that satisfies min. resolution
                SizeCollection sizes;
                Random rand = new Random((int)DateTime.Now.Ticks);
                string url = "";
                int pick = -1, i;

                while (pick == -1)
                {
                    pick = rand.Next(flickrResults.Count);

                    if (bannedWalls.Contains(flickrResults[pick].WebUrl))
                    {
                        //wallpaper banned, pick another
                        flickrResults.RemoveAt(pick);
                        pick = -1;
                    }
                    else
                    {
                        sizes = flickr.PhotosGetSizes(flickrResults[pick].PhotoId);

                        for (i = sizes.Count - 1; i >= 0; i--)
                            if (sizes[i].Width < minW || sizes[i].Height < minH)
                                break;

                        i++;
                        if (i == sizes.Count) //image not large enough
                        {
                            banWallpaper(flickrResults[pick].WebUrl); //ban it so future searches can ignore it
                            flickrResults.RemoveAt(pick);

                            pick = -1;
                        }
                        else
                            url = sizes[i].Source;
                    }

                    if (flickrResults.Count == 0) //no suitable wallpapers found
                        return "";
                }

                //get valid filename from title
                string filename = flickrResults[pick].Title;
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                    filename = filename.Replace(c, '_');

                filename += url.Substring(url.LastIndexOf('.')); //append file extension from url

                //dl image
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, dlFolder + filename);
                }

                flickrPage = flickrResults[pick].WebUrl;
                return dlFolder + filename;
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
