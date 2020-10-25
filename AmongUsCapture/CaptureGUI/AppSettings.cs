using System.ComponentModel;
using System.Runtime.CompilerServices;
using Config.Net;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CaptureGUI
{
    public interface IAppSettings : INotifyPropertyChanged
    {
        [Option(DefaultValue = false)]
        bool RanBefore { get; set; }

        [Option(DefaultValue = false)]
        bool DarkMode { get; set; }

        [Option(DefaultValue = 18d)]
        double FontSize { get; set; }

        [Option(DefaultValue = false)]
        bool Debug { get; set; }

        [Option(DefaultValue = true)]
        bool CheckForUpdate { get; set; }

        [Option(DefaultValue = "")]
        string Host { get; set; }

        [Option(DefaultValue = "")]
        string ConnectCode { get; set; }
    }
}
