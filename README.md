# What is Waedit?

## First, a Little History

In the early 1980s, Intel developed AEDIT, a full-screen text editor intended
for by programmers and technical writers working on Intel's microprocessor
development systems.  Intel later released a version that ran on the IBM PC.

AEDIT was notable for its friendly user interface and comprehensive editing
features.  It also included a powerful macro facility that allowed users to
automate repetitive tasks and implement custom commands.  However, it was
somewhat constrained by the hardware of the day which typically provided
limited memory and a character-only display of limited size.

## Okay, So *What Is Waedit?*

Waedit (think "AEDIT for Windows") is an "almost clone" of AEDIT which
generally preserves AEDIT's user interface, while at the same time exploiting
modern hardware as appropriate.  In particular:

- AEDIT assumed an 80-column display, and implemented a somewhat clunky (by
today's standards) mechanism for dealing with long lines.  Waedit runs in a
resizable window that can be as wide as your monitor.  If you then have lines
that are still to long to fit, Waedit will automatically scroll horizontally as
needed to keep the cursor in view.

- Waedit uses the Windows clipboard in place of AEDIT's fixed-size "block
buffer".  This eliminates the size restriction, and also allows easy
cut-and-paste transfer of text to and from other programs.

- Waedit provides a multi-level undo/redo feature not present in AEDIT.

- Waedit relaxes several other minor memory-related restrictions in AEDIT.

# How Do I Compile and Run Waedit?

The files in this repro support compilation from either within Visual Studio or
the command line.  To use Visual Studio, simply load the solution file
(waedit.sln) into Visual Studio and build the project normally.  Alternately,
execute either gor.bat or god.bat from the command line to create the release
or debug version, respectively.

Waedit doesn't require any installation per se.  Just run the .EXE file in the
normal fashion.

# What if I Don't Want to Compile It?

Grab the pre-compiled .EXE and WAEDIT.MAC from the ???  TBD -- what do I call
the "release" area ???

# How Do I Use It?

If you have used AEDIT in the past, you will be instantly comfortable in
Waedit.  Otherwise (or if you need a refresher), the [original AEDIT
manual](docs/aedit.pdf) will get you started.  Once you're familiar with the
basics, [this file](docs/diffs.md) explains the differences between AEDIT and
Waedit.

In addition, [this file](docs/notes.html) contains an informal development
diary that explains some of the design and internal workings of Waedit.

# What Can I Do With It?

This project is released under the MIT License - see the [LICENSE](LICENSE)
file for details.

# Known Bugs

## Long Strings mess up status bar

## Hidden Cursor sometimes below last line

## Block -> Put -> Aborb with Ctrl-C -> Cursor goes to beginning of the file?

