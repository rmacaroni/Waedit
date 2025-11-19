/* ////////////////////////////////////////////////////////////////////////////
				 TabManager.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Here lie various tab-related functions

REVISIONS:	15 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;

namespace waedit
{
partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

/*  This list contains the currently defined tab stops, including a dummy 0 at
    the beginning of the list to make things easy.  */

List<int> tabStops = new List<int>(new int[]{0, 8});

/* ////////////////////////////////////////////////////////////////////////////
				 GetTabStop()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns the next tab stop beyond a specified
		position.

REVISIONS:	15 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

int GetTabStop(int poz)
{
    int i;
    int diff;
    
    i = tabStops.Count - 1;			// Index of last tab stop
    if (poz < tabStops[i])
    {
	while (i > 0)
	{
	    if (poz >= tabStops[i-1])
	    {
		return tabStops[i];
	    }
	    i--;
	}
	throw new Exception("Coding error in GetTabStop()");
    }
    else
    {
	diff = tabStops[i] - tabStops[i-1];
	return tabStops[i] + ((((poz - tabStops[i]) / diff) + 1) * diff);
    }
}						// End GetTabStop()

/* ////////////////////////////////////////////////////////////////////////////
			       GetTabSettings()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns a string containing a comma-separated
		list of the current tab stops, excluding the dummy zero.  The
		function DoST() in executor.cs uses it to initialize the line
		editor with the current tab settings as part of the Set Tabs
		command.

REVISIONS:	15 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

string GetTabSettings()
{
    string rv;
    int i;

    rv = tabStops[1].ToString();
    for (i=2; i<tabStops.Count; i++)
    {
	rv += "," + tabStops[i].ToString();
    }
    return rv;    
}

/* ////////////////////////////////////////////////////////////////////////////
				TabsSavedOkay()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function validates a string of tab settings as entered by
		the user.  If successful, it saves the settings in 'tabStops'
		and returns true.  Otherwise it returns false.

REVISIONS:	15 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

bool TabsSavedOkay(string tabs)
{
    string[]	s;				// Parse tabs into this array
    List<int>	tempTabStops;
    int		lastOne;
    int		tabStop;
    int		i;

    s = tabs.Split(new char[]{','},		// Parse tabs into an array of
	StringSplitOptions.RemoveEmptyEntries);	//  strings
    if (s.Length == 0)				// We gotta have at least one
    {						//  tab stop specified
	return false;
    }
    tempTabStops = new List<int>(new int[]{0});	// Start with the dummy zero
    lastOne = 0;
    foreach(string t in s)			// For each tab setting
    {
	tabStop = Int32.Parse(t);		// Convert to integer
	if (tabStop > lastOne)			// If monotonically increasing,
	{					//  add to accumulating list
	    tempTabStops.Add(tabStop);
	    lastOne = tabStop;
	}
	else					// Not monotonically increasing
	{					// That's bad
	    return false;
	}					// End 'not ... increasing'
    }						// End 'for each tab setting'

/*  We have a list of monotonically increasing tab stops if we get here.  Now
    work backwards from the end of the list and remove any redundant settings.
    */

    for (i=tempTabStops.Count-1; i>=2; i--)
    {
	if ((tempTabStops[i]   - tempTabStops[i-1]) ==
	    (tempTabStops[i-1] - tempTabStops[i-2]))
	{
	    tempTabStops.RemoveAt(i);
	}
	else
	{
	    break;
	}
    }
    tabStops = tempTabStops;			// Save fixed-up list
    return true;				// All is well if we get here
}						// End TabsSavedOkay()


}						// End class MainForm
}						// End namespace waedit

