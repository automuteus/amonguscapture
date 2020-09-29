using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AmongUsCapture
{
    public partial class UserForm : Form
    {
        private ClientSocket clientSocket;

        public UserForm(ClientSocket clientSocket)
        {
            this.clientSocket = clientSocket;
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;

            // Load URL
            URLTextBox.Text = Config.GetInstance().GetOrDefault("url", "");

            // Connect on Enter
            this.AcceptButton = ConnectButton;

            if (DarkTheme())
            {
                EnableDarkTheme();
            }
        }

        private void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            GameCodeBox.BeginInvoke((MethodInvoker)delegate
            {
                GameCodeBox.Text = e.LobbyCode;
            });
            
        }

        private void OnLoad(object sender, EventArgs e)
        {
            //TestFillConsole(100);
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {

            Program.conInterface.WriteTextFormatted($"[§6CHAT§f] {PlayerColorToColorCode(e.Color)}{e.Sender}§f: §f{e.Message}§f");
            //WriteLineToConsole($"[CHAT] {e.Sender}: {e.Message}");
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


            GameCodeBox.BackColor = DarkGrey;
            GameCodeBox.ForeColor = White;

            GameCodeGB.BackColor = LighterGrey;
            GameCodeGB.ForeColor = White;

            GameCodeCopyButton.BackColor = BluePurpleAccent;
            GameCodeCopyButton.ForeColor = White;

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
            Program.conInterface.WriteTextFormatted($"[§6PlayerChange§f] {PlayerColorToColorCode(e.Color)}{e.Name}§f: §f{e.Action}§f");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            URLTextBox.Text = Config.GetInstance().GetOrDefault("url", "");
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            this.CurrentState.BeginInvoke((MethodInvoker)delegate
            {
                CurrentState.Text = e.NewState.ToString();
            });
            Program.conInterface.WriteTextFormatted($"[§aGameMemReader§f] State changed to §b{e.NewState}§f");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
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
            List<String> colors = new List<string>
            {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "a",
                "b",
                "c",
                "d",
                "e",
                "f",
                "o",
                "n",
                "r"
            };
            foreach (var color in colors)
            {
                this.WriteLineFormatted($"{color} = §{color}{color}");
            }
        }

        public void WriteConsoleLineFormatted(String moduleName, Color moduleColor, String message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            var normalColor = DarkTheme() ? Color.White : Color.Black;
            this.AppendColoredTextToConsole("[", normalColor, false);
            this.AppendColoredTextToConsole(moduleName, moduleColor, false);
            this.AppendColoredTextToConsole($"]: {message}", normalColor, true);
        }

        public void AppendColoredTextToConsole(String line, Color color, bool addNewLine = false)
        {
            if (!(ConsoleTextBox is null))
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker)delegate
                {
                    ConsoleTextBox.SuspendLayout();
                    ConsoleTextBox.SelectionColor = color;
                    ConsoleTextBox.AppendText(addNewLine
                        ? $"{line}{Environment.NewLine}"
                        : line);
                    ConsoleTextBox.ScrollToCaret();
                    ConsoleTextBox.ResumeLayout();
                });
            }
        }

        public void WriteLineToConsole(String line)
        {
            if (!(ConsoleTextBox is null))
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker)delegate
                {
                    ConsoleTextBox.AppendText(line + "\n");
                });
            }
            
        }
        private string PlayerColorToColorCode(PlayerColor pColor)
        {
            //Red = 0,
            //Blue = 1,
            //Green = 2,
            //Pink = 3,
            //Orange = 4,
            //Yellow = 5,
            //Black = 6,
            //White = 7,
            //Purple = 8,
            //Brown = 9,
            //Cyan = 10,
            //Lime = 11
            string OutputCode = "";
            switch (pColor)
            {
                case PlayerColor.Red: OutputCode = "§c"; break;
                case PlayerColor.Blue: OutputCode = "§1"; break;
                case PlayerColor.Green: OutputCode = "§2"; break;
                case PlayerColor.Pink: OutputCode = "§d"; break;
                case PlayerColor.Orange: OutputCode = "§o"; break;
                case PlayerColor.Yellow: OutputCode = "§e"; break;
                case PlayerColor.Black: OutputCode = "§0"; break;
                case PlayerColor.White: OutputCode = "§f"; break;
                case PlayerColor.Purple: OutputCode = "§5"; break;
                case PlayerColor.Brown: OutputCode = "§n"; break;
                case PlayerColor.Cyan: OutputCode = "§b"; break;
                case PlayerColor.Lime: OutputCode = "§a"; break;
            }
            return OutputCode;
        }

        public void WriteLineFormatted(string str, bool acceptnewlines = true)
        {
            if (!(ConsoleTextBox is null))
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker)delegate
                {
                    if (!String.IsNullOrEmpty(str))
                    {
                        if (!acceptnewlines)
                        {
                            str = str.Replace('\n', ' ');
                        }
                        string[] parts = str.Split(new char[] { '§' });
                        if (parts[0].Length > 0)
                        {
                            AppendColoredTextToConsole(parts[0], Color.White, false);
                        }
                        for (int i = 1; i < parts.Length; i++)
                        {
                            Color charColor = Color.White;
                            if (parts[i].Length > 0)
                            {
                                switch (parts[i][0])
                                {
                                    case '0': charColor = Color.Gray; break; //Should be Black but Black is non-readable on a black background
                                    case '1': charColor = Color.RoyalBlue; break;
                                    case '2': charColor = Color.Green; break;
                                    case '3': charColor = Color.DarkCyan; break;
                                    case '4': charColor = Color.DarkRed; break;
                                    case '5': charColor = Color.MediumPurple; break;
                                    case '6': charColor = Color.DarkKhaki; break;
                                    case '7': charColor = Color.Gray; break;
                                    case '8': charColor = Color.DarkGray; break;
                                    case '9': charColor = Color.LightBlue; break;
                                    case 'a': charColor = Color.Lime; break;
                                    case 'b': charColor = Color.Cyan; break;
                                    case 'c': charColor = Color.Red; break;
                                    case 'd': charColor = Color.Magenta; break;
                                    case 'e': charColor = Color.Yellow; break;
                                    case 'f': charColor = Color.White; break;
                                    case 'o': charColor = Color.Orange; break;
                                    case 'n': charColor = Color.SaddleBrown; break;
                                    case 'r': charColor = Color.Gray; break;
                                }

                                if (parts[i].Length > 1)
                                {
                                    AppendColoredTextToConsole(parts[i].Substring(1, parts[i].Length - 1), charColor, false);
                                }
                            }
                        }
                    }
                    AppendColoredTextToConsole("", Color.White, true);
                });
                
            }
                
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            if(!(this.GameCodeBox.Text is null || this.GameCodeBox.Text == ""))
            {
                System.Windows.Forms.Clipboard.SetText(this.GameCodeBox.Text);
            } 
           
        }

    }

}