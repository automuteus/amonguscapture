using System.ComponentModel;
using System.Runtime.CompilerServices;
using Config.Net;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CaptureGUI
{
    public interface IAppSettings : INotifyPropertyChanged
    {
        [Option(DefaultValue = false)]
        bool ranBefore { get; set; }

        [Option(DefaultValue = false)]
        bool DarkMode { get; set; }

        [Option(DefaultValue = 18d)]
        double fontSize { get; set; }

        [Option(DefaultValue = false)]
        bool debug { get; set; }

        [Option(DefaultValue = "")]
        string host { get; set; }

        [Option(DefaultValue = "")]
        string connectCode { get; set; }
    }
}
