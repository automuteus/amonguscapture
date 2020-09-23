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
        public static RichTextBox ConsoleOutPut = null;
        public UserForm()
        {
            clientSocket = new ClientSocket();
            ConsoleOutPut = ConsoleTextBox;
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;

            // Submit on Enter
            this.AcceptButton = ConnectButton;
        }

        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            this.ConsoleTextBox.BeginInvoke((MethodInvoker)delegate {
                ConsoleTextBox.AppendText(e.Name + ": " + e.Action+"\n");
            });
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
            this.ConsoleTextBox.BeginInvoke((MethodInvoker)delegate {
                ConsoleTextBox.AppendText("State changed to " + e.NewState+"\n");
            });
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

        private async void doConnect(string url)
        {
            try
            {
                await clientSocket.Connect(url);
            } catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
                ConnectCodeBox.Enabled = true;
                ConnectButton.Enabled = true;
                URLTextBox.Enabled = true;
                return;
            }

            // We could connect to the URL -> save it
            Config.GetInstance().Set("url", url);

            _ = clientSocket.SendConnectCode(ConnectCodeBox.Text);

            _ = Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
        }

        private void ConnectCodeBox_TextChanged(object sender, EventArgs e)
        {
            ConnectButton.Enabled = (ConnectCodeBox.Enabled && ConnectCodeBox.Text.Length == 6 && !ConnectCodeBox.Text.Contains(" "));
        }
    }
}
