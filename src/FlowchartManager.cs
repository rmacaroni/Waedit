/* ////////////////////////////////////////////////////////////////////////////
                              FlowchartManager.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    This file contains code related to the flowchart viewer.

REVISIONS:       7 Aug 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace waedit
{

/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */    
                            partial class MainForm
/* @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ */    
{

/* ////////////////////////////////////////////////////////////////////////////
                                    Fields
//////////////////////////////////////////////////////////////////////////// */

NamedPipeServerStream	pipeServer;
bool			waitTaskRunning = false;
Thread			waitThread;

/* ////////////////////////////////////////////////////////////////////////////
                            ManageFlowchartViewer()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:    Execute() calls this function after every keystroke to manage
                communications with the flowchart viewer.

METHOD:		START Manage Viewer
		  IF Connected?\No\Yes
		    IF Wait task running?\No\Yes
		      Start wait task
		      Set "task running" flag
		    ENDIF
		  ENDIF
		  IF Connected?\Yes\No
		    Clear "task running" flag
		    Send buffer to pipe
		  ENDIF
		END                

REVISIONS:       9 Oct 25 - RAC - Genesis, with hints from ChatGPT
//////////////////////////////////////////////////////////////////////////// */

void ManageFlowchartViewer()
{
    if (!flowchartSupportOn) return;		// Do nothing if not enabled
    if (crs.Peek() is MacroRunner) return;	// Do nothing if in a macro

    if ((pipeServer == null ||			// No connection, so start the
         !pipeServer.IsConnected) &&		//  wait task if it's not
	!waitTaskRunning)			//  already running
    {					
        pipeServer = new NamedPipeServerStream(	// Make a new pipe
	    "EditorToFlowchart",		// Pipe name
            PipeDirection.Out,			// We be talking ...
            1,					//  ... to one client only
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        waitThread = new Thread(WaitTask);	// Fire up the wait task
        waitThread.IsBackground = true;
        waitThread.Start();
        waitTaskRunning = true;
    }        					// End 'start wait task'

    if (pipeServer != null &&			// Now try to send the buffer
        pipeServer.IsConnected)			//  if connection is alive
    {
	waitTaskRunning = false;
        try { SendBuffer(); }
        catch (IOException) { CleanupPipe(); }
        catch (ObjectDisposedException) { CleanupPipe(); }
    }
}                                               // End ManageFlowchartViewer()

/* ////////////////////////////////////////////////////////////////////////////
                                 CleanupPipe()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Called when something bad happens and when the app shuts down
		to keep everything nice and tidy.

REVISIONS:	 9 Oct 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

void CleanupPipe()
{
    try
    {
	if (pipeServer != null)
	{
	    pipeServer.Close();
            pipeServer = null;
        }
    }
    catch { /* ignore */ }
}						// End CleanupPipe()

/* ////////////////////////////////////////////////////////////////////////////
                                     SendBuffer()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This thing packages up the contents of the text buffer in a
		suitable for to the flowchart viewer, then sends along with its
                length via the pipe.

REVISIONS:	 9 Oct 25 - GPT - Dummied-up test version
		 9 Oct 25 - RAC - Real version, with hints from DoQEandU() in
                 		   executors.cs
//////////////////////////////////////////////////////////////////////////// */

void SendBuffer()
{
    List<byte>	l;				// Get text buffer data here
    string	s;				// Put here to convert newlines
    byte[]	data;

    l = text.GetBlock(0, text.GetCharCount() - 1);
    s = Encoding.ASCII.GetString(l.ToArray());
    s = s.Replace("\r","\r\n");
    data = Encoding.ASCII.GetBytes(s);

    byte[] length = BitConverter.GetBytes(data.Length);
    pipeServer.Write(length, 0, length.Length);
    pipeServer.Write(data, 0, data.Length);
    pipeServer.Flush();
}						// End SendBuffer()

/* ////////////////////////////////////////////////////////////////////////////
                                  WaitTask()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function runs in a separate thread, waiting until a
		a connection is made on the pipe server.  When that happens, it
                gleefully terminates.

NOTES:		ChatGPT suggested a bit of error handling that is not
		implemented here.  Given the target user (me), let's live on
                edge and see what happens.

		If this thing survives as a one-line function, I wonder if we
                could just pass WaitForConnection() to the Thread constructor.
                Probably.

REVISIONS:	 9 Oct 25 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

void WaitTask()
{
    pipeServer.WaitForConnection();
}

}                                               // End partial class Mainform

}                                               // End namespace waedit


