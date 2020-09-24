using AmongUsCapture.ConsoleTypes;
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
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            if(DarkTheme())
            {
                EnableDarkTheme();
            }
            
        }

        private void OnLoad(object sender, EventArgs e)
        {
            //TestFillConsole(100);
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            WriteLineToConsole($"[CHAT] {e.Sender}: {e.Message}");
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


        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            WriteLineToConsole(e.Name + ": " + e.Action);
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
            if(ConnectCodeBox.TextLength == 6)
            {
                clientSocket.SendConnectCode(ConnectCodeBox.Text);
                //ConnectCodeBox.Enabled = false;
                //SubmitButton.Enabled = false;
            }
        }

        private void ConsoleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (AutoScrollMenuItem.Checked)
            {
                ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
                ConsoleTextBox.ScrollToCaret();
            }
        }

        private void TestFillConsole(int entries) //Helper test method to see if filling console works.
        {
            for (int i = 0; i < entries; i++)
            {
                WriteLineToConsole(i.ToString());
            }
        }

        public void WriteLineToConsole(String line)
        {
            if(!(ConsoleTextBox is null))
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker)delegate {
                    ConsoleTextBox.AppendText(line + "\n");
                });
            }
            
        }


    }
}
