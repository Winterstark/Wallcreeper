Wallcreeper
===========

Wallcreeper is a wallpaper manager for Windows with a unique feature: it selects the wallpaper that best fits the current conditions, such as:
* Current time (is it day, night, or sunrise/sunset?)
* Current season
* Local weather (clear, clouds, rain, snow)

This system ensures you will never get a bright and sunny wallpaper in the middle of the night, a snow-covered landscape during summer, etc.

The program takes the wallpapers from pre-organized themes, which you either have to create yourself or download in the form of [wallpaper packs](http://sourceforge.net/projects/wallcreeper/files/Wallpaper%20packs/).

However, in the latest version Wallcreeper enables you to use online sources such as Flickr or my own collection of already organized themes (hosted on Dropbox). **What this means is that after you download Wallcreeper (~800 Kb) you instantly gain access to 2000+ wallpapers that will always fit the moment.**

You can freely browse and download the online themes through [this file](https://github.com/Winterstark/Wallcreeper/blob/master/online%20themes.md).


Installation
--------------

1. (You need to have [.NET framework](http://www.microsoft.com/en-us/download/details.aspx?id=30653) installed on your computer)
2. Download [the release](https://github.com/Winterstark/Wallcreeper/releases)
3. Extract
4. Run Wallcreeper.exe


Usage
-------

After you start Wallcreeper it won't appear on the screen, but you can find its icon in the notification area (tray), double-clicking which will bring the program's main window up.

### Wallpaper themes

A wallpaper theme is a collection of wallpapers that is only active during a specific set of conditions, namely: current date, time, and weather conditions.

![Screenshot: wallpaper themes](http://i.imgur.com/HAFDROe.png)

To set the active time of a wallpaper theme, use:
* Predefined ranges (day, night, twilight)
* 24 hrs format, e.g. 16 or 16:00
* Custom ranges: 7 - 11
* Any of the above formats, separated by commas: 12, 16 - 16:45, 20 - 21

To set the active date of a wallpaper theme, use:
* Predefined options (4 seasons, Full moon, etc.)
* Custom date in the day-month format: 31.10.
* Custom ranges: 20.2. - 5.3.
* Any of the above formats, separated by commas: 5.5., 10.5. - 21.5., 31.5.

The Wallcreeper release version comes bundled with a set of themes that adapts to the time of day (day, night, or sunset/sunrise), four seasons, local weather (clear, clouds, rain, or snow), as well as special events (e.g. full moon, winter holidays), which amounts to a total of 42 themes.

### Wallpaper sources

![Screenshot: wallpaper sources](http://i.imgur.com/ufvgAJr.png)

A new feature in the latest version of Wallcreeper is that you don't need to have an existing wallpaper collection organized into themes. Instead, Wallcreeper can now download wallpapers straight from the cloud - specifically, from my collection of 2036 (and continually growing) wallpapers (in 1600x900 resolution) on Dropbox.

You can also use Flickr as another source of images, and you can specify the minimum acceptable resolution for them. You can also use all three wallpaper sources (local, Dropbox, and Flickr) at once and specify how often to use a particular source.

### Windows themes

![Screenshot: Windows themes](http://i.imgur.com/Gv2IWk6.png)

Wallcreeper can also control your Windows appearance through Windows Themes. This can be useful if you want Windows colors to reflect the current season; for example, white during winter and blue during summer.

Note that when a Theme is applied the Windows Personalization window pops up, so themes should be used sparingly.

### Wallpaper packs

This feature enables users to share their wallpaper packs with ease. A wallpaper pack is a collection of several themes archived together with information about their active conditions.

If you want to download the Dropbox-hosted collection go [here](https://sourceforge.net/projects/wallcreeper/files/Wallpaper%20packs/). The advantage of having the collection locally is that you can use Windows's built-in wallpaper manager (on Vista and newer), so whenever a wallpaper changes you will see a nice transition animation. You will also get the option to change wallpaper from the Desktop context menu.

### Options

![Screenshot: options](http://i.imgur.com/y229iHR.png)

In the Options tab you can change the following:
* How often the wallpaper changes
* How often Wallcreeper needs to check the weather
* Your latitute, longitude, and time zone (used in determining sunrise/sunset times)
* Whether Wallcreeper runs when Windows starts
* Checking for updates automatically
* Using Windows Vista/7 wallpaper manager

This final option is *very important*: if it is disabled then Wallcreeper sets the current wallpaper itself. Turning it on will make Wallcreeper apply wallpapers by using the Windows built-in manager, which brings several useful features:
* Wallpaper changes come with the nice Windows fade animation
* The desktop right-click context menu contains the option Next desktop background

However, there are a couple of potential issues with using this method. The first time you use it Windows Personalization window will appear and you will (probably) have to close it manually. Also, sometimes Windows gets confused and stops updating the wallpaper theme - it fixes itself after Windows Explorer restarts.

### Changing weather status

Because the weather service is not perfect, sometimes Wallcreeper will activate the wrong theme. To fix this issue click on the weather icon in the lower-right corner of the main window. Continue clicking until you activate the correct weather status, just be careful not to click too quickly because a double-click will reset the weather icon.

### Tray menu

![Screenshot: tray menu](http://i.imgur.com/RWUStHz.png)

Besides double-clicking the tray icon to bring up the main window, right-clicking it also has some useful options:
* Change wallpaper (only works when you're not using Windows Vista/7 wallpaper manager)
* Locate current wallpaper - display the current wallpaper on the disk (only works if the current wallpaper is a local file)
* Open wallpaper webpage (conversely, this option only works if the current wallpaper *is* from an online source)
* Save wallpaper to local theme (also works only with online sources) - saves the wallpaper to your local directory
* Ban wallpaper - the current wallpaper won't appear again


Credits
---------
* Uses [Flickr API](https://www.flickr.com/services/api/)
* Wallcreeper icon original image from [Wikimedia Commons](http://commons.wikimedia.org/wiki/File:Tichodroma_muraria02_cropped.jpg)
* Weather and moon phases icons by [VClouds](http://vclouds.deviantart.com/art/VClouds-Weather-2-179058977)
