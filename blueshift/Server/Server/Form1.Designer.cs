using System;
using System.Windows.Forms;

namespace Blueshift_Server
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
            this.IPBox = new System.Windows.Forms.TextBox();
            this.PortBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ServerLogBox = new System.Windows.Forms.RichTextBox();
            this.UsersList = new System.Windows.Forms.ListBox();
            this.ServerInputBox = new System.Windows.Forms.RichTextBox();
            this.ServerHostButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // IPBox
            // 
            this.IPBox.Location = new System.Drawing.Point(58, 49);
            this.IPBox.Name = "IPBox";
            this.IPBox.Size = new System.Drawing.Size(100, 20);
            this.IPBox.TabIndex = 0;
            // 
            // PortBox
            // 
            this.PortBox.Location = new System.Drawing.Point(209, 49);
            this.PortBox.Name = "PortBox";
            this.PortBox.Size = new System.Drawing.Size(100, 20);
            this.PortBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port:";
            // 
            // ServerLogBox
            // 
            this.ServerLogBox.HideSelection = false;
            this.ServerLogBox.Location = new System.Drawing.Point(35, 98);
            this.ServerLogBox.Name = "ServerLogBox";
            this.ServerLogBox.Size = new System.Drawing.Size(450, 208);
            this.ServerLogBox.TabIndex = 4;
            this.ServerLogBox.Text = "";
            // 
            // UsersList
            // 
            this.UsersList.FormattingEnabled = true;
            this.UsersList.Location = new System.Drawing.Point(491, 43);
            this.UsersList.Name = "UsersList";
            this.UsersList.Size = new System.Drawing.Size(89, 290);
            this.UsersList.TabIndex = 5;
            // 
            // ServerInputBox
            // 
            this.ServerInputBox.Enabled = false;
            this.ServerInputBox.Location = new System.Drawing.Point(35, 312);
            this.ServerInputBox.Name = "ServerInputBox";
            this.ServerInputBox.Size = new System.Drawing.Size(450, 20);
            this.ServerInputBox.TabIndex = 6;
            this.ServerInputBox.Text = "";
            this.ServerInputBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MessageBoxKeyPressed);
            // 
            // ServerHostButton
            // 
            this.ServerHostButton.Location = new System.Drawing.Point(315, 48);
            this.ServerHostButton.Name = "ServerHostButton";
            this.ServerHostButton.Size = new System.Drawing.Size(61, 21);
            this.ServerHostButton.TabIndex = 7;
            this.ServerHostButton.Text = "Host";
            this.ServerHostButton.UseVisualStyleBackColor = true;
            this.ServerHostButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 361);
            this.Controls.Add(this.ServerHostButton);
            this.Controls.Add(this.ServerInputBox);
            this.Controls.Add(this.UsersList);
            this.Controls.Add(this.ServerLogBox);
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

        private System.Windows.Forms.TextBox IPBox;
        private System.Windows.Forms.TextBox PortBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox ServerLogBox;
        private System.Windows.Forms.ListBox UsersList;
        private System.Windows.Forms.RichTextBox ServerInputBox;
        private System.Windows.Forms.Button ServerHostButton;
    }
}

