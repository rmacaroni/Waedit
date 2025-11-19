/* ////////////////////////////////////////////////////////////////////////////
			       MarginManager.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Here's where we manage the margin settings

REVISIONS:	15 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;

namespace waedit
{
partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

int[] margins = { 0, 0, 78 };			// Indent, left, right

public int indentMargin { get { return margins[0]; } }
public int leftMargin   { get { return margins[1]; } }
public int rightMargin  { get { return margins[2]; } }

/* ////////////////////////////////////////////////////////////////////////////
			      GetMarginSettings()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns a string containing the margin settings
		formatted to initialize the line editor.

REVISIONS:	15 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

string GetMarginSettings()
{
    return String.Format("{0},{1},{2}", margins[0], margins[1], margins[2]);
}

/* ////////////////////////////////////////////////////////////////////////////
			      MarginsSavedOkay()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function validates a string of margins settings as entered
		by the user.  If successful, it saves the settings and returns
		true.  Otherwise, it returns false.

REVISIONS:	15 Aug 16 - RAC - Genesis, with substantial hints from
				   TabsSavedOkay()
//////////////////////////////////////////////////////////////////////////// */

bool MarginsSavedOkay(string settings)
{
    int		i;				// A generic int
    string[]	s;				// Put margin settings here
    int[]	tempMargins;

    s = settings.Split(new char[]{','});	// Parse out the various fields

    if (s.Length > 3)				// What?  More than three
    {						//  margin settings?  No
	return false;				//  comprende, amigo
    }

    tempMargins = (int[])margins.Clone();	// Start with previous values
    for (i=0; i<s.Length; i++)			// For each of the supplied
    {						//  settings
	if (s[i] != "")				// Replace with new value if
	{					//  supplied
	    tempMargins[i] = Int32.Parse(s[i]);
	}
    }

    if ((tempMargins[2] <= tempMargins[0]) ||	// Croak if the settings are
	(tempMargins[2] <= tempMargins[1]))	//  invalid
    {
    	return false;
    }

    margins = (int[])tempMargins.Clone();	// Otherwise, use the new ones
    return true;				// All is well if we get here
}						// End MarginsSavedOkay()

}						// End class MainForm
}						// End namespace waedit

