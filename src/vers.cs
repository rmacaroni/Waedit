/* ////////////////////////////////////////////////////////////////////////////
                                    vers.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This file contains a version string and a revision log for
		Waedit

Version 1.0	17 Jan 26 - As published on GitHub on this date

Versioh 1.1	?? ??? ?? - Disabled Autosize on the status strip so multi-line
			     strings don't mess things up.
                          - Fixed problem where B -> P -> Ctrl-C sent cursor to
                             the beginning of the file.
                          - Set a minimum size for the form to make sure at
                             least one character is visible on the screen.
                             This avoids some problems with the cursor
                             positioning logic.
                          - Fixed so the cursor is no longer sometimes hidden
                             by the status bar.
//////////////////////////////////////////////////////////////////////////// */

namespace waedit
{

partial class MainForm
{
    const string VERSION_STRING = "Waedit 1.1";
}						// End partial class Mainform
}                                               // End namespace waedit

