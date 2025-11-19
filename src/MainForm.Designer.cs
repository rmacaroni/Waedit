namespace waedit
{
    partial class MainForm
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
	    this.cursorBlinkTimer = new System.Windows.Forms.Timer(this.components);
	    this.statusStrip = new System.Windows.Forms.StatusStrip();
	    this.menuLabel = new System.Windows.Forms.ToolStripStatusLabel();
	    this.stateLabel = new System.Windows.Forms.ToolStripStatusLabel();
	    this.lineLabel = new System.Windows.Forms.ToolStripStatusLabel();
	    this.columnLabel = new System.Windows.Forms.ToolStripStatusLabel();
	    this.editorPanel = new System.Windows.Forms.Panel();
	    this.statusStrip.SuspendLayout();
	    this.SuspendLayout();
	    // 
	    // cursorBlinkTimer
	    // 
	    this.cursorBlinkTimer.Interval = 500;
	    this.cursorBlinkTimer.Tick += new System.EventHandler(this.cursorBlinkTimer_Tick);
	    // 
	    // statusStrip
	    // 
	    this.statusStrip.Font = new System.Drawing.Font("Tahoma", 8F);
	    this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLabel,
            this.stateLabel,
            this.lineLabel,
            this.columnLabel});
	    this.statusStrip.Location = new System.Drawing.Point(0, 550);
	    this.statusStrip.Name = "statusStrip";
	    this.statusStrip.Size = new System.Drawing.Size(781, 23);
	    this.statusStrip.TabIndex = 0;
	    this.statusStrip.Text = "statusStrip1";
	    // 
	    // menuLabel
	    // 
	    this.menuLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
	    this.menuLabel.Name = "menuLabel";
	    this.menuLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
	    this.menuLabel.Size = new System.Drawing.Size(560, 18);
	    this.menuLabel.Spring = true;
	    this.menuLabel.Text = "Again   Buffer   Delete   Execute   Find   -find   Get   Insert   Jump   Tab for " +
		"more ...";
	    this.menuLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	    // 
	    // stateLabel
	    // 
	    this.stateLabel.AutoSize = false;
	    this.stateLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
	    this.stateLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
	    this.stateLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
	    this.stateLabel.Name = "stateLabel";
	    this.stateLabel.Size = new System.Drawing.Size(75, 18);
	    this.stateLabel.Text = "Command";
	    // 
	    // lineLabel
	    // 
	    this.lineLabel.AutoSize = false;
	    this.lineLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
	    this.lineLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
	    this.lineLabel.Name = "lineLabel";
	    this.lineLabel.Size = new System.Drawing.Size(72, 18);
	    this.lineLabel.Text = "Line 1";
	    // 
	    // columnLabel
	    // 
	    this.columnLabel.AutoSize = false;
	    this.columnLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
	    this.columnLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
	    this.columnLabel.Name = "columnLabel";
	    this.columnLabel.Size = new System.Drawing.Size(59, 18);
	    this.columnLabel.Text = "Col 0";
	    // 
	    // editorPanel
	    // 
	    this.editorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
	    this.editorPanel.Location = new System.Drawing.Point(0, 0);
	    this.editorPanel.Name = "editorPanel";
	    this.editorPanel.Size = new System.Drawing.Size(781, 550);
	    this.editorPanel.TabIndex = 1;
	    this.editorPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.editorPanel_Paint);
	    this.editorPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorPanel_MouseDown);
	    // 
	    // MainForm
	    // 
	    this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	    this.ClientSize = new System.Drawing.Size(781, 573);
	    this.Controls.Add(this.editorPanel);
	    this.Controls.Add(this.statusStrip);
	    this.Name = "MainForm";
	    this.Text = "Waedit";
	    this.Load += new System.EventHandler(this.MainForm_Load);
	    this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
	    this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
	    this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
	    this.Resize += new System.EventHandler(this.MainForm_Resize);
	    this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
	    this.statusStrip.ResumeLayout(false);
	    this.statusStrip.PerformLayout();
	    this.ResumeLayout(false);
	    this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.Timer cursorBlinkTimer;
	private System.Windows.Forms.StatusStrip statusStrip;
	private System.Windows.Forms.Panel editorPanel;
	private System.Windows.Forms.ToolStripStatusLabel menuLabel;
	private System.Windows.Forms.ToolStripStatusLabel stateLabel;
	private System.Windows.Forms.ToolStripStatusLabel lineLabel;
	private System.Windows.Forms.ToolStripStatusLabel columnLabel;
    }
}

