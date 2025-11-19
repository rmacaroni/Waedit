/* ////////////////////////////////////////////////////////////////////////////
                                 LineEditor.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This class implements a simple string editor for use with
                Waedit.

REVISIONS:       12 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;

namespace waedit
{
public class LineEditor
{

/* ////////////////////////////////////////////////////////////////////////////
			       Names for Numbers
//////////////////////////////////////////////////////////////////////////// */

enum LMD
{
    LEFT,
    RIGHT
}

/* ////////////////////////////////////////////////////////////////////////////
                                     Data
//////////////////////////////////////////////////////////////////////////// */

string s;               // The string being edited, plus a dummy blank at the
			//  end so we'll have someplace to show the cursor even
			//  if the string is empty.

int cursorIndex;
LMD lastMoveDirection;
string allowedCharacters;
bool firstKey;

/* ////////////////////////////////////////////////////////////////////////////
                                  Constructor
//////////////////////////////////////////////////////////////////////////// */

public LineEditor(string initialString, string okCharacters)
{
    allowedCharacters = okCharacters;
    s = initialString + " ";
    cursorIndex = 0;
    lastMoveDirection = LMD.LEFT;
    firstKey = true;
}

/* ////////////////////////////////////////////////////////////////////////////
                                    Edit()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This function receives a single input byte and edits the string
                according to the following rules:

                INPUT   ACTION
                -----   ------
                Ctrl-Z  Erase the entire string
                Ctrl-A  Erase from the cursor to the end of the string
                Ctrl-X  Erase from the cursor to the beginning of the string
                Left    If possible, move the cursor one position to the left
                Right   If possible, move the cursor one position to the right
                Home    Move the cursor to the beginning or end of the string,
			 according to the last move direction.
                Bksp    If possible, delete the byte to the left of the cursor
                Delete  If possible, delete the byte under the cursor
                ---------------------------------------------------------------
                Others  If no other keys have yet been hit, then replace the
			 entire string with the new byte.  Otherwise, just add
			 the byte to the string at the cursor position.

REVISIONS:      12 Aug 16 - RAC - Genesis
		14 Aug 16 - RAC - Expanded the rule for "other" bytes as
				   specified above.
//////////////////////////////////////////////////////////////////////////// */

public void Edit(int herb)
{
    switch (herb)
    {
        case C.CTRL_Z:
	    s = " ";
            cursorIndex = 0;
        break;

        case C.CTRL_A:
            s = s.Remove(cursorIndex) + " ";
        break;

        case C.CTRL_X:
            s = s.Substring(cursorIndex);
            cursorIndex = 0;
        break;

        case C.LEFT:
	    cursorIndex = Math.Max((cursorIndex - 1), 0);
	    lastMoveDirection = LMD.LEFT;
        break;

        case C.RIGHT:
	    cursorIndex = Math.Min((cursorIndex + 1), s.Length - 1);
	    lastMoveDirection = LMD.RIGHT;
        break;

        case C.HOME:
	    cursorIndex = (lastMoveDirection == LMD.LEFT) ? 0 : s.Length - 1;
        break;

        case '\b':
	    if (cursorIndex > 0)
	    {
		s = s.Remove(--cursorIndex, 1);
	    }
        break;

        case C.DELETE:
	    if (cursorIndex < (s.Length - 1))
	    {
		s = s.Remove(cursorIndex, 1);
	    }
        break;

	default:
	    if (allowedCharacters != "")
	    {
		if (allowedCharacters.IndexOf((char)herb) == -1)
		{
		    return;
		}
	    }
	    if (firstKey)
	    {
		s = " ";
	    }
            s = s.Insert(cursorIndex,
                System.Text.Encoding.ASCII.
                GetString(new[]{(byte)herb}));
	    cursorIndex++;
	break;
    }                                           // End switch
    firstKey = false;
}                                               // End Edit()

/* ////////////////////////////////////////////////////////////////////////////
			     GetStringWithBlank()
			    GetStringWithoutBlank()
			      GetCursorPosition()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	These functions do what you'd expect from their names.

REVISIONS:	13 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public string GetStringWithBlank()
{
    return s;
}						// End GetStringWithBlank()

/* ///////////////////////////////////////////////////////////////////////// */

public string GetStringWithoutBlank()
{
    return s.Substring(0, s.Length-1);
}						// End GetStringWithBlank()

/* ///////////////////////////////////////////////////////////////////////// */

public int GetCursorPosition()
{
    return cursorIndex;
}						// End GetCursorPosition()

}                                               // End class LineEditor
}						// End namespace

