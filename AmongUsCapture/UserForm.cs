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
        }

        private void UserForm_Load(object sender, EventArgs e)
        {

        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            this.CurrentState.BeginInvoke((MethodInvoker)delegate {
                CurrentState.Text = e.NewState.ToString();
            });
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            clientSocket.SendConnectCode(ConnectCodeBox.Text);
        }
    }
}
