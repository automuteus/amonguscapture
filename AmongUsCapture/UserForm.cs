using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AmongUsCapture
{
    public partial class UserForm : Form
    {
        ClientSocket clientSocket;
        public UserForm(ClientSocket sock)
        {
            clientSocket = sock;
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            ConnectCodeBox.TextChanged += ConnectCodeBox_TextChanged;
            if(DarkTheme())
            {
                EnableDarkTheme();
            }
            
        }
        private bool DarkTheme()
        {
            bool is_dark_mode = false;
            try
            {
                var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
                if (v != null && v.ToString() == "0")
                    is_dark_mode = true;
            }
            catch { }
            return is_dark_mode;
        }
        private void EnableDarkTheme()
        {
            var BluePurpleAccent = Color.FromArgb(114, 137, 218);
            var White = Color.White;
            var AlmostWhite = Color.FromArgb(153, 170, 181);
            var LighterGrey = Color.FromArgb(44, 47, 51);
            var DarkGrey = Color.FromArgb(35, 39, 42);

            ConsoleTextBox.BackColor = LighterGrey;
            ConsoleTextBox.ForeColor = White;

            ConsoleGroupBox.BackColor = DarkGrey;
            ConsoleGroupBox.ForeColor = White;

            UserSettings.BackColor = DarkGrey;
            UserSettings.ForeColor = White;

            CurrentStateGroupBox.BackColor = LighterGrey;
            CurrentStateGroupBox.ForeColor = White;

            ConnectCodeGB.BackColor = LighterGrey;
            ConnectCodeGB.ForeColor = White;

            ConnectCodeBox.BackColor = DarkGrey;
            ConnectCodeBox.ForeColor = White;


            SubmitButton.BackColor = BluePurpleAccent;
            SubmitButton.ForeColor = White;
            

            BackColor = DarkGrey;
            ForeColor = White;

        }

        private void ConnectCodeBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            this.ConsoleTextBox.BeginInvoke((MethodInvoker)delegate {
                ConsoleTextBox.AppendText(e.Name + ": " + e.Action+"\n");
            });
        }

        private void UserForm_Load(object sender, EventArgs e)
        {

        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            this.CurrentState.BeginInvoke((MethodInvoker)delegate {
                CurrentState.Text = e.NewState.ToString();
            });
            this.ConsoleTextBox.BeginInvoke((MethodInvoker)delegate {
                ConsoleTextBox.AppendText("State changed to " + e.NewState+"\n");
            });
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if(ConnectCodeBox.TextLength == 6)
            {
                clientSocket.SendConnectCode(ConnectCodeBox.Text);
                ConnectCodeBox.Enabled = false;
                SubmitButton.Enabled = false;
            }
        }
    }
}
