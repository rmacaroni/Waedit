/* ////////////////////////////////////////////////////////////////////////////
			      LineEditorManager()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This module contains logic for using the line editor.

REVISIONS:	13 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;

namespace waedit
{
partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

LineEditorForm	lef = null;
LineEditor	le;

/* ////////////////////////////////////////////////////////////////////////////
			      OpenLineEditor()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Various keyboard event handlers call this function when they
		need to get a string from the user.  This function invokes the
		line editor in a thread-safe way and displays it centered on
		the main form.

REVISIONS:	13 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

delegate void OLE_Callback(string l, string i, string ok);

void OpenLineEditor
(
    string labelText,	    // Display this string on the line editor dialog
    string initialText,	    // Initialize the editor with this string
    string ok		    // If non-null, accept only these characters
) {
    if (InvokeRequired)
    {
	OLE_Callback d = new OLE_Callback(OpenLineEditor);
	Invoke(d, new object[] { labelText, initialText, ok });
    }
    else
    {
	le = new LineEditor(initialText, ok);
	if (crs.Count == 1)
        {
	    lef = new LineEditorForm(labelText, initialText, ok, this, le);
	    lef.Show();
	}
    }
}						// End OpenLineEditor()

/* ////////////////////////////////////////////////////////////////////////////
			       CloseLineEditor()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function closes the line editor.

REVISIONS:	14 Aug 16 - RAC - Genesis
		17 Aug 16 - RAC - Made thread safe
//////////////////////////////////////////////////////////////////////////// */

delegate void CLE_Callback();

void CloseLineEditor()
{
    if (InvokeRequired)
    {
	CLE_Callback d = new CLE_Callback(CloseLineEditor);
	Invoke(d);
    }
    else
    {
	if (lef != null)
	{
	    lef.Close();
	    lef = null;
	}
    }
}

/* ////////////////////////////////////////////////////////////////////////////
			       FeedLineEditor()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function passes a byte to the line editor.

REVISIONS:	14 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void FeedLineEditor(int herb)
{
    if (lef != null) lef.StopCursor();
    le.Edit(herb);
    if (lef != null) lef.StartCursor();
}

/* ////////////////////////////////////////////////////////////////////////////
				GetLineEditorResult()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function grabs the edited string from the line editor.  If
                'trim' is false, it returns the entire string.  Otherwise, it
                returns only the part to the left of the cursor position.

REVISIONS:	14 Aug 16 - RAC - Genesis
		21 Aug 16 - RAC - Added 'trim' parameter
//////////////////////////////////////////////////////////////////////////// */

string GetLineEditorResult(bool trim)
{
    return trim ?
	le.GetStringWithoutBlank().Substring(0, le.GetCursorPosition()) :
	le.GetStringWithoutBlank();
}

}						// End class MainForm
}						// End namespace waedit

