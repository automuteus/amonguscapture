using CaptureGUI;
using MahApps.Metro.Controls.Dialogs;

namespace AmongUsCapture.CaptureGUI
{
    public class UserDataContext
    {
        public IDialogCoordinator DialogCoordinator { get; set; }
        public IAppSettings Settings { get; set; }

        public UserDataContext(IDialogCoordinator dialogCoordinator, IAppSettings settings)
        {
            DialogCoordinator = dialogCoordinator;
            Settings = settings;
        }
    }
}