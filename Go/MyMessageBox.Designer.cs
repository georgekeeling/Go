namespace GoPlanner
{
  partial class MyMessageBox
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
      this.messageBox = new System.Windows.Forms.TextBox();
      this.buttonOK = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // messageBox
      // 
      this.messageBox.BackColor = System.Drawing.SystemColors.Control;
      this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.messageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.messageBox.Location = new System.Drawing.Point(41, 29);
      this.messageBox.Multiline = true;
      this.messageBox.Name = "messageBox";
      this.messageBox.ReadOnly = true;
      this.messageBox.Size = new System.Drawing.Size(490, 158);
      this.messageBox.TabIndex = 0;
      this.messageBox.Text = "line1\r\nline2\r\nline3\r\n4\r\n5\r\n6";
      // 
      // buttonOK
      // 
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.Location = new System.Drawing.Point(464, 214);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(171, 60);
      this.buttonOK.TabIndex = 1;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(247, 214);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(171, 60);
      this.button2.TabIndex = 2;
      this.button2.Text = "??";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Visible = false;
      // 
      // button3
      // 
      this.button3.DialogResult = System.Windows.Forms.DialogResult.Abort;
      this.button3.Location = new System.Drawing.Point(42, 214);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(171, 60);
      this.button3.TabIndex = 3;
      this.button3.Text = "??";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Visible = false;
      // 
      // MyMessageBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonOK;
      this.ClientSize = new System.Drawing.Size(656, 299);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.messageBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MyMessageBox";
      this.Text = "MyMessageBox";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox messageBox;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
  }
}