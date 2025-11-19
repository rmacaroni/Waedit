/* ////////////////////////////////////////////////////////////////////////////
				     c.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Constants live here in peace and harmony.

REVISIONS:	 5 Aug 16 - RAC - Adapted from earlier version
//////////////////////////////////////////////////////////////////////////// */

namespace waedit
{
class C
{

/*  Codes for some special keys that will coexist peacefully with the values
    normally returned for printable characters  */

public const byte PGUP   = 0x80;
public const byte PGDN   = 0x81;
public const byte HOME   = 0x82;
public const byte LEFT   = 0x83;
public const byte UP     = 0x84;
public const byte RIGHT  = 0x85;
public const byte DOWN   = 0x86;
public const byte DELETE = 0x87;
#if DEBUG
public const byte F1	 = 0x8c;
#endif

/*  Codes for special characters in the text buffer and various other needs  */

public const byte EOL = 0x0d;		// End of line, or <CR> if you prefer
public const byte BAD = 0x88;		// Bad escape sequence in macro file
public const byte EOF = 0x89;		// End of file
public const byte EOM = 0x8a;		// End of macro

/*  Code for special character in the exchange buffer  */

public const byte NO_EXCHANGE = 0xff;

/*  Other characters worth naming  */

public const byte CTRL_A = 0x01;
public const byte CTRL_C = 0x03;
public const byte CTRL_E = 0x05;
public const byte BKSP	 = 0x08;
public const byte LF	 = 0x0a;
public const byte CR     = 0x0d;
public const byte ENTER  = 0x0d;
public const byte CTRL_N = 0x0e;
public const byte CTRL_U = 0x15;
public const byte CTRL_V = 0x16;
public const byte CTRL_X = 0x18;
public const byte CTRL_Z = 0x1a;
public const byte ESC	 = 0x1b;
public const byte TAB	 = 0x09;
public const byte BLANK	 = 0x20;

/*  Names for find directions  */

public enum FD
{
    FORWARD,
    REVERSE
}

public const int INFINITE = -1;			// For infinite repeat counts

}						// End class c
}						// End namespace waedit

