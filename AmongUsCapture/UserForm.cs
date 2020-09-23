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
        public static RichTextBox ConsoleOutPut = null;
        public UserForm(ClientSocket sock)
        {
            clientSocket = sock;
            ConsoleOutPut = ConsoleTextBox;
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
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
            clientSocket.SendConnectCode(ConnectCodeBox.Text);
            ConnectCodeBox.Enabled = false;
            SubmitButton.Enabled = false;
        }
    }
}
