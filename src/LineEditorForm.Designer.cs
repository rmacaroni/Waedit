namespace waedit
{
    partial class LineEditorForm
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
	    this.components = new System.ComponentModel.Container();
	    this.label = new System.Windows.Forms.Label();
	    this.editPanel = new System.Windows.Forms.Panel();
	    this.cursorBlinkTimer = new System.Windows.Forms.Timer(this.components);
	    this.SuspendLayout();
	    // 
	    // label
	    // 
	    this.label.Location = new System.Drawing.Point(19, 22);
	    this.label.Name = "label";
	    this.label.Size = new System.Drawing.Size(91, 13);
	    this.label.TabIndex = 0;
	    this.label.Text = "String to Edit:";
	    this.label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
	    // 
	    // editPanel
	    // 
	    this.editPanel.BackColor = System.Drawing.SystemColors.Window;
	    this.editPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
	    this.editPanel.Location = new System.Drawing.Point(116, 13);
	    this.editPanel.Name = "editPanel";
	    this.editPanel.Size = new System.Drawing.Size(257, 30);
	    this.editPanel.TabIndex = 1;
	    this.editPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.editPanel_Paint);
	    // 
	    // cursorBlinkTimer
	    // 
	    this.cursorBlinkTimer.Interval = 500;
	    this.cursorBlinkTimer.Tick += new System.EventHandler(this.cursorBlinkTimer_Tick);
	    // 
	    // LineEditorForm
	    // 
	    this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	    this.ClientSize = new System.Drawing.Size(392, 35);
	    this.Controls.Add(this.editPanel);
	    this.Controls.Add(this.label);
	    this.Name = "LineEditorForm";
	    this.Text = "LineEditorForm";
	    this.TopMost = true;
	    this.Load += new System.EventHandler(this.LineEditorForm_Load);
	    this.ResumeLayout(false);

	}

	#endregion

	private System.Windows.Forms.Label label;
	private System.Windows.Forms.Panel editPanel;
	private System.Windows.Forms.Timer cursorBlinkTimer;
    }
}