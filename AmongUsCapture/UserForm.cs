using AmongUsCapture.ConsoleTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Security.Policy;
using SocketIOClient;

namespace AmongUsCapture
{
    public partial class UserForm : Form
    {
        ClientSocket clientSocket;

        public UserForm(ClientSocket clientSocket)
        {
            this.clientSocket = clientSocket;
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;

            // Load URL
            URLTextBox.Text = Config.GetInstance().GetOrDefault("url", "");

            // Submit on Enter
            this.AcceptButton = ConnectButton;

            if (DarkTheme())
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

            UrlGB.BackColor = LighterGrey;
            UrlGB.ForeColor = White;

            URLTextBox.BackColor = DarkGrey;
            URLTextBox.ForeColor = White;


            ConnectButton.BackColor = BluePurpleAccent;
            ConnectButton.ForeColor = White;


            BackColor = DarkGrey;
            ForeColor = White;

        }

        private void ConnectCodeBox_Enter(object sender, EventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                ConnectCodeBox.Select(0, 0);
            });
        }
        private void ConnectCodeBox_Click(object sender, EventArgs e)
        {
            if (ConnectCodeBox.Enabled)
            {
                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    ConnectCodeBox.Select(0, 0);
                });
            }

        }

        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            WriteLineToConsole(e.Name + ": " + e.Action);
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            URLTextBox.Text = Config.GetInstance().GetOrDefault("url", "");
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            this.CurrentState.BeginInvoke((MethodInvoker)delegate {
                CurrentState.Text = e.NewState.ToString();
            });

            WriteLineToConsole("State changed to " + e.NewState);
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {

            ConnectCodeBox.Enabled = false;
            ConnectButton.Enabled = false;
            URLTextBox.Enabled = false;

            var url = "http://localhost:8123";
            if (URLTextBox.Text != "")
            {
                url = URLTextBox.Text;
            }

            doConnect(url);
        }

        private void doConnect(string url)
        {
            clientSocket.OnConnected += (sender, e) =>
            {
                // We could connect to the URL -> save it
                Config.GetInstance().Set("url", url);

                clientSocket.SendConnectCode(ConnectCodeBox.Text);
                Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            };

            try
            {
                clientSocket.Connect(url);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
                ConnectCodeBox.Enabled = true;
                ConnectButton.Enabled = true;
                URLTextBox.Enabled = true;
                return;
            }
        }

        private void ConnectCodeBox_TextChanged(object sender, EventArgs e)
        {
            ConnectButton.Enabled = (ConnectCodeBox.Enabled && ConnectCodeBox.Text.Length == 6 && !ConnectCodeBox.Text.Contains(" "));
        }

        public void WriteLineToConsole(String line)
        {
            if (!(ConsoleTextBox is null))
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker)delegate {
                    ConsoleTextBox.AppendText(line + "\n");
                });
            }

        }
    }
}
