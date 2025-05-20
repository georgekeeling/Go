namespace GoPlanner
{
  partial class Connect_Test
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
      this.ConnectButton = new System.Windows.Forms.Button();
      this.messagesList = new System.Windows.Forms.TextBox();
      this.GetGamesButton = new System.Windows.Forms.Button();
      this.PingButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // ConnectButton
      // 
      this.ConnectButton.Location = new System.Drawing.Point(44, 52);
      this.ConnectButton.Name = "ConnectButton";
      this.ConnectButton.Size = new System.Drawing.Size(196, 64);
      this.ConnectButton.TabIndex = 0;
      this.ConnectButton.Text = "Connect";
      this.ConnectButton.UseVisualStyleBackColor = true;
      this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
      // 
      // messagesList
      // 
      this.messagesList.Location = new System.Drawing.Point(401, 64);
      this.messagesList.MaxLength = 327677;
      this.messagesList.Multiline = true;
      this.messagesList.Name = "messagesList";
      this.messagesList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.messagesList.Size = new System.Drawing.Size(496, 299);
      this.messagesList.TabIndex = 1;
      // 
      // GetGamesButton
      // 
      this.GetGamesButton.Location = new System.Drawing.Point(44, 131);
      this.GetGamesButton.Name = "GetGamesButton";
      this.GetGamesButton.Size = new System.Drawing.Size(196, 65);
      this.GetGamesButton.TabIndex = 2;
      this.GetGamesButton.Text = "Get games";
      this.GetGamesButton.UseVisualStyleBackColor = true;
      this.GetGamesButton.Click += new System.EventHandler(this.GetGamesButton_Click);
      // 
      // PingButton
      // 
      this.PingButton.Location = new System.Drawing.Point(44, 228);
      this.PingButton.Name = "PingButton";
      this.PingButton.Size = new System.Drawing.Size(196, 65);
      this.PingButton.TabIndex = 3;
      this.PingButton.Text = "Ping";
      this.PingButton.UseVisualStyleBackColor = true;
      this.PingButton.Click += new System.EventHandler(this.PinPingButton_Click);
      // 
      // Connect_Test
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1056, 450);
      this.Controls.Add(this.PingButton);
      this.Controls.Add(this.GetGamesButton);
      this.Controls.Add(this.messagesList);
      this.Controls.Add(this.ConnectButton);
      this.Name = "Connect_Test";
      this.Text = "Connection Test";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button ConnectButton;
    private System.Windows.Forms.TextBox messagesList;
    private System.Windows.Forms.Button GetGamesButton;
    private System.Windows.Forms.Button PingButton;
  }
}