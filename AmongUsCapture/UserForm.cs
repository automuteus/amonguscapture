using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;
using TextColorLibrary;
using Timer = System.Threading.Timer;

namespace AmongUsCapture
{
    public partial class UserForm : MetroForm
    {
        private ClientSocket clientSocket;
        private LobbyEventArgs lastJoinedLobby;
        public static Color NormalTextColor = Color.Black;
        private static object locker = new Object();

        private Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }
        public UserForm(ClientSocket clientSocket)
        {
            this.clientSocket = clientSocket;
            InitializeComponent();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;

            // Load URL
            URLTextBox.Text = Settings.PersistentSettings.host;

            // Connect on Enter
            this.AcceptButton = ConnectButton;

            if (DarkTheme())
            {
                EnableDarkTheme();
            }
            else
            {
                this.metroStyleExtender1.SetApplyMetroTheme(ConsoleTextBox, false);
                ConsoleTextBox.ResetBackColor();
                ConsoleTextBox.ResetForeColor();
            }
            NormalTextColor = DarkTheme() ? Color.White : Color.Black;
        }

        private void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            GameCodeBox.BeginInvoke((MethodInvoker)delegate
            {
                GameCodeBox.Text = e.LobbyCode;
            });
            lastJoinedLobby = e;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            //TestFillConsole(1000);
        }

        private string getRainbowText(string nonRainbow, int shift = 0)
        {
            string OutputString = "";
            for (int i = 0; i < nonRainbow.Length; i++)
            {
                OutputString += Rainbow((float)((i+shift)% nonRainbow.Length) / nonRainbow.Length).ToTextColor() + nonRainbow[i];
            }
            return OutputString;
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki, $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Sender}{NormalTextColor.ToTextColor()}: {e.Message}");
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
            this.Theme = MetroThemeStyle.Dark;
            this.metroStyleManager1.Theme = MetroThemeStyle.Dark;
            this.metroStyleExtender1.StyleManager.Theme = MetroThemeStyle.Dark;
            //this.metroStyleExtender1.SetApplyMetroTheme(ConsoleTextBox, true);
            
            //ConsoleTextBox.BackColor = LighterGrey;
            //ConsoleTextBox.ForeColor = White;

            //ConsoleGroupBox.BackColor = DarkGrey;
            //ConsoleGroupBox.ForeColor = White;

            //UserSettings.BackColor = DarkGrey;
            //UserSettings.ForeColor = White;

            //CurrentStateGroupBox.BackColor = LighterGrey;
            //CurrentStateGroupBox.ForeColor = White;

            //ConnectCodeGB.BackColor = LighterGrey;
            //ConnectCodeGB.ForeColor = White;

            //ConnectCodeBox.BackColor = DarkGrey;
            //ConnectCodeBox.ForeColor = White;

            //UrlGB.BackColor = LighterGrey;
            //UrlGB.ForeColor = White;

            //URLTextBox.BackColor = DarkGrey;
            //URLTextBox.ForeColor = White;


            //ConnectButton.BackColor = BluePurpleAccent;
            //ConnectButton.ForeColor = White;


            //GameCodeBox.BackColor = DarkGrey;
            //GameCodeBox.ForeColor = White;

            //GameCodeGB.BackColor = LighterGrey;
            //GameCodeGB.ForeColor = White;

            //GameCodeCopyButton.BackColor = BluePurpleAccent;
            //GameCodeCopyButton.ForeColor = White;

            //BackColor = DarkGrey;
            //ForeColor = White;
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
            Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            URLTextBox.Text = Settings.PersistentSettings.host;
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            this.CurrentState.BeginInvoke((MethodInvoker)delegate
            {
                CurrentState.Text = e.NewState.ToString();
            });
            Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
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

        public void setColor(MetroColorStyle color)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                this.Style = color;
                this.metroStyleExtender1.Style = color;
                this.metroStyleManager1.Style = color;
                this.metroStyleManager1.Style = color;
            });
        }

        private void doConnect(string url)
        {
            clientSocket.OnConnected += (sender, e) =>
            {
                
                Settings.PersistentSettings.host = url;
                
            };

            try
            {
                clientSocket.Connect(url, ConnectCodeBox.Text);
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
            //if (AutoScrollMenuItem.Checked && canAutoScroll)
            //{
            //    ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
            //    ConsoleTextBox.ScrollToCaret();
            //}
        }

        private void autoscroll()
        {
            if (AutoScrollMenuItem.Checked)
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker) delegate
                {
                    ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
                    ConsoleTextBox.ScrollToCaret();
                });
            }
        }

        private void TestFillConsole(int entries) //Helper test method to see if filling console works.
        {
            for (int i = 0; i < entries; i++)
            {
                var nonString = "Wow! Look at this pretty text!";
                Settings.conInterface.WriteModuleTextColored("Rainbow", Rainbow((float)i / entries), getRainbowText(nonString, i));
            };
            //this.WriteColoredText(getRainbowText("This is a Pre-Release from Carbon's branch."));
        }

        public void WriteConsoleLineFormatted(String moduleName, Color moduleColor, String message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            this.WriteColoredText($"{NormalTextColor.ToTextColor()}[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
        }

        public void WriteColoredText(String ColoredText)
        {
            lock (locker)
            {
                foreach (var part in TextColor.toParts(ColoredText))
                {
                    this.AppendColoredTextToConsole(part.text, part.textColor, false);
                }
                this.AppendColoredTextToConsole("", Color.White, true);
            }
            autoscroll();


        }

        public void AppendColoredTextToConsole(String line, Color color, bool addNewLine = false)
        {
            if (!(ConsoleTextBox is null))
            {
                ConsoleTextBox.BeginInvoke((MethodInvoker)delegate
                {
                    lock (locker)
                    {
                        ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
                        ConsoleTextBox.SuspendLayout();
                        ConsoleTextBox.SelectionColor = color;
                        ConsoleTextBox.SelectedText = addNewLine ? $"{line}{Environment.NewLine}" : line;
                        //ConsoleTextBox.ScrollToCaret();
                        ConsoleTextBox.ResumeLayout();
                    }
                });
            }
        }

        public void WriteLineToConsole(String line)
        {
            if (!(ConsoleTextBox is null))
            {
                lock (locker)
                {
                    ConsoleTextBox.BeginInvoke((MethodInvoker)delegate
                    {
                        ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
                        ConsoleTextBox.AppendText(line + "\n");
                    });
                }
                autoscroll();
            }

            
        }

        private Color PlayerColorToColorOBJ(PlayerColor pColor)
        {
            Color OutputCode = Color.White;
            switch (pColor)
            {
                case PlayerColor.Red: OutputCode = Color.Red; break;
                case PlayerColor.Blue: OutputCode = Color.RoyalBlue; break;
                case PlayerColor.Green: OutputCode = Color.Green; break;
                case PlayerColor.Pink: OutputCode = Color.Magenta; break;
                case PlayerColor.Orange: OutputCode = Color.Orange; break;
                case PlayerColor.Yellow: OutputCode = Color.Yellow; break;
                case PlayerColor.Black: OutputCode = Color.Gray; break;
                case PlayerColor.White: OutputCode = Color.White; break;
                case PlayerColor.Purple: OutputCode = Color.MediumPurple; break;
                case PlayerColor.Brown: OutputCode = Color.SaddleBrown; break;
                case PlayerColor.Cyan: OutputCode = Color.Cyan; break;
                case PlayerColor.Lime: OutputCode = Color.Lime; break;
            }
            return OutputCode;
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
                    lock (locker)
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
                    }
                    autoscroll();
                    
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