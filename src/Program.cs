using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace waedit
{
static class Program
{

/* ////////////////////////////////////////////////////////////////////////////
				    Main()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Where it all begins.

REVISIONS:	22 Aug 16 - RAC - Added command line parameter processing to
				   VS2008 generated skeleton
		17 Sep 16 - RAC - No longer supports the output file
                22 Sep 16 - RAC - Restored "TO NUL" syntax to open file for
				   read-only access
//////////////////////////////////////////////////////////////////////////// */

const string syntaxMessage = "Syntax: WAEDIT [filename]";

[STAThread]

static void Main(string[] args)
{
    MainForm mf;

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    mf = new MainForm();

    if (args.Length == 0)			// No parameters given, so we
    {						//  don't have a filename
	mf.currentFilename = "";
    }

    if (args.Length >= 1)			// Always use first parameter
    {						//  (if given) as the input
	mf.currentFilename  = args[0];		//  filename
    }

    if ((args.Length == 3) &&			// Mark the file as read-only
	(args[1].ToUpper() == "TO") &&		//  if followed on the command
        (args[2].ToUpper() == "NUL"))		//  line by "TO NUL"
    {
	mf.readOnly = true;
    }
    Application.Run(mf);			// Ok so far - start up the GUI
}						// End Main()

}						// End class Program 
}						// End namespace waedit

