/* ////////////////////////////////////////////////////////////////////////////
                                  display.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    Here's where we keep the screen image and all the code related
                to drawing the screen.  Note that this is part of the MainForm
                class so we can easily get the client area size, a device
                context, etc.

NOTE:           As used throughout this module, the term "screen" refers to the
                panel containing the editor window.

REVISIONS:       2 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace waedit
{

class LineImage
{
    public List<byte>   expandedText = new List<byte>();
    public List<int>    screenToTextMap = new List<int>();
    public List<int>    textToScreenMap = new List<int>();
    public List<byte>   style = new List<byte>();
    public bool         stylesChange = false;
}                                               // End class LineImage

partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
                               Names for Numbers
//////////////////////////////////////////////////////////////////////////// */

const byte      WHITE    = 0;           // Character style values
const byte      BAR      = 1;
const byte      SELECTED = 2;

Color           NORMAL_TEXT   = Color.Black;
Color           SELECTED_TEXT = Color.White;
Color           NORMAL_BGND   = Color.White;
Color           BAR_BGND      = Color.FromArgb(225, 225, 255);
Color		MAC_BGND      = Color.FromArgb(245, 225, 255);
Color           SELECTED_BGND = Color.FromArgb( 10,  36, 106);
Color           TEXT_LIMIT    = Color.FromArgb(200, 200, 255);
Color           CURSOR	      = Color.Red;

Color GetBarColor() { return RecordingMacro() ? MAC_BGND : BAR_BGND; }

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

List<LineImage> screenImage = new List<LineImage>();	// The screen image

object screenImageLock = new object();  /* Guards against concurrent calls to
					    BuildScreenImage() */

bool	cursorIsShowing;	// Flag that toggles to blink the cursor

Font	font;			// The current font

int	cellHeight = 14;        // Character height, in pixels
int	cellWidth = 8;          // Character width, in pixels
int	charsOnScreen = 0;	// Horizontal screen size, in character widths
int	linesOnScreen = 0;	// Vertical screen size, in character heights

int	topLine = 0;            // Index of topmost line on screen
int     leftCharacter = 0;      // Index of leftmost char on screen

int	cursorX = 0;		// Cursor X relative to screen's left edge
int	currentLine = 0;	// Cursor Y relative to beginning of the file

/* ////////////////////////////////////////////////////////////////////////////
                            cursorBlinkTimer_Tick()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    The framework calls this function at regular intervals to blink
                the cursor.

REVISIONS:       5 Aug 16 - RAC - Genesis, with hints from the similar function
                                   in the original Waedit.
		25 Sep 16 - RAC - Fixed so the cursor thickness and position
				   track the font size better.
//////////////////////////////////////////////////////////////////////////// */

private void cursorBlinkTimer_Tick(object sender, EventArgs e)
{
    PaintCursor(cursorIsShowing ? Color.White : CURSOR);
}                                               // End cursorBlinkTimer_Tick()

void PaintCursor(Color color)
{
    Pen         pen;
    int         x, y;
    Graphics	g;

    g = editorPanel.CreateGraphics();
    pen = new Pen(color);
    pen.Width = Math.Max((cellHeight + 9) / 14, 2);
    x = (cursorX * cellWidth) + (cellHeight + 4) / 6;
    y = ((currentLine - topLine + 1) * cellHeight) + 1;
    g.DrawLine(pen, x, y, x + cellWidth, y);
    cursorIsShowing = !cursorIsShowing;
    pen.Dispose();
    g.Dispose();
}						// End PaintCursor()

