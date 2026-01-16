Need a link in this file to the AEDIT manual.
Need a link in this file to the notes.html file.
Need a link in this file to the examples.mac file.

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
    <tr>
      <td align="center">
        <p>--</p>
      </td>
      <td>
        <p> AEDIT does not have a true Undo/Redo function.  </p>
      </td>
      <td>
        <p> Waedit captures a snapshot of the text buffer after every command
           that changes the text.  Repeatedly pressing Z in the command mode
           will undo the most recent changes by backing up one at a time
           through the most recent snapshots.  Waedit will store up to 1000
           snapshots.  When that limit is reached, it deletes the oldest
           snapshots as needed to make room for new ones.  </p>
        <p> Pressing Y at the command line cycles <i>forward</i> through the
           stored snapshots, effectively redoing any previous commands that
           were undone by pressing Z.  </p>
        <p> The Z (Undo) command issues an error if an attempt is made to back
           up past the beginning of the stored snapshots.  Likewise, the Y
           (Redo) command issues an error if an attempt is made to go beyond
           the end of the stored snapshots.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>--</p>
      </td>
      <td>
        <p> AEDIT has no mouse support. </p>
      </td>
      <td>
        <p> Waedit has limited mouse support: </p>
        <ul>
          <li> Right-clicking on a character will move the cursor to that
               character.  </li>
          <li> The scroll wheel will scroll the text vertically.  </li>
        </ul>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>12</p>
      </td>
      <td>
        <p>AEDIT is activated by typing AEDIT <CR></p>
      </td>
      <td>
        <p>The Waedit executable is in the file Waedit.EXE.  It can be invoked
           in several ways (e.g. from the command line, from a shortcut, or by
           clicking a file that has been associated with Waedit) like any other
           Windows program.</p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>12, 19</p>
      </td>
      <td>
        <p>AEDIT shows both a "Message Line" and a "Prompt Line" at the bottom
           of the display.</p>
      </td>
      <td>
        <p>Waedit displays prompts in a single status bar, and displays
           messages by either temporarily overwriting the prompts, or via
           pop-up windows.  Waedit's status bar also shows the current editing
           mode (Command, Insert, or Exchange) along with the current cursor
           position.</p>
      </td>
    </tr>
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
           fixed, generally following standard Windows convention.  Here are
           the assignments:</p>
         <ul>
         <li> rubout = Backspace </li>
         <li> delch  = Del </li>
         <li> delli = Ctrl-Z </li>
         <li> delr = Ctrl-A </li>
         <li> dell = Ctrl-X </li>
         <li> undo = Ctrl-U </li>
         <li> mexec = Ctrl-E </li>
         <li> fetn = Ctrl-N </li>
         <li> fets = Ctrl-V </li>
         </ul>
      </td>
    </tr>
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
           background.</p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>15, 41</p>
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
    <tr>
      <td align="center">
        <p>19</p>
      </td>
      <td>
        <p>AEDIT implements a fixed 80-column display.</p>
      </td>
      <td>
        <p>Waedit runs in a window that can be resized to any reasonable
           dimensions.  </p>
      </td>
    </tr>
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
    <tr>
      <td align="center">
        <p>22</p>
      </td>
      <td>
        <p>AEDIT allows line-edited input up to 60 characters in length, and
           provides a mechanism for entering control characters.  </p>
      </td>
      <td>
        <p>In Waedit, line-edited input can be of any length, but there is no
           way to enter control characters.</p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>22</p>
      </td>
      <td>
        <p>AEDIT displays certain status information in a message line.</p>
      </td>
      <td>
        <p>Waedit has no corresponding display, largely because the status
           displayed either does not apply to Waedit, or is displayed in some
           other way.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>23</p>
      </td>
      <td>
        <p>AEDIT beeps to warn of certain illegal input.</p>
      </td>
      <td>
        <p>Waedit is mercifully silent.</p>
      </td>
    </tr>
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
        <p>Waedit supports lines of unlimited length.  It displays as many
           characters as will fit into the current window, with automatic
           horizontal and vertical scrolling to keep the cursor always in
           view.</p>
      </td>
    </tr>
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
    <tr>
      <td align="center">
        <p>24</p>
      </td>
      <td>
        <p>AEDIT displays the command execution count on the message line.</p>
      </td>
      <td>
        <p>Waedit temporarily replaces the "mode" field of the status bar with
           the command execution count as it is being entered.</p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>24</p>
      </td>
      <td>
        <p>AEDIT implements three buffers as described in the manual.</p>
      </td>
      <td>
        <p>Since Waedit does not implement the "Other" command, it doesn't have
           anything corresponding to AEDIT's "OTHER" buffer.  </p>
        <p> Waedit uses the Windows clipboard in place of AEDIT's 2K byte
           "block buffer".  This removes the 2K limit, and allows for easy
           cutting and pasting of text between Waedit and other programs.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>25, 26</p>
      </td>
      <td>
        <p>In AEDIT, there is "no recovery" from some of the delete commands,
           and AEDIT explicitly limits the count on the delch command to
           prevent accidental destruction of the file.</p>
      </td>
      <td>
        <p>Waedit has an Undo/Redo feature that will recover from such
           accidents.</p>
      </td>
    </tr>
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
           relates to the retrieval of text deleted by the most recent delete
           left (Ctrl-X), delete right (Ctrl-A), or delete line (Ctrl-Z)
           command.  Do not confuse this with the global Undo command described
           at the beginning of this document that reverses the effect of
           <i>any</i> command that changes the text buffer.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>28</p>
      </td>
      <td>
        <p>Ctrl-C and the forward slash affect Insert Mode as described in the
        AEDIT manual.</p>
      </td>
      <td>
        <p>In Waedit, the behaviors described for Ctrl-C and the forward slash
           are not implemented.</p>
      </td>
    </tr>
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
    <tr>
      <td align="center">
        <p>30, 31</p>
      </td>
      <td>
        <p>The AEDIT Find and -Find commands work as described in the
           manual.</p>
      </td>
      <td>
        <p>The corresponding commands in Waedit work generally the same, except
           the target string is entered via a pop-up dialog rather than the
           prompt line, and Waedit does not report the number of strings found
           or the "not found" message.</p>
      </td>
    </tr>
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
    <tr>
      <td align="center">
        <p>35</p>
      </td>
      <td>
        <p>In AEDIT, the jump command does not have a match subcommand.</p>
      </td>
      <td>
        <p>In Waedit, the match subcommand, executed by pressing M, works like
           this: If the cursor is on a parenthesis, a square bracket, or a
           curly brace, the cursor moves to the matching character if one
           exists.  Otherwise, the cursor does not move.</p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>36, 37</p>
      </td>
      <td>
        <p> In AEDIT, the Buffer and Delete subcommands use an internal 2K byte
           "block buffer" to store blocks of text.</p>
      </td>
      <td>
        <p> Waedit uses the Windows clipboard in place of AEDIT's 2K byte
           "block buffer".  This removes the 2K limit, and allows for easy
           cutting and pasting of text between Waedit and other programs.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>39</p>
      </td>
      <td>
        <p>In AEDIT, the Get command retrieves the contents of its block buffer
           if no filename is given.  </p>
      </td>
      <td>
        <p>In Waedit, the Get command retrieves the contents of the Windows
           clipboard if no filename is given.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>40</p>
      </td>
      <td>
        <p>AEDIT's View command works as described in the manual.</p>
      </td>
      <td>
        <p>Waedit does not support the "set viewrow" command.  In Waedit, the
           viewrow is always set to the default R/5, as described in the
           manual.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>15, 41</p>
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
    <tr>
      <td align="center">
        <p>43</p>
      </td>
      <td>
        <p>In AEDIT, the Set command has several subcommands as described in
           the manual.</p>
      </td>
      <td>
        <p>Waedit does not support these Set subcommands:</p>
         <ul>
         <li> Autonl </li>
         <li> Bak-File </li>
         <li> Go </li>
         <li> Highbit </li>
         <li> Leftcol </li>
         <li> Showfind </li>
         <li> Viewrow </li>
         </ul>
       <p> Waedit adds these Set subcommands:</p>
         <ul>
         <li> Font - Sets the height of the displayed font, in pixels.</li>
         <li> flOw - Enables interaction with a proprietary flowchart drawing
              program.  Has no effect if the flowchart program is not
              installed. </li>
         </ul>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>52</p>
      </td>
      <td>
        <p>AEDIT has a Hex command as described in the manual. </p>
      </td>
      <td>
        <p>Waedit does not implement the Hex command. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>54-56</p>
      </td>
      <td>
        <p>AEDIT's Quit command works as described in the manual. </p>
      </td>
      <td>
        <p>Waedit's Quit command is similar but somewhat simpler because Waedit
           does not support the secondary file, the viewonly option, or the
           forwardonly option.</p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>58</p>
      </td>
      <td>
        <p>AEDIT's Window command works as described in the manual. </p>
      </td>
      <td>
        <p>Waedit does not support the Window subcommand. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>59</p>
      </td>
      <td>
        <p>In AEDIT, pressing K invokes the Kill_wnd command. </p>
      </td>
      <td>
        <p>Waedit does not implement the Kill_wnd command.  However, Waedit
           does have an entirely unrelated Kill command that provides for
           conditional execution within macros.  See the entries dated
           September 18 and later in the notes.html file for details.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>60</p>
      </td>
      <td>
        <p>AEDIT's !system command works as described in the manual.  </p>
      </td>
      <td>
        <p>Waedit does not support the !system command.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>65-76</p>
      </td>
      <td>
        <p>AEDIT is invoked as described in Chapter 4 of the manual. </p>
      </td>
      <td>
        <p> In Waedit, the only command line parameter is the input filename.
           So typing "Waedit <i>filename</i>" opens <i>filename</i> for
           editing, while typing "Waedit" (without a filename) starts Waedit
           with an empty buffer.  You can also invoke Waedit by selecting a
           file with a file type (.txt, e.g.) that has been associated with
           Waedit.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>78</p>
      </td>
      <td>
        <p> AEDIT displays a message to indicate that a macro is being defined.
           </p>
      </td>
      <td>
        <p> Waedit changes the color of the alignment bars from blue to red
           when macro definition is underway.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>78</p>
      </td>
      <td>
        <p>AEDIT limits macro names to 60 characters. </p>
      </td>
      <td>
        <p>Waedit allows macro names of any length. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>78</p>
      </td>
      <td>
        <p> AEDIT reserves limited memory for macro storage.  </p>
      </td>
      <td>
        <p> Waedit imposes no such limit. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>80</p>
      </td>
      <td>
        <p> If you specify a null filename with the Macro Get subcommand, AEDIT
           gets the present text buffer as a macro file.  </p>
      </td>
      <td>
        <p>Waedit gives an error in this circumstance.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>81</p>
      </td>
      <td>
        <p>In AEDIT, the Macro List subcommand lists the names of all currently
           defined macros. </p>
      </td>
      <td>
        <p>In Waedit, the Macro List command inserts all currently defined
           macro definitions into the text.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>83</p>
      </td>
      <td>
        <p>The AEDIT manual describes two macro modes. </p>
      </td>
      <td>
        <p>Waedit only supports modeless macros. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>84</p>
      </td>
      <td>
        <p> When executing a macro, AEDIT gets responses to certain prompts
           from the keyboard instead of from the macro itself.  </p>
      </td>
      <td>
        <p>When executing a macro, Waedit gets <i>all</i> input from the macro.
           This is a bug in Waedit.  It can be avoided to a large extent by
           avoiding (in macros) the few commands that generate the prompts
           listed in the manual.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>84</p>
      </td>
      <td>
        <p>AEDIT limits macro nesting to 8 levels.</p>
      </td>
      <td>
        <p>In Waedit, the limit is 1000 levels. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>97</p>
      </td>
      <td>
        <p>The AEDIT manual specifies a set of read-only string variables.
           </p>
      </td>
      <td>
        <p>Waedit does not support SI, SO, or SW. </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>99-101</p>
      </td>
      <td>
        <p>The AEDIT manual specifies a set of local variables. </p>
      </td>
      <td>
        <p>Waedit does not support these local variables:</p>
        <ul>
          <li> CNTEXE </li>
          <li> CNTFND </li>
          <li> CNTMAC </li>
          <li> CNTREP </li>
          <li> CURWD </li>
          <li> INOTHR </li>
          <li> LSTFND </li>
          <li> NXTTAB </li>
          <li> NXTWD </li>
        </ul>
        <p> Also, in Waedit, the variable for the current right margin setting
           is spelled RMARGN for consistency with LMARGN and IMARGN.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>103-110</p>
      </td>
      <td>
        <p>The AEDIT manual describes the Calc command.</p>
      </td>
      <td>
        <p>The Waedit Calc command works substantially the same as in AEDIT,
           with these exceptions: </p>
        <ul>
        <li> Waedit recognizes only single and double quotes as string
           delimiters.  </li>
        <li> Waedit does not limit the length of string constants or string
           variables.  </li>
        <li> Waedit does not allow nested assignments.  In other words,
           statements like "N1 = N2 = 55" and "3 + 5 + (N3 = 9)" are illegal.
           </li>
        </ul>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>111-113</p>
      </td>
      <td>
        <p> AEDIT includes a file <i>useful.mac</i> that contains a number of
        pre-written macros. </p>
      </td>
      <td>
        <p> Some of the macros in <i>useful.mac</i> do not work with Waedit.
           To avoid confusion, <i>useful.mac</i> is not included with
           Waedit.  Instead, the included file <i>examples.mac</i> contains
           examples of macros that do work with Waedit.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>111</p>
      </td>
      <td>
        <p>In AEDIT, the memory available to store macros is limited. </p>
      </td>
      <td>
        <p>In Waedit, the memory for storing macros is essentially unlimited.
           </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>114-117</p>
      </td>
      <td>
        <p> The AEDIT manual shows a technique involving the Find command for
           simulating conditional branches and loops within macros, along with
           a few examples taken from the <i>useful.mac</i> file that use
           this technique.  </p>
      </td>
      <td>
        <p> Waedit macros use a slightly different technique for simulating
           conditional branches and loops within macros.  The entries dated
           September 18 and later in the notes.html file explain the technique
           and give examples of its use.  The <i>examples.mac</i> file contains
           additional examples.  </p>
      </td>
    </tr>
    <tr>
      <td align="center">
        <p>119-126</p>
      </td>
      <td>
        <p> Chapter 9 of the AEDIT describes how to configure AEDIT for use
           with various terminals.  </p>
      </td>
      <td>
        <p> None of this applies to Waedit. </p>
      </td>
    </tr>
  </tbody>
</table>
