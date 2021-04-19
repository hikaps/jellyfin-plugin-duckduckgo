using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Providers;
using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.DuckDuckGo.Providers
{
    public class ImageProvider : IRemoteImageProvider
    {
        private readonly IServerConfigurationManager _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFileSystem _fileSystem;
        private string query;
        public ImageProvider(IServerConfigurationManager config, IHttpClientFactory httpClientFactory, IFileSystem fileSystem)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _fileSystem = fileSystem;

            Current = this;
        }

        internal static ImageProvider Current { get; private set; }

        public string Name => "DuckDuckGo";

        public bool Supports(BaseItem item)
            => item is MusicAlbum;


        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary,
            };
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var album = (MusicAlbum)item;
            this.query = Plugin.Instance.Configuration.prefix + " " + album.Name + " " + Plugin.Instance.Configuration.suffix;
            var list = await DownloadImages(cancellationToken);
            return list;
        }

        internal async Task<List<RemoteImageInfo>> DownloadImages(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);
            var vqdtoken = await getToken(httpClient);
            var images = await findImages(vqdtoken, httpClient);
            var list = new List<RemoteImageInfo>();
            foreach (JsonElement item in images.EnumerateArray())
            {
                string image = item.GetProperty("image").ToString();
                string title = item.GetProperty("title").ToString();
                int height = item.GetProperty("height").GetInt32();
                int width = item.GetProperty("width").GetInt32();

                list.Add(new RemoteImageInfo
                {
                    Url = image,
                    Type = ImageType.Primary,
                    ProviderName = Name,
                    Height = height,
                    Width = width
                });
            }
            return list;
        }

        internal async Task<String> getToken(HttpClient httpClient)
        {

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("q", this.query),
            };
            var content = new FormUrlEncodedContent(pairs);
            var response = await httpClient.PostAsync(Plugin.BaseUrl, content);
            var match = Regex.Match(await response.Content.ReadAsStringAsync(), @"vqd=([\d-]+)\&");
            return match.Groups[1].Value;
        }



        internal async Task<JsonElement> findImages(string vqdtoken, HttpClient httpClient)
        {
            var uri = this.buildRequestUrl(vqdtoken);
            var response = await httpClient.GetAsync(uri.ToString());
            var json = await response.Content.ReadAsStringAsync();
            var jsondoc = JsonDocument.Parse(json).RootElement;
            return jsondoc.GetProperty("results");
        }

        internal Uri buildRequestUrl(string vqdtoken)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            UriBuilder baseUri = new UriBuilder(Plugin.BaseUrl + "i.js");
            baseUri.Query = baseUri.Query.ToString() + "&q=" + this.query + "&vqd=" + vqdtoken;

            return baseUri.Uri;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(new Uri(url), cancellationToken);
        }

    }
}
