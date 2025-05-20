namespace GoPlanner
{
  partial class HelpAbout
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpAbout));
      this.Speech = new System.Windows.Forms.RichTextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.Version = new System.Windows.Forms.Label();
      this.CartaPaper = new System.Windows.Forms.LinkLabel();
      this.CartaCode = new System.Windows.Forms.LinkLabel();
      this.SuspendLayout();
      // 
      // Speech
      // 
      this.Speech.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.Speech.Location = new System.Drawing.Point(40, 52);
      this.Speech.Name = "Speech";
      this.Speech.ReadOnly = true;
      this.Speech.Size = new System.Drawing.Size(616, 214);
      this.Speech.TabIndex = 0;
      this.Speech.Text = "Go Planner was requested by David Keeling.\n\nIcons from https://icon-icons.com/.\n\n" +
    "Huge thanks to Andy Carta who provided the scoring algorithms.\n";
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.Location = new System.Drawing.Point(581, 341);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 46);
      this.button1.TabIndex = 1;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(35, 352);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(322, 25);
      this.label1.TabIndex = 2;
      this.label1.Text = "This program by George Keeling";
      // 
      // Version
      // 
      this.Version.AutoSize = true;
      this.Version.Location = new System.Drawing.Point(35, 391);
      this.Version.Name = "Version";
      this.Version.Size = new System.Drawing.Size(177, 25);
      this.Version.TabIndex = 3;
      this.Version.Text = "V6.0, 8 Feb 2025";
      // 
      // CartaPaper
      // 
      this.CartaPaper.AutoSize = true;
      this.CartaPaper.Location = new System.Drawing.Point(35, 292);
      this.CartaPaper.Name = "CartaPaper";
      this.CartaPaper.Size = new System.Drawing.Size(127, 25);
      this.CartaPaper.TabIndex = 4;
      this.CartaPaper.TabStop = true;
      this.CartaPaper.Text = "Carta Paper";
      // 
      // CartaCode
      // 
      this.CartaCode.AutoSize = true;
      this.CartaCode.Location = new System.Drawing.Point(198, 292);
      this.CartaCode.Name = "CartaCode";
      this.CartaCode.Size = new System.Drawing.Size(121, 25);
      this.CartaCode.TabIndex = 5;
      this.CartaCode.TabStop = true;
      this.CartaCode.Text = "Carta Code";
      // 
      // HelpAbout
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.button1;
      this.ClientSize = new System.Drawing.Size(714, 445);
      this.Controls.Add(this.CartaCode);
      this.Controls.Add(this.CartaPaper);
      this.Controls.Add(this.Version);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.Speech);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "HelpAbout";
      this.Text = "About Go Planner";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RichTextBox Speech;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label Version;
    private System.Windows.Forms.LinkLabel CartaPaper;
    private System.Windows.Forms.LinkLabel CartaCode;
  }
}