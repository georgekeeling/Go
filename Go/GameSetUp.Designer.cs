namespace GoPlanner
{
  partial class GameSetUp
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
      this.ServerStatus = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.PlayerName = new System.Windows.Forms.TextBox();
      this.ButtonTellServerName = new System.Windows.Forms.Button();
      this.ButtonTellServerOpponent = new System.Windows.Forms.Button();
      this.OpponentName = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.ButtonCancel = new System.Windows.Forms.Button();
      this.ButtonStart = new System.Windows.Forms.Button();
      this.ServerError = new System.Windows.Forms.Label();
      this.NameError = new System.Windows.Forms.Label();
      this.OpponentError = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // ServerStatus
      // 
      this.ServerStatus.Location = new System.Drawing.Point(191, 25);
      this.ServerStatus.Name = "ServerStatus";
      this.ServerStatus.ReadOnly = true;
      this.ServerStatus.Size = new System.Drawing.Size(591, 31);
      this.ServerStatus.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(31, 31);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(142, 25);
      this.label1.TabIndex = 1;
      this.label1.Text = "Server Status";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(31, 106);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(68, 25);
      this.label2.TabIndex = 2;
      this.label2.Text = "Name";
      // 
      // PlayerName
      // 
      this.PlayerName.Location = new System.Drawing.Point(191, 100);
      this.PlayerName.Name = "PlayerName";
      this.PlayerName.Size = new System.Drawing.Size(407, 31);
      this.PlayerName.TabIndex = 3;
      // 
      // ButtonTellServerName
      // 
      this.ButtonTellServerName.Location = new System.Drawing.Point(626, 98);
      this.ButtonTellServerName.Name = "ButtonTellServerName";
      this.ButtonTellServerName.Size = new System.Drawing.Size(155, 32);
      this.ButtonTellServerName.TabIndex = 4;
      this.ButtonTellServerName.Text = "Tell Server";
      this.ButtonTellServerName.UseVisualStyleBackColor = true;
      this.ButtonTellServerName.Click += new System.EventHandler(this.ButtonTellServerName_Click);
      // 
      // ButtonTellServerOpponent
      // 
      this.ButtonTellServerOpponent.Location = new System.Drawing.Point(627, 179);
      this.ButtonTellServerOpponent.Name = "ButtonTellServerOpponent";
      this.ButtonTellServerOpponent.Size = new System.Drawing.Size(155, 32);
      this.ButtonTellServerOpponent.TabIndex = 7;
      this.ButtonTellServerOpponent.Text = "Tell Server";
      this.ButtonTellServerOpponent.UseVisualStyleBackColor = true;
      this.ButtonTellServerOpponent.Click += new System.EventHandler(this.ButtonTellServerOpponent_Click);
      // 
      // OpponentName
      // 
      this.OpponentName.Location = new System.Drawing.Point(192, 181);
      this.OpponentName.Name = "OpponentName";
      this.OpponentName.Size = new System.Drawing.Size(407, 31);
      this.OpponentName.TabIndex = 6;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(32, 187);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(106, 25);
      this.label3.TabIndex = 5;
      this.label3.Text = "Opponent";
      // 
      // ButtonCancel
      // 
      this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.ButtonCancel.Location = new System.Drawing.Point(454, 514);
      this.ButtonCancel.Name = "ButtonCancel";
      this.ButtonCancel.Size = new System.Drawing.Size(172, 44);
      this.ButtonCancel.TabIndex = 8;
      this.ButtonCancel.Text = "Cancel";
      this.ButtonCancel.UseVisualStyleBackColor = true;
      // 
      // ButtonStart
      // 
      this.ButtonStart.Location = new System.Drawing.Point(655, 513);
      this.ButtonStart.Name = "ButtonStart";
      this.ButtonStart.Size = new System.Drawing.Size(161, 44);
      this.ButtonStart.TabIndex = 9;
      this.ButtonStart.Text = "Start Game";
      this.ButtonStart.UseVisualStyleBackColor = true;
      // 
      // ServerError
      // 
      this.ServerError.AutoSize = true;
      this.ServerError.Location = new System.Drawing.Point(186, 60);
      this.ServerError.Name = "ServerError";
      this.ServerError.Size = new System.Drawing.Size(54, 25);
      this.ServerError.TabIndex = 10;
      this.ServerError.Text = "XXX";
      // 
      // NameError
      // 
      this.NameError.AutoSize = true;
      this.NameError.Location = new System.Drawing.Point(191, 136);
      this.NameError.Name = "NameError";
      this.NameError.Size = new System.Drawing.Size(70, 25);
      this.NameError.TabIndex = 11;
      this.NameError.Text = "label4";
      // 
      // OpponentError
      // 
      this.OpponentError.AutoSize = true;
      this.OpponentError.Location = new System.Drawing.Point(191, 215);
      this.OpponentError.Name = "OpponentError";
      this.OpponentError.Size = new System.Drawing.Size(70, 25);
      this.OpponentError.TabIndex = 12;
      this.OpponentError.Text = "label4";
      // 
      // GameSetUp
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.ButtonCancel;
      this.ClientSize = new System.Drawing.Size(872, 574);
      this.Controls.Add(this.OpponentError);
      this.Controls.Add(this.NameError);
      this.Controls.Add(this.ServerError);
      this.Controls.Add(this.ButtonStart);
      this.Controls.Add(this.ButtonCancel);
      this.Controls.Add(this.ButtonTellServerOpponent);
      this.Controls.Add(this.OpponentName);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.ButtonTellServerName);
      this.Controls.Add(this.PlayerName);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.ServerStatus);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "GameSetUp";
      this.Text = "Game Set Up";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox ServerStatus;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button ButtonTellServerName;
    private System.Windows.Forms.Button ButtonTellServerOpponent;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonStart;
    private System.Windows.Forms.Label ServerError;
    private System.Windows.Forms.Label NameError;
    private System.Windows.Forms.Label OpponentError;
    public System.Windows.Forms.TextBox PlayerName;
    public System.Windows.Forms.TextBox OpponentName;
  }
}