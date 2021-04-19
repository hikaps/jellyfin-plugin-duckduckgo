<h1 align="center">Jellyfin DuckDuckGo Plugin</h1>

## About

The Jellyfin DuckDuckGo plugin allows you to scrapes images for your Music albums from <a href="https://duckduckgo.com/">DuckDuckGo</a>. To change your search queries, manually change the album name and search for images again. You can also add predefined prefix or suffix (default: "poster") to all your queries by changing them in settings.


## Build & Installation Process

1. Clone this repository

2. Ensure you have .NET Core SDK set up and installed

3. Build the plugin with your favorite IDE or the `dotnet` command:

```
dotnet publish --configuration Release --output bin
```

4. Place the resulting `Jellyfin.Plugin.DuckDuckGo.dll` file in a folder called `plugins/` inside your Jellyfin data directory
