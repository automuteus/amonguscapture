using System;
using System.Windows;
using System.Windows.Controls;
using AmongUsCapture;
using Bindables;

namespace AUCapture_WPF.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    [DependencyProperty]
    public partial class PlayerControl : UserControl
    {
        public bool AliveStatus { get; set; }
        public uint PlayerHatID { get; set; }
        public uint PlayerPetID { get; set; }
        public uint PlayerPantsID { get; set; }
        public string PlayerName { get;set; }
        public PlayerColor Color { get; set; }
        public PlayerControl()
        {
            InitializeComponent();
        }
    }
}
