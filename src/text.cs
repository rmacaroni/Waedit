/* ////////////////////////////////////////////////////////////////////////////
				    text.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This is the Text class.  It implements the buffer that holds
                the text being edited, along with a few rudimentary functions
                for inserting, deleting, and finding text in the buffer.

NOTE:		There's a pretty good chance that we'll eventually need to
                rewrite this class for performance reasons.  Therefore, try to
                keep its interface as narrow and clean as possible.

REVISIONS:	 1 Aug 16 - RAC - Genesis
		 5 Aug 15 - RAC - Started over, with huge hints from the first
		 		   attempt
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Collections.Generic;
using System.Text;

namespace waedit
{
class Text
{

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

internal List<byte> buffer = new List<byte>();	// The main text buffer
List<int>	lineIndex  = new List<int>();	// See BuildLineIndex(), below
int[]		tags;				// The four tags
bool		unsavedChanges;			// Supports warning the user
						//  against closing the editor
                                                //  without saving changes
bool		commandChangedBuffer;		// Supports undo/redo logic ...
						//  grep for details

/* ////////////////////////////////////////////////////////////////////////////
				    Text()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The default constructor.

REVISIONS:	 1 Aug 16 - RAC - Skeleton
//////////////////////////////////////////////////////////////////////////// */

public Text()
{
    Initialize();
}						// End Text()

/* ///////////////////////////////////////////////////////////////////////// */

public void Initialize()
{
    tags = new int[4] { -1, -1, -1, -1 };
    buffer.Clear();
    buffer.Add(C.EOF);
    BuildLineIndex();
    unsavedChanges = false;
}						// End Initialize()

/* ////////////////////////////////////////////////////////////////////////////
                                  Text(Text)
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Copy constructor for building undo/redo snapshots.

REVISIONS:	14 Jul 25 - GPT - Genesis
//////////////////////////////////////////////////////////////////////////// */

public Text(Text other)
{
    buffer = new List<byte>(other.buffer);
    lineIndex = new List<int>(other.lineIndex);
    tags = (int[])other.tags.Clone();
    unsavedChanges = other.unsavedChanges;
    commandChangedBuffer = other.commandChangedBuffer;
}						// End Text(Text)

/* ////////////////////////////////////////////////////////////////////////////
			       BuildLineIndex()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function creates the line index.  The line index contains
                an index to the beginning of each line of text within the text
                buffer, plus one more at the end that points just past the EOF
                marker.  This last entry eliminates the need in several places
                to treat the end of file as a special case.

REVISIONS:	 2 Aug 16 - RAC - Genesis
		28 Sep 16 - RAC - Changed 'for' loop to 'foreach' for better
                		   performance.  What we really need, however,
                                   is something a lot smarter.
//////////////////////////////////////////////////////////////////////////// */

public void BuildLineIndex()
{
    int	i;					// A generic int

    lineIndex.Clear();				// Start with a clean slate
    lineIndex.Add(0);				// First line is always here

    i = 0;
    foreach(byte b in buffer)
    {
	if ((b == C.EOL) || (b == C.EOF))	// Found an EOL or EOF
        {
	    lineIndex.Add(i+1);			// Next line starts on the next
        }					//  byte
	i++;
    }						// End 'for each byte'
}						// End BuildLineIndex()

/* ////////////////////////////////////////////////////////////////////////////
				GetLineCount()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns the number of lines currently in the text
		buffer.

REVISIONS:	 2 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public int GetLineCount()
{
    return lineIndex.Count - 1;
}						// End GetLineCount()

/* ////////////////////////////////////////////////////////////////////////////
				GetLineStart()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns the position within the text buffer of a
		specified line.

REVISIONS:	 3 Aug 16 - RAC - Adapted from GetLine()
		 8 Aug 16 - RAC - Changed to allow access to the dummy line
				   index entry that follows the EOF.
//////////////////////////////////////////////////////////////////////////// */

public int GetLineStart(int i)
{
    if (i >= lineIndex.Count)
    {
	throw new Exception("i out of range in GetLineStart()");
    }
    return lineIndex[i];
}						// End GetLineStart()

/* ////////////////////////////////////////////////////////////////////////////
				GetCharCount()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns the number of characters currently in the
		text buffer.

REVISIONS:	 7 Aug 16 - RAC - Adapted from GetLineCount()
//////////////////////////////////////////////////////////////////////////// */

