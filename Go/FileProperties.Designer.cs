namespace GoPlanner
{
  partial class FileProperties
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
      this.label1 = new System.Windows.Forms.Label();
      this.textFileName = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.ButtonOK = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.textDirectory = new System.Windows.Forms.TextBox();
      this.textGameName = new System.Windows.Forms.TextBox();
      this.textDimensions = new System.Windows.Forms.TextBox();
      this.textStones = new System.Windows.Forms.TextBox();
      this.textWarnings = new System.Windows.Forms.RichTextBox();
      this.textAuthor = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.ButtonCancel = new System.Windows.Forms.Button();
      this.textKomi = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.textHandicap = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.textResult = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this.textRules = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(40, 41);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(109, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "File Name";
      // 
      // textFileName
      // 
      this.textFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textFileName.Location = new System.Drawing.Point(243, 38);
      this.textFileName.Name = "textFileName";
      this.textFileName.ReadOnly = true;
      this.textFileName.Size = new System.Drawing.Size(561, 31);
      this.textFileName.TabIndex = 1;
      this.textFileName.TabStop = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(40, 436);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(182, 25);
      this.label2.TabIndex = 2;
      this.label2.Text = "Game Name (GN)";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(40, 99);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(172, 25);
      this.label3.TabIndex = 3;
      this.label3.Text = "Source Directory";
      // 
      // ButtonOK
      // 
      this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.ButtonOK.Location = new System.Drawing.Point(434, 760);
      this.ButtonOK.Name = "ButtonOK";
      this.ButtonOK.Size = new System.Drawing.Size(154, 55);
      this.ButtonOK.TabIndex = 4;
      this.ButtonOK.Text = "&OK";
      this.ButtonOK.UseVisualStyleBackColor = true;
      this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(40, 527);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(152, 25);
      this.label4.TabIndex = 5;
      this.label4.Text = "Load warnings";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(40, 147);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(124, 25);
      this.label5.TabIndex = 6;
      this.label5.Text = "Dimensions";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(40, 188);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(79, 25);
      this.label6.TabIndex = 7;
      this.label6.Text = "Stones";
      // 
      // textDirectory
      // 
      this.textDirectory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textDirectory.Location = new System.Drawing.Point(243, 93);
      this.textDirectory.Name = "textDirectory";
      this.textDirectory.ReadOnly = true;
      this.textDirectory.Size = new System.Drawing.Size(561, 31);
      this.textDirectory.TabIndex = 8;
      this.textDirectory.TabStop = false;
      // 
      // textGameName
      // 
      this.textGameName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textGameName.Location = new System.Drawing.Point(243, 434);
      this.textGameName.Name = "textGameName";
      this.textGameName.Size = new System.Drawing.Size(561, 31);
      this.textGameName.TabIndex = 1;
      // 
      // textDimensions
      // 
      this.textDimensions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textDimensions.Location = new System.Drawing.Point(243, 141);
      this.textDimensions.Name = "textDimensions";
      this.textDimensions.ReadOnly = true;
      this.textDimensions.Size = new System.Drawing.Size(561, 31);
      this.textDimensions.TabIndex = 11;
      this.textDimensions.TabStop = false;
      // 
      // textStones
      // 
      this.textStones.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textStones.Location = new System.Drawing.Point(243, 188);
      this.textStones.Name = "textStones";
      this.textStones.ReadOnly = true;
      this.textStones.Size = new System.Drawing.Size(561, 31);
      this.textStones.TabIndex = 12;
      this.textStones.TabStop = false;
      // 
      // textWarnings
      // 
      this.textWarnings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textWarnings.Location = new System.Drawing.Point(243, 525);
      this.textWarnings.Name = "textWarnings";
      this.textWarnings.ReadOnly = true;
      this.textWarnings.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.textWarnings.Size = new System.Drawing.Size(561, 199);
      this.textWarnings.TabIndex = 3;
      this.textWarnings.Text = "";
      // 
      // textAuthor
      // 
      this.textAuthor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textAuthor.Location = new System.Drawing.Point(243, 477);
      this.textAuthor.Name = "textAuthor";
      this.textAuthor.Size = new System.Drawing.Size(561, 31);
      this.textAuthor.TabIndex = 2;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(40, 479);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(124, 25);
      this.label7.TabIndex = 14;
      this.label7.Text = "Author (US)";
      // 
      // ButtonCancel
      // 
      this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.ButtonCancel.Location = new System.Drawing.Point(622, 760);
      this.ButtonCancel.Name = "ButtonCancel";
      this.ButtonCancel.Size = new System.Drawing.Size(154, 55);
      this.ButtonCancel.TabIndex = 5;
      this.ButtonCancel.Text = "&Cancel";
      this.ButtonCancel.UseVisualStyleBackColor = true;
      // 
      // textKomi
      // 
      this.textKomi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textKomi.Location = new System.Drawing.Point(243, 242);
      this.textKomi.Name = "textKomi";
      this.textKomi.ReadOnly = true;
      this.textKomi.Size = new System.Drawing.Size(561, 31);
      this.textKomi.TabIndex = 16;
      this.textKomi.TabStop = false;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(40, 242);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(112, 25);
      this.label8.TabIndex = 15;
      this.label8.Text = "Komi (KM)";
      // 
      // textHandicap
      // 
      this.textHandicap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textHandicap.Location = new System.Drawing.Point(243, 291);
      this.textHandicap.Name = "textHandicap";
      this.textHandicap.ReadOnly = true;
      this.textHandicap.Size = new System.Drawing.Size(561, 31);
      this.textHandicap.TabIndex = 18;
      this.textHandicap.TabStop = false;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(40, 291);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(103, 25);
      this.label9.TabIndex = 17;
      this.label9.Text = "Handicap";
      // 
      // textResult
      // 
      this.textResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textResult.Location = new System.Drawing.Point(243, 337);
      this.textResult.Name = "textResult";
      this.textResult.ReadOnly = true;
      this.textResult.Size = new System.Drawing.Size(561, 31);
      this.textResult.TabIndex = 20;
      this.textResult.TabStop = false;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(40, 337);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(122, 25);
      this.label10.TabIndex = 19;
      this.label10.Text = "Result (RE)";
      // 
      // textRules
      // 
      this.textRules.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textRules.Location = new System.Drawing.Point(243, 380);
      this.textRules.Name = "textRules";
      this.textRules.ReadOnly = true;
      this.textRules.Size = new System.Drawing.Size(561, 31);
      this.textRules.TabIndex = 22;
      this.textRules.TabStop = false;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(40, 380);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(117, 25);
      this.label11.TabIndex = 21;
      this.label11.Text = "Rules (RU)";
      // 
      // FileProperties
      // 
      this.AcceptButton = this.ButtonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.ButtonCancel;
      this.ClientSize = new System.Drawing.Size(862, 917);
      this.Controls.Add(this.textRules);
      this.Controls.Add(this.label11);
      this.Controls.Add(this.textResult);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.textHandicap);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.textKomi);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.ButtonCancel);
      this.Controls.Add(this.textAuthor);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.textWarnings);
      this.Controls.Add(this.textStones);
      this.Controls.Add(this.textDimensions);
      this.Controls.Add(this.textGameName);
      this.Controls.Add(this.textDirectory);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.ButtonOK);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textFileName);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FileProperties";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.Text = "File Properties";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textFileName;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button ButtonOK;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox textDirectory;
    private System.Windows.Forms.TextBox textGameName;
    private System.Windows.Forms.TextBox textDimensions;
    private System.Windows.Forms.TextBox textStones;
    private System.Windows.Forms.RichTextBox textWarnings;
    private System.Windows.Forms.TextBox textAuthor;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.TextBox textKomi;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox textHandicap;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox textResult;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox textRules;
    private System.Windows.Forms.Label label11;
  }
}