using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AUCapture_WPF.Annotations;

namespace AUCapture_WPF.Models
{
    public class ConnectionStatus : INotifyPropertyChanged
    {

        private bool _connected;

        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                OnPropertyChanged();

            }
        }
        private string _connectionName;

        public string ConnectionName
        {
            get => _connectionName;
            set
            {
                _connectionName = value;
                OnPropertyChanged();

            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
