namespace Blueshift_Client
{
    partial class Form1
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
            this.ClientConnectButton = new System.Windows.Forms.Button();
            this.ClientInputBox = new System.Windows.Forms.RichTextBox();
            this.UsersList = new System.Windows.Forms.ListBox();
            this.ClientLogBox = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.PortBox = new System.Windows.Forms.TextBox();
            this.IPBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ClientConnectButton
            // 
            this.ClientConnectButton.Location = new System.Drawing.Point(307, 37);
            this.ClientConnectButton.Name = "ClientConnectButton";
            this.ClientConnectButton.Size = new System.Drawing.Size(61, 21);
            this.ClientConnectButton.TabIndex = 15;
            this.ClientConnectButton.Text = "Connect";
            this.ClientConnectButton.UseVisualStyleBackColor = true;
            this.ClientConnectButton.Click += new System.EventHandler(this.ClientConnectButton_Click);
            // 
            // ClientInputBox
            // 
            this.ClientInputBox.Enabled = false;
            this.ClientInputBox.Location = new System.Drawing.Point(27, 301);
            this.ClientInputBox.Name = "ClientInputBox";
            this.ClientInputBox.Size = new System.Drawing.Size(450, 20);
            this.ClientInputBox.TabIndex = 14;
            this.ClientInputBox.Text = "";
            this.ClientInputBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MessageBoxKeyPressed);
            // 
            // UsersList
            // 
            this.UsersList.FormattingEnabled = true;
            this.UsersList.Location = new System.Drawing.Point(483, 32);
            this.UsersList.Name = "UsersList";
            this.UsersList.Size = new System.Drawing.Size(89, 290);
            this.UsersList.TabIndex = 13;
            // 
            // ClientLogBox
            // 
            this.ClientLogBox.HideSelection = false;
            this.ClientLogBox.Location = new System.Drawing.Point(27, 87);
            this.ClientLogBox.Name = "ClientLogBox";
            this.ClientLogBox.Size = new System.Drawing.Size(450, 208);
            this.ClientLogBox.TabIndex = 12;
            this.ClientLogBox.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(166, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "IP:";
            // 
            // PortBox
            // 
            this.PortBox.Location = new System.Drawing.Point(201, 38);
            this.PortBox.Name = "PortBox";
            this.PortBox.Size = new System.Drawing.Size(100, 20);
            this.PortBox.TabIndex = 9;
            // 
            // IPBox
            // 
            this.IPBox.Location = new System.Drawing.Point(50, 38);
            this.IPBox.Name = "IPBox";
            this.IPBox.Size = new System.Drawing.Size(100, 20);
            this.IPBox.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 361);
            this.Controls.Add(this.ClientConnectButton);
            this.Controls.Add(this.ClientInputBox);
            this.Controls.Add(this.UsersList);
            this.Controls.Add(this.ClientLogBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PortBox);
            this.Controls.Add(this.IPBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ClientConnectButton;
        private System.Windows.Forms.RichTextBox ClientInputBox;
        private System.Windows.Forms.ListBox UsersList;
        private System.Windows.Forms.RichTextBox ClientLogBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PortBox;
        private System.Windows.Forms.TextBox IPBox;
    }
}

