using System.ComponentModel;
using System.Runtime.CompilerServices;
using Config.Net;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace AUCapture_WPF
{
    public interface IAppSettings : INotifyPropertyChanged
    {
        [Option(DefaultValue = "")]
        string discordToken { get; set; }

        [Option(DefaultValue = false)]
        bool ApiServer { get; set; }

        [Option(DefaultValue = false)]
        bool ranBefore { get; set; }

        [Option(DefaultValue = false)]
        bool DarkMode { get; set; }

        [Option(DefaultValue = true)]
        bool FocusOnToken { get; set; }

        [Option(DefaultValue = 18d)]
        double fontSize { get; set; }

        [Option(DefaultValue = false)]
        bool debug { get; set; }

        [Option(DefaultValue = true)]
        bool checkForUpdate { get; set; }

        [Option(DefaultValue = "")]
        string host { get; set; }

        [Option(DefaultValue = "")]
        string connectCode { get; set; }
    }
}
