/* ////////////////////////////////////////////////////////////////////////////
				    calc.cs
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This module implements a slightly simplified version of the
                Calc command from the original Aedit, as described in the Intel
                Aedit manual.  The simplification here is that we don't allow
                nested assignments like the original Aedit did.  That means
                that statements like "N1 = N2 = 55" and "3 + 5 + (N3 = 9)" are
                illegal.
      
                There are two main functions here.  Evaluate() is the top level
                function of a recursive descent parser that analyzes calc
                statements and calculates their value.  Evaluate() calls
                GetNextToken() to parse individual tokens one by one from the
                input.  Both of these functions call HandleCalcError() to deal
                with any errors.

REVISIONS:	14 Sep 16 - RAC - Adapted from an earlier prototype program
//////////////////////////////////////////////////////////////////////////// */

using System;
using System.Windows.Forms;

namespace waedit
{
partial class MainForm
{

/* ////////////////////////////////////////////////////////////////////////////
                          Data Structure Definitions
//////////////////////////////////////////////////////////////////////////// */

struct Token									// GetNextToken() puts its results into a variable of
{										//  this type
    public TT	  type;								// One of the token type identifiers, enumerated below
    public int	  nValue;							// Token's numerical value, or sometimes a subtype
    public string sValue;							// Token's string value if it has one
}

/* ////////////////////////////////////////////////////////////////////////////
			       Names for Numbers
//////////////////////////////////////////////////////////////////////////// */

enum TT										// GetNextToken() assigns one of these types to each
{										//  token found.
    ASSIGN,
    DIVIDE,
    END,
    EXP,
    LOGICAL_OP,
    MINUS,
    MOD,
    N_IND,
    NAMED_NUMERIC_VAR,
    NAMED_STRING_VAR,
    NEG,
    NUMBER,
    NVAR,
    ONES_COMP,
    PLUS,
    POS,
    RELATIONAL_OP,
    S_IND,
    SHIFT_OP,
    STRING,
    SVAR,
    TIMES,
    LEFT_PAREN,
    RIGHT_PAREN
}

enum TST									// For certain token types, GetNextToken() provides
{										//  further classification by assigning one of these
    AND,									//  values to 'nValue'.  As an example, tokens for the
    OR,										//  AND, OR, and XOR operators all have the type
    XOR,									//  LOGICAL_OP and a subtype of AND, OR, or XOR.  See 
    EQ,										//  the function NExpression(), below to see how these
    GT,										//  fields are used.
    LT,
    GE,
    LE,
    NE,
    SHL,
    SHR,
    SAL,
    SAR,
    ROL,
    ROR
}

const int TRUE = -1;
const int FALSE = 0;

const char EOE = '\x89';         // "End of expressions" marker			// Evaluate() appends this character to the statement
										//  as a unique and easy-to-detect end marker.  This
                                                                                //  makes it easier handle syntax errors at the end of
                                                                                //  the statement.

/* ////////////////////////////////////////////////////////////////////////////
				     Data
//////////////////////////////////////////////////////////////////////////// */

string		calcInput;							// Original Calc input, from the user or a macro

string		expression;							// Same thing, but with the EOE added to the end.
										//  This is the input to the parser.

int		eIndex;								// Index to the current position within 'expression'

Token		curTok;								// Info about the most recent token found by
										//  GetNextToken()

bool		isAssignment;							// Result of a preliminary scan performed by
										//  Evaluate() to determine whether or not the
                                                                                //  statement contains an assignment

string		calcError;							// Cleared by DoNullC() before any processing, and
										//  maintained thereafter by HandleCalcError() to
                                                                                //  retain only the first of any errors discovered
                                                                                //  during the parse.  This lets us report only the
                                                                                //  first of the (potentially many) error conditions
                                                                                //  that might be detected.

public string[]	S = new string[] { "", "", "", "", "", "", "", "", "", "" };	// Storage for the ten global string variables

public int[]	N = new int[10];						// Storage for the ten numeric string variables

/* ////////////////////////////////////////////////////////////////////////////
				   DoNullC()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	HandleCommandState() calls this function when the user hits C
		to invoke the Calc command.		

REVISIONS:	14 Sep 16 - RAC - Preliminary version for testing from the
				   command line
		16 Sep 16 - RAC - Replaced with the real version
//////////////////////////////////////////////////////////////////////////// */

void DoNullC()
{
    calcInput = GetUserString("Calc: ", calcInput, "", true);			// Get calc statement from the user or a macro
    if (calcInput.Length > 0)
    {
	calcError = "";								// No errors yet
	currentPrompt = Evaluate(calcInput);					// Evaluate and place result on the prompt line
	if (calcError != "")							// Oops - there was an error, so report the error
        {									//  on the prompt line instead
	    currentPrompt = calcError;
        }
	calcResultPending = true;						// This prevents Execute() (in executors.cs) from
    }										//  overwriting the calc result with a menu prompt
}

/* ////////////////////////////////////////////////////////////////////////////
			       HandleCalcError()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	Other functions call this one to handle parsing errors.

REVISIONS:	15 Sep 16 - RAC - Skeleton
//////////////////////////////////////////////////////////////////////////// */

void HandleCalcError(string s)
{
    if (calcError == "")							// Save the error message for reporting later, but
    {										//  only if it is the first one reported for the
	calcError = s;								//  current statement
    }
}

/* ////////////////////////////////////////////////////////////////////////////
				  Evaluate()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	This function implements a recursive descent parser that
                handles the Calc command.  For reference, here's the grammar
                for Calc input:
		
		calc_statement := s_assignment
		               |  s_expression
		               |  n_assignment
		               |  n_expression
		
		s_assignment   := Sn = s_expression
		               |  S(n_expression) = s_expression
		
		s_expression   := S(n_expression)
		               |  quoted_string
		               |  string_variable
		
		n_assignment   := Nn = n_expression
		               |  N(n_expression) = n_expression
		
		n_expression   := logicalops | relops
		               |  logcialops & relops
		               |  logicalops ^ relops
		               |  relops
		
		       relops  := relops == srops
		               |  relops <> srops
		               |  relops <= srops
		               |  relops >= srops
		               |  relops < srops
		               |  relops > srops
		               |  srops
		
		       srops   := srops $CLS$ exp
		               |  srops $CRS$ exp
		               |  srops $LLS$ exp
		               |  srops $LRS$ exp
		               |  srops $ALS$ exp
		               |  srops $ARS$ exp
		               |  exp
		
		       exp     := exp + term
		               |  exp - term
		               |  term
		
		       term    := term * factor
		               |  term / factor
		               |  term \ factor
		               |  factor
		
		       factor  := factor ** primary
		               |  primary
		
		       primary := +primary
		               |  -primary
		               |  ~primary
		               |  !primary
		               |  #primary
		               |  element
		
		       element := Ni
		               |  N(n_expression)
		               |  (n_expression)
		               |  numeric_constant
		               |  named_calc_variable

REVISIONS:	14 Sep 16 - RAC - Genesis, with hints from an earlier prototype
				   program
		16 Sep 16 - RAC - Fixed to return results as a string rather
				   than sending them to the console.
//////////////////////////////////////////////////////////////////////////// */

string Evaluate(string s)
{
    int nResult;
    string sResult;

    expression = s + (char)EOE;							// Add an "end of expression" marker to the statement
    										//  to make parsing easier

    isAssignment = false;							// Do a preliminary scan of the statement to see
    eIndex = 0;									//  whether or not it contains an assignment operator,
    do										//  and place the result in 'isAssignment'.  This will
    {										//  allow CalcStatement(), below, to more easily
	GetNextToken();								//  distinguish between assignment statements and
        if (curTok.type == TT.ASSIGN)						//  those that simply begin with a global variable
	{									//  reference
	    isAssignment = true;
            break;
        }

    } while (curTok.type != TT.END);

    eIndex = 0;									// Now start over, do the actual statement evaluation,
    if (CalcStatement(out nResult, out sResult))				//  and return the result as a string for display on
    {										//  the prompt line
	return sResult;
    }
    else
    {
	return String.Format("{0}      {0:X}h", nResult);
    }
}

/* ////////////////////////////////////////////////////////////////////////////
                                CalcStatement()
                                 SExpression()
                                 NExpression()
                                RelationalOps()
                                  ShiftOps()
                                   AddOps()
                                 MultiplyOps()
                                   Factor()
                                   Primary()
                                   Element()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	These functions together comprise the calc parser.  Each of
		them corresponds to one of the productions (I think that's the
                correct term?) in the grammer described above in the header
                block for Evaluate().  Read up on recursive descent parsers if
                you want to understand exactly how all this works.

REVISIONS:	19 Sep 16 - RAC - Added this header to previously implemented
				   code
//////////////////////////////////////////////////////////////////////////// */

bool CalcStatement(out int nResult, out string sResult)				// Returns true for string results, false otherwise
{
    int n;

    sResult = "";
    nResult = 0;

    GetNextToken();								// Get the first token
    if (isAssignment)								// Assignment operator found earlier
    {
	switch (curTok.type)
        {
            case TT.S_IND:							// Indirectly referenced string assignment
	    case TT.SVAR:							// Directly referenced string assignment
		if (curTok.type == TT.S_IND)					// Indirectly referenced
                {
		    GetNextToken();						// Skip S(
		    n = NExpression();						// Get which variable to assign to
		    if (!Expect(TT.RIGHT_PAREN)) return true;			// Consume the expected '(' or croak if abasent
		    if ((n < 0) || (n > 9))					// Also croak if the variable identifier isn't kosher
		    {
                        HandleCalcError(String.Format("Invalid variable " +
                            "index {0} near position {1}", n, eIndex-2));
			return true;
		    }
                }
                else								// Directly referenced
                {
		    n = curTok.nValue - '0';					// Get which variable to assign to
		    GetNextToken();						// Skip Sn
                }

		if (!Expect(TT.ASSIGN)) return true;				// Consume assignment operator, or croak if absent
                sResult = SExpression();					// Get the result
		if (!Expect(TT.END)) return true;				// If a string expression ends the input (as it
		S[n] = sResult;							//  should), do the assignment
		return true;							// This is a string result

            case TT.N_IND:							// Indirectly referenced numeric assignment
	    case TT.NVAR:							// Directly referenced numeric assignment
		if (curTok.type == TT.N_IND)					// Indirectly referenced
		{
		    GetNextToken();						// Skip N(
		    n = NExpression();						// Get which variable to assign to
		    if (!Expect(TT.RIGHT_PAREN)) return false;			// Consume the expected '(' or croak if abasent 
		    if ((n < 0) || (n > 9))					// Also croak if the variable identifier isn't kosher
		    {
                        HandleCalcError(String.Format("Invalid variable " +
                            "index {0} near position {1}", n, eIndex-2));
			return false;
		    }
                }
                else								// Directly referenced
                {
		    n = curTok.nValue - '0';					// Get which variable to assign to
                    GetNextToken();						// Skip Nn
                }
		if (!Expect(TT.ASSIGN)) return false;				// Consume assignment operator, or croak if absent
                nResult = NExpression();					// Get the result
                if (!Expect(TT.END)) return false;				// If a numeric expression ends the input (as it
		N[n] = nResult;							//  should), do the assignment
		return false;

            default:								// Invalid assignment statement
                HandleCalcError(String.Format("Invalid assignment detected " +
                    "near position {0}", eIndex));
                return false;
        }
    }										// End 'assignment operator found earlier'

    else									// No assignment operator found earlier
    {
	switch (curTok.type)
        {
	    case TT.SVAR:							// It's a string expression
            case TT.S_IND:
            case TT.NAMED_STRING_VAR:
	    case TT.STRING:
		sResult = SExpression();
		Expect(TT.END);
		return true;							// This is a string result

	    default:								// It's a numeric expression
		nResult = NExpression();
		Expect(TT.END);
                return false;							// This is a numeric result               
        }
    }										// End 'no assignment operator found earlier'
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

string SExpression()
{
    int n;

    switch (curTok.type)
    {
	case TT.SVAR:
	    n = curTok.nValue - '0';						// Get which variable to return
	    GetNextToken();							// Skip Sn
	    return S[n];            
        
        case TT.S_IND:
	    GetNextToken();							// Skip S(
	    n = NExpression();							// Get which variable to return
	    Expect(TT.RIGHT_PAREN);						// Consume expected right parenthesis
	    if ((n >= 0) && (n < 10))
	    {
		return S[n];
	    }
            else
            {
		HandleCalcError(String.Format("Invalid variable index {0} " +
                    "near position {1}", n, eIndex-2));
                return "";
            }

	case TT.NAMED_STRING_VAR:
	case TT.STRING:
	    GetNextToken();							// Skip varaible reference or quoted string
	    return curTok.sValue;

	default:
	    HandleCalcError(String.Format("Syntax error near position {0}",
		eIndex));
	    return "";
    }
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int NExpression()
{
    int left;
    int right;
    TST opType;
    
    left = RelationalOps();
    while (curTok.type == TT.LOGICAL_OP)
    {
	opType = (TST) curTok.nValue;
	GetNextToken();
        right = RelationalOps();
        switch (opType)
        {
            case TST.AND: left &= right; break;
	    case TST.OR:  left |= right; break;
	    case TST.XOR: left ^= right; break;
        }
    }
    return left;
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int RelationalOps()
{
    int left;
    int right;
    TST opType;
    
    left = ShiftOps();
    while (curTok.type == TT.RELATIONAL_OP)
    {
	opType = (TST) curTok.nValue;
	GetNextToken();
        right = ShiftOps();
        switch (opType)
        {
	    case TST.EQ: left = (left == right) ? TRUE : FALSE;  break;
	    case TST.NE: left = (left != right) ? TRUE : FALSE;  break;
	    case TST.GT: left = (left >  right) ? TRUE : FALSE;  break;
	    case TST.LT: left = (left <  right) ? TRUE : FALSE;  break;
	    case TST.GE: left = (left >= right) ? TRUE : FALSE;  break;
	    case TST.LE: left = (left <= right) ? TRUE : FALSE;  break;
        }
    }
    return left;
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int ShiftOps()
{
    int left;
    int right;
    TST opType;

    left = AddOps();
    while (curTok.type == TT.SHIFT_OP)
    {
	opType = (TST) curTok.nValue;
	GetNextToken();
        right = AddOps();
        switch (opType)
	{
	    case TST.SHL: left = left << right;								    break;
	    case TST.SAL: unchecked { left = (left & (int) 0x80000000) | ((left << right) & 0x7fffffff); }  break;
	    case TST.SHR: left = (int)((uint)left >> right);						    break;
	    case TST.SAR: left = left >> right;								    break;
	    case TST.ROL: left = RotateLeft (left, right);						    break;
	    case TST.ROR: left = RotateRight(left, right);						    break;
        }
    }
    return left;
}

int RotateLeft(int value, int count)
{
    return (int)(((uint)value << count) | ((uint)value >> (32 - count)));
}

int RotateRight(int value, int count)
{
    return (int)(((uint)value >> count) | ((uint)value << (32 - count)));
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int AddOps()
{
    int left;
    int right;
    TT tokType;

    left = MultiplyOps();
    while ((curTok.type == TT.PLUS) || (curTok.type == TT.MINUS))
    {
	tokType = curTok.type;
        GetNextToken();
	right = MultiplyOps();
        switch (tokType)
        {
	    case TT.PLUS:  left += right;  break;
            case TT.MINUS: left -= right;  break;
        }
    }
    return left;
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int MultiplyOps()
{
    int left;
    int right;
    TT tokType;
   
    left = Factor();								// A term has to start with a factor
    while ((curTok.type == TT.TIMES) ||						// For as long as another factor is forthcoming
	   (curTok.type == TT.DIVIDE) ||
           (curTok.type == TT.MOD))	
    {	
	tokType = curTok.type;							// Remember which operation to do
        GetNextToken();								// Skip the * or / or %
	right = Factor();							// Get the next factor

	switch(tokType)
        {
            case TT.DIVIDE:
            case TT.MOD:
		if (right == 0)
                {
		    HandleCalcError("Attempted divide by zero");
                    return 0;
                }
	    break;
        }

        switch (tokType)							// Do the multiply, divide, or MOD, depending on the
        {									//  saved token type
	    case TT.TIMES:  left *= right;  break;
            case TT.DIVIDE: left /= right;  break;
            case TT.MOD:    left %= right;  break;
        }
    }										// End 'for as long as another factor is forthcoming'
    return left;								// The answer
}										// End Term()

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int Factor()
{
    int left;
    int right;
   
    left = Primary();
    while (curTok.type == TT.EXP)
    {
	GetNextToken();
	right = Primary();
	left = (int) Math.Pow(left, right);
    }
    return left;
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int Primary()
{
    switch (curTok.type)
    {
        case TT.PLUS:       GetNextToken();  return Primary();
        case TT.MINUS:      GetNextToken();  return -Primary();
        case TT.ONES_COMP:  GetNextToken();  return ~Primary();
        case TT.POS:        GetNextToken();  return (Primary() >= 0) ? TRUE : FALSE;
        case TT.NEG:        GetNextToken();  return (Primary() <  0) ? TRUE : FALSE;
    }
    return Element();
}

/* //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// */

int Element()
{
    int n;

    switch (curTok.type)
    {
        case TT.NUMBER:
	case TT.NAMED_NUMERIC_VAR:
	    n = curTok.nValue;
            GetNextToken();
            return n;	    

	case TT.NVAR:
	    n = curTok.nValue - '0';
            GetNextToken();
            return N[n];	    
        
        case TT.N_IND:
	    GetNextToken();
            n = NExpression();
	    Expect(TT.RIGHT_PAREN);
	    if ((n >= 0) && (n < 10))
	    {
		return N[n];
	    }
            else
            {
		HandleCalcError(String.Format("Invalid variable index {0} " +
		    "near position {1}", n, eIndex-2));
                return 0;
            }
        
        case TT.LEFT_PAREN:
	    GetNextToken();
            n = NExpression();
	    Expect(TT.RIGHT_PAREN);
            return n;
        
	default:
	    HandleCalcError("Unexpected end of input");
            return 0;
    }
}

/* ////////////////////////////////////////////////////////////////////////////
                                   Expect()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The parser calls this function in a number of places where it
                is expecting a particular token, but only needs to skip over
                it.  This function checks to make sure the specified token is
                present.  If it is, this function skips over it and returns
                true.  Otherwise it posts an error message and returns false.

REVISIONS:	19 Sep 16 - RAC - Added this comment block to the previously
				   implemented function
//////////////////////////////////////////////////////////////////////////// */

bool Expect(TT expectedTokenType)
{
    if (curTok.type == expectedTokenType)
    {
	GetNextToken();
        return true;
    }
    else
    {
	HandleCalcError(String.Format("{1} expected near position {0}",
	    eIndex + 1, expectedTokenType));
	return false;
    }
}

/* ////////////////////////////////////////////////////////////////////////////
				GetNextToken()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	The parser calls this function as needed to extract the next
                token from the input string and populate 'curTok' with
                corresponding information as described right here:

		Name:         SVAR
		Description:  Any of the string variables S0-9
		N Value:      The character after the S, which can be 0-9.
		S Value:      --
		---------------------------------------------------------------
		Name:         S_IND
		Description:  "S(", indicating the beginning of an indirect
			      string variable reference
		N Value:      --
		S Value:      --
		---------------------------------------------------------------
		Name:         STRING
		Description:  A quoted string, delimited by either ' or "
			      characters
		N Value:      --
		S Value:      The string enclosed by the delimiters
		---------------------------------------------------------------
		Name:         NVAR
		Description:  Any of the numeric variables N0-9
		N Value:      The character after the N, which can be 0-9.
		S Value:      --
		---------------------------------------------------------------
		Name:         N_IND
		Description:  "N(", indicating the beginning of an indirect
		              numeric variable reference
		N Value:      --
		S Value:      --
		---------------------------------------------------------------
		Name:         ASSIGN
		Description:  The equals sign
		N Value:      --
		S Value:      --
		---------------------------------------------------------------
		Name:         LOGICAL_OP    
		Description:  A logical operator
		N Value:      AND, OR, or XOR to indicate the specific logical
			      operator
		S Value:      --
		---------------------------------------------------------------
		Name:         RELATIONAL_OP
		Description:  A relational operator
		N Value:      EQ, NE, LT, GT, LE, or GE to indicate the
		              specific relational operator
		S Value:      --
		---------------------------------------------------------------
		Name:         SHIFT_OP      
		Description:  A shift operator
		N Value:      CLS, CRS, LLS, LRS, ALS, or ARS to indicate the
		              specific shift operator
		S Value:      --
		---------------------------------------------------------------
		Name:         PLUS
		Description:  The plus sign
		N Value:      --            
		S Value:      --
		---------------------------------------------------------------
		Name:         MINUS
		Description:  The minus sign
		N Value:      --            
		S Value:      --
		---------------------------------------------------------------
		Name:         TIMES
		Description:  The asterisk
		N Value:      --            
		S Value:      --
		---------------------------------------------------------------
		Name:         DIVIDE
		Description:  The forward slash
		N Value:      --            
		S Value:      --
		---------------------------------------------------------------
		Name:         MOD
		Description:  The backslash
		N Value:      --            
		S Value:      --
		---------------------------------------------------------------
		Name:         EXP
		Description:  The double asterisk
		N Value:      --            
		S Value:      --
		---------------------------------------------------------------
		Name:         ONES_COMP
		Description:  The tilde
		N Value:      --
		S Value:      --
		---------------------------------------------------------------
		Name:         POS
		Description:  The exclamation point
		N Value:      --
		S Value:      --
		---------------------------------------------------------------
		Name:         NEG
		Description:  The pound sign
		N Value:      --
		S Value:      --
		---------------------------------------------------------------
                Name:         NUMBER        
		Description:  A numeric constant          
		N Value:      The constant's value, as an integer
		S Value:      --
		---------------------------------------------------------------
		Name:         NAMED_VAR
		Description:  One of the named numeric variables
		N Value:      The variable's value, as an integer
		S Value:      --
		---------------------------------------------------------------
		Name:         END
		Description:  Returned when the end of the input is reached or
			      an error occurs
		N Value:      --	
		S Value:      --

REVISIONS:	14 Sep 16 - RAC - Genesis, with hints from an earlier prototype
				   program
//////////////////////////////////////////////////////////////////////////// */

void GetNextToken()
{
    char delimiter;

    while ((expression[eIndex] == ' ') || (expression[eIndex] == '\t'))
    {
	eIndex++;
    }

    switch (expression[eIndex])
    {
        case EOE:  curTok.type = TT.END;                                                         return;
        case '&':  curTok.type = TT.LOGICAL_OP;     curTok.nValue = (int)TST.AND;  eIndex += 1;  return;
        case '|':  curTok.type = TT.LOGICAL_OP;     curTok.nValue = (int)TST.OR;   eIndex += 1;  return;
        case '^':  curTok.type = TT.LOGICAL_OP;     curTok.nValue = (int)TST.XOR;  eIndex += 1;  return;
        case '+':  curTok.type = TT.PLUS;                                          eIndex += 1;  return;
        case '-':  curTok.type = TT.MINUS;                                         eIndex += 1;  return;
        case '/':  curTok.type = TT.DIVIDE;                                        eIndex += 1;  return;
        case '%':  curTok.type = TT.MOD;                                           eIndex += 1;  return;
        case '~':  curTok.type = TT.ONES_COMP;                                     eIndex += 1;  return;
        case '!':  curTok.type = TT.POS;                                           eIndex += 1;  return;
        case '#':  curTok.type = TT.NEG;                                           eIndex += 1;  return;
        case '(':  curTok.type = TT.LEFT_PAREN;                                    eIndex += 1;  return;
        case ')':  curTok.type = TT.RIGHT_PAREN;                                   eIndex += 1;  return;

        case '*':
            if (expression[eIndex+1] == '*')
            {      curTok.type = TT.EXP;                                           eIndex += 2;  return;  }
            else
            {      curTok.type = TT.TIMES;                                         eIndex += 1;  return;  }

        case '<':
            if (expression[eIndex+1] == '=')
            {      curTok.type = TT.RELATIONAL_OP;  curTok.nValue = (int)TST.LE;   eIndex += 2;  return;  }
            else if (expression[eIndex+1] == '>')
            {      curTok.type = TT.RELATIONAL_OP;  curTok.nValue = (int)TST.NE;   eIndex += 2;  return;  }
            else
            {      curTok.type = TT.RELATIONAL_OP;  curTok.nValue = (int)TST.LT;   eIndex += 1;  return;  }

        case '>':
            if (expression[eIndex+1] == '=')
            {      curTok.type = TT.RELATIONAL_OP;  curTok.nValue = (int)TST.GE;   eIndex += 2;  return;  }
            else
            {      curTok.type = TT.RELATIONAL_OP;  curTok.nValue = (int)TST.GT;   eIndex += 1;  return;  }

        case '=':
            if (expression[eIndex+1] == '=')
            {      curTok.type = TT.RELATIONAL_OP;  curTok.nValue = (int)TST.EQ;   eIndex += 2;  return;  }
            else
            {      curTok.type = TT.ASSIGN;                                        eIndex += 1;  return;  }

        case 's':
        case 'S':
            switch (expression[eIndex+1])
            {
                case '(':
                   curTok.type = TT.S_IND;                                         
                   eIndex += 2;
                   return;
                
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                   curTok.type = TT.SVAR;
                   curTok.nValue = expression[eIndex+1];              
                   eIndex += 2;
                   return;

		case 'l':
                case 'L':
		    switch (expression[eIndex+2])
                    {
                	case '0': case '1': case '2': case '3': case '4':
                	case '5': case '6': case '7': case '8': case '9':
			    curTok.type = TT.NAMED_NUMERIC_VAR;
			    curTok.nValue = S[expression[eIndex+2] - '0'].Length;
                            eIndex += 3;
                            return;
                    }
		    break;
            }
            break;

        case 'n':
        case 'N':
            switch (expression[eIndex+1])
            {
                case '(':
                   curTok.type = TT.N_IND;                                         
                   eIndex += 2;
                   return;
                
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                   curTok.type = TT.NVAR;
                   curTok.nValue = expression[eIndex+1];              
                   eIndex += 2;
                   return;
            }
            break;
    }

    if (Match("SHL"))    { curTok.type = TT.SHIFT_OP;          curTok.nValue = (int)TST.SHL;                       return; }
    if (Match("SHR"))    { curTok.type = TT.SHIFT_OP;          curTok.nValue = (int)TST.SHR;                       return; }
    if (Match("SAL"))    { curTok.type = TT.SHIFT_OP;          curTok.nValue = (int)TST.SAL;                       return; }
    if (Match("SAR"))    { curTok.type = TT.SHIFT_OP;          curTok.nValue = (int)TST.SAR;                       return; }
    if (Match("ROL"))    { curTok.type = TT.SHIFT_OP;          curTok.nValue = (int)TST.ROL;                       return; }
    if (Match("ROR"))    { curTok.type = TT.SHIFT_OP;          curTok.nValue = (int)TST.ROR;                       return; }
    if (Match("BOF"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = (currentChar == 0) ?  TRUE : FALSE; return; }
    if (Match("COL"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = leftCharacter + cursorX;            return; }
    if (Match("CURPOS")) { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = currentChar;                        return; }
    if (Match("IMARGN")) { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = indentMargin;                       return; }
    if (Match("LMARGN")) { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = leftMargin;                         return; }
    if (Match("LOWCH"))  { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = ToLower(GetChar(currentChar));      return; }
    if (Match("NSTLVL")) { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = crs.Count-1;                        return; }
    if (Match("ROW"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = currentLine - topLine;              return; }
    if (Match("LINE"))   { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = currentLine + 1;                    return; }
    if (Match("RMARGN")) { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = rightMargin;                        return; }
    if (Match("SB"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = Clipboard.GetText();                return; }
    if (Match("SE"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = currentFilename;                    return; }
    if (Match("SG"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = getFilename;                        return; }
    if (Match("SM"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = macroFilename;                      return; }
    if (Match("SP"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = putFilename;                        return; }
    if (Match("SR"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = replaceString;                      return; }
    if (Match("ST"))     { curTok.type = TT.NAMED_STRING_VAR;  curTok.sValue = findTarget;                         return; }
    if (Match("SLB"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = Clipboard.GetText().Length;         return; }
    if (Match("SLE"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = currentFilename.Length;             return; }
    if (Match("SLG"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = getFilename.Length;                 return; }
    if (Match("SLM"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = macroFilename.Length;               return; }
    if (Match("SLP"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = putFilename.Length;                 return; }
    if (Match("SLR"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = replaceString.Length;               return; }
    if (Match("SLT"))    { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = findTarget.Length;                  return; }
    if (Match("TAGA"))   { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = text.GetTag(0);                     return; }
    if (Match("TAGB"))   { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = text.GetTag(1);                     return; }
    if (Match("TAGC"))   { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = text.GetTag(2);                     return; }
    if (Match("TAGD"))   { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = text.GetTag(3);                     return; }
    if (Match("UPCH"))   { curTok.type = TT.NAMED_NUMERIC_VAR; curTok.nValue = ToUpper(GetChar(currentChar));      return; }
    if (Match("EOF"))    { curTok.type = TT.NAMED_NUMERIC_VAR;
			   curTok.nValue = (currentChar == (text.GetCharCount() - 1)) ?  TRUE : FALSE;             return; }

    if (Match("CURCH"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = (currentChar < (text.GetCharCount() - 1)) ? GetChar(currentChar) : 0;
        return;
    }

    if (Match("NXTCH"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = (currentChar < (text.GetCharCount() - 2)) ? GetChar(currentChar + 1) : 0;
        return;
    }

    if (Match("ISWHTE") || Match("ISWHITE"))
    {
        byte b;

        curTok.type = TT.NAMED_NUMERIC_VAR;
        b = GetChar(currentChar);
        curTok.nValue = ((b == C.BLANK) || (b == C.TAB) || (b == C.ENTER)) ? TRUE : FALSE;
        return;
    }

    if (Match("ISDEL"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = IsADelimiter(GetChar(currentChar)) ? TRUE : FALSE;
        return;
    }

    if (Match("DATE"))
    {
	DateTime now;

	curTok.type = TT.NAMED_NUMERIC_VAR;
	now = DateTime.Now;
        curTok.nValue = now.Year + 10000 * now.Day + 1000000 * now.Month;
        return;
    }

    if (Match("TIME"))
    {
	DateTime now;

	curTok.type = TT.NAMED_NUMERIC_VAR;
	now = DateTime.Now;
	curTok.nValue = now.Second + 100 * now.Minute + 10000 * now.Hour;
        return;
    }

#if false

    if (Match("SI"))
    {
	curTok.type = TT.NAMED_STRING_VAR;
        curTok.sValue = "SI";
        return;
    }

    if (Match("SO"))
    {
	curTok.type = TT.NAMED_STRING_VAR;
        curTok.sValue = "SO";
        return;
    }

    if (Match("SW"))
    {
	curTok.type = TT.NAMED_STRING_VAR;
        curTok.sValue = "SW";
        return;
    }

    if (Match("INOTHR"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("LSTFND"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("CNTEXE"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("CNTFND"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("CNTMAC"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("CNTREP"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("CURWD"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("NXTTAB"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

    if (Match("NXTWD"))
    {
	curTok.type = TT.NAMED_NUMERIC_VAR;
        curTok.nValue = 0;
        return;
    }

#endif

    if ((expression[eIndex] == '"') ||
	(expression[eIndex] == '\''))
    {
	delimiter = expression[eIndex];
	curTok.sValue = "";
	while (true)
        {
	    if (expression[++eIndex] == EOE)
            {
		curTok.type = TT.END;
                return;
            }
	    if (expression[eIndex] == delimiter)
            {
		curTok.type = TT.STRING;
		eIndex++;
                return;
            }
            curTok.sValue += expression[eIndex];
        }        
    }

    curTok.sValue = "";
    while (true)
    {
	if ("0123456789ABCDEFHO".IndexOf((char)ToUpper(expression[eIndex])) < 0)
	{
	    if (curTok.sValue == "")
            {
		HandleCalcError(String.Format("Unrecognized input near position {0}", eIndex));
		curTok.type = TT.END;
                return;
            }
	    try
            {
		curTok.type = TT.NUMBER;
		if (curTok.sValue[curTok.sValue.Length-1] == 'H')
                {
		    curTok.sValue = curTok.sValue.Substring(0, curTok.sValue.Length-1);
		    curTok.nValue = Convert.ToInt32(curTok.sValue, 16);
                    return;
                }
		else if (curTok.sValue[curTok.sValue.Length-1] == 'D')
                {
		    curTok.sValue = curTok.sValue.Substring(0, curTok.sValue.Length-1);
		    curTok.nValue = Convert.ToInt32(curTok.sValue, 10);
                    return;
                }
                
		else if (curTok.sValue[curTok.sValue.Length-1] == 'O')
                {
		    curTok.sValue = curTok.sValue.Substring(0, curTok.sValue.Length-1);
		    curTok.nValue = Convert.ToInt32(curTok.sValue, 8);
                    return;
                }
                
		else if (curTok.sValue[curTok.sValue.Length-1] == 'B')
                {
		    curTok.sValue = curTok.sValue.Substring(0, curTok.sValue.Length-1);
		    curTok.nValue = Convert.ToInt32(curTok.sValue, 2);
                    return;
                }
                
		else
                {
		    curTok.nValue = Convert.ToInt32(curTok.sValue, 10);
                    return;
                }
            }
            catch
            {
		HandleCalcError(String.Format("Invalid number ending near position {0}", eIndex));
                curTok.type = TT.END;
                return;
            }
        }
	curTok.sValue += (char)ToUpper(expression[eIndex++]);
    }
}						// End GetNextToken()

/* ////////////////////////////////////////////////////////////////////////////
				    Match()
///////////////////////////////////////////////////////////////////////////////
DESCRIPTION:	If the next bit of the expression matches the given string
                (ignoring case), this function advances 'eIndex' by the length
                of the given string and returns true.  Otherwise it just
                returns false.

REVISIONS:	14 Sep 16 - RAC - Genesis
//////////////////////////////////////////////////////////////////////////// */

bool Match(string s)
{
    if (expression.IndexOf(s, eIndex, StringComparison.OrdinalIgnoreCase) == eIndex)
    {
	eIndex += s.Length;
        return true;
    }
    return false;
}						// End Match()

}						// End class MainForm
}						// End namespace waedit