/* ////////////////////////////////////////////////////////////////////////////
                           editorPanel_MouseWheel()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function handles mouse wheel events.  For each one, we
		simulate a few up or down arrow keypresses depending on the
                direction of the wheel movement.

REVISIONS:	28 Sep 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

private void editorPanel_MouseWheel(object sender, MouseEventArgs e)
{
    int i;				// Lines to scroll per wheel detent

    i = SystemInformation.MouseWheelScrollLines * Math.Abs(e.Delta) / 120;

    if (e.Delta > 0)
    {
	DoNullUp(Math.Max(i, i + currentLine - topLine));
    }
    else
    {
	DoNullDown(Math.Max(i, i + topLine + linesOnScreen- currentLine - 1));
    }
    BuildScreenImage();
    UpdateDisplay();
}						// End editorPanel_MouseWheel()

/* ////////////////////////////////////////////////////////////////////////////
                            editorPanel_MouseDown()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The framework calls this function whenever a the mouse is
                right-clicked inside the editor panel.  With any luck, we can
                respond by moving the cursor to the clicked-on character.

REVISIONS:	27 Sep 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

private void editorPanel_MouseDown(object sender, MouseEventArgs e)
{
    int cellX;		// 0-indexed X coordinate of clicked-on character cell
    int cellY;		// 0-indexed Y coordinate of clicked-on character cell
    int scrX;

    if (e.Button != MouseButtons.Right) return;

/*  Convert from mouse coordinates to character cell coordinates.  */

    cellX = (e.X - ((cellHeight + 4) / 6)) / cellWidth;
    cellY = e.Y / cellHeight;

/*  Thwart attempts to move the cursor beyond the end of the text.  */

    currentLine = cellY + topLine;
    currentLine = Math.Min(currentLine, text.GetLineCount() - 1);

/*  Find the new character position within the line, then the character
    position within the text buffer.  */

    scrX = ScreenToTextX(currentLine - topLine, cellX + leftCharacter);
    currentChar = scrX + text.GetLineStart(currentLine);

    UpdateCursor();				// More bureaucracy
    UpdateDisplay();				// Proudly show your work
}						// End editorPanel_MouseDown()

/* ////////////////////////////////////////////////////////////////////////////
                              editorPanel_Paint()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    The framework calls this function whenever it needs to paint
                the panel.

REVISIONS:       2 Aug 16 - RAC - Genesis
		 8 Aug 16 - RAC - Enhanced to support horizontal scrolling
		10 Aug 16 - RAC - Changed so we now paint the panel instead of
				   the form
//////////////////////////////////////////////////////////////////////////// */

private void editorPanel_Paint(object sender, PaintEventArgs e)
{
    PaintScreen();
}						// End MainForm_FormClosed()

/* ///////////////////////////////////////////////////////////////////////// */

