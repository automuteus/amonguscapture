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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.UserSettings = new System.Windows.Forms.GroupBox();
            this.ConnectCodeGB = new System.Windows.Forms.GroupBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.ConnectCodeBox = new System.Windows.Forms.MaskedTextBox();
            this.UrlGB = new System.Windows.Forms.GroupBox();
            this.URLTextBox = new System.Windows.Forms.TextBox();
            this.CurrentStateGroupBox = new System.Windows.Forms.GroupBox();
            this.CurrentState = new System.Windows.Forms.Label();
            this.ConsoleGroupBox = new System.Windows.Forms.GroupBox();
            this.ConsoleTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.UserSettings.SuspendLayout();
            this.ConnectCodeGB.SuspendLayout();
            this.UrlGB.SuspendLayout();
            this.CurrentStateGroupBox.SuspendLayout();
            this.ConsoleGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.UserSettings);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ConsoleGroupBox);
            this.splitContainer1.Size = new System.Drawing.Size(824, 476);
            this.splitContainer1.SplitterDistance = 274;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Text = "splitContainer1";
            // 
            // UserSettings
            // 
            this.UserSettings.Controls.Add(this.ConnectCodeGB);
            this.UserSettings.Controls.Add(this.UrlGB);
            this.UserSettings.Controls.Add(this.CurrentStateGroupBox);
            this.UserSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserSettings.Location = new System.Drawing.Point(0, 0);
            this.UserSettings.Name = "UserSettings";
            this.UserSettings.Size = new System.Drawing.Size(274, 476);
            this.UserSettings.TabIndex = 0;
            this.UserSettings.TabStop = false;
            this.UserSettings.Text = "Settings";
            // 
            // ConnectCodeGB
            // 
            this.ConnectCodeGB.Controls.Add(this.ConnectButton);
            this.ConnectCodeGB.Controls.Add(this.ConnectCodeBox);
            this.ConnectCodeGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.ConnectCodeGB.Location = new System.Drawing.Point(3, 74);
            this.ConnectCodeGB.Name = "ConnectCodeGB";
            this.ConnectCodeGB.Size = new System.Drawing.Size(268, 61);
            this.ConnectCodeGB.TabIndex = 2;
            this.ConnectCodeGB.TabStop = false;
            this.ConnectCodeGB.Text = "Connect Code";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.BackColor = System.Drawing.SystemColors.Control;
            this.ConnectButton.Enabled = false;
            this.ConnectButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.ConnectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ConnectButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ConnectButton.Location = new System.Drawing.Point(183, 22);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // ConnectCodeBox
            // 
            this.ConnectCodeBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConnectCodeBox.Font = new System.Drawing.Font("Cascadia Code", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ConnectCodeBox.Location = new System.Drawing.Point(9, 22);
            this.ConnectCodeBox.Mask = ">AAAAAA";
            this.ConnectCodeBox.Name = "ConnectCodeBox";
            this.ConnectCodeBox.Size = new System.Drawing.Size(100, 23);
            this.ConnectCodeBox.TabIndex = 1;
            this.ConnectCodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ConnectCodeBox.TextChanged += new System.EventHandler(this.ConnectCodeBox_TextChanged);
            // 
            // UrlGB
            // 
            this.UrlGB.Controls.Add(this.URLTextBox);
            this.UrlGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.UrlGB.Location = new System.Drawing.Point(3, 19);
            this.UrlGB.Name = "UrlGB";
            this.UrlGB.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.UrlGB.Size = new System.Drawing.Size(268, 55);
            this.UrlGB.TabIndex = 1;
            this.UrlGB.TabStop = false;
            this.UrlGB.Text = "URL";
            // 
            // URLTextBox
            // 
            this.URLTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.URLTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.URLTextBox.Location = new System.Drawing.Point(10, 19);
            this.URLTextBox.Name = "URLTextBox";
            this.URLTextBox.PlaceholderText = "http://localhost:8123";
            this.URLTextBox.Size = new System.Drawing.Size(248, 23);
            this.URLTextBox.TabIndex = 0;
            // 
            // CurrentStateGroupBox
            // 
            this.CurrentStateGroupBox.Controls.Add(this.CurrentState);
            this.CurrentStateGroupBox.Location = new System.Drawing.Point(6, 414);
            this.CurrentStateGroupBox.Name = "CurrentStateGroupBox";
            this.CurrentStateGroupBox.Size = new System.Drawing.Size(262, 56);
            this.CurrentStateGroupBox.TabIndex = 3;
            this.CurrentStateGroupBox.TabStop = false;
            this.CurrentStateGroupBox.Text = "Current State";
            // 
            // CurrentState
            // 
            this.CurrentState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CurrentState.AutoSize = true;
            this.CurrentState.Location = new System.Drawing.Point(38, 28);
            this.CurrentState.Name = "CurrentState";
            this.CurrentState.Size = new System.Drawing.Size(59, 15);
            this.CurrentState.TabIndex = 0;
            this.CurrentState.Text = "Loading...";
            this.CurrentState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ConsoleGroupBox
            // 
            this.ConsoleGroupBox.Controls.Add(this.ConsoleTextBox);
            this.ConsoleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleGroupBox.Location = new System.Drawing.Point(0, 0);
            this.ConsoleGroupBox.Name = "ConsoleGroupBox";
            this.ConsoleGroupBox.Size = new System.Drawing.Size(546, 476);
            this.ConsoleGroupBox.TabIndex = 0;
            this.ConsoleGroupBox.TabStop = false;
            this.ConsoleGroupBox.Text = "Console Output";
            // 
            // ConsoleTextBox
            // 
            this.ConsoleTextBox.AutoWordSelection = true;
            this.ConsoleTextBox.BackColor = System.Drawing.Color.White;
            this.ConsoleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConsoleTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ConsoleTextBox.Location = new System.Drawing.Point(3, 19);
            this.ConsoleTextBox.Name = "ConsoleTextBox";
            this.ConsoleTextBox.ReadOnly = true;
            this.ConsoleTextBox.Size = new System.Drawing.Size(540, 454);
            this.ConsoleTextBox.TabIndex = 0;
            this.ConsoleTextBox.TabStop = false;
            this.ConsoleTextBox.Text = "";
            // 
            // UserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 476);
            this.Controls.Add(this.splitContainer1);
            this.Name = "UserForm";
            this.Text = "Among Us Capture";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.UserSettings.ResumeLayout(false);
            this.ConnectCodeGB.ResumeLayout(false);
            this.ConnectCodeGB.PerformLayout();
            this.UrlGB.ResumeLayout(false);
            this.UrlGB.PerformLayout();
            this.CurrentStateGroupBox.ResumeLayout(false);
            this.CurrentStateGroupBox.PerformLayout();
            this.ConsoleGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox UserSettings;
        private System.Windows.Forms.GroupBox ConsoleGroupBox;
        private System.Windows.Forms.RichTextBox ConsoleTextBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.MaskedTextBox ConnectCodeBox;
        private System.Windows.Forms.GroupBox CurrentStateGroupBox;
        private System.Windows.Forms.Label CurrentState;
        private System.Windows.Forms.GroupBox ConnectCodeGB;
        private System.Windows.Forms.GroupBox UrlGB;
        private System.Windows.Forms.TextBox URLTextBox;
    }
}