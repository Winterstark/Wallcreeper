Wallcreeper
===========

Wallcreeper is a wallpaper manager for Windows with a unique feature: it selects the wallpaper that best fits the current conditions, such as:
* current time (is it day, night, or sunrise/sunset?)
* current season
* local weather (clear, clouds, rain, snow)

Using such conditions means you will never get a bright and sunny wallpaper in the middle of the night, a snow-covered landscape during summer, etc.

The program takes the wallpapers from pre-organized themes, which you either have to create yourself or download in the form of wallpaper packs.

However, in the latest version Wallcreeper enables you to use online sources such as Flickr or my cloud-based collection of already organized themes. *What this means is that after you download Wallcreeper (??? MBytes) you instantly gain access to 2000+ wallpapers that will always fit the moment.*


Installation
--------------

1. (You need to have [.NET framework](http://www.microsoft.com/en-us/download/details.aspx?id=30653) installed on your computer)
2. Download [the release](https://github.com/Winterstark/Wallcreeper/releases)
3. Extract
4. Run Wallcreeper.exe


Usage
-------

After you start Wallcreeper it won't appear on the screen, but you can find its icon in the tray, double-clicking which will bring the program's main window up.

### Wallpaper themes

A wallpaper theme is a collection of wallpapers that is only active during a specific set of conditions, namely: current time, season, and weather conditions. This allows you to always have a wallpaper matching the current conditions.

![Screenshot: wallpaper themes](http://i.imgur.com/HAFDROe.png)

The Wallcreeper release version comes bundled with a set of themes that adapts to the time of day (day, night, or sunset/sunrise), four seasons, local weather (clear, clouds, rain, or snow), as well as special events (e.g. full moon, winter holidays), which amounts to a total of 42 themes.

### Wallpaper sources

![Screenshot: wallpaper sources](http://i.imgur.com/ufvgAJr.png)

A new feature in the latest version of Wallcreeper is that you don't need to have an existing wallpaper collection organized into themes. Instead, Wallcreeper can now download wallpapers straight from the cloud - specifically, from my collection of 2036 (and continually growing) wallpapers (in 1600x900 resolution) on Dropbox.

You can also use Flickr as another source of images, and you can specify the minimum acceptable resolution for them. You can also use all three wallpaper sources (local, Dropbox, and Flickr) at once and specify how often to use a particular source.

### Windows themes

![Screenshot: Windows themes](http://i.imgur.com/Gv2IWk6.png)

Wallcreeper can also control your Windows appearance through Windows Themes. This can be useful if you want Windows colors to reflect the current season, for example.

Note that when a Theme is applied the Windows Personalization window pops up, so themes should be used sparingly.

### Wallpaper packs

This feature enables users to share their wallpaper packs with ease. A wallpaper pack is a collection of several themes archived together with information about their active conditions.

If you want to download the official wallpaper packs (in 1600x900 resolution) go [here](https://sourceforge.net/projects/wallcreeper/files/Wallpaper%20packs/).

### Options

![Screenshot: options](http://i.imgur.com/y229iHR.png)

In the Options tab you can change the following:
* how often the wallpaper changes
* how often Wallcreeper needs to check the weather
* your latitute, longitude, and time zone (used in determining sunrise/sunset times)
* whether Wallcreeper runs when Windows starts
* checking for updates automatically
* using Windows Vista/7 wallpaper manager

This final option is *very important*: if it is disabled then Wallcreeper sets the current wallpaper itself. Turning it on will make Wallcreeper apply wallpapers by using the Windows built-in manager, which brings several useful features:
* wallpaper changes come with the nice Windows fade animation
* the desktop right-click context menu contains the option Next desktop background

However, there are a couple of hitches with using this method. The first time you use it the Windows Personalization window will appear and you will (probably) have to close it manually. Also, sometimes the Windows wallpaper manager goes on the fritz and stops updating, in which case you will have to restart Windows Explorer.

### Tray menu

Besides double-clicking the tray icon to bring up the main window, right-clicking it also has some useful options:
* Change wallpaper (only works when you're not using Windows Vista/7 wallpaper manager)
* Locate current wallpaper - display the current wallpaper on the disk (only works if the current wallpaper is a local file)
* Open wallpaper webpage (conversely, this option only works if the current wallpaper if from an online source)


Credits
---------

* Wallcreeper icon original image from [Wikimedia Commons](http://commons.wikimedia.org/wiki/File:Tichodroma_muraria02_cropped.jpg)
* Weather and moon phases icons by [VClouds](http://vclouds.deviantart.com/art/VClouds-Weather-2-179058977)