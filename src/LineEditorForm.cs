/* ////////////////////////////////////////////////////////////////////////////
			       LineEditorForm.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This class implements a form containing a label and a line
		editor.  It'll be useful for grabbing various input strings in
		Waedit.

REVISIONS:	13 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace waedit
{
public partial class LineEditorForm : Form
{

/* ////////////////////////////////////////////////////////////////////////////
			       Names for Numbers
//////////////////////////////////////////////////////////////////////////// */

const int FONT_HEIGHT = 13;

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

LineEditor	le;
Font		font;
Graphics	g;
Point		point;
int		cellWidth;
bool		cursorIsShowing;
int		charsOnScreen;
int		leftCharacter;
SolidBrush	brush;
MainForm	mainForm;

/* ////////////////////////////////////////////////////////////////////////////
			  Constructor and Destructor
//////////////////////////////////////////////////////////////////////////// */

public LineEditorForm(string labelString, string initialString, string okChars,
		      MainForm mf, LineEditor ed)
{
    mainForm = mf;
    le = ed;
    InitializeComponent();
    label.Text = labelString;
    g = editPanel.CreateGraphics();
    font = new Font (
	new FontFamily("Lucida Console"),
	FONT_HEIGHT, FontStyle.Regular,
	GraphicsUnit.Pixel);
    cellWidth = TextRenderer.
        MeasureText(g, ".", font, new Size(10000, 10000),
        TextFormatFlags.NoPadding).Width + 1;
    charsOnScreen = editPanel.Size.Width / cellWidth - 1;
    point.X = 0;
    point.Y = ((editPanel.Size.Height - FONT_HEIGHT) / 2) - 2;
    brush = new SolidBrush(Color.White);
}						// End LineEditorForm()

/* ///////////////////////////////////////////////////////////////////////// */

~LineEditorForm()
{
    g.Dispose();
    font.Dispose();
    brush.Dispose();
}

/* ////////////////////////////////////////////////////////////////////////////
			       editPanel_Paint()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The framework calls this function as needed to update the
                panel.  Since there's no editing involved, we don't need to
                mess with the cursor as does Edit(), above.

REVISIONS:	14 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

private void editPanel_Paint(object sender, PaintEventArgs e)
{
    UpdatePanel();
}

/* ////////////////////////////////////////////////////////////////////////////
				 UpdatePanel()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Edit() and editPanel_Paint(), above, call this function to
		paint the string into the panel on the line editor form.

REVISIONS:	14 Aug 16 - RAC - Nearly identical to the old EditGuts()
				   function
//////////////////////////////////////////////////////////////////////////// */

private void UpdatePanel()
{
    int i;
    int	x;
    string s;

/*  Scroll horizontally if necessary to keep the cursor visible  */

    if (le.GetCursorPosition() < leftCharacter)
    {
	leftCharacter = le.GetCursorPosition();
    }
    else if (le.GetCursorPosition() > leftCharacter + (charsOnScreen - 1))
    {
	leftCharacter = le.GetCursorPosition() - (charsOnScreen - 1);
    }

/*  Another instance of the anti-blinking kludge.  */

    TextRenderer.DrawText(g, "\\", font, new Point(editPanel.Size.Width - 10,
        editPanel.Size.Height - 10), Color.White, Color.White);

/*  Display the characters in the string one by one.  We do this (instead of
    rendering the entire string at once) in order to replace any non-printable
    characters with something that will display reasonably.  */

    x = 0;					// Start at left edge of panel
    for (i=leftCharacter;			// For each character in string
        i<le.GetStringWithBlank().Length; i++)
    {
	point.X = x++ * cellWidth;		// Calculate X position
	if (point.X > editPanel.Size.Width)	// Stop if we go beyond the
	{					//  right side of the panel
	    break;
	}
	s = le.GetStringWithBlank().		// Get character to display
	    Substring(i, 1);

	if ((s[0] < ' ') || (s[0] > '~'))	// Replace with | if non-
	{					//  printable
	    s = "|";
	}
	
        TextRenderer.DrawText(g, s, font,	// Draw the character
            point, Color.Black, Color.White,
            TextFormatFlags.Default |
            TextFormatFlags.NoPrefix);
    }						// End 'for each character'

    point.X += cellWidth;			// Erase area to the right of
    g.FillRectangle(brush, point.X, point.Y,	//  the text
        editPanel.Size.Width - point.X,
        FONT_HEIGHT);
}						// End Update Panel

/* ////////////////////////////////////////////////////////////////////////////
			     CreateParams Override
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Override of CreateParams to make the form unfocusable.

REVISIONS:	13 Aug 16 - RAC - Copied from an earlier test program
//////////////////////////////////////////////////////////////////////////// */

const int WS_THICKFRAME    = 0x00040000;
const int WS_CHILD	   = 0x40000000;
const int WS_EX_NOACTIVATE = 0x08000000;
const int WS_EX_TOOLWINDOW = 0x00000080;

protected override CreateParams CreateParams
{
    get
    {
        CreateParams rv = base.CreateParams;

        rv.Style = WS_THICKFRAME | WS_CHILD;
        rv.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;

        return rv;
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

private void cursorBlinkTimer_Tick(object sender, EventArgs e)
{
    PaintCursor(cursorIsShowing ? Color.White : Color.Black);
}

/* ///////////////////////////////////////////////////////////////////////// */

void PaintCursor(Color color)
{
    Pen         pen;
    int         x, y;

    pen = new Pen(color);
    pen.Width = 2;
    x = ((le.GetCursorPosition() - leftCharacter) * cellWidth) + 3;
    y = point.Y + FONT_HEIGHT + 1;
    g.DrawLine(pen, x, y, x + cellWidth, y);
    cursorIsShowing = !cursorIsShowing;
    pen.Dispose();
}						// End PaintCursor()

/* ///////////////////////////////////////////////////////////////////////// */

private void LineEditorForm_Load(object sender, EventArgs e)
{
    cursorBlinkTimer.Enabled = true;
    Left = mainForm.Left + (mainForm.Width / 2) - (Width / 2);
    Top = mainForm.Top + (mainForm.Height / 2) - (Height / 2);
}

/* ////////////////////////////////////////////////////////////////////////////
				 StopCursor()
				 StartCursor()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	StopCursor() stops the cursor so we can mess with the line
		editor in peace.  StartCursor() fires it back up and then
                updates the display to show whatever changed.

REVISIONS:	 9 Sep 16 - RAC - Adapted from the old Edit() function
//////////////////////////////////////////////////////////////////////////// */

delegate void Delegate();

public void StopCursor()
{
    if (InvokeRequired)
    {
	Delegate d = new Delegate(StopCursor);
        Invoke(d);
    }
    else
    {
	cursorBlinkTimer.Stop();		// Erase the cursor
	PaintCursor(Color.White);
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

public void StartCursor()
{
    if (InvokeRequired)
    {
	Delegate d = new Delegate(StartCursor);
        Invoke(d);
    }
    else
    {
	UpdatePanel();				// Go update the panel
	PaintCursor(Color.Black);		// Restore the cursor
	cursorIsShowing = true;
	cursorBlinkTimer.Start();
    }
}


}						// End class LineEditorForm
}						// End namespace letest

