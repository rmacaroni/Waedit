/* ////////////////////////////////////////////////////////////////////////////
                                    undo.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This file contains code related to the undo/redo mechanism.

REVISIONS:	13 Jul 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace waedit
{

/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */    
                                class Snapshot
/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */    
{

/* ////////////////////////////////////////////////////////////////////////////
                                     Data
//////////////////////////////////////////////////////////////////////////// */

public int	cursorPosition;			// What it says
public Text	buffer;				// Text buffer and tags
public int	command;			// Command that triggered the
						//  snapshot

/* ////////////////////////////////////////////////////////////////////////////
                             Snapshot Constructor
//////////////////////////////////////////////////////////////////////////// */

public Snapshot(Text text, int cmd)
{
    buffer = new Text(text); // assumes a deep copy constructor
    command = cmd;
}						// End constructor

#if DEBUG

/* ////////////////////////////////////////////////////////////////////////////
                                    Dump()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This thing dumps the interesting parts of a Snapshot object to
		the console for debugging porpoises.

REVISIONS:	14 Jul 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void Dump()
{
    string raw;

    Console.WriteLine("Command: {0}", command);
    raw = System.Text.Encoding.ASCII.GetString(	// Get buffer contents
        buffer.buffer.ToArray());
    raw = raw.Replace("\r", "\r\n");		// Fix up newlines
    raw = raw.Substring(0, raw.Length-1);	// Chop off the EOF marker
    Console.WriteLine("Buffer:");
    Console.WriteLine(raw);
}						// End Dump()

#endif						// End '#if DEBUG'

}						// End class Snapshot

/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */    
                            partial class MainForm
/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */    
{

/* ////////////////////////////////////////////////////////////////////////////
                                     Data
//////////////////////////////////////////////////////////////////////////// */

List<Snapshot>	undoHistory = new List<Snapshot>(); // The snapshot list
int	currentUndoIndex = -1;			// Index of current snapshot

#if DEBUG

/* ////////////////////////////////////////////////////////////////////////////
                               DumpUndoHistory()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Dumps all snapshots in the history using Snapshot.Dump(),
		and highlights the current one.

REVISIONS:	14 Jul 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void DumpUndoHistory()
{
    int i;					// A generic integer

    Console.WriteLine("========== Undo History ==========");

    for (i = 0; i < undoHistory.Count; i++)
    {
        string marker = (i == currentUndoIndex) ? " <== current" : "";
        Console.WriteLine("Snapshot {0}{1}:", i, marker);
        undoHistory[i].Dump();
    }

    if (undoHistory.Count == 0)
        Console.WriteLine("(empty)");

    Console.WriteLine("==================================");
}						// End DumpUndoHistory()

#endif						// End '#if DEBUG'

/* ////////////////////////////////////////////////////////////////////////////
                              TakeUndoSnapshot()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function creates a new snapshot of the editor state and
                appends it to the undo history.  Then, if the history is too
                long, it deletes the oldest snapshot.

REVISIONS:	14 Jul 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

void TakeUndoSnapshot(int command)
{
    const int MAX_SNAPSHOTS = 1000;		// Arbitrary undo history limit

    undoHistory.Add(new Snapshot(text, command));
    currentUndoIndex++;
    if (undoHistory.Count > MAX_SNAPSHOTS)	// History limit exceeded
    {
        undoHistory.RemoveAt(0);		// Remove oldest snapshot
        currentUndoIndex--;
    }						// End 'history limit exceeded'
}						// End TakeUndoSnapshot()

/* ////////////////////////////////////////////////////////////////////////////
                                 DoUndoPrep()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Execute() calls this function before executing any command.
                Under a particular set of circumstances (see the code below),
                this function clears the 'text.commandChangedBuffer' flag and
                sets the current snapshot to the current cursor position.

REVISIONS:	14 Jul 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void DoUndoPrep(int command)
{
    if ((command == 'Z')	    ||		// Bail if command is Undo, or
	(command == 'Y')	    ||		//  command is Redo, or
	(RecordingMacro())	    ||		//  we're recording a macro, or
	(crs.Peek() is MacroRunner) ||		//  we're running a macro, or
        (crs.Peek().currentState !=		//  we're not in the command
	    ST.COMMAND)) 			//  state
    {
	return;
    }
    text.NoCommandChangesYet();			// Else mark the buffer as
    undoHistory[currentUndoIndex].		//  unchanged and note current
        cursorPosition = currentChar;		//  cursor position
}

/* ////////////////////////////////////////////////////////////////////////////
                                DoUndoCleanup()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    Execute() calls this function after executing any command.
                Under a particular set of circumstances (see the code below),
                this function checks to see if the command changed the buffer.
                If it did, this function clears the redo history and then takes
                a snapshot.

REVISIONS:	14 Jul 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void DoUndoCleanup(int command)
{
    int redoCount;

    if ((command == 'Z')	    ||		// Bail if command is Undo, or
	(command == 'Y')	    ||		//  command is Redo, or
	(RecordingMacro())	    ||		//  we're recording a macro, or
	(crs.Peek() is MacroRunner) ||		//  we're running a macro, or
        (crs.Peek().currentState !=		//  we're not in the command
	    ST.COMMAND)) 			//  state
    {
	return;
    }
    if (text.CommandChangedBuffer())		// Buffer changed
    {
	redoCount = undoHistory.Count -		// Number of redo snapshots
        	currentUndoIndex - 1;		//  present
	if (redoCount > 0)			// If there are any redos,
        {					//  remove them from the list
	    undoHistory.RemoveRange(
                currentUndoIndex + 1, redoCount);
        }
	TakeUndoSnapshot(command);
    }						// End 'buffer changed'
}						// End DoUndoCleanup()

/* ////////////////////////////////////////////////////////////////////////////
                                    Undo()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function implements the undo command.

REVISIONS:	14 Jul 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

void Undo()
{
    Snapshot snap;				// Put snapshot here

    if (currentUndoIndex <= 0)			// Nothing to undo
    {
	MessageBox.Show("Nothing to undo.");	// Complain to user
        return;
    }
    snap = undoHistory[--currentUndoIndex];	// Back up and get snapshot
    text = new Text(snap.buffer);		// Update the text buffer and
    currentChar = snap.cursorPosition;		//  cursor position
}						// End Undo()

/* ////////////////////////////////////////////////////////////////////////////
                                    Redo()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function implements the redo command.

REVISIONS:	14 Jul 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

void Redo()
{
    Snapshot snap;				// Put snapshot here

    if (currentUndoIndex >= undoHistory.Count - 1)
    {
	MessageBox.Show("Nothing to redo.");	// Complain to user
        return;
    }
    snap = undoHistory[++currentUndoIndex];	// Advance one and get snapshot
    text = new Text(snap.buffer);		// Update the text buffer and
    currentChar = snap.cursorPosition;		//  cursor position
}						// End Redo()

}						// End partial class Mainform

}						// End namespace waedit

