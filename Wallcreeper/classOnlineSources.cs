using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using FlickrNet;

namespace Wallcreeper
{
    class Imgur
    {
        class Album
        {
            public string Name, ID;
            public List<string> Images;

            public Album(string line)
            {
                string[] parts = line.Split(new string[] { " -> " }, StringSplitOptions.RemoveEmptyEntries);
                Name = parts[0];
                ID = parts[1];

                Images = null;
            }

            public string GetWallpaperURL(List<string> bannedWalls)
            {
                if (Images == null)
                {
                    //first time using this album -> download list of image URLs
                    WebRequest request = WebRequest.Create("https://api.imgur.com/3/album/" + ID + "/images");
                    request.Method = "GET";
                    request.Headers.Add("Authorization", "Client-ID b2504c1a64c87c5");

                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);

                    string content = reader.ReadToEnd();

                    reader.Close();
                    response.Close();

                    //parse album contents
                    Images = new List<string>();
                    string[] links = content.Split(new string[] { "\"link\":\"" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string image in links)
                        if (image.Length > 4 && image.Substring(0, 4) == "http")
                        {
                            int ub = image.IndexOf('"');
                            Images.Add(image.Substring(0, ub).Replace("\\/","/"));
                        }
                }

                //remove banned wallpapers from the list
                bannedWalls.Add(Images[0]);

                foreach (string banned in bannedWalls)
                    if (Images.Contains(banned))
                        Images.Remove(banned);

                //get random wallpaper
                Random rng = new Random((int)DateTime.Now.Ticks);
                return Images[rng.Next(Images.Count)];
            }
        }

        Album[] albums;


        public Imgur(string albumListPath)
        {
            //load imgur album list
            if (File.Exists(albumListPath))
            {
                StreamReader file = new StreamReader(albumListPath);
                string[] lines = file.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                file.Close();

                albums = new Album[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                    albums[i] = new Album(lines[i]);
            }
        }

        public string GetWallpaper(string theme, string dlFolder, out string imgurLink, List<string> bannedWalls)
        {
            imgurLink = "";

            try
            {
                //find the Imgur theme that corresponds to this theme
                string url = "";

                foreach (Album album in albums)
                    if (album.Name == theme)
                    {
                        url = album.GetWallpaperURL(bannedWalls); //get random wallpaper from the album
                        break;
                    }

                if (url == "")
                    return "";

                //download wallpaper
                string filename = url.Substring(url.LastIndexOf('/') + 1);

                using (WebClient webClient2 = new WebClient())
                {
                    webClient2.DownloadFile(url, dlFolder + filename);
                }

                imgurLink = url.Replace("i.imgur", "imgur").Replace(".jpg", "");
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
        public static string GetWallpaper(string theme, int minW, int minH, string dlFolder, out string flickrPage, List<string> bannedWalls, Action<string> banWallpaper)
        {
            flickrPage = "";

            theme = theme.Replace("Twilight", "Sunset"); //because flickr usually returns nothing when the tag combination contains "Twilight"

            try
            {
                Flickr flickr = new Flickr("928a5bfb36cc1ea3160c6d236c2c76d4");

                //search flickr
                PhotoSearchOptions opts = new PhotoSearchOptions();

                opts.Tags = theme.Replace(" - ", ",");
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
