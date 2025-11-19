/* ////////////////////////////////////////////////////////////////////////////
			       CommandRunners.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This file contains three classes.  CommandRunner is a base
		class that contains most of the logic for grabbing a repeat
                count and a command byte from either the keyboard or a macro.
                KeyboardPoller and MacroRunner are derived from CommandRunner
                and contain the logic specific to running from the keyboard or
                from a macro.

REVISIONS:	 5 Sep 16 - RAC - Genesis, with hints from various false starts
		 9 Sep 16 - RAC - Revised somewhat to handle Insert and
				   Exchange modes as states rather than
                                   commands
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace waedit
{

/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */
                      public abstract class CommandRunner
/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

protected static bool	abortAllMacros = false;	// See GetInputByte() comments
public string		countString = "";	// Build the count string here
protected MainForm	mainForm;
int			lastCommand;
public MainForm.ST	currentState = MainForm.ST.COMMAND;
public string		macroName = "";
public bool	    	commandFailure;		/* Set by the executor
                                                    functions to indicate
                                                    "failure", as described on
                                                    page 88 of the Intel Aedit
                                                    manual.  */

/* ////////////////////////////////////////////////////////////////////////////
                             StartCommandRunner()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function lets us invoke Run() as a thread even though it
		returns a bool

REVISIONS:	27 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void StartCommandRunner() { Run(); }

/* ////////////////////////////////////////////////////////////////////////////
				     Run()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function grabs repeat counts and command bytes from the
		current input stream (either the keyboard or a macro) and
                eventually calls mainForm.Execute() to do the actual work.

REVISIONS:	 5 Sep 16 - RAC - Genesis
		 9 Sep 16 - RAC - Revised somewhat to handle Insert and
				   Exchange modes as states rather than
                                   commands
                27 Sep 16 - RAC - Changed to return a bool and set up a
                                   temporary, dummy return
//////////////////////////////////////////////////////////////////////////// */

enum MS
{
    IN_PROGRESS,
    ENDED,
    ABORTED
}


public bool Run()
{
    MS r;

/*  Push this CommandRunner instance onto a FIFO stack.  Executor functions
    will then grab input via whichever CommandRunner object is at the top of
    the stack.  This will magically connect the Executor functions to the
    correct input source without any concern on their part about keyboards,
    macros, or macro nesting levels.  */

    mainForm.crs.Push(this);
    while (true)
    {
	if (currentState == MainForm.ST.COMMAND)
	{
	    if ((r = HandleCommandState()) != MS.IN_PROGRESS)
            {
		break;
            }
	}
	else
	{
	    if ((r = HandleInsertAndExchangeStates()) != MS.IN_PROGRESS)
            {
                break;
            }
	}
    }
    mainForm.crs.Pop();
    return (r == MS.ENDED) & !abortAllMacros;
}

/* ///////////////////////////////////////////////////////////////////////// */

MS HandleCommandState()
{
    char	last;
    bool	abortMacro;

    last = GetExpandedKeystroke(false);
    if (last == 0)				// Return when macro ends
    {						// (This won't happen when
	return MS.ENDED;			//  input is from the keyboard)
    }

    if ((last == '\b') &&                       // Backspace removes the last
        (countString != ""))                    //  character in the count
    {                                           //  string if there is anything
        countString = countString.Remove (      //  to remove.  Otherwise it
            countString.Length - 1);            //  will be interpreted as a
    }                                           //  command
    else if (last == '/')                       // Infinite count replaces any
    {                                           //  previous input
        countString = "/";
    }                                           // End 'got a slash'
    else if (Char.IsDigit(last))                // Got one or more digits
    {
        if (countString == "/")                 // Numeric input replaces slash
        {                                       //  if present, otherwise
            countString = "";                   //  appends to number being
        }                                       //  constructed
        countString += last;
        if (countString.Length > 6)             // But don't let the repeat
        {                                       //  count get out of hand
            countString = countString .
                Substring(0, 6);
        }
    }                                           // End 'got one or more digits'
    else                                        // Must be a potential command,
    {                                           //  so go try to execute it
        abortMacro = ManageRepeats(last);
        countString = "";                       // Clear the count
	if (abortMacro && (this is MacroRunner))
        {
	    return MS.ABORTED;
        }
    }                                           // End 'must be a command'
    return MS.IN_PROGRESS;
}						// End HandleCommandState()