public int GetCharCount()
{
    return buffer.Count;
}						// End GetCharCount()

/* ////////////////////////////////////////////////////////////////////////////
				   GetTag()
				   SetTag()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	These functions get and set the tags.

REVISIONS:	16 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void SetTag(int i, int poz)
{
    tags[i] = poz;
    commandChangedBuffer = true;
}

/* ///////////////////////////////////////////////////////////////////////// */

public int GetTag(int i)
{
    return tags[i];
}

/* ////////////////////////////////////////////////////////////////////////////
			  RepairTagsAfterDeletion()
			  RepairTagsAfterInsertion()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	These functions repair the tags array after changes to the text
		buffer.

REVISIONS:	16 Aug 16 - RAC - Skeletons
//////////////////////////////////////////////////////////////////////////// */

void RepairTagsAfterDeletion(int deletionStart, int length)
{
    int		i;				// A generic integer

    for (i=0; i<tags.Length; i++)		// For each tag
    {
	if (tags[i] >= deletionStart + length)	// Tag is after the deleted
	{					//  block
	    tags[i] -= length;			// Back up tag by the size of
	}					//  the deleted block
	else if (tags[i] >= deletionStart)	// Tag is in the deleted block
	{
	    tags[i] = deletionStart;		// Place tag just after the
	}					//  part that was deleted
    }						// End 'for each tag'
}						// End RepairTagsAfterDeletion

/* ///////////////////////////////////////////////////////////////////////// */

void RepairTagsAfterInsertion(int insertionStart, int length)
{
    int		i;				// A generic integer

    for (i=0; i<tags.Length; i++)		// For each tag
    {
	if (tags[i] >= insertionStart)		// Tag is at or after the
	{					//  insertion point
	    tags[i] += length;			// Move tag forward by the size
	}					//  of the insertion
    }						// End 'for each tag'
}						// End RepairTagsAfterInsertion

/* ////////////////////////////////////////////////////////////////////////////
				  GetBlock()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function returns a specified block of text

REVISIONS:	16 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public List<byte> GetBlock(int start, int length)
{
    return buffer.GetRange(start, length);
}

/* ////////////////////////////////////////////////////////////////////////////
				 DeleteBlock()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function deletes a block of specified length starting at a
		specified index.

REVISIONS:	16 Aug 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void DeleteBlock(int start, int length)
{
    unsavedChanges = true;
    commandChangedBuffer = true;
    buffer.RemoveRange(start, length);
    BuildLineIndex();
    RepairTagsAfterDeletion(start, length);
}

/* ////////////////////////////////////////////////////////////////////////////
				 InsertBlock()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function inserts a list of bytes into the buffer at a
		specified index.

REVISIONS:	16 Aug 16 - RAC - Genesis
		28 Sep 16 - RAC - Created an overload for callers who want to
				   handle the BuildLineIndex() call on their
                                   own for performance reasons
//////////////////////////////////////////////////////////////////////////// */

public void InsertBlock(int i, List<byte> block)
{
    InsertBlockGuts(i, block, true);
}

//////////////////////////////////////////////////////////////////////////// */

public void InsertBlock(int i, List<byte> block, bool buildLineIndex)
{
    InsertBlockGuts(i, block, buildLineIndex);
}

//////////////////////////////////////////////////////////////////////////// */

void InsertBlockGuts(int i, List<byte> block, bool buildLineIndex)
{
    unsavedChanges = true;
    commandChangedBuffer = true;
    buffer.InsertRange(i, block);
    if (buildLineIndex) BuildLineIndex();
    RepairTagsAfterInsertion(i, block.Count);
}						// End InsertBlock()

