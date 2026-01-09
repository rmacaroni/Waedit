Margins are at 8, 11, 78

# Differences Between AEDIT and Waedit

The table below explains the major differences between Waedit and Intel's
original AEDIT program.  The first column identifies the page or pages where
the feature or behavior is documented in the AEDIT manual.  The second column
briefly describes how AEDIT works, and the third column explains how Waedit
differs from AEDIT.

<table>
  <thead>
    <tr>
      <th align="center">Manual Page</th>
      <th align="center">How AEDIT Works</th>
      <th align="center">How Waedit Works</th>
    </tr>
  </thead>
  <tbody>
*******************************************************************************
    <tr>
      <td align="center">
        <p>12</p>
      </td>
      <td>
        <p>AEDIT is activated by typing AEDIT <CR></p>
      </td>
      <td>
        <p>The Waedit executable is in the file Waedit.EXE.  It can be invoked
           in several ways (e.g.  from the command line, from a shortcut, or by
           clicking a file that has been associated with Waedit) like any other
           Windows program.</p>
      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>12, 19</p>
      </td>
      <td>
        <p>AEDIT shows both a "Message Line" and a "Prompt Line" at the bottom
           of the display.</p>
      </td>
      <td>
        <p>Waedit displays prompts in a single status bar, and     displays
           mesages by either temporarily overwriting the prompts, or via pop-up
           windows.  Waedit's status bar also shows the current editing mode
           (Command, Insert, or Exchange) along with the current cursor
           position.</p>
      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>13</p>
      </td>
      <td>
        <p>AEDIT was designed to work with many different terminals, and
           therefore provides a mechanism to assign various program functions
           to terminal keys.  </p>
      </td>
      <td>
        <p>In Waedit, the assignment of program functions to keyboard keys is
           fixed, generally following standard Windows convention.  A separate
           table near the end of this document specifies these assignments
           completely.  To help make sense of the tutorial in Chapter 1 of the
           AEDIT manual, here's a quick list of the function-to-key assignments
           mentioned there:</p>
         <ul>
         <li> rubout = Backspace </li>
         <li> delch  = Del </li>
         <li> delli = Ctrl-Z </li>
         <li> delr = Ctrl-A </li>
         </ul>
      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>14</p>
      </td>
      <td>

        <p>AEDIT indicates the beginning and end of a selected block of text
           with the @ sign.</p>

      </td>
      <td>

        <p>Waedit highlights selected text blocks in white text on a dark
           backtround.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>15</p>
      </td>
      <td>

        <p>AEDIT has an "Other" command that enables editing two files
           simultaneously.  </p>

      </td>
      <td>

        <p>Waedit does not implement the "Other" command.  If you want to edit
           two (or more) files at the same time, invoke multiple copies of
           Waedit.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>19</p>
      </td>
      <td>

        <p>AEDIT implements fixed 80-column display.</p>

      </td>
      <td>

        <p>Waedit runs in a window that can be resized to any reasonable
           dimensions.  </p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>20, 22</p>
      </td>
      <td>

        <p>AEDIT prompts for both single-character commands and longer strings
           on the prompt line.</p>

      </td>
      <td>

        <p>Waedit uses pop-up dialogs to prompt for longer strings.  </p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>22</p>
      </td>
      <td>

        <p>AEDIT allows line-edited input up to 60 characters in length, and
           provides a mechanims for entering control characters.  </p>

      </td>
      <td>

        <p>In Waedit, line-edited input can be of any length, but there is no
           way to enter control characters.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>22</p>
      </td>
      <td>

        <p>AEDIT displays certain status information in a message line</p>

      </td>
      <td>

        <p>Waedit has no corresponding display, largely because the status
           displayed either does not apply to Waedit, or is displayed in some
           other way.  </p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>23</p>
      </td>
      <td>

        <p>AEDIT beeps to warn of certain illegal input</p>

      </td>
      <td>

        <p>Waedit is mercifully silent</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>23</p>
      </td>
      <td>

        <p>AEDIT splits long lines into 255-character segments, and implements
           a somewhat cumbersome method for viewing lines longer than the
           80-character display width.</p>

      </td>
      <td>

        <p>Waedit supports lines of unlimited length.  It displays however many
           characters will fit into the current window, with automatic
           horizontal and vertical scrolling to keep the cursor always in
           view.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>23</p>
      </td>
      <td>

        <p>AEDIT assumes lines are terminated by CR/LF pairs.</p>

      </td>
      <td>

        <p>When reading a file, Waedit detects whether the lines are terminated
           by CRs, LFs, or CR/LF pairs, and handles the input accordingly.  It
           always writes files with CR/LF line terminators.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>24</p>
      </td>
      <td>

        <p>AEDIT displays the command execution count on the mssage line./p>

      </td>
      <td>

        <p>Waedit temporarily replaces the "mode" field of the status bar with
           the command execution count as it is being entered.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>24</p>
      </td>
      <td>

        <p>AEDIT implements three buffers as described in the manual.</p>

      </td>
      <td>

        <p>Since Waedit does not implement the "Other" command, it doesn't
        have anything corresponding to AEDIT's "OTHER" buffer.</p>
        <p>Waedit uses the Windows clipboard in place of AEDIT's 2K "block
        buffer".  This removes the 2K limit, and allows for easy cutting and
        pasting of text between Waedit and other programs. </p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>25, 26</p>
      </td>
      <td>

        <p>In AEDIT, there is "no recovery" from some of the delete commands,
           and AEDIT explicitly limits the count on the delch command do
           prevent accidental destruction of the file.</p>

      </td>
      <td>

        <p>Waedit has an Undo/Redo feature that will recover from such
           accidents.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>27</p>
      </td>
      <td>

        <p>The AEDIT manual suggests configuring the Undo command to
           Ctrl-Y for some obscure reason related to a mysterious "Terminal
           Support Code".</p>

      </td>
      <td>

        <p>In Waedit, the assignment of program functions to keyboard keys is
           fixed.  The Undo command is assigned to Ctrl-U.</p>

        <p> <b>Note:</b> The undo function invoked via Ctrl-U specifically
           relates to the resoration of text deleted by the most recent delete
           left (Ctrl-X), delete right (Ctrl-A), or delete line (Ctrl-Z)
           command.  Do not confuse this with the global Undo command described
           later in this document that reverses the effect of <i>any</i>
           command that changes the text buffer.  </p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>28</p>
      </td>
      <td>

        <p>Ctrl-C and the forward shash affect Insert Mode as described in the
        AEDIT manual.</p>

      </td>
      <td>

        <p>In Waedit, the behaviors described for Ctrl-C and the forward shash
           are not implemented.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>29</p>
      </td>
      <td>

        <p>In AEDIT, Xchange mode works as described in the manual.</p>

      </td>
      <td>

        <p>In Waedit, there is no limit to the number of characters that can be
           exchanged.  In addition, rubout works slightly differently, and
           Ctrl-C is ignored.  </p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>30, 31</p>
      </td>
      <td>

        <p>The AEDIT Find and -Find commands works as described in the
           manual.</p>

      </td>
      <td>

        <p>The corresponding commands in Waedit work generally the same, except
           the target string is entered via a pop-up dialog rather than the
           prompt line, and Waedit does not report the number of strings found
           or the "not found" message.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>32, 33</p>
      </td>
      <td>

        <p>The AEDIT Replace and ?Replace commands work as described in the
           manual.</p>

      </td>
      <td>

        <p>The corresponding commands in Waedit work generally the same, except
           the target and replacement strings are entered via pop-up dialogs
           rather than the prompt line, and Waedit does not report the "not
           found" message.</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
*******************************************************************************
    <tr>
      <td align="center">
        <p>##</p>
      </td>
      <td>

        <p>AEDIT Behavior</p>

      </td>
      <td>

        <p>Waedit behavior</p>

      </td>
    </tr>
  </tbody>
</table>

Various function keys:

rubout
delch
dell
delr
delli
undo (Ctrl-U)
mexec
fetn
fets
