/* ////////////////////////////////////////////////////////////////////////////
				 utilities.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Here lie various utility functions that would otherwise be
		homeless.

REVISIONS:	 9 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System.Collections.Generic;

namespace waedit
{

partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
                                   ToUpper()
                                   ToLower()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	ToUpper() and ToLower() functions that probably make all kinds
                of invalid assumptions in order to operate on int variables
                instead of chars.

REVISIONS:	 9 Aug 16 - RAC - Moved ToUpper() here from executor.cs
		18 Sep 16 - RAC - Added ToLower()
//////////////////////////////////////////////////////////////////////////// */

public static int ToUpper(int i)
{
    if ((i >= 'a') && (i <= 'z'))
    {
	i -= 32;
    }
    return i;
}

/* ///////////////////////////////////////////////////////////////////////// */

public static int ToLower(int i)
{
    if ((i >= 'A') && (i <= 'Z'))
    {
	i += 32;
    }
    return i;
}

/* ////////////////////////////////////////////////////////////////////////////
			     DetectLineDelimiter()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function searches for the first likely line delimiter in a
		given string, and returns same if found.  Otherwise it returns
		the default "\r\n".

REVISIONS:	20 Aug 16 - RAC - Adapted from a similar function in Mbedit.
//////////////////////////////////////////////////////////////////////////// */

string DetectLineDelimiter(string s)
{
    int		i;
    string	rv;

    rv = "\r\n";				// Assume CR/LF
    for (i=0; i<s.Length; i++)			// Search for first EOL
    {
	if (s[i] == '\n')			// Found a LF
	{
	    rv = "\n";				// That's all we need to know
	    break;
	}
	if (s[i] == '\r')			// Found a CR
	{
	    rv = "\r";				// Assume no following LF 
	    if (i < s.Length - 1)		// There is one more character
	    {
		if (s[i+1] == '\n')		// It's a newline
		{
		    rv = "\r\n";		// That changes everything
		}
	    }
	    break;
	}
    }
    return rv;
}						// End DetectLineDelimiter()

/* ////////////////////////////////////////////////////////////////////////////
			`	   GetLine()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns a specified line from the text buffer.

REVISIONS:	27 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

List<byte> GetLine(int line)
{
    return
        text.GetBlock(text.GetLineStart(line),
        text.GetLineStart(line+1) -
        text.GetLineStart(line));
}

/* ////////////////////////////////////////////////////////////////////////////
                                IsADelimiter()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns true if the specified character is in the
		currently defined token delimiter set, or false otherwise.

REVISIONS:	23 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public static bool IsADelimiter(int b)
{
    if ((b <= ' ') || (b > '~'))		// All non-printable characters
    {						//  and ' ' are considered to
	return true;				//  be delimiters ...
    }
    return delimiterSet.IndexOf((char)b) != -1;	// ... as are all charaxcters
}						//  in 'delimiterSet'

}						// End class MainForm
}						// End namespace waedit
