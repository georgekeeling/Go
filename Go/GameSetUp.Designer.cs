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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameSetUp));
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
      this.label4 = new System.Windows.Forms.Label();
      this.PlayHours = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.PlayMinutes = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.AllowUndos = new System.Windows.Forms.CheckBox();
      this.PlayerStone = new System.Windows.Forms.Button();
      this.OpponentStone = new System.Windows.Forms.Button();
      this.TimeError = new System.Windows.Forms.Label();
      this.AudibleReminders = new System.Windows.Forms.CheckBox();
      this.label9 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // ServerStatus
      // 
      this.ServerStatus.Location = new System.Drawing.Point(235, 25);
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
      this.PlayerName.Location = new System.Drawing.Point(235, 100);
      this.PlayerName.MaxLength = 10;
      this.PlayerName.Name = "PlayerName";
      this.PlayerName.Size = new System.Drawing.Size(192, 31);
      this.PlayerName.TabIndex = 3;
      // 
      // ButtonTellServerName
      // 
      this.ButtonTellServerName.Location = new System.Drawing.Point(670, 98);
      this.ButtonTellServerName.Name = "ButtonTellServerName";
      this.ButtonTellServerName.Size = new System.Drawing.Size(155, 32);
      this.ButtonTellServerName.TabIndex = 4;
      this.ButtonTellServerName.Text = "Tell Server";
      this.ButtonTellServerName.UseVisualStyleBackColor = true;
      this.ButtonTellServerName.Click += new System.EventHandler(this.ButtonTellServerName_Click);
      // 
      // ButtonTellServerOpponent
      // 
      this.ButtonTellServerOpponent.Location = new System.Drawing.Point(671, 179);
      this.ButtonTellServerOpponent.Name = "ButtonTellServerOpponent";
      this.ButtonTellServerOpponent.Size = new System.Drawing.Size(155, 32);
      this.ButtonTellServerOpponent.TabIndex = 7;
      this.ButtonTellServerOpponent.Text = "Tell Server";
      this.ButtonTellServerOpponent.UseVisualStyleBackColor = true;
      this.ButtonTellServerOpponent.Click += new System.EventHandler(this.ButtonTellServerOpponent_Click);
      // 
      // OpponentName
      // 
      this.OpponentName.Location = new System.Drawing.Point(236, 181);
      this.OpponentName.MaxLength = 10;
      this.OpponentName.Name = "OpponentName";
      this.OpponentName.Size = new System.Drawing.Size(191, 31);
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
      this.ButtonCancel.Location = new System.Drawing.Point(665, 508);
      this.ButtonCancel.Name = "ButtonCancel";
      this.ButtonCancel.Size = new System.Drawing.Size(161, 44);
      this.ButtonCancel.TabIndex = 8;
      this.ButtonCancel.Text = "Cancel";
      this.ButtonCancel.UseVisualStyleBackColor = true;
      // 
      // ButtonStart
      // 
      this.ButtonStart.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.ButtonStart.Location = new System.Drawing.Point(456, 508);
      this.ButtonStart.Name = "ButtonStart";
      this.ButtonStart.Size = new System.Drawing.Size(161, 44);
      this.ButtonStart.TabIndex = 9;
      this.ButtonStart.Text = "Start Game";
      this.ButtonStart.UseVisualStyleBackColor = true;
      // 
      // ServerError
      // 
      this.ServerError.AutoSize = true;
      this.ServerError.Location = new System.Drawing.Point(230, 60);
      this.ServerError.Name = "ServerError";
      this.ServerError.Size = new System.Drawing.Size(54, 25);
      this.ServerError.TabIndex = 10;
      this.ServerError.Text = "XXX";
      // 
      // NameError
      // 
      this.NameError.AutoSize = true;
      this.NameError.Location = new System.Drawing.Point(235, 136);
      this.NameError.Name = "NameError";
      this.NameError.Size = new System.Drawing.Size(70, 25);
      this.NameError.TabIndex = 11;
      this.NameError.Text = "label4";
      // 
      // OpponentError
      // 
      this.OpponentError.AutoSize = true;
      this.OpponentError.Location = new System.Drawing.Point(235, 215);
      this.OpponentError.Name = "OpponentError";
      this.OpponentError.Size = new System.Drawing.Size(70, 25);
      this.OpponentError.TabIndex = 12;
      this.OpponentError.Text = "label4";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(32, 284);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(100, 25);
      this.label4.TabIndex = 13;
      this.label4.Text = "Play time";
      // 
      // PlayHours
      // 
      this.PlayHours.Location = new System.Drawing.Point(278, 278);
      this.PlayHours.MaxLength = 2;
      this.PlayHours.Name = "PlayHours";
      this.PlayHours.Size = new System.Drawing.Size(36, 31);
      this.PlayHours.TabIndex = 14;
      this.PlayHours.Text = "99";
      this.PlayHours.TextChanged += new System.EventHandler(this.PlayHours_TextChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(230, 284);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(42, 25);
      this.label5.TabIndex = 15;
      this.label5.Text = "hh:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(340, 284);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(52, 25);
      this.label6.TabIndex = 17;
      this.label6.Text = "mm:";
      // 
      // PlayMinutes
      // 
      this.PlayMinutes.Location = new System.Drawing.Point(391, 278);
      this.PlayMinutes.MaxLength = 2;
      this.PlayMinutes.Name = "PlayMinutes";
      this.PlayMinutes.Size = new System.Drawing.Size(36, 31);
      this.PlayMinutes.TabIndex = 16;
      this.PlayMinutes.Text = "99";
      this.PlayMinutes.TextChanged += new System.EventHandler(this.PlayMinutes_TextChanged);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(473, 284);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(138, 25);
      this.label7.TabIndex = 18;
      this.label7.Text = "(each player)";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(31, 359);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(131, 25);
      this.label8.TabIndex = 19;
      this.label8.Text = "Allow Undos";
      // 
      // AllowUndos
      // 
      this.AllowUndos.AutoSize = true;
      this.AllowUndos.Checked = true;
      this.AllowUndos.CheckState = System.Windows.Forms.CheckState.Checked;
      this.AllowUndos.Location = new System.Drawing.Point(236, 359);
      this.AllowUndos.Name = "AllowUndos";
      this.AllowUndos.Size = new System.Drawing.Size(28, 27);
      this.AllowUndos.TabIndex = 20;
      this.AllowUndos.UseVisualStyleBackColor = true;
      this.AllowUndos.CheckedChanged += new System.EventHandler(this.AllowUndos_CheckedChanged);
      // 
      // PlayerStone
      // 
      this.PlayerStone.BackColor = System.Drawing.Color.White;
      this.PlayerStone.Image = global::GoPlanner.Properties.Resources.StoneBlack;
      this.PlayerStone.Location = new System.Drawing.Point(461, 93);
      this.PlayerStone.Name = "PlayerStone";
      this.PlayerStone.Size = new System.Drawing.Size(40, 40);
      this.PlayerStone.TabIndex = 21;
      this.PlayerStone.UseVisualStyleBackColor = false;
      this.PlayerStone.Click += new System.EventHandler(this.PlayerStone_Click);
      // 
      // OpponentStone
      // 
      this.OpponentStone.BackColor = System.Drawing.Color.White;
      this.OpponentStone.Image = global::GoPlanner.Properties.Resources.StoneBlack;
      this.OpponentStone.Location = new System.Drawing.Point(461, 175);
      this.OpponentStone.Name = "OpponentStone";
      this.OpponentStone.Size = new System.Drawing.Size(40, 40);
      this.OpponentStone.TabIndex = 22;
      this.OpponentStone.UseVisualStyleBackColor = false;
      this.OpponentStone.Click += new System.EventHandler(this.OpponentStone_Click);
      // 
      // TimeError
      // 
      this.TimeError.AutoSize = true;
      this.TimeError.Location = new System.Drawing.Point(235, 324);
      this.TimeError.Name = "TimeError";
      this.TimeError.Size = new System.Drawing.Size(70, 25);
      this.TimeError.TabIndex = 23;
      this.TimeError.Text = "label4";
      // 
      // AudibleReminders
      // 
      this.AudibleReminders.AutoSize = true;
      this.AudibleReminders.Location = new System.Drawing.Point(235, 423);
      this.AudibleReminders.Name = "AudibleReminders";
      this.AudibleReminders.Size = new System.Drawing.Size(28, 27);
      this.AudibleReminders.TabIndex = 24;
      this.AudibleReminders.UseVisualStyleBackColor = true;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(32, 423);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(185, 25);
      this.label9.TabIndex = 25;
      this.label9.Text = "Audible reminders";
      // 
      // GameSetUp
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.ButtonCancel;
      this.ClientSize = new System.Drawing.Size(872, 574);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.AudibleReminders);
      this.Controls.Add(this.TimeError);
      this.Controls.Add(this.OpponentStone);
      this.Controls.Add(this.PlayerStone);
      this.Controls.Add(this.AllowUndos);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.PlayMinutes);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.PlayHours);
      this.Controls.Add(this.label4);
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
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Button PlayerStone;
    private System.Windows.Forms.Button OpponentStone;
    private System.Windows.Forms.Label TimeError;
    public System.Windows.Forms.TextBox PlayHours;
    public System.Windows.Forms.TextBox PlayMinutes;
    public System.Windows.Forms.CheckBox AllowUndos;
    private System.Windows.Forms.CheckBox AudibleReminders;
    private System.Windows.Forms.Label label9;
  }
}