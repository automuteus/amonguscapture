using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AmongUsCapture
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            this.playerCountLabel.BeginInvoke((MethodInvoker) delegate {
                playerCountLabel.Text = e.NewState.ToString();
            });
        }
    }

}
