/* ////////////////////////////////////////////////////////////////////////////
				 executors.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Here are all the routines that actually do something to the
		editor.

REVISIONS:	 5 Sep 16 - RAC - Adapted from Revision 1 logic
                 9 Sep 16 - RAC - Revised somewhat to handle the Insert and
                                   Exchange modes as states rather than
                                   commands.
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace waedit
{

partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
			       Names for Numbers
//////////////////////////////////////////////////////////////////////////// */

enum LD                                 // Last direction names
{
    NONE,
    UP,
    DOWN,
    LEFT,
    RIGHT
};

enum DT                                 // Values for 'lastDeleteType'
{
    CTRL_A,
    CTRL_X,
    CTRL_Z
};

public enum ST				// Values for currentState
{
    COMMAND,
    INSERT,
    EXCHANGE
};

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

public bool	again;					// Set by ManageRepeats() to indicate commands invoked via the Again command
public bool	readOnly = false;			// Set true at startup if "TO NUL" follows the filename on the command line
LD          	lastDirection = LD.NONE;		// Direction of last cursor move
bool	    	autoIndent = true;			// Current setting of the "indent" option.
bool        	blanksForTabs = false;			// Ditto for the "notabs" option
public bool	displayMacroExecution = false;		// Ditto for the "display" option
bool        	caseSensitiveSearches = false;		// Ditto for the "case" option
bool        	findOnlyTokenStrings = false;		// Ditto for the "K_token" option
bool		flowchartSupportOn = false;		// Ditto for the "flOw" option
public int	radix = 10;				// Ditto for the "radix" option
DT          	lastDeleteType;				// Used by Ctrl-A, Ctrl-X, Ctrl-Z, and Ctrl-U
public string   findTarget = "";
public string   replaceString = "";
public string	putFilename = "";
public string	macroFilename = "";
public string   getFilename = "";
string	    	newMacroName = "";
static string	delimiterSet = @"!""#%&'()*+,-./:;<=>?@[\]^`{|}~";
List<byte>  	buffer;					// Scratch buffer used by some of the macro-related commands
public string	currentPrompt = mainPromptStrings[0];	// Text to display on the prompt line.
public Stack<CommandRunner> crs =			// See comments for Run() in commandrunners.cs
    new Stack<CommandRunner>();
List<byte>	undoBuffer = new List<byte>();		// Saves text for restoration by backspace in the exchange state
List<byte>	lineBuffer = new List<byte>();		/* Saves text deleted by most recent of Ctrl-A, Ctrl-X, or Ctrl-Z for possible
							    restoration by Ctrl-U.  */
public Dictionary<string, List<byte>> macros =		// Dictionary of currently defined macros.  Uses the macro names for the keys
    new Dictionary<string, List<byte>>();
bool	calcResultPending;				/* Flag that prevents the normal prompt updating mechanism from overwriting
							    results of the Calc command.  */
int currentMainPrompt = 0;				// 0 or 1 to select between the two main prompt strings

static string[] mainPromptStrings = {
    "Again   Buffer   Calc   Delete   Execute   Find   -find   Get   " +
        "Insert   Jump   Macro       Tab for more ...",
    "Paragraph   Quit   Replace   ?Replace   Set   Tag   View   Xchange   " +
        "Y)Redo   Z)Undo       Tab for more ..."
    };

/* ////////////////////////////////////////////////////////////////////////////
				   Execute()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	ManageRepeats() calls this function to execute a given command
                once.  This function returns true if the command was marked as
                failed, or false otherwise.

REVISIONS:	 5 Sep 16 - RAC - Genesis
		 7 Sep 16 - RAC - Revised to work during block selection
		 9 Sep 16 - RAC - Fixed to handle the Insert and Exchange modes
				   as states rather than commands.
//////////////////////////////////////////////////////////////////////////// */

public bool Execute(int herb, int count)
{
    crs.Peek().commandFailure = false;


    DoUndoPrep(ToUpper(herb));
    switch (crs.Peek().currentState)
    {
	case ST.COMMAND:    HandleCommandState(herb, count);	break;
	case ST.INSERT:	    HandleInsertState(herb, count);	break;
	case ST.EXCHANGE:   HandleExchangeState(herb, count);	break;
    }
    ManageFlowchartViewer();
    DoUndoCleanup(ToUpper(herb));

    if (calcResultPending)
    {
	calcResultPending = false;
    }
    else
    {
	if (crs.Peek().currentState == ST.COMMAND)
	{
	    currentPrompt = Selecting() ?  "Buffer Delete Find -find Jump Put" :
	        mainPromptStrings[currentMainPrompt];
	}
	else
	{
	    currentPrompt = "Press Esc for commands";
	}
    }
    return crs.Peek().commandFailure;
}						// End Execute()

/* ///////////////////////////////////////////////////////////////////////// */

/*  The command state has two modes that we'll call "Normal" and "Selecting".
    Normal mode is the one that's active most of the time.  Selecting mode is
    the one that's active following the B or D commands where the user is
    allowed to select a block of text that can be deleted (D), copied to the
    Windows clipboard (B), or saved to a text file (P).

    Some of the commands are availabe only in normal mode, while others behave
    differently depending on the current mode.  The three switch statements in
    HandleCommandState(), below, determine how it all works.  */

void HandleCommandState(int herb, int count)
{
    switch (ToUpper(herb))			// Keys active in both modes
    {
        case C.UP:      DoNullUp(count);        	break;
        case C.DOWN:    DoNullDown(count);      	break;
        case C.LEFT:    DoNullLeft(count);      	break;
        case C.RIGHT:   DoNullRight(count);     	break;
        case C.PGUP:    DoNullPgUp(count);		break;
        case C.PGDN:    DoNullPgDn(count);		break;
        case C.HOME:    DoNullHome(count);		break;
        case C.ENTER:   DoNullReturn(count);		break;
        case '-':
        case 'F':       DoNullF(herb, count);		break;
        case 'J':       DoNullJ();			break;
    }

    if (!Selecting()) switch (ToUpper(herb))	// Normal mode keys
    {
        case C.ESC:     DoNullEscape();         	break;
        case C.CTRL_A:  DoNullCtrlA();          	break;
        case C.CTRL_C:  DoNullCtrlC();          	break;
        case C.CTRL_E:  DoNullE(true, count);		break;
        case C.CTRL_U:  DoNullCtrlU();          	break;
        case C.CTRL_X:  DoNullCtrlX();          	break;
        case C.CTRL_Z:  DoNullCtrlZ();          	break;
        case '\b':      DoNullBackspace();		break;
        case C.DELETE:  DoNullDelete(count);		break;
        case C.TAB:     DoNullTab();            	break;
        case 'B':
        case 'D':       DoNullB();              	break;
	case 'C':	DoNullC();			break;
	case 'E':	DoNullE(false, count);		break;
        case 'G':       DoNullG(count);         	break;
        case 'I':       DoNullI();              	break;
        case 'K':       DoNullK(count);         	break;
	case 'M':	DoNullM();			break;
        case 'P':       DoNullP(count);         	break;
        case 'Q':       DoNullQ();              	break;
        case 'R':
        case '?':       DoNullR(herb, count);   	break;
	case 'S':	DoNullS();			break;
	case 'T':	DoNullT();			break;
	case 'V':	MoveToViewRow();		break;
	case 'X':	DoNullX();			break;
	case 'Y':	DoNullY();			break;
	case 'Z':	DoNullZ();			break;
#if DEBUG
	case C.F1:	DoNullF1();			break;
#endif
	default:	MaybeRunAMacro(herb, count);	break;
    }

    else switch (ToUpper(herb))			// Selecting mode keys
    {
	case C.CTRL_C:	DoSelectCtrlC();		break;
        case C.ESC:	DoSelectEscape();		break;
        case 'B':
        case 'D':
        case 'P':       DoSelectDone(herb);     	break;
    }

}						// End HandleCommandState()

/* ///////////////////////////////////////////////////////////////////////// */

