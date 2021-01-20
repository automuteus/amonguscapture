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

        public static readonly DependencyProperty PlayerHatDependencyProperty = 
            DependencyProperty.Register("PlayerHatID", 
                typeof(uint), typeof(PlayerControl));

        public static readonly DependencyProperty PlayerPetDependencyProperty = 
            DependencyProperty.Register("PlayerPetID", 
                typeof(uint), typeof(PlayerControl));

        public static readonly DependencyProperty PlayerPantsDependencyProperty = 
            DependencyProperty.Register("PlayerPantsID", 
                typeof(uint), typeof(PlayerControl));

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
        public uint PlayerHatID
        {
            get => (uint)GetValue(PlayerHatDependencyProperty);
            set => SetValue(PlayerHatDependencyProperty, value);
        }

        public uint PlayerPetID
        {
            get => (uint)GetValue(PlayerHatDependencyProperty);
            set => SetValue(PlayerHatDependencyProperty, value);
        }
        public uint PlayerPantsID
        {
            get => (uint)GetValue(PlayerPantsDependencyProperty);
            set => SetValue(PlayerPantsDependencyProperty, value);
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
