using MetroFramework.Controls;

namespace AmongUsCapture
{
    partial class UserForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.UserSettings = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.CurrentStateGroupBox = new System.Windows.Forms.GroupBox();
            this.CurrentState = new System.Windows.Forms.Label();
            this.GameCodeGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.GameCodeBox = new System.Windows.Forms.TextBox();
            this.GameCodeCopyButton = new System.Windows.Forms.Button();
            this.ConnectCodeGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ConnectCodeBox = new System.Windows.Forms.MaskedTextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.UrlGB = new System.Windows.Forms.GroupBox();
            this.URLTextBox = new System.Windows.Forms.TextBox();
            this.ConsoleGroupBox = new System.Windows.Forms.GroupBox();
            this.ConsoleTextBox = new System.Windows.Forms.RichTextBox();
            this.metroContextMenu1 = new MetroFramework.Controls.MetroContextMenu(this.components);
            this.AutoScrollMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.metroStyleExtender1 = new MetroFramework.Components.MetroStyleExtender(this.components);
            this.metroStyleManager1 = new MetroFramework.Components.MetroStyleManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.UserSettings.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.CurrentStateGroupBox.SuspendLayout();
            this.GameCodeGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.ConnectCodeGB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.UrlGB.SuspendLayout();
            this.ConsoleGroupBox.SuspendLayout();
            this.metroContextMenu1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(20, 60);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.UserSettings);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ConsoleGroupBox);
            this.splitContainer1.Size = new System.Drawing.Size(784, 396);
            this.splitContainer1.SplitterDistance = 259;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Text = "splitContainer1";
            // 
            // UserSettings
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.UserSettings, true);
            this.UserSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.UserSettings.Controls.Add(this.tableLayoutPanel2);
            this.UserSettings.Controls.Add(this.ConnectCodeGB);
            this.UserSettings.Controls.Add(this.UrlGB);
            this.UserSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.UserSettings.Location = new System.Drawing.Point(0, 0);
            this.UserSettings.Name = "UserSettings";
            this.UserSettings.Size = new System.Drawing.Size(259, 396);
            this.UserSettings.TabIndex = 0;
            this.UserSettings.TabStop = false;
            this.UserSettings.Text = "Settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.CurrentStateGroupBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.GameCodeGB, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 269);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(253, 124);
            this.tableLayoutPanel2.TabIndex = 5;
            // 
            // CurrentStateGroupBox
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.CurrentStateGroupBox, true);
            this.CurrentStateGroupBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CurrentStateGroupBox.Controls.Add(this.CurrentState);
            this.CurrentStateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentStateGroupBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CurrentStateGroupBox.Location = new System.Drawing.Point(3, 65);
            this.CurrentStateGroupBox.Name = "CurrentStateGroupBox";
            this.CurrentStateGroupBox.Size = new System.Drawing.Size(247, 56);
            this.CurrentStateGroupBox.TabIndex = 3;
            this.CurrentStateGroupBox.TabStop = false;
            this.CurrentStateGroupBox.Text = "Current State";
            // 
            // CurrentState
            // 
            this.CurrentState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroStyleExtender1.SetApplyMetroTheme(this.CurrentState, true);
            this.CurrentState.AutoSize = true;
            this.CurrentState.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CurrentState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CurrentState.Location = new System.Drawing.Point(38, 28);
            this.CurrentState.Name = "CurrentState";
            this.CurrentState.Size = new System.Drawing.Size(59, 15);
            this.CurrentState.TabIndex = 0;
            this.CurrentState.Text = "Loading...";
            this.CurrentState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GameCodeGB
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.GameCodeGB, true);
            this.GameCodeGB.AutoSize = true;
            this.GameCodeGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.GameCodeGB.Controls.Add(this.tableLayoutPanel3);
            this.GameCodeGB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCodeGB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.GameCodeGB.Location = new System.Drawing.Point(3, 3);
            this.GameCodeGB.Name = "GameCodeGB";
            this.GameCodeGB.Size = new System.Drawing.Size(247, 56);
            this.GameCodeGB.TabIndex = 4;
            this.GameCodeGB.TabStop = false;
            this.GameCodeGB.Text = "RoomCode";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.Controls.Add(this.GameCodeBox, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.GameCodeCopyButton, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(241, 34);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // GameCodeBox
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.GameCodeBox, true);
            this.GameCodeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.GameCodeBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCodeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.GameCodeBox.Location = new System.Drawing.Point(3, 3);
            this.GameCodeBox.MaxLength = 6;
            this.GameCodeBox.Name = "GameCodeBox";
            this.GameCodeBox.PlaceholderText = "No Game Found";
            this.GameCodeBox.ReadOnly = true;
            this.GameCodeBox.Size = new System.Drawing.Size(135, 23);
            this.GameCodeBox.TabIndex = 0;
            this.GameCodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // GameCodeCopyButton
            // 
            this.GameCodeCopyButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GameCodeCopyButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCodeCopyButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.GameCodeCopyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GameCodeCopyButton.Location = new System.Drawing.Point(144, 3);
            this.GameCodeCopyButton.Name = "GameCodeCopyButton";
            this.GameCodeCopyButton.Size = new System.Drawing.Size(94, 28);
            this.GameCodeCopyButton.TabIndex = 1;
            this.GameCodeCopyButton.Text = "Copy";
            this.GameCodeCopyButton.UseVisualStyleBackColor = true;
            this.GameCodeCopyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // ConnectCodeGB
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.ConnectCodeGB, true);
            this.ConnectCodeGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConnectCodeGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ConnectCodeGB.Controls.Add(this.tableLayoutPanel1);
            this.ConnectCodeGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.ConnectCodeGB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConnectCodeGB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ConnectCodeGB.Location = new System.Drawing.Point(3, 74);
            this.ConnectCodeGB.Name = "ConnectCodeGB";
            this.ConnectCodeGB.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ConnectCodeGB.Size = new System.Drawing.Size(253, 63);
            this.ConnectCodeGB.TabIndex = 4;
            this.ConnectCodeGB.TabStop = false;
            this.ConnectCodeGB.Text = "Connect Code";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.ConnectCodeBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ConnectButton, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(247, 41);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // ConnectCodeBox
            // 
            this.ConnectCodeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroStyleExtender1.SetApplyMetroTheme(this.ConnectCodeBox, true);
            this.ConnectCodeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ConnectCodeBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConnectCodeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ConnectCodeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ConnectCodeBox.Location = new System.Drawing.Point(6, 6);
            this.ConnectCodeBox.Mask = ">AAAAAAAA";
            this.ConnectCodeBox.Name = "ConnectCodeBox";
            this.ConnectCodeBox.Size = new System.Drawing.Size(114, 22);
            this.ConnectCodeBox.TabIndex = 0;
            this.ConnectCodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ConnectCodeBox.Click += new System.EventHandler(this.ConnectCodeBox_Click);
            this.ConnectCodeBox.TextChanged += new System.EventHandler(this.ConnectCodeBox_TextChanged);
            this.ConnectCodeBox.Enter += new System.EventHandler(this.ConnectCodeBox_Enter);
            // 
            // ConnectButton
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.ConnectButton, true);
            this.ConnectButton.AutoSize = true;
            this.ConnectButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConnectButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ConnectButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConnectButton.Enabled = false;
            this.ConnectButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.ConnectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConnectButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ConnectButton.Location = new System.Drawing.Point(126, 6);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(115, 29);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseCompatibleTextRendering = true;
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // UrlGB
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.UrlGB, true);
            this.UrlGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.UrlGB.Controls.Add(this.URLTextBox);
            this.UrlGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.UrlGB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.UrlGB.Location = new System.Drawing.Point(3, 19);
            this.UrlGB.Name = "UrlGB";
            this.UrlGB.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.UrlGB.Size = new System.Drawing.Size(253, 55);
            this.UrlGB.TabIndex = 1;
            this.UrlGB.TabStop = false;
            this.UrlGB.Text = "URL";
            // 
            // URLTextBox
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.URLTextBox, true);
            this.URLTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.URLTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.URLTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.URLTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.URLTextBox.Location = new System.Drawing.Point(10, 19);
            this.URLTextBox.Name = "URLTextBox";
            this.URLTextBox.PlaceholderText = "http://localhost:8123";
            this.URLTextBox.Size = new System.Drawing.Size(233, 23);
            this.URLTextBox.TabIndex = 0;
            // 
            // ConsoleGroupBox
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.ConsoleGroupBox, true);
            this.ConsoleGroupBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ConsoleGroupBox.Controls.Add(this.ConsoleTextBox);
            this.ConsoleGroupBox.Controls.Add(this.checkBox1);
            this.ConsoleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleGroupBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ConsoleGroupBox.Location = new System.Drawing.Point(0, 0);
            this.ConsoleGroupBox.Name = "ConsoleGroupBox";
            this.ConsoleGroupBox.Size = new System.Drawing.Size(521, 396);
            this.ConsoleGroupBox.TabIndex = 0;
            this.ConsoleGroupBox.TabStop = false;
            this.ConsoleGroupBox.Text = "Console Output";
            // 
            // ConsoleTextBox
            // 
            this.metroStyleExtender1.SetApplyMetroTheme(this.ConsoleTextBox, true);
            this.ConsoleTextBox.AutoWordSelection = true;
            this.ConsoleTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.ConsoleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConsoleTextBox.ContextMenuStrip = this.metroContextMenu1;
            this.ConsoleTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ConsoleTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ConsoleTextBox.Location = new System.Drawing.Point(3, 19);
            this.ConsoleTextBox.Name = "ConsoleTextBox";
            this.ConsoleTextBox.ReadOnly = true;
            this.ConsoleTextBox.Size = new System.Drawing.Size(515, 374);
            this.ConsoleTextBox.TabIndex = 0;
            this.ConsoleTextBox.TabStop = false;
            this.ConsoleTextBox.Text = "";
            this.ConsoleTextBox.TextChanged += new System.EventHandler(this.ConsoleTextBox_TextChanged);
            // 
            // metroContextMenu1
            // 
            this.metroContextMenu1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroContextMenu1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.metroContextMenu1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.metroContextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AutoScrollMenuItem});
            this.metroContextMenu1.Name = "metroContextMenu1";
            this.metroContextMenu1.Size = new System.Drawing.Size(129, 26);
            // 
            // AutoScrollMenuItem
            // 
            this.AutoScrollMenuItem.Checked = true;
            this.AutoScrollMenuItem.CheckOnClick = true;
            this.AutoScrollMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoScrollMenuItem.Name = "AutoScrollMenuItem";
            this.AutoScrollMenuItem.Size = new System.Drawing.Size(128, 22);
            this.AutoScrollMenuItem.Text = "Autoscroll";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(250, 315);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 19);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // metroStyleManager1
            // 
            this.metroStyleManager1.Owner = this;
            this.metroStyleManager1.Style = MetroFramework.MetroColorStyle.Red;
            // 
            // UserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 476);
            this.Controls.Add(this.splitContainer1);
            this.Name = "UserForm";
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.AeroShadow;
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.Style = MetroFramework.MetroColorStyle.Red;
            this.Text = "Among Us Capture";
            this.Theme = MetroFramework.MetroThemeStyle.Default;
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.UserSettings.ResumeLayout(false);
            this.UserSettings.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.CurrentStateGroupBox.ResumeLayout(false);
            this.CurrentStateGroupBox.PerformLayout();
            this.GameCodeGB.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ConnectCodeGB.ResumeLayout(false);
            this.ConnectCodeGB.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.UrlGB.ResumeLayout(false);
            this.UrlGB.PerformLayout();
            this.ConsoleGroupBox.ResumeLayout(false);
            this.ConsoleGroupBox.PerformLayout();
            this.metroContextMenu1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.GroupBox ConnectCodeGB;
        private System.Windows.Forms.GroupBox ConsoleGroupBox;
        private System.Windows.Forms.GroupBox CurrentStateGroupBox;
        private System.Windows.Forms.GroupBox GameCodeGB;
        private System.Windows.Forms.GroupBox UrlGB;
        private System.Windows.Forms.GroupBox UserSettings;
        private System.Windows.Forms.Label CurrentState;
        private System.Windows.Forms.MaskedTextBox ConnectCodeBox;
        private System.Windows.Forms.RichTextBox ConsoleTextBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox GameCodeBox;
        private System.Windows.Forms.TextBox URLTextBox;
        private MetroFramework.Components.MetroStyleExtender metroStyleExtender1;
        private MetroFramework.Components.MetroStyleManager metroStyleManager1;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Button GameCodeCopyButton;
        private MetroContextMenu metroContextMenu1;
        private System.Windows.Forms.ToolStripMenuItem AutoScrollMenuItem;
    }
}