void HandleInsertState(int herb, int count)
{
    switch (ToUpper(herb))
    {
        case C.UP:      DoNullUp(count);        	break;
        case C.DOWN:    DoNullDown(count);      	break;
        case C.LEFT:    DoNullLeft(count);      	break;
        case C.RIGHT:   DoNullRight(count);     	break;
        case C.PGUP:    DoNullPgUp(count);		break;
        case C.PGDN:    DoNullPgDn(count);		break;
        case C.HOME:    DoNullHome(count);		break;
        case C.CTRL_A:  DoNullCtrlA();          	break;
        case C.CTRL_E:  DoNullE(true, count);		break;
        case C.CTRL_U:  DoNullCtrlU();          	break;
        case C.CTRL_X:  DoNullCtrlX();          	break;
        case C.CTRL_Z:  DoNullCtrlZ();          	break;
        case C.DELETE:  DoNullDelete(count);		break;
        case '\b':      DoNullBackspace();      	break;
        case C.ESC:     DoIEscape();            	break;
        case C.ENTER:   DoIReturn();            	break;
        case C.TAB:     DoIDefault(herb);       	break;
#if DEBUG
	case C.F1:					break;
#endif
        default:
            if (herb < C.BLANK)
            {
                MaybeRunAMacro(herb, count);
            }
            else
            {
                DoIDefault(herb);
            }
        break;
    }
}						// End HandleInsertState()

/* ///////////////////////////////////////////////////////////////////////// */

void HandleExchangeState(int herb, int count)
{
    switch (ToUpper(herb))
    {
        case C.UP:      DoNullUp(count);        	break;
        case C.DOWN:    DoNullDown(count);      	break;
        case C.LEFT:    DoNullLeft(count);      	break;
        case C.RIGHT:   DoNullRight(count);     	break;
        case C.PGUP:    DoNullPgUp(count);		break;
        case C.PGDN:    DoNullPgDn(count);		break;
        case C.HOME:    DoNullHome(count);		break;
        case C.CTRL_A:  DoNullCtrlA();          	break;
        case C.CTRL_E:  DoNullE(true, count);		break;
        case C.CTRL_U:  DoNullCtrlU();          	break;
        case C.CTRL_X:  DoNullCtrlX();          	break;
        case C.CTRL_Z:  DoNullCtrlZ();          	break;
        case C.DELETE:  DoNullDelete(count);		break;
        case '\b':      DoXBackspace();         	break;
        case C.ESC:     DoXEscape();            	break;
        case C.ENTER:   DoXReturn();            	break;
        case C.TAB:     DoXDefault(herb);       	break;
#if DEBUG
	case C.F1:					break;
#endif
        default:
            if (herb < C.BLANK)
            {
                MaybeRunAMacro(herb, count);
            }
            else
            {
                DoXDefault(herb);
            }
        break;
    }
}						// End HandleExchangeState()

/* ///////////////////////////////////////////////////////////////////////// */