/* ///////////////////////////////////////////////////////////////////////// */

MS HandleInsertAndExchangeStates()
{
    char c;
    bool	abortMacro;
    
    if ((c = GetExpandedKeystroke(true)) != 0)
    {
	abortMacro = mainForm.Execute(c, 1);
	if (abortMacro && (this is MacroRunner))
        {
	    return MS.ABORTED;
        }
    }
    return MS.IN_PROGRESS;
}

/* ////////////////////////////////////////////////////////////////////////////
			    GetExpandedKeystroke()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Others call this function to get keyboard input.  This function
                returns one character which may come from the keyboard, or a
                macro, or a queue of characters resulting from a previous
                expansion of Ctrl-N or Ctrl-V input.
                
                The input parameter indicates whether or not this function is
                supposed to recognize Ctrl-V.

REVISIONS:	17 Sep 16 - RAC - Preliminary version that replaces
                                   GetNumberOrKeystroke() and
                                   GetNumberOrStringOrKeystroke()
		17 Sep 16 - RAC - Filled in for real
//////////////////////////////////////////////////////////////////////////// */

Queue<char> keb = new Queue<char>();		// Keyboard expansion buffer 

public char GetExpandedKeystroke(bool recognizeCtrlV)
{
    byte	herb;
    string	s = "";
    bool	idOk;
    int		i;
    
    while (true)
    {
	if (keb.Count > 0)							// If there's something in the queue, just return it
	{
	    return keb.Dequeue();
	}
	herb = GetInputByte();							// Otherwise, get a byte from the keyboard (or macro)
	if (herb == C.CTRL_N)							// Need to expand numeric variable
        {
	    mainForm.currentPrompt = "Enter numeric variable ID";		// Prompt for numeric variable ID
            herb = GetInputByte();						// Get same
	    if ((herb >= '0') && (herb <= '9'))
            {
		i = mainForm.N[herb - '0'];
		if (recognizeCtrlV)
                {
		    switch (mainForm.radix)
                    {
			case 'A': 
			    if ((i >= 32) && (i <= 126))                        
                            {
	                        s = String.Format("{0}", (char) i);
                            }
                            else
                            {
				s = "?";
                            }
			break;

                        case 2:
                        case 8:
                        case 10:
                        case 16:  s = Convert.ToString(i, mainForm.radix).ToUpper(); break;
                    }		

		    foreach (char c in s)
                    {
		        keb.Enqueue(c);
                    }

                }

		else
                {
		    foreach (char c in String.Format("{0}", Math.Abs(i)))
                    {
		        keb.Enqueue(c);
                    }
                }
            }
            else
            {
		mainForm.currentPrompt = "Invalid numeric variable ID";
            }
        }									// End 'need to expand numeric variable'
	else if ((herb == C.CTRL_V) && recognizeCtrlV)				// Need to expand string variable
        {
	    idOk = true;							// Assume success
	    mainForm.currentPrompt = "Enter string variable ID";		// Prompt for string variable ID
            herb = GetInputByte();						// Get same

	    switch (MainForm.ToUpper(herb))
            {
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
		    s = mainForm.S[herb - '0'];
                break;
                case 'B':  s = Clipboard.GetText();	  break;
                case 'E':  s = mainForm.currentFilename;  break;
                case 'G':  s = mainForm.getFilename;      break;
                case 'M':  s = mainForm.macroFilename;    break;
                case 'P':  s = mainForm.putFilename;      break;
                case 'R':  s = mainForm.replaceString;    break;
                case 'T':  s = mainForm.findTarget;       break;

		default:   s = "";  idOk = false;  break;			  
            }
	    if (idOk)
            {
		foreach (char c in s)
                {
		    keb.Enqueue(c);
                }
            }
            else
            {
		mainForm.currentPrompt = "Invalid string variable ID";
            }
        }									// End 'need to expand string variable'
	else									// No expansion -- just return character as is
        {
	    return (char) herb;
        }									// End 'no expansion'
    }										// End 'while (true)'
}										// End GetExpandedKeystroke()

abstract public byte GetInputByte();

