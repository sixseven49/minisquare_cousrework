using Compiler.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Tokenization
{
    /// <summary>
    /// A tokenizer for the reader language
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// The reader getting the characters from the file
        /// </summary>
        private IFileReader Reader { get; }

        /// <summary>
        /// The characters currently in the token
        /// </summary>
        private StringBuilder TokenSpelling { get; } = new StringBuilder();

        /// <summary>
        /// Checks if HasErrors is true or false. If it is true don't set it to true again.
        /// </summary>
        private void isItDirty()
        {
            if (!Reporter.TokenizerHasErrors) // if false set it to true
            {
                //I changed the complers sectiont to not doa return if there are errors but just to print them out
                // This is because It is nice to see exactly whats wrong
                Reporter.TokenizerHasErrors = true;
            }
        }

        /// <summary>
        /// Createa a new tokenizer
        /// </summary>
        /// <param name="reader">The reader to get characters from the file</param>
        /// <param name="reporter">The error reporter to use</param>
        public Tokenizer(IFileReader reader, ErrorReporter reporter)
        {
            Reader = reader;
            Reporter = reporter;
        }

        /// <summary>
        /// Gets all the tokens from the file
        /// </summary>
        /// <returns>A list of all the tokens in the file in the order they appear</returns>
        public List<Token> GetAllTokens()
        {
            List<Token> tokens = new List<Token>();
            Token token = GetNextToken();
            while (token.Type != TokenType.EndOfText)
            {
                tokens.Add(token);
                token = GetNextToken();
            }
            tokens.Add(token);
            Reader.Close();
            return tokens;
        }

        /// <summary>
        /// Scan the next token
        /// </summary>
        /// <returns>True if and only if there is another token in the file</returns>
        private Token GetNextToken()
        {
            // Skip forward over any white spcae and comments
            SkipSeparators();

            // Remember the starting position of the token
            Position tokenStartPosition = Reader.CurrentPosition;

            // Scan the token and work out its type
            TokenType tokenType = ScanToken();

            // Create the token
            Token token = new Token(tokenType, TokenSpelling.ToString(), tokenStartPosition);
            Debugger.Write($"Scanned {token}");

            // Report an error if necessary
            if (tokenType == TokenType.Error)
            {
                // setting has errors to true
                isItDirty();
                // Report the error here. Adding token to error positions
                Reporter.TokenizerErrorPositions.Add($"Error has occured here: {token}");
            }

            return token;
        }

        /// <summary>
        /// Skip forward until the next character is not whitespace or a comment
        /// </summary>
        private void SkipSeparators()
        {
            while (Reader.Current == '!' || IsWhiteSpace(Reader.Current))
            {
                if (Reader.Current == '!')
                    Reader.SkipRestOfLine();
                else
                    Reader.MoveNext();
            }
        }
        private bool isLowerLetterDigit(char current)
        {
            if (char.IsLetterOrDigit(current) && char.IsLower(current))
                return true;
            else
                return false;
        }
        private bool isLowerLetter(char current)
        {
            return (char.IsLetter(current) && char.IsLower(current));
        }

        /// <summary>
        /// Find the next token
        /// </summary>
        /// <returns>The type of the next token</returns>
        /// <remarks>Sets tokenSpelling to be the characters in the token</remarks>
        private TokenType ScanToken()
        {
            TokenSpelling.Clear();
            if (char.IsLetter(Reader.Current))
            {
                // Reading an identifier
                TakeIt(); // takes the charcter
                while (isLowerLetterDigit(Reader.Current))
                {
                    TakeIt(); //keep taking it as long as its letter or a digit
                }
                if (TokenTypes.IsKeyword(TokenSpelling)) // if it is a keyword... 
                {
                    return TokenTypes.GetTokenForKeyword(TokenSpelling); // return word
                }
                else if (TokenSpelling.ToString().Any(char.IsUpper))
                {
                    return TokenType.Error;
                }
                else
                {
                    return TokenType.Identifier; // if not, retun idenitfer
                }
            }
            else if (Reader.Current == '_')
            {
                TakeIt(); // take the _
                while (isLowerLetter(Reader.Current))
                { // if the next bit is a leter keep taking it
                    TakeIt();
                }
                // once it finds a non letter item...
                return TokenType.Identifier;
            }
            else if (Reader.Current == '’')
            {
                TakeIt();
                // do while is lower letter or a figit or a punctuation mark or white space
                while (isLowerLetterDigit(Reader.Current) || IsPunctuation(Reader.Current) || IsWhiteSpace(Reader.Current)) // == graphic
                {
                    TakeIt();
                }
                if (Reader.Current == '’') //and ends with ' it is a character literal
                    return TokenType.CharLiteral; //it is a character literal
                else
                    return TokenType.Error;
            }
            else if (char.IsDigit(Reader.Current))
            {
                // Reading an integer
                TakeIt();
                while (char.IsDigit(Reader.Current))
                    TakeIt();
                return TokenType.IntLiteral;
            }
            else if (IsOperator(Reader.Current))
            {
                // Read an operator
                TakeIt();
                // if the next this is an = sign then take it to before delaring it as an oeprator
                if (Reader.Current == '=') 
                    TakeIt();
                return TokenType.Operator;
            }
            else if (Reader.Current == ':')
            {
                // Read an :
                // Is it a : or a :=
                TakeIt();
                if (Reader.Current == '=')
                {
                    TakeIt();
                    return TokenType.Becomes;
                }
                else
                {
                    return TokenType.Colon;
                }
            }
            else if (Reader.Current == ';')
            {
                // Read a ;
                TakeIt();
                return TokenType.Semicolon;
            }
            else if (Reader.Current == '~')
            {
                // Read a ~
                TakeIt();
                return TokenType.Is;
            }
            else if (Reader.Current == '(')
            {
                // Read a (
                TakeIt();
                return TokenType.LeftBracket;
            }
            else if (Reader.Current == ')')
            {
                // Read a )
                TakeIt();
                return TokenType.RightBracket;
            }
            else if (Reader.Current == '\'')
            {
                // Read a '
                TakeIt();
                // Take whatever the character is
                TakeIt();
                // Try getting the closing '
                if (Reader.Current == '\'')
                {
                    TakeIt();
                    return TokenType.CharLiteral;
                }
                else
                {
                    // Could do some better error handling here but we weren't asked to
                    return TokenType.Error;
                }
            }
            else if (Reader.Current == default(char))
            {
                // Read the end of the file
                TakeIt();
                return TokenType.EndOfText;
            }
            else
            {
                // Encountered a character we weren't expecting
                TakeIt();
                return TokenType.Error;
            }
        }


        /// <summary>
        /// Appends the current character to the current token then moves to the next character
        /// </summary>
        private void TakeIt()
        {
            TokenSpelling.Append(Reader.Current);
            Reader.MoveNext();
        }

        /// <summary>
        /// Checks whether a character is white space
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if and only if c is a whitespace character</returns>
        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }


        /// <summary>
        /// Checks whether a character is an operator
        /// + | - | * | / | < | > | = | \ | >= | <=
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if and only if the character is an operator in the language</returns>
        private static bool IsOperator(char c)
        {
            switch (c)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '<':
                case '>':
                case '=':
                case '\\':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the parameter p qualifies as a puncuation or not
        /// //. | , | ?
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool IsPunctuation(char p)
        {
            switch (p)
            {
                case '.':
                case ',':
                case '?':
                    return true;
                default:
                    return false;
            }
        }
    }
}
