using Config.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CaptureGUI
{
    internal interface AppSettings : INotifyPropertyChanged
    {
        [Option(DefaultValue = false)]
        bool ranBefore { get; set; }

        [Option(DefaultValue = false)]
        bool DarkMode { get; set; }

        [Option(DefaultValue = 18d)]
        double fontSize { get; set; }

    }
}