/* ////////////////////////////////////////////////////////////////////////////
                                ManageRepeats()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    Run() calls this function after it has grabbed a repeat count
                (which may be null) and a command byte from the input stream.

                THis function parses the repeat count and passes it along with
                the command to the executor for processing.  It also takes care
                of some housekeeping related to the Again command.  This
                function returns true if the command is marked as failed, or
                false otherwise.

REVISIONS:       5 Sep 16 - RAC - Genesis, with hints from the Revision 1 logic
                 9 Sep 16 - RAC - Added logic for aborting macros
                27 Sep 16 - RAC - Revised substantially to move the repeat
                                   logic down into the individual commands.
//////////////////////////////////////////////////////////////////////////// */

bool ManageRepeats(int herb)
{
    int repeatCount;

/*  Parse the repeat count.  */

    if (countString == "")
    {
        repeatCount = 1;
    }
    else if (countString == "/")
    {
	repeatCount = C.INFINITE;
    }
    else
    {
        repeatCount = Int32.Parse(countString);
    }

/*  Give special priority to Ctrl-C if typed on the keyboard  */

    if (mainForm.CtrlCIsWaiting())
    {
        herb = mainForm.GetKeyFromKeyboard();
        abortAllMacros = true;
    }

    if (MainForm.ToUpper(herb) == 'A')		// For the Again command, set
    {						//  a flag that forces executor
        mainForm.again = true;			//  functions to use previous
    }						//  find/replace strings, etc.
    else
    {
	mainForm.again = false;
        lastCommand = herb;
    }
    return mainForm.Execute(lastCommand, repeatCount);
}                                               // End ManageRepeats()

}						// End class CommandRunner

/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */
                     class KeyboardPoller : CommandRunner
/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */
{

/* ////////////////////////////////////////////////////////////////////////////
				  Constructor
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This guy saves a refernce to the main form so that we can
                access the keyboard buffer and the 'repeating' flag.

REVISIONS:	 9 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public KeyboardPoller(MainForm mf)
{
    mainForm = mf;
}

/* ////////////////////////////////////////////////////////////////////////////
                                GetInputByte()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function returns the next input character from the
                keyboard.

REVISIONS:       4 Sep 16 - RAC - Genesis
		 5 Sep 16 - RAC - Added calls to update the screen
//////////////////////////////////////////////////////////////////////////// */

override public byte GetInputByte()
{
    abortAllMacros = false;
    mainForm.BuildScreenImage();
    mainForm.UpdateDisplay();

    return mainForm.GetKeyFromKeyboard();
}

}						// End class KeyboardPoller

/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */
                       class MacroRunner : CommandRunner
/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

List<byte>	macro;
int		mci;				// Macro character index

/* ////////////////////////////////////////////////////////////////////////////
				  Constructor
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This guy saves a refernce to the main form so that we can
                access the keyboard buffer and the 'repeating' flag.

REVISIONS:	 9 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public MacroRunner(MainForm mf, List<byte> _macro)
{
    mainForm = mf;
    macro = _macro;
    mci = 0;
}

/* ////////////////////////////////////////////////////////////////////////////
                                GetInputByte()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function returns the next input character from the current
                macro, or 0 if we reached the end of the macro.

REVISIONS:       5 Sep 16 - RAC - Dummy skeleton
		 8 Sep 16 - RAc - Filled in for real
                10 Sep 16 - RAC - Added logic to deal with runaway macro
				   nesting
//////////////////////////////////////////////////////////////////////////// */

override public byte GetInputByte()
{

/*  If the macro nesting level ever gets too deep, we'll set 'abortAllMacros'
    true here, which will force the current and subsequent calls to this
    function to return 0, thereby terminating any and all macros that happen to
    be executing.
    
    This will eventually pop all the MacroRunner objects from the crs stack.
    At that point, the next request for input will be handled by
    KeyBoardPoller.GetInputByte(), which will return everything to normal by
    setting 'abortAllMacros' false. 
    
    We use this same mechanism to unwind the macro stack if the user hits
    Ctrl-C while macros are being run.  */

    if (mainForm.crs.Count > 1000)
    {
	abortAllMacros = true;
	MessageBox.Show("Macro nesting too deep");
    }
    if (abortAllMacros) return (byte) 0;
    if (mainForm.displayMacroExecution)
    {
	mainForm.BuildScreenImage();
	mainForm.UpdateDisplay();
    }
    return (mci < macro.Count) ? macro[mci++] : (byte) 0;
}

}						// End class MacroRunner

}						// End namespace waedit