/* ////////////////////////////////////////////////////////////////////////////
                                ReplaceBlock()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The R and P commands (Replace and Paragraph reformat) used to
                work by stupidly calling DeleteBlock() followed by
                ReplaceBlock().  This made for lots of possibly expensive data
                manipulation and an extra call to BuildLineImage().  This
                function lets them do the same thing a little more efficiently.

REVISIONS:	21 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

public void ReplaceBlock
(
    int		start,				// Where to replace
    int		length,				// How much to replace
    List<byte>	block				// What to replace with
) {
    unsavedChanges = true;
    commandChangedBuffer = true;
    if (length != block.Count)			// Blocks not the same size
    {
	if (block.Count > length)		// We need more space
        {
	    buffer.InsertRange(start,		// Expand the buffer as needed
		new byte[block.Count-length]);
	    RepairTagsAfterInsertion (		// Keep the tags up to date
		start, block.Count - length);
        }					// End 'we need more space'
        else					// We need less space
        {
	    buffer.RemoveRange(start,		// Shrink the buffer as needed
		length-block.Count);
	    RepairTagsAfterDeletion (		// Keep the tags up to date
		start, length - block.Count);
        }					// End 'we need less space'
    }						// End 'blocks not same size'
    foreach (byte b in block)			// Copy the replacement string
    {						//  into the buffer
	buffer[start++] = b;
    }
}						// End ReplaceBlock()

/* ////////////////////////////////////////////////////////////////////////////
				    Find()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function searches the buffer for a specified string.  It
		returns the index of the beginning of the string if successful,
		or -1 if the string was not found.

REVISIONS:	17 Aug 16 - RAC - Genesis
		29 Aug 16 - RAC - Fixed to return -1 if target string is empty
                19 Sep 16 - RAC - Rewrote to improve performance
		23 Sep 16 - RAC - Enhanced to optionally find only token
				   strings
//////////////////////////////////////////////////////////////////////////// */

public int Find
(
    string target,				// The target string
    int	   searchIndex,				// Buffer index to start search
    bool   caseSensitive,			// Flag: respect case
    C.FD   direction,				// Forward or reverse search
    bool   tokenStringsOnly
) {
    if (target == "")
    {
	return -1;
    }
    if (direction == C.FD.FORWARD)		// Forward search
    {
	while (searchIndex < (buffer.Count - target.Length))
	{
	    if (Match(target, searchIndex, caseSensitive, tokenStringsOnly))
            {
		return searchIndex;
            }
	    searchIndex++;
        }
    }						// End 'forward search'
    else					// Backward search
    {
	searchIndex -= target.Length;
        while (searchIndex >= 0)
        {
	    if (Match(target, searchIndex, caseSensitive, tokenStringsOnly))
            {
		return searchIndex;
            }
	    searchIndex--;
        }
    }						// End 'backward search'
    return -1;					// Search failed if we get here
}						// End Find()

/* ///////////////////////////////////////////////////////////////////////// */

bool Match
(
    string target,				// The target string
    int	   searchIndex,				// Buffer index to start search
    bool   caseSensitive,			// Flag: respect case
    bool   tokenStringsOnly
) {
    if (tokenStringsOnly)
    {
	if (((searchIndex != 0) && !MainForm.IsADelimiter(buffer[searchIndex-1])) ||
	    !MainForm.IsADelimiter(buffer[searchIndex + target.Length]))
	{
	    return false;
        }
    }

    foreach (byte c in target)
    {
	if (caseSensitive)
        {
	    if (c != buffer[searchIndex++]) return false;
        }
        else
        {
	    if (MainForm.ToUpper(c) != MainForm.ToUpper(buffer[searchIndex++])) return false;
        }
	
    }
    return true;				// Found match if all bytes match
}						// End Match()

/* ////////////////////////////////////////////////////////////////////////////
                             NoUnsavedChangesYet()
                             HaveUnsavedChanges()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	These functions give external access to the flag that indicates
		whether or not the buffer has been changed since the last time
		it was marked as clean.

REVISIONS:	22 Aug 16 - RAC - Genesis
		13 Jul 25 - RAC - Gave these guys better names
//////////////////////////////////////////////////////////////////////////// */

public void NoUnsavedChangesYet() { unsavedChanges = false; }

/* ///////////////////////////////////////////////////////////////////////// */

public bool HaveUnsavedChanges() { return unsavedChanges; }

/* ////////////////////////////////////////////////////////////////////////////
                             NoCommandChangesYet()
                            CommandChangedBuffer()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	These functions give external access to the flag that tracks
                buffer changes for the undo/redo logic.  This flag is cleared
                before and checked after each command execution to manage the
                undo snapshots.

REVISIONS:	13 Jul 25 - RAC - Modelled after the similar functions for the
				   'unsavedChanges' flag, above
//////////////////////////////////////////////////////////////////////////// */

public void NoCommandChangesYet() { commandChangedBuffer = false; }

/* ///////////////////////////////////////////////////////////////////////// */

public bool CommandChangedBuffer() { return commandChangedBuffer; }

}						// End class Text
}						// End namespace waedit

