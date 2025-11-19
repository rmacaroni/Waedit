/* ////////////////////////////////////////////////////////////////////////////
                                 GapBuffer.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This file implements a simple gap buffer class for use in the
                new Windows version of Waedit.  The goal is to implement an
                efficient gap buffer internally in 'buffer', but to present a
                virtual List<byte> interface to the users of the class.  This
                so the existing code (that previously used a List<byte> for the
                buffer) doesn't have to change.

REVISIONS:	21 Sep 16 - RAC - Genesis, with hints from a CodeProject
				   article.
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;

namespace waedit
{

class GapBuffer
{

/* ////////////////////////////////////////////////////////////////////////////
                                     Data
//////////////////////////////////////////////////////////////////////////// */

List<byte>	buffer = new List<byte>();	// The buffer
int		gapStart = 0;			// Index of first byte in gap
int		gapEnd = 0;			// Index of byte after the gap

/* ////////////////////////////////////////////////////////////////////////////
                                Count Property
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This property returns the number of characters in the virtual
                buffer, which is just the length of the internal array minus
                the current length of the gap.

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public int Count
{
    get
    {
	return buffer.Count - (gapEnd - gapStart);
    }
}

/* ////////////////////////////////////////////////////////////////////////////
                                    Indexer
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public byte this[int i]
{
    get
    {
	if (i >= gapStart)
	{
	    i += (gapEnd - gapStart);
        }
	return buffer[i];        
    }
}

/* ////////////////////////////////////////////////////////////////////////////
                                     Add()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function adds a single byte to the end of the buffer.
		This is trivial because it doesn't involve the gap in any way.

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void Add(byte b)
{
    buffer.Add(b);
}

/* ////////////////////////////////////////////////////////////////////////////
                                    Clear()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function clears the virtual buffer.  The easy way to do
		that is to simply expand the gap to occupy all of the internal
                buffer.

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void Clear()
{
    gapStart = 0;
    gapEnd = buffer.Count;
}

/* ////////////////////////////////////////////////////////////////////////////
                                  GetRange()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns a specified range of data from the
		buffer.  The requested range is usually small, so we'll just
                copy the data one byte at a time to the result object.

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public List<byte> GetRange(int index, int count)
{
    List<byte> rv = new List<byte>();		// Put the result here

    while (count-- > 0)
    {
	rv.Add(this[index++]);
    }
    return rv;
}

/* ////////////////////////////////////////////////////////////////////////////
                                 InsertRange()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function inserts a block of data into the buffer.

METHOD:		START Insert Range
		  CALL Move gap to insertion point
		  IF Will gap hold new data?\No\Yes
		    Expand the gap as needed
		  ENDIF
		  Copy new data into the gap
		  Adjust the gap start pointer
		END

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void InsertRange(int index, List<byte> data)
{
    int expandGapBy;

    MoveGapTo(index);				// Move gap to insertion point
    if (data.Count > (gapEnd - gapStart))	// Gap not big enough
    {
	expandGapBy = data.Count + 100000;
	buffer.InsertRange(index, new byte[expandGapBy]);
	gapEnd += expandGapBy;
    }						// End 'gap not big enough'
    foreach (byte b in data)
    {
	buffer[gapStart++] = b;
    }
}

/* ////////////////////////////////////////////////////////////////////////////
                                 RemoveRange()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function removes a specified range from the buffer.

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void RemoveRange(int index, int count)
{
    MoveGapTo(index);				// Put gap before data to axe
    gapEnd += count;				// Expand gap over removed data
}

/* ////////////////////////////////////////////////////////////////////////////
                                  MoveGapTo()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function moves the gap to a given spot in the buffer by
		moving the text either before or after the buffer so it "flows"
                around the buffer.

REVISIONS:	21 Sep 16 - RAC - Genesis, with hints from the old Waedit code
//////////////////////////////////////////////////////////////////////////// */

void MoveGapTo(int index)
{
    int	src;					// Move text from here
    int dst;					// Move text to here
    int len;					// Move this many bytes

    if (gapStart == index)			// Do nothing if the gap is
    {						//  already in the right spot
	return;
    }
    if (index < gapStart)
    {
	len = gapStart - index;
	src = index + len;
        dst = index + (gapEnd - gapStart) + len;
	gapStart -= len;
        gapEnd   -= len;
	while (len-- > 0)
        {
	    buffer[--dst] = buffer[--src];
        }
    }
    else
    {
	len = index - gapStart;
        src = gapEnd;
        dst = gapStart;
	gapStart += len;
        gapEnd   += len;
	while (len-- > 0)
        {
	    buffer[dst++] = buffer[src++];
        }
    }
}

}						// End class GapBuffer

}						// End namespace waedit

