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
            ConnectCodeBox.TextChanged += ConnectCodeBox_TextChanged;

            // Submit on Enter
            this.AcceptButton = SubmitButton;
        }

        private void ConnectCodeBox_TextChanged(object sender, EventArgs e)
        {
            if (ConnectCodeBox.TextLength == 6 && ConnectCodeBox.Enabled)
            {
                SubmitButton.Enabled = true;
            }
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
            ConnectCodeBox.Enabled = false;
            SubmitButton.Enabled = false;

            var url = "http://localhost:8123";
            // Validate URL
            if (URLTextBox.Text != "")
            {
                url = URLTextBox.Text;
            }

            try
            {
                new Uri(url);
            } catch (UriFormatException _)
            {
                // TODO: Invalid URL entered
                return;
            }

            doConnect(url);
        }

        private async void doConnect(string url)
        {
            await clientSocket.Connect(url);
            _ = clientSocket.SendConnectCode(ConnectCodeBox.Text);

            _ = Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
        }
    }
}
