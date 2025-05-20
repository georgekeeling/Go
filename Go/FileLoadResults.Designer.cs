namespace GoPlanner
{
  partial class FileLoadResults
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileLoadResults));
      this.Results = new System.Windows.Forms.TextBox();
      this.ButtonOK = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // Results
      // 
      this.Results.Location = new System.Drawing.Point(30, 21);
      this.Results.Multiline = true;
      this.Results.Name = "Results";
      this.Results.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.Results.Size = new System.Drawing.Size(607, 498);
      this.Results.TabIndex = 0;
      // 
      // ButtonOK
      // 
      this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.ButtonOK.Location = new System.Drawing.Point(489, 551);
      this.ButtonOK.Name = "ButtonOK";
      this.ButtonOK.Size = new System.Drawing.Size(147, 42);
      this.ButtonOK.TabIndex = 1;
      this.ButtonOK.Text = "&OK";
      this.ButtonOK.UseVisualStyleBackColor = true;
      this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
      // 
      // FileLoadResults
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.ButtonOK;
      this.ClientSize = new System.Drawing.Size(661, 603);
      this.Controls.Add(this.ButtonOK);
      this.Controls.Add(this.Results);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.Name = "FileLoadResults";
      this.Text = "File Load Results";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button ButtonOK;
    public System.Windows.Forms.TextBox Results;
  }
}