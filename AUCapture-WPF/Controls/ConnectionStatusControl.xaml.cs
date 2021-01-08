using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AUCapture_WPF.Controls
{
    /// <summary>
    /// Interaction logic for ConnectionStatusControl.xaml
    /// </summary>
    public partial class ConnectionStatusControl : UserControl
    {
        public static readonly DependencyProperty ConnectedProperty = 
            DependencyProperty.Register("ConnectionStatus", 
                typeof(bool), typeof(ConnectionStatusControl));

        public static readonly DependencyProperty NameProperty = 
            DependencyProperty.Register("ConnectionName", 
                typeof(string), typeof(ConnectionStatusControl));

        public bool ConnectionStatus
        {
            get => (bool)GetValue(ConnectedProperty);
            set => SetValue(ConnectedProperty, value);
        }
        public string ConnectionName
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public ConnectionStatusControl()
        {
            InitializeComponent();
        }
    }
}
