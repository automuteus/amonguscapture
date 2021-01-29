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
using AmongUsCapture;
using AUCapture_WPF.Controls;

namespace AUCapture_WPF
{
    /// <summary>
    /// Interaction logic for BasePlayerGenerator.xaml
    /// </summary>
    public partial class BasePlayerGenerator : UserControl
    {
        public static readonly DependencyProperty PlayerMainBrushDependencyProperty = DependencyProperty.Register("PlayerMainBrush", typeof(Brush), typeof(BasePlayerGenerator));
        public Brush PlayerMainBrush
        {
            get => (Brush)GetValue(PlayerMainBrushDependencyProperty);
            set => SetValue(PlayerMainBrushDependencyProperty, value);
        }
        public static readonly DependencyProperty PlayerSecondaryBrushDependencyProperty = DependencyProperty.Register("PlayerSecondaryBrush", typeof(Brush), typeof(BasePlayerGenerator));
        public Brush PlayerSecondaryBrush
        {
            get => (Brush)GetValue(PlayerSecondaryBrushDependencyProperty);
            set => SetValue(PlayerSecondaryBrushDependencyProperty, value);
        }
        public static readonly DependencyProperty AliveDependencyProperty = DependencyProperty.Register("Alive", typeof(bool), typeof(BasePlayerGenerator));
        public bool Alive
        {
            get => (bool)GetValue(AliveDependencyProperty);
            set => SetValue(AliveDependencyProperty, value);
        }
        public BasePlayerGenerator()
        {
            InitializeComponent();
        }
    }
}
