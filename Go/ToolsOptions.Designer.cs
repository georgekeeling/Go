namespace GoPlanner
{
  partial class ToolsOptions
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolsOptions));
      this.label1 = new System.Windows.Forms.Label();
      this.TopScriptCombo = new System.Windows.Forms.ComboBox();
      this.TopDirectionCombo = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.OmitLetters = new System.Windows.Forms.TextBox();
      this.CloseButton = new System.Windows.Forms.Button();
      this.LeftDirectionCombo = new System.Windows.Forms.ComboBox();
      this.LeftScriptCombo = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.AllRefsCombo = new System.Windows.Forms.ComboBox();
      this.ShowSeqCheckBox = new System.Windows.Forms.CheckBox();
      this.EnableSave = new System.Windows.Forms.CheckBox();
      this.StartBlack = new System.Windows.Forms.CheckBox();
      this.StartAlternating = new System.Windows.Forms.CheckBox();
      this.ShowRemovals = new System.Windows.Forms.CheckBox();
      this.ShowTerritory = new System.Windows.Forms.CheckBox();
      this.ShowConnections = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.ScoreResult = new System.Windows.Forms.TextBox();
      this.ShowNone = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(76, 76);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Top";
      // 
      // TopScriptCombo
      // 
      this.TopScriptCombo.AllowDrop = true;
      this.TopScriptCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.TopScriptCombo.FormattingEnabled = true;
      this.TopScriptCombo.Items.AddRange(new object[] {
            "None",
            "Arabic Numerals 1-19",
            "Roman letters",
            "Chinese Numerals",
            "Arabic Numerals 0-18"});
      this.TopScriptCombo.Location = new System.Drawing.Point(180, 76);
      this.TopScriptCombo.MaxDropDownItems = 4;
      this.TopScriptCombo.Name = "TopScriptCombo";
      this.TopScriptCombo.Size = new System.Drawing.Size(248, 33);
      this.TopScriptCombo.TabIndex = 1;
      this.TopScriptCombo.SelectedIndexChanged += new System.EventHandler(this.TopScriptCombo_SelectedIndexChanged);
      // 
      // TopDirectionCombo
      // 
      this.TopDirectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.TopDirectionCombo.FormattingEnabled = true;
      this.TopDirectionCombo.Items.AddRange(new object[] {
            "Start at left",
            "Start at right"});
      this.TopDirectionCombo.Location = new System.Drawing.Point(477, 76);
      this.TopDirectionCombo.Name = "TopDirectionCombo";
      this.TopDirectionCombo.Size = new System.Drawing.Size(181, 33);
      this.TopDirectionCombo.TabIndex = 2;
      this.TopDirectionCombo.SelectedIndexChanged += new System.EventHandler(this.TopDirectionCombo_SelectedIndexChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(685, 110);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(121, 25);
      this.label2.TabIndex = 3;
      this.label2.Text = "Omit letters";
      // 
      // OmitLetters
      // 
      this.OmitLetters.Location = new System.Drawing.Point(848, 108);
      this.OmitLetters.MaxLength = 3;
      this.OmitLetters.Name = "OmitLetters";
      this.OmitLetters.Size = new System.Drawing.Size(79, 31);
      this.OmitLetters.TabIndex = 4;
      this.OmitLetters.TextChanged += new System.EventHandler(this.OmitLetters_TextChanged);
      // 
      // CloseButton
      // 
      this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.CloseButton.Location = new System.Drawing.Point(793, 577);
      this.CloseButton.Name = "CloseButton";
      this.CloseButton.Size = new System.Drawing.Size(154, 49);
      this.CloseButton.TabIndex = 5;
      this.CloseButton.Text = "Close";
      this.CloseButton.UseVisualStyleBackColor = true;
      // 
      // LeftDirectionCombo
      // 
      this.LeftDirectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.LeftDirectionCombo.FormattingEnabled = true;
      this.LeftDirectionCombo.Items.AddRange(new object[] {
            "Start at top",
            "Start at bottom"});
      this.LeftDirectionCombo.Location = new System.Drawing.Point(477, 137);
      this.LeftDirectionCombo.Name = "LeftDirectionCombo";
      this.LeftDirectionCombo.Size = new System.Drawing.Size(181, 33);
      this.LeftDirectionCombo.TabIndex = 8;
      this.LeftDirectionCombo.SelectedIndexChanged += new System.EventHandler(this.LeftDirectionCombo_SelectedIndexChanged);
      // 
      // LeftScriptCombo
      // 
      this.LeftScriptCombo.AllowDrop = true;
      this.LeftScriptCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.LeftScriptCombo.FormattingEnabled = true;
      this.LeftScriptCombo.Items.AddRange(new object[] {
            "None",
            "Arabic Numerals 1-19",
            "Roman letters",
            "Chinese Numerals",
            "Arabic Numerals 0-18"});
      this.LeftScriptCombo.Location = new System.Drawing.Point(180, 137);
      this.LeftScriptCombo.MaxDropDownItems = 4;
      this.LeftScriptCombo.Name = "LeftScriptCombo";
      this.LeftScriptCombo.Size = new System.Drawing.Size(248, 33);
      this.LeftScriptCombo.TabIndex = 7;
      this.LeftScriptCombo.SelectedIndexChanged += new System.EventHandler(this.LeftScriptCombo_SelectedIndexChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(76, 137);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(48, 25);
      this.label3.TabIndex = 6;
      this.label3.Text = "Left";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.AllRefsCombo);
      this.groupBox1.Location = new System.Drawing.Point(41, 27);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(907, 178);
      this.groupBox1.TabIndex = 9;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Reference numbers";
      // 
      // AllRefsCombo
      // 
      this.AllRefsCombo.AllowDrop = true;
      this.AllRefsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.AllRefsCombo.FormattingEnabled = true;
      this.AllRefsCombo.Items.AddRange(new object[] {
            "Custom",
            "None",
            "Computer Standard",
            "IRL Standard (½ Chinese)",
            "Programmer A-Z",
            "Programmer 1-19",
            "Programmer 0-18",
            "Cartesian"});
      this.AllRefsCombo.Location = new System.Drawing.Point(231, 0);
      this.AllRefsCombo.MaxDropDownItems = 4;
      this.AllRefsCombo.Name = "AllRefsCombo";
      this.AllRefsCombo.Size = new System.Drawing.Size(329, 33);
      this.AllRefsCombo.TabIndex = 10;
      this.AllRefsCombo.SelectedIndexChanged += new System.EventHandler(this.AllRefsCombo_SelectedIndexChanged);
      // 
      // ShowSeqCheckBox
      // 
      this.ShowSeqCheckBox.AutoSize = true;
      this.ShowSeqCheckBox.Location = new System.Drawing.Point(87, 235);
      this.ShowSeqCheckBox.Name = "ShowSeqCheckBox";
      this.ShowSeqCheckBox.Size = new System.Drawing.Size(243, 29);
      this.ShowSeqCheckBox.TabIndex = 10;
      this.ShowSeqCheckBox.Text = "Show play sequence";
      this.ShowSeqCheckBox.UseVisualStyleBackColor = true;
      this.ShowSeqCheckBox.CheckedChanged += new System.EventHandler(this.ShowSeqCheckBox_CheckedChanged);
      // 
      // EnableSave
      // 
      this.EnableSave.AutoSize = true;
      this.EnableSave.Location = new System.Drawing.Point(87, 283);
      this.EnableSave.Name = "EnableSave";
      this.EnableSave.Size = new System.Drawing.Size(495, 29);
      this.EnableSave.TabIndex = 11;
      this.EnableSave.Text = "Enable Save (when unchecked Save=Save As)";
      this.EnableSave.UseVisualStyleBackColor = true;
      this.EnableSave.CheckedChanged += new System.EventHandler(this.EnableSave_CheckedChanged);
      // 
      // StartBlack
      // 
      this.StartBlack.AutoSize = true;
      this.StartBlack.Location = new System.Drawing.Point(87, 340);
      this.StartBlack.Name = "StartBlack";
      this.StartBlack.Size = new System.Drawing.Size(251, 29);
      this.StartBlack.TabIndex = 12;
      this.StartBlack.Text = "Start with Black stone\r\n";
      this.StartBlack.UseVisualStyleBackColor = true;
      // 
      // StartAlternating
      // 
      this.StartAlternating.AutoSize = true;
      this.StartAlternating.Location = new System.Drawing.Point(87, 393);
      this.StartAlternating.Name = "StartAlternating";
      this.StartAlternating.Size = new System.Drawing.Size(312, 29);
      this.StartAlternating.TabIndex = 13;
      this.StartAlternating.Text = "Start with Alternating stones";
      this.StartAlternating.UseVisualStyleBackColor = true;
      // 
      // ShowRemovals
      // 
      this.ShowRemovals.AutoSize = true;
      this.ShowRemovals.Location = new System.Drawing.Point(27, 36);
      this.ShowRemovals.Name = "ShowRemovals";
      this.ShowRemovals.Size = new System.Drawing.Size(198, 29);
      this.ShowRemovals.TabIndex = 14;
      this.ShowRemovals.Text = "Show Removals";
      this.ShowRemovals.UseVisualStyleBackColor = true;
      this.ShowRemovals.CheckedChanged += new System.EventHandler(this.ShowRemovals_CheckedChanged);
      // 
      // ShowTerritory
      // 
      this.ShowTerritory.AutoSize = true;
      this.ShowTerritory.Location = new System.Drawing.Point(27, 89);
      this.ShowTerritory.Name = "ShowTerritory";
      this.ShowTerritory.Size = new System.Drawing.Size(183, 29);
      this.ShowTerritory.TabIndex = 15;
      this.ShowTerritory.Text = "Show Territory";
      this.ShowTerritory.UseVisualStyleBackColor = true;
      this.ShowTerritory.CheckedChanged += new System.EventHandler(this.ShowTerritory_CheckedChanged);
      // 
      // ShowConnections
      // 
      this.ShowConnections.AutoSize = true;
      this.ShowConnections.Location = new System.Drawing.Point(27, 139);
      this.ShowConnections.Name = "ShowConnections";
      this.ShowConnections.Size = new System.Drawing.Size(223, 29);
      this.ShowConnections.TabIndex = 16;
      this.ShowConnections.Text = "Show Connections";
      this.ShowConnections.UseVisualStyleBackColor = true;
      this.ShowConnections.CheckedChanged += new System.EventHandler(this.ShowConnections_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.ScoreResult);
      this.groupBox2.Controls.Add(this.ShowNone);
      this.groupBox2.Controls.Add(this.ShowConnections);
      this.groupBox2.Controls.Add(this.ShowTerritory);
      this.groupBox2.Controls.Add(this.ShowRemovals);
      this.groupBox2.Location = new System.Drawing.Point(60, 449);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(652, 186);
      this.groupBox2.TabIndex = 17;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Score results";
      // 
      // ScoreResult
      // 
      this.ScoreResult.Location = new System.Drawing.Point(159, 0);
      this.ScoreResult.Name = "ScoreResult";
      this.ScoreResult.ReadOnly = true;
      this.ScoreResult.Size = new System.Drawing.Size(325, 31);
      this.ScoreResult.TabIndex = 18;
      // 
      // ShowNone
      // 
      this.ShowNone.AutoSize = true;
      this.ShowNone.Checked = true;
      this.ShowNone.CheckState = System.Windows.Forms.CheckState.Checked;
      this.ShowNone.Location = new System.Drawing.Point(356, 36);
      this.ShowNone.Name = "ShowNone";
      this.ShowNone.Size = new System.Drawing.Size(154, 29);
      this.ShowNone.TabIndex = 17;
      this.ShowNone.Text = "Show None";
      this.ShowNone.UseVisualStyleBackColor = true;
      this.ShowNone.CheckedChanged += new System.EventHandler(this.ShowNone_CheckedChanged);
      // 
      // ToolsOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.CloseButton;
      this.ClientSize = new System.Drawing.Size(959, 638);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.StartAlternating);
      this.Controls.Add(this.StartBlack);
      this.Controls.Add(this.EnableSave);
      this.Controls.Add(this.ShowSeqCheckBox);
      this.Controls.Add(this.LeftDirectionCombo);
      this.Controls.Add(this.LeftScriptCombo);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.CloseButton);
      this.Controls.Add(this.OmitLetters);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.TopDirectionCombo);
      this.Controls.Add(this.TopScriptCombo);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.groupBox1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ToolsOptions";
      this.Text = "Options";
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox TopScriptCombo;
    private System.Windows.Forms.ComboBox TopDirectionCombo;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox OmitLetters;
    private System.Windows.Forms.Button CloseButton;
    private System.Windows.Forms.ComboBox LeftDirectionCombo;
    private System.Windows.Forms.ComboBox LeftScriptCombo;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox AllRefsCombo;
    private System.Windows.Forms.CheckBox ShowSeqCheckBox;
    private System.Windows.Forms.CheckBox EnableSave;
    private System.Windows.Forms.CheckBox StartBlack;
    private System.Windows.Forms.CheckBox StartAlternating;
    private System.Windows.Forms.GroupBox groupBox2;
    public System.Windows.Forms.CheckBox ShowRemovals;
    public System.Windows.Forms.CheckBox ShowTerritory;
    public System.Windows.Forms.CheckBox ShowConnections;
    public System.Windows.Forms.TextBox ScoreResult;
    public System.Windows.Forms.CheckBox ShowNone;
  }
}