void PaintScreen()
{
    int         line;
    Point       point = new Point();
    int         x;
    Color       textColor = NORMAL_TEXT;
    Color       backgroundColor = NORMAL_BGND;
    SolidBrush  brush;
    Pen         pen;
    List<byte>	clippedText;
    List<byte>	clippedStyle;
    int		rightCount;
    Graphics	g;

    g = editorPanel.CreateGraphics();

/*  The following line is a royal kludge to keep the first line from blinking
    if it's been more than about 100-200 msec since the last call to this
    function.  I have no idea why this is necessary or why it works.  */

    TextRenderer.DrawText(g, "\\", font, new Point(editorPanel.Size.Width - (cellHeight / 3),
        editorPanel.Size.Height - (cellHeight / 3)), Color.White, Color.White);

    lock (screenImageLock)
    {
        for (line=0; line<screenImage.Count; line++) // For each line of text
        {

        /*  Here we make copies of this line's 'style' and 'expandedText' lists
            with the parts that extend beyond the left edge of the screen
            clipped off.  While we're at it, we also clip off anything that
            would extend beyond the right edge of the screen!  */

            rightCount = Math.Min(screenImage[line].style.Count -
                leftCharacter, charsOnScreen);
	    
            if (rightCount <= 0)		// If the entire line is cut
	    {					//  off, both lists are empty
		clippedText = new List<byte>();
		clippedStyle = new List<byte>();
	    }
	    else				// Otherwise, clip off the left
	    {					//  part of the original lists 
                clippedText  = screenImage[line].expandedText.
                    GetRange(leftCharacter, rightCount);
                clippedStyle = screenImage[line].style.
                    GetRange(leftCharacter, rightCount);
	    }
        
            if (screenImage[line].stylesChange) // If style changes within the
            {                                   //  line, display char by char
                x = 0;                          // Count characters here
                point.Y = line * cellHeight;    // Calculate Y position
                StyleToColors (                 // In case clippedText is empty
                    screenImage[line].style[0],
                    ref textColor,
                    ref backgroundColor);
                foreach (byte c in clippedText)	// For each character on line
                {
                    point.X = x * cellWidth;    // Calculate X position
                    if (point.X >               // Stop if we run off the right
                        editorPanel.Size.Width) //  side of the screen
                    {
                        break;
                    }
    
                    StyleToColors (             // Go set up colors
                        clippedStyle[x++],
                        ref textColor,
                        ref backgroundColor);
        
                    TextRenderer.DrawText(g,    // Render the character
                        System.Text.Encoding.ASCII.
                        GetString(new[]{c}), font,
                        point, textColor,
                        backgroundColor,
                        TextFormatFlags.Default |
                        TextFormatFlags.NoPrefix);
                }                               // End 'for each character'
            }                                   // End 'style changes'
    
            else                                // All characters have the same
            {                                   //  style, so we can display
                point.X = 0;                    //  the entire line with one
                point.Y = line * cellHeight;    //  call to DrawText()
    
                StyleToColors (                 // Go set up colors
                    screenImage[line].style[0],
                    ref textColor,
                    ref backgroundColor);
    
                TextRenderer.DrawText(g, System.Text.Encoding.ASCII.
                    GetString(clippedText.ToArray()), font, point, textColor,
                    backgroundColor, TextFormatFlags.Default |
                    TextFormatFlags.NoPrefix);
    
                point.X = (clippedText.Count - 1) * cellWidth;
            }                                   // End 'chars have same style'
    
        /*  Fill area to the right of this line.  */

            backgroundColor = (((line + topLine) / 3) % 2 != 0) ?  NORMAL_BGND
                : GetBarColor();
            brush = new SolidBrush(backgroundColor);
            point.X +=  cellWidth + (cellHeight + 4) / 6;
            g.FillRectangle(brush,
                point.X, point.Y, editorPanel.Size.Width - point.X, cellHeight);

        /*  Fill small area to the left of this line.  */

	    point.X = 0;
            g.FillRectangle(brush, point.X, point.Y, (cellHeight + 4) / 6,
                cellHeight);
            brush.Dispose();
        }                                       // End 'for each line of text'
    }                                           // End lock

/*  Paint alignment bars in the screen area below the last line of text.  */

    do
    {
        backgroundColor = (((line + topLine) / 3) % 2 != 0) ?  NORMAL_BGND :
            GetBarColor();
        brush = new SolidBrush(backgroundColor);
        g.FillRectangle(brush,  2, line * cellHeight, editorPanel.Size.Width,
            cellHeight);
        line++;
        brush.Dispose();
    } while (line * cellHeight < editorPanel.Size.Height);

/*  Draw a vertical guide line for character position 79.  */

    pen = new Pen(TEXT_LIMIT);
    pen.Width = 1;
    g.DrawLine(pen, (79 - leftCharacter) * cellWidth + 3, 0,
                    (79 - leftCharacter) * cellWidth + 3,
                    editorPanel.Size.Height);
    g.DrawLine(pen, (134 - leftCharacter) * cellWidth + 3, 0,
                    (134 - leftCharacter) * cellWidth + 3,
                    editorPanel.Size.Height);

    pen.Dispose();
    g.Dispose();
}                                               // End PaintScreen()

/* ////////////////////////////////////////////////////////////////////////////
                                StyleToColors()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function sets up the text and background colors for a
                specified style.

REVISIONS:       3 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void StyleToColors (byte style, ref Color textColor, ref Color bkgdColor)
{
    switch (style)
    {
        case WHITE:
            textColor = NORMAL_TEXT;
            bkgdColor = NORMAL_BGND;
        break;

        case BAR:
            textColor = NORMAL_TEXT;
            bkgdColor = GetBarColor();
        break;

        case SELECTED:
            textColor = SELECTED_TEXT;
            bkgdColor = SELECTED_BGND;
        break;

        default:
            throw new Exception("Invalid style in StyleToColors()");
        /* break; */                // Warns of "unreachable code"
    }
}

