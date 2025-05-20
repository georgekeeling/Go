namespace GoPlanner
{
  partial class ScoreResults
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
      this.TheScores = new System.Windows.Forms.TextBox();
      this.OKbutton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // TheScores
      // 
      this.TheScores.Location = new System.Drawing.Point(33, 48);
      this.TheScores.Multiline = true;
      this.TheScores.Name = "TheScores";
      this.TheScores.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.TheScores.Size = new System.Drawing.Size(1116, 572);
      this.TheScores.TabIndex = 0;
      // 
      // OKbutton
      // 
      this.OKbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.OKbutton.Location = new System.Drawing.Point(982, 664);
      this.OKbutton.Name = "OKbutton";
      this.OKbutton.Size = new System.Drawing.Size(166, 50);
      this.OKbutton.TabIndex = 1;
      this.OKbutton.Text = "OK";
      this.OKbutton.UseVisualStyleBackColor = true;
      // 
      // ScoreResults
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1208, 750);
      this.Controls.Add(this.OKbutton);
      this.Controls.Add(this.TheScores);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ScoreResults";
      this.Text = "Score Results";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button OKbutton;
    public System.Windows.Forms.TextBox TheScores;
  }
}