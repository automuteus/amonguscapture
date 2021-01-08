using System;
using System.Windows;
using System.Windows.Controls;
using AmongUsCapture;

namespace AUCapture_WPF.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        public static readonly DependencyProperty AliveProperty = 
            DependencyProperty.Register("AliveStatus", 
                typeof(bool), typeof(PlayerControl));

        public static readonly DependencyProperty PlayerNameProperty = 
            DependencyProperty.Register("PlayerName", 
                typeof(string), typeof(PlayerControl));

        public static readonly DependencyProperty ColorProperty = 
            DependencyProperty.Register("Color", 
                typeof(PlayerColor), typeof(PlayerControl));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("Prop changed");
        }

        public bool AliveStatus
        {
            get => (bool)GetValue(AliveProperty);
            set => SetValue(AliveProperty, value);
        }
        public string PlayerName
        {
            get => (string)GetValue(PlayerNameProperty);
            set => SetValue(PlayerNameProperty, value);
        }
        public PlayerColor Color
        {
            get => (PlayerColor)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
        public PlayerControl()
        {
            InitializeComponent();
        }
    }
}