/* ////////////////////////////////////////////////////////////////////////////
			       MainForm_Resize()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The framework calls this whenever the main form is resized.  We
		respond by rebuilding the screen image to correspond to the new
		are we have for painting.

REVISIONS:	 18 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

private void MainForm_Resize(object sender, EventArgs e)
{
    BuildScreenImage();
}						// End MainForm_FormResize()

/* ////////////////////////////////////////////////////////////////////////////
                               BuildLineImage()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function builds and returns a LineImage object for a
                specified line.

NOTE:           Right now we've just hard-coded tab stops at every 8th
                character position.  This will need some adjustment someday
                when we have a real table of tab stops somewhere.

REVISIONS:       2 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

LineImage BuildLineImage(int line)
{
    int         iIn, iOut;                      // Byte indexes
    byte        style;
    int         textIndex;
    byte        firstStyle = 0;
    int		tabStop;
    List<byte>	raw;

    LineImage li = new LineImage();             // Make a new line image

    raw = GetLine(line);
    iOut = 0;
    for (iIn=0; iIn<raw.Count; iIn++)           // For each input character
    {
        textIndex = text.GetLineStart(line) +
            iIn;

    /*  Set the style for this character according to whether or not it's in
        selected text and whether or not it happens to be on an alignment bar.
        */

        if (Selecting() &&
	    (textIndex >= SelectionStartPoint()) &&
            (textIndex <  SelectionEndPoint()))
        {
            style = SELECTED; 
        }
        else if ((line / 3) % 2 != 0)
        {
            style = WHITE;
        }
        else
        {
            style = BAR;
        }

    /*  This bit tracks whether the line contains any style changes.  If it
        does, we have to paint it character by character.  Otherwise, we can
        paint the entire line with one call.  */

        if (iIn == 0)
        {
            firstStyle = style;
        }
        else
        {
            if (style != firstStyle)
            {
                li.stylesChange = true;
            }
        }

        li.screenToTextMap.Add(iIn);
        li.textToScreenMap.Add(iOut++);
        li.style.Add(style);

        switch (raw[iIn])
        {
            case C.TAB:
                li.expandedText.Add((byte)' ');
		tabStop = GetTabStop(iOut-1);
		while (iOut < tabStop)
		{
                    li.screenToTextMap.Add(iIn + 1);
                    li.expandedText.Add((byte)' ');
                    li.style.Add(style);
                    iOut++;
		}
            break;

            case C.EOL:
                li.expandedText.Add((byte)' ');
            break;

            case C.EOF:
                li.expandedText.Add((byte)'|');
            break;

            default:
                li.expandedText.Add(raw[iIn]);
            break;
        }
    }                                           // End 'for each input char'
    return li;
}                                               // End BuildLineImage()

/* ////////////////////////////////////////////////////////////////////////////
                              BuildScreenImage()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function builds an image of the area of the screen to be
                displayed.  This image contains:

                  - The text itself, with tabs expanded

                  - Arrays that map the characters' on-screen positions with
                    their positions in the text buffer, in both directions.

                  - Arrays that indicate how each character should be rendered
                    (i.e., whether it should be 1) normal, 2) normal with
                    alignment bar background, or 3) selected).

REVISIONS:       2 Aug 16 - RAC - Skeleton
//////////////////////////////////////////////////////////////////////////// */

public void BuildScreenImage()
{
    int         line;                           // Screen line, 0 at top

    lock (screenImageLock)
    {
        screenImage.Clear();                    // Start with a clean slate
        linesOnScreen =                         // See how many lines will fit
           editorPanel.Size.Height/cellHeight;	//  on the screen
	charsOnScreen =
	    editorPanel.Size.Width / cellWidth - 1;
        line = 0;                               // Start at the top screen line
        while ((line < linesOnScreen) &&        // Loop until the screen is
               (line + topLine <                //  full or we run out of lines
                    text.GetLineCount()))
        {
            screenImage.Add(BuildLineImage(line + topLine));
            line++;
        }                                       // End 'loop until screen full'
    }                                           // End lock
}                                               // End BuildScreenImage()

