/* ////////////////////////////////////////////////////////////////////////////
				  keyboard.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Here's where we keep the FIFO buffer for editor input.  During
                normal operation, event handlers for the main form place
                keystrokes into this buffer as they occur.  In either case, the
                Executor pulls the keystrokes from the buffer as it needs them.

REVISIONS:	27 Jul 16 - RAC - Genesis, in a preliminary test program
		 3 Aug 16 - RAC - Ported to here
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;
using System.Threading;

namespace waedit
{

partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

Queue<byte> keyboardBuffer = new Queue<byte>();
object bufferLock = new object();
EventWaitHandle waitHandle = new AutoResetEvent(false);

/* ////////////////////////////////////////////////////////////////////////////
				  QueueKey()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The KeyPress and KeyDown handlers call this function to put
                incoming keystrokes into a FIFO buffer that's read by the main
                editor loop thread.

REVISIONS:	27 Jul 16 - RAC - Genesis, in a preliminary test program
		 3 Aug 16 - RAC - Ported to here
//////////////////////////////////////////////////////////////////////////// */

void QueueKey(byte herb)
{
    lock (bufferLock)
    {
	if (herb == C.CTRL_C)			// Toss anything in the FIFO if
	{					//  the user hits Ctrl-C
	    keyboardBuffer.Clear();
	}
	keyboardBuffer.Enqueue(herb);		// Add new byte to the FIFO
    }						// End lock
    waitHandle.Set();				// Signal new byte availability
}						// End QueueKey()

/* ////////////////////////////////////////////////////////////////////////////
			     GetKeyFromKeyboard()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The main editor loop calls this function to get input keys.  If
                no keyboard input is available, this function blocks.

REVISIONS:	27 Jul 16 - RAC - Genesis, in a preliminary test program
		 3 Aug 16 - RAC - Ported to here
		 2 Sep 16 - RAC - Fixed to capture macro keystrokes
//////////////////////////////////////////////////////////////////////////// */

public byte GetKeyFromKeyboard()
{
    byte herb;

    while (true)
    {
	lock (bufferLock)
	{
	    if (keyboardBuffer.Count > 0)
	    {
		herb = keyboardBuffer.Dequeue();
		if (RecordingMacro())
                {
		    macros[newMacroName].Add(herb);
                }
		return herb;
	    }
	}					// End lock
	waitHandle.WaitOne();			// Block for non-empty FIFO
    }						// End while
}						// End GetKeyFromKeyboard()

/* ////////////////////////////////////////////////////////////////////////////
			       CtrlCIsWaiting()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns true if a Ctrl-C is sitting at the head
                of the queue, or false otherwise.  ManageRepeats() (in
                commandrunners.cs) calls this function periodically to see if
                the user wants to abort some lengthy operation.

REVISIONS:	 1 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public bool CtrlCIsWaiting()
{
    return ((keyboardBuffer.Count > 0) &&
	    (keyboardBuffer.Peek() == C.CTRL_C));
}

}						// End class MainForm
}						// End namespace waedit
