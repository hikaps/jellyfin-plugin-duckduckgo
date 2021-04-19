using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DuckDuckGo.Configuration
{

    public class PluginConfiguration : BasePluginConfiguration
    {
        public string prefix { get; set; }
        public string suffix { get; set; }

        public PluginConfiguration()
        {
            prefix = "";
            suffix = "poster";
        }
    }
}