/* ////////////////////////////////////////////////////////////////////////////
				  UpdateFont()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function is called at startup and thereafter whenever the
		font size changes.  It updates a shared Font object accordingly
		along with the font width.

REVISIONS:	18 Aug 16 - RAC - Extracted from PaintScreen()
//////////////////////////////////////////////////////////////////////////// */

public void UpdateFont()
{
    Size size = new Size(10000, 10000);
    Graphics g = editorPanel.CreateGraphics();

    font = new Font (                           // Cook up a new font
        new FontFamily("Lucida Console"),
        cellHeight,
        FontStyle.Regular,
        GraphicsUnit.Pixel);

    cellWidth = TextRenderer.			// Find the width of one
        MeasureText(g, ".", font, size,		//  character
        TextFormatFlags.NoPadding).Width;

    g.Dispose();
}

/* ////////////////////////////////////////////////////////////////////////////
				ScreenToTextX()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	For a given horizontal position within a given tabs-expanded
                line, this function returns index to the corresponding
                character within the line in the text buffer.

REVISIONS:	 7 Aug 16 - RAC - Genesis
		 8 Aug 16 - RAC - Rewrote for added beauty
//////////////////////////////////////////////////////////////////////////// */

int ScreenToTextX(int line, int x)
{
    x = Math.Min(x, screenImage[line].screenToTextMap.Count - 1);
    return screenImage[line].screenToTextMap[x];
}

/* ////////////////////////////////////////////////////////////////////////////
			       UpdateDisplay()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The executor thread calls this function after every command to
                show the command results.

REVISIONS:	 6 Aug 16 - RAC - Genesis, with help from the internet on the
				   Invoke nonsense.  See this in particular:
				   https://msdn.microsoft.com/en-us/library/
				       ms171728(v=vs.110).aspx
//////////////////////////////////////////////////////////////////////////// */

delegate void UpdateDisplayCallback();

public void UpdateDisplay()
{
    if (InvokeRequired)
    {
	UpdateDisplayCallback d = new UpdateDisplayCallback(UpdateDisplay);
	Invoke(d);
    }
    else
    {
	PaintScreen();

	cursorBlinkTimer.Stop();
	PaintCursor(CURSOR);
	cursorIsShowing = true;
	cursorBlinkTimer.Start();

	UpdateStatusBar();
	UpdateTitleBar();
    }
}

/* ////////////////////////////////////////////////////////////////////////////
                               UpdateTitleBar()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	What it says.

REVISIONS:	18 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void UpdateTitleBar()
{
    if (currentFilename == "")
    {
	this.Text = "Waedit - Untitled";
    }
    else
    {
	this.Text = "Waedit - " + currentFilename;
    }
}

/* ////////////////////////////////////////////////////////////////////////////
			       UpdateStatusBar()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	What it says.

REVISIONS:	11 Aug 16 - RAC - Genesis, with the menu strings yanked from
				   the original Waedit source code.
//////////////////////////////////////////////////////////////////////////// */

void UpdateStatusBar()
{
/*  Set up the cursor position indicators  */

    lineLabel.Text = String.Format("Line {0}", currentLine + 1);
    columnLabel.Text = String.Format("Col {0}", leftCharacter + cursorX);

/*  Set up the menu prompt  */

    menuLabel.Text = currentPrompt;

/*  Set up the state indicator  */

    if (crs.Peek().countString != "")
    {
	stateLabel.Text = crs.Peek().countString;
    }
    else
    {
	switch (crs.Peek().currentState)
        {
	    case ST.COMMAND:    stateLabel.Text = "Command";	break;
	    case ST.INSERT:	stateLabel.Text = "Insert";	break;
	    case ST.EXCHANGE:	stateLabel.Text = "Exchange";	break;

        }
    }   
}

}                                               // End class MainForm
}                                               // End namespace waedit