#if DEBUG
void DoNullF1()
{
    DumpUndoHistory();
}
#endif

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullZ()
{
    Undo();					// Restore from undo history
    UpdateCursor();				// Resore the display
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullY()
{
    Redo();					// Restore from undo history
    UpdateCursor();				// Resore the display
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullI()
{
    crs.Peek().currentState = ST.INSERT;
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullX()
{
    crs.Peek().currentState = ST.EXCHANGE;
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullTab()
{
    if (++currentMainPrompt >= mainPromptStrings.Length)
    {
	currentMainPrompt = 0;
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullUp(int count)
{
    undoBuffer.Clear();
    lastDirection = LD.UP;
    if (count == C.INFINITE)
    {
	currentLine = 0;
    }
    else
    {
	crs.Peek().commandFailure |= (currentLine == 0);
	currentLine -= count;
    }
    FinishVerticalCursorMove(false);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullDown(int count)
{
    undoBuffer.Clear();
    lastDirection = LD.DOWN;
    if (count == C.INFINITE)
    {
	currentLine = text.GetLineCount() - 1;
    }
    else
    {
	crs.Peek().commandFailure |= (currentLine == (text.GetLineCount() - 1));
	currentLine += count;
    }
    FinishVerticalCursorMove(false);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullLeft(int count)
{
    undoBuffer.Clear();
    lastDirection = LD.LEFT;
    MoveLeft(count);
}

/* ///////////////////////////////////////////////////////////////////////// */

void MoveLeft(int count)
{
    if (count == C.INFINITE)
    {
	currentChar = 0;    
    }
    else
    {
	crs.Peek().commandFailure |= (currentChar == 0);
	currentChar -= count;
    }
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullRight(int count)
{
    undoBuffer.Clear();
    lastDirection = LD.RIGHT;
    if (count == C.INFINITE)
    {
	currentChar = text.GetCharCount() - 1;
    }
    else
    {
	crs.Peek().commandFailure |= (currentChar == (text.GetCharCount() - 1));
	currentChar += count;
    }
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullPgUp(int count)
{
    undoBuffer.Clear();
    lastDirection = LD.UP;
    if (count == C.INFINITE)
    {
	currentLine = 0;
    }
    else
    {
	crs.Peek().commandFailure |= (currentLine == 0);
	currentLine -= count * (linesOnScreen - 1);
    }
    FinishVerticalCursorMove(true);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullPgDn(int count)
{
    undoBuffer.Clear();
    lastDirection = LD.DOWN;
    if (count == C.INFINITE)
    {
	currentLine = text.GetLineCount() - 1;
    }
    else
    {
	crs.Peek().commandFailure |= (currentLine == (text.GetLineCount() - 1));
	currentLine += count * (linesOnScreen - 1);
    }
    FinishVerticalCursorMove(true);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullHome(int count)
{
    undoBuffer.Clear();
    switch (lastDirection)
    {
        case LD.LEFT:
            currentChar = text.GetLineStart(currentLine);
            UpdateCursor();
        break;

        case LD.RIGHT:
            currentChar = text.GetLineStart(currentLine + 1) - 1;
            UpdateCursor();
        break;

        case LD.UP:
            DoNullPgUp(count);
        break;

        case LD.DOWN:
            DoNullPgDn(count);
        break;
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullReturn(int count)
{
    byte b;

    DoNullDown((count == C.INFINITE) ? count : count - 1);
    currentChar = text.GetLineStart(currentLine + 1);
    if (autoIndent) {
        if (currentLine < text.GetLineCount() - 1)
        {
            while(true)
            {
                b = GetChar(currentChar);
                if ((b != ' ') && (b != '\t'))
                {
                    break;
                }
                currentChar++;
            }
        }
    }
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullDelete(int count)
{
    PositionWindowHorizontally();
    if (count == C.INFINITE) return;
    text.DeleteBlock(currentChar, Math.Min(count, text.GetCharCount() - 1 - currentChar));
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullBackspace()
{
    if (currentChar > 0)
    {
        MoveLeft(1);
        DeleteChar(currentChar);
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void FinishVerticalCursorMove(bool forceViewRow)
{
    int scrX;

/*  Thwart any attempt to move the cursor beyond the actual text.  */

    currentLine = Math.Max(currentLine, 0);
    currentLine = Math.Min(currentLine, text.GetLineCount() - 1);

/*  Scroll vertically so the cursor is on the screen.  */

    if (forceViewRow)
    {
        MoveToViewRow();
    }
    else
    {
        PositionWindowVertically();
    }

/*  Point 'currentChar' the spot in the text buffer that corresponds to the new
    cursor position.  This helps take care of the oddball situation when the
    cursor lands in the no-man's land following an expanded tab or in the
    no-man's land beyond the end of a line.  */

    BuildScreenImage();
    scrX = ScreenToTextX(currentLine - topLine, cursorX + leftCharacter);
    currentChar = scrX + text.GetLineStart(currentLine);
}

/* ///////////////////////////////////////////////////////////////////////// */

void MoveToViewRow()
{
    topLine = Math.Max(currentLine - (linesOnScreen / 5), 0);
}

/* ///////////////////////////////////////////////////////////////////////// */

const int NEAR_LIMIT = 40;

void PositionWindowVertically()
{
    if ((currentLine < (topLine - NEAR_LIMIT)) ||
        (currentLine > (topLine + linesOnScreen + NEAR_LIMIT)))
    {
        MoveToViewRow();
    }
    else if (currentLine < topLine)
    {
        topLine = currentLine;
    }
    else if (currentLine > topLine + (linesOnScreen - 1))
    {
        topLine = currentLine - (linesOnScreen - 1);
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

public void UpdateCursor()
{

/*  Thwart any attempt to move the cursor beyond the actual text.  */

    currentChar = Math.Max(currentChar, 0);
    currentChar = Math.Min(currentChar, text.GetCharCount() - 1);

/*  Move the cursor to the line that is now current.  */

    currentLine = LineContainingCharacter(currentChar);

/*  Scroll in both directions so the cursor is in view.  */

    PositionWindowVertically();
    PositionWindowHorizontally();
}

/* ///////////////////////////////////////////////////////////////////////// */

void PositionWindowHorizontally()
{    
    int textX;  // Position of current character on line in text buffer
    int expX;   // Position of current character on line with expanded tabs

    BuildScreenImage();

/*  Find where the cursor should be horizontally relative to the expanded text
    in the screen image.  */

    textX = currentChar - text.GetLineStart(currentLine);
    expX = screenImage[currentLine - topLine].textToScreenMap[textX];

/*  Scroll horizontally to bring the cursor into view.  */

    if (expX < leftCharacter)
    {
        leftCharacter = expX;
    }
    else if (expX > leftCharacter + (charsOnScreen - 1))
    {
        leftCharacter = expX - (charsOnScreen - 1);
    }

/*  Set the cursor position relative to the left edge of the screen.  */

    cursorX = expX - leftCharacter;
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullJ()
{
    int herb;

    currentPrompt = "A tag   B tag   C tag   D tag   Start   End   " +
	"Line   Match   Position";
        
    while (true)
    {
	herb = GetInputByte();
        
	switch (ToUpper(herb))
        {
	    case C.ESC:
	    case C.CTRL_C:			return;
            case 'A':
            case 'B':
            case 'C':
            case 'D':		DoJABCD(herb);	return;
	    case 'E':		DoJE();		return;
	    case 'L':		DoJL();		return;
            case 'M':		DoJM();		return;
	    case 'P':		DoJP();		return;
	    case 'S':		DoJS();		return;
        }
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoJS()
{
    currentChar = 0;
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoJE()
{
    currentChar = text.GetCharCount() - 1;
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoJABCD(int herb)
{
    int i;

    i = text.GetTag(ToUpper(herb) - 'A');
    if (i >= 0)
    {
        currentChar = i;
        UpdateCursor();
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoJP()
{
    string      s;
    int         position;

    s = GetUserString("New position: ", "", "0123456789", true);
    if (s.Length > 8)
    {
        s = s.Substring(0, 8);
    }
    if (s != "")
    {
	position = Int32.Parse(s);
	currentChar = text.GetLineStart(currentLine) +
            ScreenToTextX(currentLine - topLine, position);
	PositionWindowHorizontally();
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoJL()
{
    string	s;
    int		line;

    s = GetUserString("New line: ", "", "0123456789", true);

    if (s.Length > 8)
    {
        s = s.Substring(0, 8);
    }
    if (s != "" && (line = Int32.Parse(s)) > 0)
    {
        line = Math.Min(line, text.GetLineCount());
        currentChar = text.GetLineStart(line - 1);
        UpdateCursor();
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoJM()
{
    int		i;				// Text buffer index
    int		upper;				// Nesting level increaser
    int		downer;				// Nesting level decreaser
    int		increment;			// Coded search direction
    int		limit;				// Search limit
    int		nestingLevel;			// Nesting level
    byte	ch;				// A generic character

    switch ((char)GetChar(currentChar))
    {
        case '(':
	    upper     =	'(';
	    downer    =	')';
	    increment =	1;
	    limit     =	text.GetCharCount() - 1;
	break;

	case ')':
	    upper     =	')';
	    downer    =	'(';
	    increment =	-1;
	    limit     =	0;
	break;

	case '[':
	    upper     =	'[';
	    downer    =	']';
	    increment =	1;
	    limit     =	text.GetCharCount() - 1;
	break;

	case ']':
	    upper     =	']';
	    downer    =	'[';
	    increment =	-1;
	    limit     =	0;
	break;

	case '{':
	    upper     =	'{';
	    downer    =	'}';
	    increment =	1;
	    limit     =	text.GetCharCount() - 1;
	break;

	case '}':
	    upper     =	'}';
	    downer    =	'{';
	    increment =	-1;
	    limit     =	0;
	break;

	default:				// Not a matchable character
	    return;
    }

    i = currentChar;				// Start at cursor position
    nestingLevel = 0;				// No nesting detected yet
    while (i != limit)				// While not at "end" of text
    {
	ch = GetChar(i += increment);		// Get next character
	if (ch == upper)
        {					// Adjust nesting level if
	    nestingLevel++;			//  indicated by this character
	}
	if (ch == downer)
        {
	    nestingLevel--;
	}
	if (nestingLevel == -1)
        {					// Found the match!
	    currentChar = i;			// Jump to it
	    UpdateCursor();
	    return;
	}
    }						// End 'not at "end" of text'
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullT()
{
    int herb;

    currentPrompt = "A tag   B tag   C tag   D tag";
        
    while (true)
    {
	herb = GetInputByte();
        
	switch (ToUpper(herb))
        {
	    case C.ESC:
	    case C.CTRL_C:			return;
            case 'A':
            case 'B':
            case 'C':
            case 'D':		DoTABCD(herb);	return;
        }
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoTABCD(int herb)
{
    text.SetTag(ToUpper(herb) - 'A', currentChar);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullQ()
{
    int herb;

    currentPrompt = (currentFilename != "") ?
	"Abort   Exit   Init   Update   Write" :
	"Abort   Init   Write";

    while (true)
    {
	herb = GetInputByte();
	switch (ToUpper(herb))
        {
	    case C.ESC:
	    case C.CTRL_C:			return;
            case 'A':						// Abort
            case 'I':		DoQAandI(herb);	return;		// Init
            case 'W':		DoQW();		return;		// Write
        }
	if (currentFilename != "")
        switch (ToUpper(herb))
        {
            case 'E':						// Exit
            case 'U':		DoQEandU(herb); break;		// Update
        }
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoQAandI(int herb)
{
    string s;

    if (ToUpper(herb) == 'A')
    {
	Close();                                // Close the main form
	return;
    }

    if (text.HaveUnsavedChanges())
    {
        if (MessageBox.Show("Text modified.  Save changes?", "Waedit",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
            DialogResult.Yes)
	{
	    return;
	}
    }

    currentFilename = 
	GetUserString("File: ", "", "", true);
    Initialize();				// Wipe out the current text
    if (File.Exists(currentFilename))		// If the input file exists ...
    {
	s = File.ReadAllText(currentFilename);	// Read it
	s = s.Replace (				// Fix up EOLs
	    DetectLineDelimiter(s), "\r");		
        text.InsertBlock(currentChar,		// Convert to List<byte> and
	    new List<byte> (			//  dump in the text buffer
	    Encoding.ASCII.GetBytes(s)));
    }						// End 'input file exists'
    text.NoUnsavedChangesYet();			// No changes to buffer yet
}						// End DoQAandI()

/* ///////////////////////////////////////////////////////////////////////// */

void DoQW()
{
    currentFilename = GetUserString("Output file: ", "", "", true);
    if (OkayToOverwrite(currentFilename))
    {
	readOnly = false;
	DoQEandU('U');
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

bool OkayToOverwrite(string filename)
{
    string s;
    
    if (!File.Exists(filename))
    {
	return true;
    }	
    s = String.Format("File '{0}' exists.  Overwrite anyway?", filename);
    return (MessageBox.Show(s, "Waedit", MessageBoxButtons.YesNo,
        MessageBoxIcon.Question) == DialogResult.Yes);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoQEandU(int herb)
{
    List<byte>	l;
    string	s;

    if (readOnly)
    {

        s = String.Format("File '{0}' was opened for read only access.\n" +
            "Overwrite anyway?", currentFilename);

	if (MessageBox.Show(s, "Waedit", MessageBoxButtons.YesNo,
	    MessageBoxIcon.Question) == DialogResult.Yes)
        {
	    readOnly = false;
        }
	else
        {
	    return;
        }
    }

    l = text.GetBlock(0, text.GetCharCount() - 1);
    s = Encoding.ASCII.GetString(l.ToArray());
    s = s.Replace("\r", "\r\n");

    try
    {
        File.WriteAllText(currentFilename, s);
    }    
    catch
    {
	s = String.Format("Error writing to file '{0}'", currentFilename);
        MessageBox.Show(s);
        return;
    }
    text.NoUnsavedChangesYet();
    if (ToUpper(herb) == 'E')
    {
	Close();                                // Close the main form
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullS()
{
    currentPrompt = "Case   Display   E_delim   Font size   Indent   K_token   Margins   Notab   flOw   Radix   Tabs";

    while (true)
    {
	switch (ToUpper(GetInputByte()))
	{
	    case C.ESC:
	    case C.CTRL_C:			return;
            case 'C':       DoSC();             return;
            case 'D':       DoSD();             return;
            case 'E':       DoSE();             return;
            case 'F':       DoSF();             return;
            case 'I':       DoSI();             return;
            case 'K':       DoSK();             return;
            case 'M':       DoSM();             return;
            case 'N':       DoSN();             return;
            case 'O':       DoSO();             return;
            case 'R':       DoSR();             return;
            case 'T':       DoST();             return;
	}
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSO()
{
    SetBinaryOption(ref flowchartSupportOn, "Flowchart support on?");
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSD()
{
    SetBinaryOption(ref displayMacroExecution, "Display macro execution?");
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSK()
{
    SetBinaryOption(ref findOnlyTokenStrings, "Find only token strings?");
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSE()
{
    delimiterSet = GetUserString("Delimiter set: ", delimiterSet, "", true);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSR()
{
    currentPrompt =  (radix == 'A') ? "[Alpha]   "   : "Alpha   "; 
    currentPrompt += (radix ==   2) ? "[Binary]   "  : "Binary   "; 
    currentPrompt += (radix ==  10) ? "[Decimal]   " : "Decimal   "; 
    currentPrompt += (radix ==  16) ? "[Hex]   "     : "Hex   "; 
    currentPrompt += (radix ==   8) ? "[Octal]   "   : "Octal   "; 

    while (true)
    {
	switch (ToUpper(GetInputByte()))
        {
	    case C.ESC:
            case C.CTRL_C:		return;
            case 'A':	radix = 'A';	return;
            case 'B':	radix = 2;	return;
            case 'D':	radix = 10;	return;
            case 'H':	radix = 16;	return;
            case 'O':	radix = 8;	return;
        }
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSF()
{
    string	s;
    int		tempFontSize;
    
    s = GetUserString("New font size: ", cellHeight.ToString(), "0123456789",
	true);
    if (Int32.TryParse(s, out tempFontSize) &&
        (tempFontSize > 0) && (tempFontSize <= 200))
    {
        cellHeight = tempFontSize;
        UpdateFont();
    }
    else
    {
        MessageBox.Show("Bad font size.");
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoST()
{
    string	s;

    s = GetUserString("New tab settings: ", GetTabSettings(), "0123456789,",
    	true);
    if (s == "") s = GetTabSettings();
    if (TabsSavedOkay(s))
    {
        PositionWindowHorizontally();
    }
    else
    {
        MessageBox.Show("Bad tabs.");
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSM()
{
    string	s;

    s = GetUserString("Indent, left, right: ", GetMarginSettings(),
	"0123456789,", true);

    if (!MarginsSavedOkay(s))
    {
        MessageBox.Show("Bad margins.");
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSC()
{
    SetBinaryOption(ref caseSensitiveSearches, "Do case-sensitive searches?");
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSN()
{
    SetBinaryOption(ref blanksForTabs, "Insert blanks for tabs?");
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSI()
{
    SetBinaryOption(ref autoIndent, "Automatically indent during insertion?");
}

/* ////////////////////////////////////////////////////////////////////////////
                                 DoNullCtrlA()
                                 DoNullCtrlU()
                                 DoNullCtrlX()
                                 DoNullCtrlZ()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    These are handlers for Ctrl-A (delete to the end of the line),
                Ctrl-X (delete to the beginning of the line), Ctrl-Z (delete
                the entire line), and Ctrl-U (insert the most recent text
                deleted by one of the other three)

REVISIONS:      16 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void DoNullCtrlA()
{
    int         sonl;                           // Index to start of next line
    int         length;                         // Length of deletion

    undoBuffer.Clear();
    lastDeleteType = DT.CTRL_A;
    sonl = text.GetLineStart(currentLine + 1);  // Find start of next line
    length = sonl - currentChar - 1;            // Number of bytes to delete
    lineBuffer =                                // Save the block of stuff that
        text.GetBlock(currentChar, length);     //  we're about to delete
    text.DeleteBlock(currentChar, length);      // Delete it
    PositionWindowHorizontally();               // So cursor displays correctly
}                                               // End DoNullCtrlA()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullCtrlU()
{
    undoBuffer.Clear();
    if (lastDeleteType == DT.CTRL_Z)
    {
        currentChar = text.GetLineStart(currentLine);
    }
    text.InsertBlock(currentChar, lineBuffer);    
    PositionWindowHorizontally();               // So cursor displays correctly
}                                               // End DoNullCtrlU()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullCtrlX()
{
    int         sotl;                           // Index to start of this line
    int         length;                         // Length of deletion

    undoBuffer.Clear();
    lastDeleteType = DT.CTRL_X;
    sotl = text.GetLineStart(currentLine);      // Find start of this line
    length = currentChar - sotl;                // Number of bytes to delete
    lineBuffer = text.GetBlock(sotl, length);   // Save stuff being deleted
    text.DeleteBlock(sotl, length);             // Delete it
    currentChar = sotl;    
    PositionWindowHorizontally();               // So cursor displays correctly
}                                               // End DoNullCtrlX()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullCtrlZ()
{
    int         sotl;                           // Index to start of this line
    int         sonl;                           // Index to start of next line
    int         length;                         // Length of deletion
    int         scrX;

    undoBuffer.Clear();
    lastDeleteType = DT.CTRL_Z;
    sotl = text.GetLineStart(currentLine);      // Find start of this line
    sonl = text.GetLineStart(currentLine + 1);  // Find start of next line
    length = sonl - sotl;                       // Number of bytes to delete
    if (currentLine == text.GetLineCount()-1)   // If we're on the last line,
    {                                           //  don't try to copy an EOL
        length--;                               //  that isn't there, for you
    }                                           //  will be very disappointed
    lineBuffer = text.GetBlock(sotl, length);   // Save stuff being deleted
    text.DeleteBlock(sotl, length);             // Delete it
    BuildScreenImage();
    scrX = ScreenToTextX(currentLine - topLine, cursorX + leftCharacter);
    currentChar = scrX + text.GetLineStart(currentLine);
}                                               // End DoNullCtrlZ()

/* ///////////////////////////////////////////////////////////////////////// */

void SetBinaryOption(ref bool flag, string prompt)
{
    currentPrompt = prompt + (flag ? " ([Y] or N)" : " (Y or [N])");

    switch (ToUpper(GetInputByte()))
    {
	case 'Y':	flag = true;	break;
	case 'N':	flag = false;	break;
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullF(int herb, int count)
{
    int		targetPosition;
    C.FD	direction;
    bool	inputAborted;
    int		attempt;

    if (!again)
    {
	findTarget = GetUserString("Find: ", findTarget, "", false,
	    out inputAborted);
	if (inputAborted)
        {
	    crs.Peek().commandFailure = true;
	    return;
        }
    }
    direction = (herb == '-') ?			// Set direction based on
	C.FD.REVERSE : C.FD.FORWARD;		//  command

    attempt = 0;
    while (true)
    {
	attempt++;
	if (count != C.INFINITE)
        {
	    if (count-- == 0) break;
        }
	if (CtrlCIsWaiting())
        {
	    break;
        }
        targetPosition = text.Find(findTarget,	// Go find the target string
            currentChar, caseSensitiveSearches,
            direction, findOnlyTokenStrings);
	if (targetPosition == -1)		// Target string not found -
        {					//  we're finished
	    break;
        }
        currentChar = targetPosition +
            ((direction == C.FD.FORWARD) ?
            findTarget.Length : 0);
    }
    UpdateCursor();
    if ((count != C.INFINITE) || (attempt == 1))
    {
        crs.Peek().commandFailure = true;
    }
}						// End DoNullF()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullR(int herb, int count)
{
    bool	inputAborted;
    int         replaceTargetPosition;
    int		attempt;

    if (!again)
    {
        findTarget = GetUserString("Replace: ", findTarget, "", false,
            out inputAborted);
        if (inputAborted)
        {
            crs.Peek().commandFailure = true;
            return;
        }
        
        replaceString = GetUserString("Replace with: ", replaceString, "",
            false, out inputAborted);
        if (inputAborted)
        {
            crs.Peek().commandFailure = true;
            return;
        }
    }

    attempt = 0;
    while (true)
    {
	attempt++;
        if (count != C.INFINITE)
        {
	    if (count-- == 0) break;
        }
	if (CtrlCIsWaiting())
        {
	    break;
        }
        replaceTargetPosition = text.Find (	// Go find the target string
            findTarget, currentChar,
            caseSensitiveSearches,
            C.FD.FORWARD,
            findOnlyTokenStrings);

        if (replaceTargetPosition == -1)        // Target string not found
        {
            break;
        }

        if (herb == '?')                        // Conditional replace
        {
            currentChar = replaceTargetPosition + findTarget.Length;
            UpdateCursor();
    
            currentPrompt = "Ok to replace? (Y or [N])";
    
            switch (ToUpper(GetInputByte()))
            {
                case C.CTRL_C:  crs.Peek().commandFailure = true;  return;
                case 'Y':                                          break;
                default:                                           continue;
            }
        }                                       // End 'conditional replace'

        text.ReplaceBlock(replaceTargetPosition, findTarget.Length,
            new List<byte> (Encoding.ASCII.GetBytes(replaceString)));
    
        currentChar = replaceTargetPosition + replaceString.Length;
	if (herb == '?') text.BuildLineIndex();	// 27 Dec 16
    }						// End while (true)
    text.BuildLineIndex();
    UpdateCursor();

    if ((count != C.INFINITE) || (attempt == 1))
    {
        crs.Peek().commandFailure = true;
    }
}						// End DoNullR()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullEscape()
{
    UpdateCursor();
}                                               // End DoNullEscape()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullB()
{
    blockAnchor = currentChar;                  // How's that for simple?
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSelectCtrlC()
{
    currentChar = blockAnchor;
    ClearSelection();
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoSelectEscape()
{
    ClearSelection();
}

/* ///////////////////////////////////////////////////////////////////////// */

string  blockContents;

void DoSelectDone(int herb)
{
    List<byte>  l;
    int         length;
    bool	putAborted;
    string	s;

    length = SelectionEndPoint() - SelectionStartPoint();
    l = text.GetBlock(SelectionStartPoint(), length);
    blockContents = Encoding.ASCII.GetString(l.ToArray());
    blockContents = blockContents.Replace("\r", "\r\n");

    switch (ToUpper(herb))
    {
        case 'B':
            if (blockContents != "")
            {
                Clipboard.Clear();
                Clipboard.SetText(blockContents);
            }
        break;

        case 'D':
            if (blockContents != "")
            {
                Clipboard.Clear();
                Clipboard.SetText(blockContents);
            }
            text.DeleteBlock(SelectionStartPoint(), length);
            currentChar = SelectionStartPoint();
            UpdateCursor();
        break;

	case 'P':
	    putFilename = GetUserString("Output file: ", putFilename, "",
		true, out putAborted);
	    if (putAborted)
            {
		DoSelectCtrlC();
                return;
            }

	    if (OkayToOverwrite(putFilename))
	    {
                try
                {
                    File.WriteAllText(putFilename, blockContents);
                }    
                catch
                {
		    s = String.Format("Error writing to file '{0}'",
			putFilename);
        	    MessageBox.Show(s);
		    DoSelectCtrlC();
                }
	    }
	    else
            {
		DoSelectCtrlC();
            }
        break;
    }
    ClearSelection();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullG(int count)
{
    string      s;                              // Text from file or clipboard

    if (count == C.INFINITE) return;
    if (!again)
    {
	getFilename = GetUserString("Input file: ", getFilename, "", true);
    }
    
    if (getFilename == "")                      // Null filename - so read from
    {                                           //  Windows clipboard
        s = Clipboard.GetText();
    }                                           // End 'read from clipboard'
    else
    {
        try
        {
            s = File.ReadAllText(getFilename);
        }
        catch
        {
	    s = String.Format("Can't read file '{0}'", getFilename);
            MessageBox.Show(s);
	    crs.Peek().commandFailure = true;
            return;
        }
    }
    s = s.Replace(DetectLineDelimiter(s), "\r");
    while (count-- != 0)
    {
	if (CtrlCIsWaiting())
        {
	    break;
        }
	text.InsertBlock(currentChar,
	    new List<byte> (Encoding.ASCII.GetBytes(s)), false);
    }
    text.BuildLineIndex();
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullP(int count)
{
    do
    {
	if (count != C.INFINITE)
        {
	    if (count-- == 0) break;
        }
	if (CtrlCIsWaiting())
        {
	    break;
        }
	DoNullPGuts();
    } while (!crs.Peek().commandFailure);
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullPGuts()
{
    int		firstParagraphLine;
    int		lastParagraphLine;
    int		firstParagraphChar;
    int		lastParagraphChar;
    int		line;
    int		i;
    int		cIndex;
    int		currentLineStart;
    bool	inDelimiters;
    List<byte>	rp;				// Reformatted paragraph
    byte	b;				// The current byte
    List<byte>	word;				// Accumulate words here
    bool	firstWord;
    int		spaces;
    bool[]	dTags = new bool[4];
    int[]	wTags = new int[4];
    int[]	pTags = new int[4];
    int		ti;

/*  Search forward from the current position for a non-blank line.  If none is
    found, simply return because there is no paragraph at or below the cursor
    to reformat.  Otherwise, press on.  */

    line = currentLine;				// Start at current line
    while (true)
    {
	if (!IsBlank(GetLine(line)))
	{
	    break;
	}
	if (line == text.GetLineCount() - 1)
	{
	    crs.Peek().commandFailure = true;
	    return;
	}	
	line++;
    }

/*  Found a non-blank line if we get here.  Now search backwards until finding
    either a blank line or the beginning of the file.  In either case, that
    will be the start of the paragraph.  */

    while ((line > 0) && !IsBlank(GetLine(line - 1)))
    {
	line--;
    }
    firstParagraphLine = line;

/*  Now search forward until finding either a blank line or the end of the
    file.  In either case, the preceding line will be the end of the paragraph.
    */

    while ((line < text.GetLineCount() - 1) && !IsBlank(GetLine(line + 1)))
    {
    	line++;
    }
    lastParagraphLine = line;

    firstParagraphChar = text.GetLineStart(firstParagraphLine);
    lastParagraphChar = text.GetLineStart(lastParagraphLine + 1) - 1;

/*  Here's the big loop where we actually reformat the paragraph.  */

    currentLineStart = 0;
    firstWord = true;
    inDelimiters = true;
    rp = new List<byte>();			// Put result here
    word = new List<byte>();			// Accumulate words here

    for (ti=0; ti<4; ti++)
    {
	dTags[ti] = false;
	wTags[ti] = -1;
	pTags[ti] = -1;
    }

    for (i=0; i<indentMargin; i++)		// Start with blanks for the
    {						//  indent margin
	rp.Add(C.BLANK);
    }

    for (cIndex = firstParagraphChar;		// For each character in the
	 cIndex <= lastParagraphChar; cIndex++)	//  paragraph
    {
	b = GetChar(cIndex);			// Get another character
	if ((b == C.BLANK) ||			// Got a delimiter
	    (b == C.TAB) ||
	    (b == C.EOL) ||
	    (b == C.EOF))
	{
	    if (b == C.EOF)			// Stop any repeated P commands
	    {					//  upon reaching end of file
		crs.Peek().commandFailure = true;
	    }

	    for (ti=0; ti<4; ti++)		// Note which (if any) of the
	    {					//  four tags happen to point
		if (cIndex == text.GetTag(ti))	//  to this delimiter
		{
		    dTags[ti] = true;
		}
	    }

	    if (!inDelimiters)
	    {
		inDelimiters = true;

            /*  We just found the end of a word.  If it will fit on the current
                line, add a blank to separate it from the previous word.
                Otherwise, add a <CR> and perform the other housekeeping needed
                to start a new line.  Then (for either case), add the word just
                found.  Except ... for the very first word in the paragraph,
                just add it without worrying about whether or not it will fit.
                */

		if (!firstWord)
		{

		    switch (rp[rp.Count - 1])
		    {
			case (byte)'.':
			case (byte)'?':
			case (byte)'!':	 spaces = 2;  break;
			default:	 spaces = 1;  break;
		    }

                    if (((rp.Count - 1 - currentLineStart) + word.Count + spaces) <=
                        rightMargin)
                    {                           // Word will fit on current
                        rp.Add(C.BLANK);        //  line
			if (spaces == 2)
			{
			    rp.Add(C.BLANK);
			}
                    }
                    else                        // Word won't fit, so start a
                    {                           //  new line
                        rp.Add(C.EOL);
                        currentLineStart = rp.Count;
                        for (i=0; i<leftMargin; i++)
                        {                           
                            rp.Add(C.BLANK);
                        }
                    }
		}				// End 'not first word'

		for (ti=0; ti<4; ti++)
		{
		    if (wTags[ti] >= 0)
		    {
			pTags[ti] = rp.Count + wTags[ti];
			wTags[ti] = -1;
		    }
		}

		rp.AddRange(word);		// Add word to the buffer
		firstWord = false;
		word.Clear();			// Get ready for next word
	    }
	}					// End 'got a delimiter'
	else					// Not a delimiter
	{
	    if (inDelimiters)
	    {
		inDelimiters = false;
		for (ti=0; ti<4; ti++)
		{
		    if (dTags[ti])
		    {
			pTags[ti] = rp.Count;
			dTags[ti] = false;
		    }
		}
	    }

	    for (ti=0; ti<4; ti++)		// Note the position within the
	    {					//  word of any characters
		if (cIndex == text.GetTag(ti))	//  pointed to by one of the
		{				//  four tags
		    wTags[ti] = word.Count;
		}
	    }
	    word.Add(b);
	}					// End 'not a delimiter
    }						// End for each character

/*  Now we can delete the original paragraph and replace it with the
    reformatted version sitting in 'rp'.  */

    text.ReplaceBlock(firstParagraphChar, lastParagraphChar - firstParagraphChar,
	rp);

    for (ti=0; ti<4; ti++)
    {
	if (pTags[ti] >= 0)
	{
	    text.SetTag(ti, pTags[ti] + firstParagraphChar);
	}
    }

    currentChar = firstParagraphChar + rp.Count + 1;
    text.BuildLineIndex();
    UpdateCursor();
}						// End DoNullP()

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullCtrlC()
{
    crs.Peek().commandFailure = true;
    if (RecordingMacro())
    {
	macros.Remove(newMacroName);
	newMacroName = "";
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullE(bool singleCharacter, int count)
{
    string s;

    if (!again)
    {
	if (singleCharacter)
        {
	    currentPrompt = "Enter single-character macro name";
	    s = new string((char)GetInputByte(), 1).ToUpper();	
        }
        else
        {
            s = GetUserString("Macro name: ", crs.Peek().macroName, "",
                true).ToUpper();
        }

	if (s != "")
        {
	    crs.Peek().macroName = s;
        }
	if (!macros.ContainsKey(crs.Peek().macroName))
	{
            MessageBox.Show(String.Format("Macro '{0}' not defined",
                crs.Peek().macroName));
	    return;
        }
    }
    if (crs.Peek().macroName != newMacroName)
    {
	do
        {
	    if (count != C.INFINITE)
            {
		if (count-- == 0) return;
            }
        } while (new MacroRunner(this, macros[crs.Peek().macroName]).Run());
    }
    else
    {
	MessageBox.Show("Can't run the macro that's being defined");
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullM()
{
    List<byte> l;

    if (RecordingMacro())
    {
	l = macros[newMacroName];		// Get reference to macro body
	l.RemoveAt(l.Count - 1);		// Jettison trailing 'M'
	if (l.Count == 0)			// Remove macro completely if
	{					//  it has no body
	    macros.Remove(newMacroName);
	}
	newMacroName = "";
    }
    else
    {
	currentPrompt = "Create   Get   Insert   List   Save";

	while (true)
        {
	    switch (ToUpper(GetInputByte()))
            {
		case C.ESC:
                case C.CTRL_C:			return;
                case 'C':	DoMC();		return;
                case 'G':	DoMG();		return;
                case 'I':	DoMI();		return;
                case 'L':	DoML();		return;
                case 'S':	DoMS();		return;
            }
        }
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoNullK(int count)
{
    if (count != 0)
    {
	crs.Peek().commandFailure = true;
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoMC()
{
    newMacroName = GetUserString("Macro name: ",
        newMacroName, "", true).ToUpper();

    if (newMacroName != "")
    {
	macros[newMacroName] = new List<byte>();
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoMG()
{
    macroFilename = GetUserString("Macro file: ", macroFilename, "", true);
    if (!File.Exists(macroFilename))
    {
	MessageBox.Show(String.Format("File '{0}' not found", 
	    macroFilename));
	return;
    }
    LoadMacroFile(macroFilename);
}

/* ///////////////////////////////////////////////////////////////////////// */

void LoadMacroFile(string fileToLoad)
{
    string	s;				// Macro file goes here
    int		i;				// Index into macro file
    string	macroName;			// Build macro names here
    byte	b;				// GetNextByte() return value
    List<byte>	macroBody;			// Build macro bodies here
    bool	initFound = false;

    try
    {
        s = File.ReadAllText(fileToLoad);
    }
    catch
    {
	MessageBox.Show(String.Format("Can't read file '{0}'", fileToLoad));
        return;
    }
    s = s.Replace("\r", "").Replace("\n", "");	// Remove CRs and LFs
    s += "\x89\x89\x89";			// Add EOFs so parsing is easy
    i = 0;					// Start at beginning of macro

LookingForMacro:

/*  Spin here looking for the beginning of a macro, the beginning of a comment,
    or the end of the file.  */

    while (true)
    {
	if (s[i] == C.EOF)			// Found end of file.  We're
        {					//  almost done
	    if (initFound)
            {
		new MacroRunner(this, macros["INIT"]).Run();
            }
	    return;
        }

	if (s[i] == 'M')			// Found the begining of a
        {					//  macro.  Go get its name
	    macroName = "";
            i++;
            goto GettingName;
        }
	
        if ((s[i]   == '\\') &&			// Found the beginning of a 
	    (s[i+1] == '*'))			//  comment.  Go skip over it
	{
	    i += 2;
            goto SkippingComment;
        }

	i++;					// Ignore any other characters
    }						// End 'looking for macro'

SkippingComment:

/*  Spin here looking for the end of a comment or the end of the file.  */

    while (true)
    {
	if (s[i] == C.EOF)			// Found end of file.  Not good
        {					//  inside a comment
            MessageBox.Show(String.Format("Unexpected EOF in macro comment " +
                "at character position {0}", i));
	    return;
        }

        if ((s[i]   == '*') &&			// Found end of the comment.
	    (s[i+1] == '\\'))			//  Restart macro search
	{
	    i += 2;
            goto LookingForMacro;
        }

	i++;					// Ignore any other characters
    }						// End 'skipping comment'

GettingName:

/*  Spin here looking for the end of the macro name.  */

    while (true)
    {
	b = GetNextByte(s, ref i);
	if (b == C.EOF)				// Found end of file.  Not good
        {					//  inside macro name
            MessageBox.Show(String.Format("Unexpected EOF in macro name " +
                "at character position {0}", i));
	    return;
        }

	if (b == C.BAD)				// Found bad escape sequence.
        {					//  Not good ever
            MessageBox.Show(String.Format("Unrecognized escape sequence " +
                "at character position {0}", i));
	    return;
        }

	if (b == C.ESC)				// Found end of macro name.  Go
        {					//  deal with the body
	    macroBody = new List<byte>();
	    goto GettingBody;
        }

	if (b > 0x7e)				// Funny escape codes not
	{					//  allowed in macro names
            MessageBox.Show(String.Format("Invalid escape code found in " +
                "macro name at character position {0}", i));
	    return;
        }

	macroName += (char) b;			// Add new char to macro name
    }						// End 'getting name'

GettingBody: 

/*  Spin here looking for the end of the macro body.  */

    while (true)
    {
	b = GetNextByte(s, ref i);
	if (b == C.EOF)				// Found end of file.  Not good
        {					//  inside macro body
            MessageBox.Show(String.Format("Unexpected EOF in macro body " +
                "at character position {0}", i));
	    return;
        }

	if (b == C.BAD)				// Found bad escape sequence.
        {					//  Not good ever
            MessageBox.Show(String.Format("Unrecognized escape sequence " +
                "at character position {0}", i));
	    return;
        }

	if (b == C.EOM)				// Found end of macro.  Save it
        {					//  and go look for another one
	    macros[macroName] = macroBody;
	    if (macroName == "INIT")
            {
		initFound = true;
            }
            goto LookingForMacro;
        }

	macroBody.Add(b);			// Add anything else to body
    }						// End 'getting body'
}						// End DoMG()

/* ///////////////////////////////////////////////////////////////////////// */

Dictionary<string, byte> cToB = new Dictionary<string, byte>()
{
    { "MM",  C.EOM    },
    { "BR",  C.ESC    },
    { "CL",  C.LEFT   },
    { "CR",  C.RIGHT  },
    { "CU",  C.UP     },
    { "CD",  C.DOWN   },
    { "CH",  C.HOME   },
    { "NL",  C.ENTER  },
    { "RB",  C.BKSP   },
    { "TB",  C.TAB    },
    { "XF",  C.DELETE },
    { "XX",  C.CTRL_X },
    { "XA",  C.CTRL_A },
    { "XZ",  C.CTRL_Z },
    { "XU",  C.CTRL_U },
    { "XE",  C.CTRL_E },
    { "XN",  C.CTRL_N },
    { "XS",  C.CTRL_V }
};

byte GetNextByte(string s, ref int i)
{
    byte rv;

    if (s[i] == C.EOF)				// Return EOF at end of file
    {
	return C.EOF;
    }

    if (s[i] != '\\')				// Return non-escaped
    {						//  characters as is
	return (byte) s[i++];	
    }

/*  We're dealing with escaped characters from here on.  */

    if (s[i+1] == '\\')				// Escaped backslash - return a
    {						//  single backslash
	i += 2;
        return (byte) '\\';
    }

    if (s[i+1] == '0')				// Beginning of a hex escape
    {						//  sequence
	if (!Uri.IsHexDigit(s[i+2]))		// Abort if the subsequent
        {					//  character isn't a hex digit
	    return C.BAD;
        }
	if (Uri.IsHexDigit(s[i+3]))		// Have two hex digits (c:
	{
	    rv = Convert.ToByte(s.Substring(i+2, 2), 16);
	    i += 4;	
        }
	else					// Only have one hex digit )c:
        {
	    rv = Convert.ToByte(s.Substring(i+2, 1), 16);
	    i += 3;
        }
	return rv;	
    }

    if (cToB.ContainsKey(s.Substring(i+1, 2)))	// See if we have one of the
    {						//  defined escape sequences
	rv = cToB[s.Substring(i+1, 2)];		// Yup
        i += 3;
        return rv;
    }

    return C.BAD;    				// Nope
}						// End GetNextByte()

/* ///////////////////////////////////////////////////////////////////////// */

void DoMI()
{
    byte herb;
    
    currentPrompt = "Ctrl-C to stop";
    buffer = new List<byte>();
    while (true)
    {
	buffer.Clear();
	switch (herb = GetInputByte())
        {
	    case C.CTRL_C:	return;
            case C.ENTER:	break;
            default:
		AddChar(herb, true);
		text.InsertBlock(currentChar, buffer);
		currentChar += buffer.Count;
		UpdateCursor();
	    break;                
        }
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

Dictionary<byte, string> bToC = new Dictionary<byte, string>()
{
    { C.ESC,	"\\BR" },
    { C.LEFT,	"\\CL" },
    { C.RIGHT,	"\\CR" },
    { C.UP,	"\\CU" },
    { C.DOWN,	"\\CD" },
    { C.HOME,	"\\CH" },
    { C.ENTER,	"\\NL" },
    { C.BKSP,	"\\RB" },
    { C.TAB,	"\\TB" },
    { C.DELETE,	"\\XF" },
    { C.CTRL_X,	"\\XX" },
    { C.CTRL_A,	"\\XA" },
    { C.CTRL_Z,	"\\XZ" },
    { C.CTRL_U,	"\\XU" },
    { C.CTRL_E,	"\\XE" },
    { C.CTRL_N,	"\\XN" },
    { C.CTRL_V,	"\\XS" }
};

/* ///////////////////////////////////////////////////////////////////////// */

List<byte> stlb(string s) { return new List<byte> (Encoding.UTF8.GetBytes(s)); }

/* ///////////////////////////////////////////////////////////////////////// */

void AddChar(byte c, bool body)
{
    if (c == '\\')
    {
	buffer.AddRange(stlb("\\\\"));
    }
    else if ((c >= 0x20) && (c <= 0x7e))
    {
	buffer.Add(c);
    }
    else if (bToC.ContainsKey(c) && body)
    {
	buffer.AddRange(stlb(bToC[c]));
    }
    else
    {
	buffer.AddRange(stlb(String.Format("\\0{0:X}", c)));
    }

}

/* ///////////////////////////////////////////////////////////////////////// */

void DoMS()
{
    crs.Peek().macroName = GetUserString("Macro name: ",
        crs.Peek().macroName, "", true).ToUpper();
    if (crs.Peek().macroName == "")
    {
        return;
    }
    if (!macros.ContainsKey(crs.Peek().macroName))
    {
        MessageBox.Show(String.Format("Macro '{0}' not defined",
            crs.Peek().macroName));
        return;
    }
    SaveOneMacro(crs.Peek().macroName);
}						// End DoMS()

/* ///////////////////////////////////////////////////////////////////////// */

void SaveOneMacro(string macroName)
{
    buffer = new List<byte>();
    buffer.Add((byte)'M');

    foreach (char c in macroName)
    {
	AddChar((byte)c, false);
    }

    buffer.AddRange(stlb(bToC[C.ESC]));

    foreach (byte b in macros[macroName])
    {
	AddChar(b, true);
    }

    buffer.AddRange(stlb("\\MM\xd"));

    text.InsertBlock(currentChar, buffer);
    currentChar += buffer.Count;
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoML()
{
    foreach(string macroName in macros.Keys)
    {
	SaveOneMacro(macroName);
    }
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoIEscape()
{
    undoBuffer.Clear();
    crs.Peek().currentState = ST.COMMAND;
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoIReturn()
{
    int         i;
    byte        b;
    int         length;

    InsertByte(C.ENTER);
    if (autoIndent)
    {

    /*  Find first nonwhite character on the line  */

        i = text.GetLineStart(currentLine);
        while (((b = GetChar(i)) == ' ') || (b == '\t'))
        {
            i++;
        }
        i = i - text.GetLineStart(currentLine); // Get its index w/in the line
        i = screenImage[currentLine - topLine]. // Get its horizontal position
            textToScreenMap[i];                 //  on the screen
        InsertBytes(C.BLANK, i);                // Insert that many blanks

	if (IsBlank(GetLine(currentLine)))
	{
	    length = text.GetLineStart(currentLine + 1) -
		     text.GetLineStart(currentLine) - 1;
	    text.DeleteBlock(text.GetLineStart(currentLine), length);
	    currentChar -= length;
        }
    }
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoIDefault(int herb)
{
    if (blanksForTabs && (herb == C.TAB))
    {
        InsertBytes((byte)' ', GetTabStop(cursorX) - cursorX);
    }
    else
    {
        InsertByte((byte)herb);
    }
    PositionWindowHorizontally();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoXEscape()
{
    undoBuffer.Clear();
    crs.Peek().currentState = ST.COMMAND;
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoXBackspace()
{
    byte        undoByte;
    int         lastIndex;

    currentChar--;                              // Back up one slot regardless
    if (undoBuffer.Count > 0)                   // Undo buffer not empty
    {
        lastIndex = undoBuffer.Count - 1;       // Get index to last undo byte
        undoByte = undoBuffer[lastIndex];       // Get the undo byte itself
        if (undoByte == C.NO_EXCHANGE)          // Exchange was past an EOL or
        {                                       //  the EOF, so we delete the
            DeleteChar(currentChar);            //  character that was inserted
        }
        else                                    // The exchange was normal, so
        {                                       //  replace the character in
            DeleteChar(currentChar);            //  the text buffer with the
            InsertByte(undoByte);               //  last one from the undo
            currentChar--;                      //  buffer
        }
        undoBuffer.RemoveAt(lastIndex);         // Discard undo byte just used
    }                                           // End 'undo buffer not empty'
    UpdateCursor();
}                                               // End DoXBackspace()

/* ///////////////////////////////////////////////////////////////////////// */

void DoXReturn()
{
    ExchangeByte(C.ENTER);
    UpdateCursor();
}

/* ///////////////////////////////////////////////////////////////////////// */

void DoXDefault(int herb)
{
    if (blanksForTabs && (herb == C.TAB))
    {
        ExchangeBytes((byte)' ', GetTabStop(cursorX) - cursorX);
    }
    else
    {
        ExchangeByte((byte)herb);
    }
    PositionWindowHorizontally();
}

/* ////////////////////////////////////////////////////////////////////////////
                                 InsertByte()
                                 InsertBytes()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    These corral some of the nonsense needed to convert from bytes
                to List<byte> objects when we have to jam just a few characters
                into the text buffer.

REVISIONS:      21 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void InsertByte(byte b)
{
    InsertBytes(b, 1);
}

/* ///////////////////////////////////////////////////////////////////////// */

void InsertBytes(byte b, int n)
{
    int         i;
    List<byte>  l;

    l = new List<byte>();                       // Make a new list containing
    i = n;                                      //  the byte(s) to be inserted
    while (i-- > 0)
    {
        l.Add(b);
    }
    text.InsertBlock(currentChar, l);           // Do the insert
    currentChar += n;                           // Note new current position
}

/* ////////////////////////////////////////////////////////////////////////////
                                ExchangeByte()
                                ExchangeBytes()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    Sort of like InsertByte() and InsertBytes(), but for use in
                eXchange mode instead of Insert mode.

REVISIONS:      21 Aug 16 - RAC - Genesis, with hints from InsertByte() and
                                   InsertBytes() and parts of the original
                                   Waedit
//////////////////////////////////////////////////////////////////////////// */

void ExchangeByte(byte newByte)
{
    byte oldByte;

    oldByte = GetChar(currentChar);
    if ((oldByte == C.EOL) || (oldByte == C.EOF))
    {
        undoBuffer.Add(C.NO_EXCHANGE);
    }
    else
    {
        undoBuffer.Add(oldByte);
        DeleteChar(currentChar);
    }
    InsertByte(newByte);
}

/* ///////////////////////////////////////////////////////////////////////// */

void ExchangeBytes(byte b, int n)
{
    while (n-- > 0)                             // We have to do them one byte
    {                                           //  at a time in order to deal
        ExchangeByte(b);                        //  properly with EOL and EOF
    }
}                                               // End ExchangeBytes()

/* ////////////////////////////////////////////////////////////////////////////
                           LineContainingCharacter()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function returns the line number of the line that contains
                a specified character.

REVISIONS:       7 Aug 16 - RAC - Genesis
                21 Aug 16 - RAC - Moved here from the Text class to make the
                                   Text class interface narrower
//////////////////////////////////////////////////////////////////////////// */

int LineContainingCharacter(int c)
{
    int i;

    for (i=1; i<=text.GetLineCount(); i++)
    {
        if (c < text.GetLineStart(i)) return i-1;
    }
    throw new Exception("Something wrong in LineContainingCharacter()");
}

/* ////////////////////////////////////////////////////////////////////////////
				   GetChar()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns a specified byte from the text buffer.

REVISIONS:	28 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

byte GetChar(int i)
{
    return text.GetBlock(i, 1)[0];
}

/* ////////////////////////////////////////////////////////////////////////////
                                 DeleteChar()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function deletes a single character at the specified index.

REVISIONS:       9 Aug 16 - RAC - Genesis
                16 Aug 16 - RAC - Fixed to call DeleteBlock() to avoid
                                   duplicated code
                21 Aug 16 - RAC - Moved here from the Text class to make the
                                   Text class interface narrower
//////////////////////////////////////////////////////////////////////////// */

public void DeleteChar(int i)
{
    text.DeleteBlock(i, 1);
}

/* ////////////////////////////////////////////////////////////////////////////
				GetInputByte()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function gets an input byte via whichever command runner
		is currently at the top of the command runner stack.  The net
                result is that the byte will come from the keyboard if no macro
                is running, or from the currently running macro if one is.

REVISIONS:	 5 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

byte GetInputByte()
{
    return crs.Peek().GetInputByte();
}

/* ////////////////////////////////////////////////////////////////////////////
				GetUserString()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function gets a string from the user (or from a macro)
		using the line editor.

REVISIONS:	 6 Sep 16 - RAC - Genesis
		 7 Sep 16 - RAC - Added optional parameter to let the caller
                 		   know if the user hit Ctrl-C
//////////////////////////////////////////////////////////////////////////// */

string GetUserString (
    string	labelText,	// Label to display in the line editor
    string	initialText,	// Start the line editor with this text
    string	okChars,	// Accept only characters in this string
    bool	normalCR,	// <CR> terminates input.  Set false for Find
    				//  and replace strings where <CR> can be
                                //  entered into the string being edited
    out bool	aborted)	// Set true on return if the user hit Ctrl-C
{
    return GUS(labelText, initialText, okChars, normalCR, out aborted);
}

/* ///////////////////////////////////////////////////////////////////////// */

string GetUserString(string labelText, string initialText, string okChars,
    bool normalCR)
{
    bool dummy;
    return GUS(labelText, initialText, okChars, normalCR, out dummy);
}

/* ///////////////////////////////////////////////////////////////////////// */

string GUS(string labelText, string initialText, string okChars, bool normalCR,
    out bool aborted)
{

    string	rv;
    byte	b;
    
    aborted = false;
    OpenLineEditor(labelText, initialText, okChars);
    while (true)
    {
	b = (byte) crs.Peek().GetExpandedKeystroke(true);
	if (b == C.CTRL_C)
        {
	    rv = initialText;
	    aborted = true;
            break;
        }
        else if (b == C.ESC)
        {
	    rv = GetLineEditorResult(false);
            break;
        }
	else if ((b == C.ENTER) && normalCR)
        {
	    rv = GetLineEditorResult(true);
            break;
        }
	else
        {
	    FeedLineEditor(b);
        }
    }
    CloseLineEditor();
    return rv;
}

/* ////////////////////////////////////////////////////////////////////////////
				   IsBlank()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function examines a list of bytes and returns true if it
		contains nothing but whitespace, or false otherwise

REVISIONS:	27 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

bool IsBlank(List<byte> s)
{
    foreach (byte b in s)
    {
	if ((b != C.BLANK) &&
	    (b != C.TAB) &&
	    (b != C.EOL) &&
	    (b != C.EOF))
	{
	    return false;
	}
    }
    return true;
}

/* ////////////////////////////////////////////////////////////////////////////
			       MaybeRunAMacro()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function is called when an unrecognized command is
                encountered in the Normal (not Selecting) mode of the NULL
                state or in the Insert or eXchange state.  Here we check to see
                if it might be a single character macro, and execute it if it
                is.

REVISIONS:	 2 Sep 16 - RAC - Genesis
		27 Sep 16 - RAC - Added logic for the new repeat scheme
//////////////////////////////////////////////////////////////////////////// */

void MaybeRunAMacro(int herb, int count)
{
    string s;
    
    s = new string((char)herb, 1).ToUpper();	// Convert herb to a string
    if (macros.ContainsKey(s))			// If herb is the name of a
    {						//  macro, run it
        if (s != newMacroName)
        {
	    do
            {
		if (count != C.INFINITE)
                {
		    if (count-- == 0) return;
                }
	    } while (new MacroRunner(this, macros[s]).Run());
        }
        else
        {
            MessageBox.Show("Can't run the macro that's being defined");
        }
    }
}						// End MaybeRunAMacro()

/* ////////////////////////////////////////////////////////////////////////////
			       RecordingMacro()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns true if we are currently recording a
		macro, or false otherwise.

REVISIONS:	 9 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

bool RecordingMacro()
{
    return (newMacroName != "");
}

void DumpMacros() { foreach(KeyValuePair<string, List<byte>> kvp in macros) {
Console.WriteLine(kvp.Key); foreach(byte b in kvp.Value) {
Console.Write(b); Console.Write(" "); } Console.WriteLine(); Console.WriteLine(); } }

}						// End 'class mainForm'
}						// End namespace waedit

