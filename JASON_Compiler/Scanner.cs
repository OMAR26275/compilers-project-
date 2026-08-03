using System;
using System.Collections.Generic;

public enum Token_Class
{
    // Datatype keywords
    Integer,
    Real,
    StringType,

    // Input and output keywords
    Read,
    Write,
    Endl,

    // Conditional keywords
    If,
    ElseIf,
    Else,
    Then,
    End,

    // Loop keywords
    Repeat,
    Until,

    // Function-related keywords
    Return,
    Main,

    // Arithmetic operators
    PlusOp,
    MinusOp,
    MultiplyOp,
    DivideOp,

    // Assignment operator
    AssignmentOp,

    // Condition operators
    EqualOp,
    LessThanOp,
    GreaterThanOp,
    NotEqualOp,

    // Boolean operators
    AndOp,
    OrOp,

    // Symbols
    Semicolon,
    Comma,
    LParenthesis,
    RParenthesis,
    LCurly,
    RCurly,

    // General lexical classes
    Identifier,
    Constant,
    StringLiteral
}

namespace JASON_Compiler
{
    public class Token
    {
        // Actual text from the source program
        public string lex;

        // Classification of the lexeme
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();

        private readonly Dictionary<string, Token_Class> ReservedWords;
        private readonly Dictionary<string, Token_Class> Operators;
        private readonly Dictionary<string, Token_Class> Symbols;

        public Scanner()
        {
            /*
             * StringComparer.OrdinalIgnoreCase means that:
             * int, INT and Int are all recognized as the same keyword.
             */
            ReservedWords =
                new Dictionary<string, Token_Class>(
                    StringComparer.OrdinalIgnoreCase
                );

            Operators = new Dictionary<string, Token_Class>();
            Symbols = new Dictionary<string, Token_Class>();

            InitializeReservedWords();
            InitializeOperators();
            InitializeSymbols();
        }

        private void InitializeReservedWords()
        {
            // Datatype keywords
            ReservedWords.Add("int", Token_Class.Integer);
            ReservedWords.Add("float", Token_Class.Real);
            ReservedWords.Add("string", Token_Class.StringType);

            // Input and output keywords
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("endl", Token_Class.Endl);

            // Loop keywords
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);

            // Conditional keywords
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("end", Token_Class.End);

            // Function-related keywords
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("main", Token_Class.Main);
        }

        private void InitializeOperators()
        {
            // Arithmetic operators
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);

            // Assignment operator
            Operators.Add(":=", Token_Class.AssignmentOp);

            // Condition operators
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);

            // Boolean operators
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);
        }

        private void InitializeSymbols()
        {
            Symbols.Add(";", Token_Class.Semicolon);
            Symbols.Add(",", Token_Class.Comma);
            Symbols.Add("(", Token_Class.LParenthesis);
            Symbols.Add(")", Token_Class.RParenthesis);
            Symbols.Add("{", Token_Class.LCurly);
            Symbols.Add("}", Token_Class.RCurly);
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();

            if (SourceCode == null)
            {
                Errors.Error_List.Add("Source code cannot be null.");
                JASON_Compiler.TokenStream = Tokens;
                return;
            }

            for (int i = 0; i < SourceCode.Length; i++)
            {
                char currentCharacter = SourceCode[i];

                // Ignore spaces, tabs and new lines
                if (char.IsWhiteSpace(currentCharacter))
                {
                    continue;
                }

                /*
                 * Comments must be checked before the divide operator,
                 * because both comments and division begin with '/'.
                 */
                if (currentCharacter == '/' &&
                    i + 1 < SourceCode.Length &&
                    SourceCode[i + 1] == '*')
                {
                    ScanComment(SourceCode, ref i);
                    continue;
                }

                // String literal
                if (currentCharacter == '"')
                {
                    ScanString(SourceCode, ref i);
                    continue;
                }

                // Identifier or reserved keyword
                if (char.IsLetter(currentCharacter))
                {
                    string lexeme = ScanIdentifier(SourceCode, ref i);
                    FindTokenClass(lexeme);
                    continue;
                }

                // Integer or floating-point number
                if (char.IsDigit(currentCharacter))
                {
                    ScanNumber(SourceCode, ref i);
                    continue;
                }

                /*
                 * Check two-character operators before checking
                 * single-character operators.
                 *
                 * Examples:
                 * :=  <>  &&  ||
                 */
                if (i + 1 < SourceCode.Length)
                {
                    string twoCharacters =
                        SourceCode.Substring(i, 2);

                    if (Operators.ContainsKey(twoCharacters))
                    {
                        AddToken(
                            twoCharacters,
                            Operators[twoCharacters]
                        );

                        i++;
                        continue;
                    }
                }

                // Single-character operator
                string currentLexeme = currentCharacter.ToString();

                if (Operators.ContainsKey(currentLexeme))
                {
                    AddToken(
                        currentLexeme,
                        Operators[currentLexeme]
                    );

                    continue;
                }

                // Symbol such as ; , ( ) { }
                if (Symbols.ContainsKey(currentLexeme))
                {
                    AddToken(
                        currentLexeme,
                        Symbols[currentLexeme]
                    );

                    continue;
                }

                // Character does not belong to the Tiny language
                Errors.Error_List.Add(
                    "Unknown symbol: " + currentCharacter
                );
            }

            JASON_Compiler.TokenStream = Tokens;
        }

        private string ScanIdentifier(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            /*
             * Identifier regular expression:
             *
             * letter(letter | digit)*
             */
            while (index < sourceCode.Length &&
                   char.IsLetterOrDigit(sourceCode[index]))
            {
                index++;
            }

            string lexeme = sourceCode.Substring(
                startIndex,
                index - startIndex
            );

            // The for loop will increment the index again
            index--;

            return lexeme;
        }

        private void ScanNumber(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Read the integer part
            while (index < sourceCode.Length &&
                   char.IsDigit(sourceCode[index]))
            {
                index++;
            }

            /*
             * Optional decimal part:
             *
             * '.' followed by one or more digits
             */
            if (index < sourceCode.Length &&
                sourceCode[index] == '.')
            {
                index++;

                // A digit must exist after the decimal point
                if (index >= sourceCode.Length ||
                    !char.IsDigit(sourceCode[index]))
                {
                    string invalidNumber =
                        sourceCode.Substring(
                            startIndex,
                            index - startIndex
                        );

                    Errors.Error_List.Add(
                        "Invalid number: " + invalidNumber
                    );

                    index--;
                    return;
                }

                while (index < sourceCode.Length &&
                       char.IsDigit(sourceCode[index]))
                {
                    index++;
                }
            }

            /*
             * Detect a second decimal point.
             *
             * Example:
             * 2.3.4
             */
            if (index < sourceCode.Length &&
                sourceCode[index] == '.')
            {
                while (index < sourceCode.Length &&
                       (char.IsDigit(sourceCode[index]) ||
                        sourceCode[index] == '.'))
                {
                    index++;
                }

                string invalidNumber =
                    sourceCode.Substring(
                        startIndex,
                        index - startIndex
                    );

                Errors.Error_List.Add(
                    "Invalid number: " + invalidNumber
                );

                index--;
                return;
            }

            string lexeme = sourceCode.Substring(
                startIndex,
                index - startIndex
            );

            FindTokenClass(lexeme);

            index--;
        }

        private void ScanString(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Skip the opening quotation mark
            index++;

            while (index < sourceCode.Length &&
                   sourceCode[index] != '"')
            {
                index++;
            }

            if (index >= sourceCode.Length)
            {
                string invalidString =
                    sourceCode.Substring(startIndex);

                Errors.Error_List.Add(
                    "Unclosed string: " + invalidString
                );

                // End scanning because the rest belongs to the string
                index = sourceCode.Length;
                return;
            }

            /*
             * Include both quotation marks in the stored lexeme.
             *
             * Example:
             * "Hello World"
             */
            string lexeme = sourceCode.Substring(
                startIndex,
                index - startIndex + 1
            );

            AddToken(
                lexeme,
                Token_Class.StringLiteral
            );
        }

        private void ScanComment(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Skip the opening /*
            index += 2;

            bool commentClosed = false;

            while (index < sourceCode.Length - 1)
            {
                if (sourceCode[index] == '*' &&
                    sourceCode[index + 1] == '/')
                {
                    commentClosed = true;

                    /*
                     * Move to the slash of the closing sequence.
                     * The for loop will move to the next character.
                     */
                    index++;
                    break;
                }

                index++;
            }

            if (!commentClosed)
            {
                string invalidComment =
                    sourceCode.Substring(startIndex);

                Errors.Error_List.Add(
                    "Unclosed comment: " + invalidComment
                );

                index = sourceCode.Length;
            }

            /*
             * Correctly closed comments are recognized and ignored.
             * They are not added to the token stream.
             */
        }

        private void FindTokenClass(string lexeme)
        {
            if (string.IsNullOrEmpty(lexeme))
            {
                return;
            }

            // Check keyword before identifier
            if (ReservedWords.ContainsKey(lexeme))
            {
                AddToken(
                    lexeme,
                    ReservedWords[lexeme]
                );

                return;
            }

            if (Operators.ContainsKey(lexeme))
            {
                AddToken(
                    lexeme,
                    Operators[lexeme]
                );

                return;
            }

            if (Symbols.ContainsKey(lexeme))
            {
                AddToken(
                    lexeme,
                    Symbols[lexeme]
                );

                return;
            }

            if (IsIdentifier(lexeme))
            {
                AddToken(
                    lexeme,
                    Token_Class.Identifier
                );

                return;
            }

            if (IsConstant(lexeme))
            {
                AddToken(
                    lexeme,
                    Token_Class.Constant
                );

                return;
            }

            Errors.Error_List.Add(
                "Invalid token: " + lexeme
            );
        }

        private void AddToken(
            string lexeme,
            Token_Class tokenClass)
        {
            Token token = new Token();

            token.lex = lexeme;
            token.token_type = tokenClass;

            Tokens.Add(token);
        }

        private bool IsIdentifier(string lexeme)
        {
            if (string.IsNullOrEmpty(lexeme))
            {
                return false;
            }

            /*
             * An identifier must begin with a letter.
             */
            if (!char.IsLetter(lexeme[0]))
            {
                return false;
            }

            /*
             * The remaining characters must be
             * letters or digits.
             */
            for (int i = 1; i < lexeme.Length; i++)
            {
                if (!char.IsLetterOrDigit(lexeme[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsConstant(string lexeme)
        {
            if (string.IsNullOrEmpty(lexeme))
            {
                return false;
            }

            bool decimalPointFound = false;

            for (int i = 0; i < lexeme.Length; i++)
            {
                char currentCharacter = lexeme[i];

                if (char.IsDigit(currentCharacter))
                {
                    continue;
                }

                if (currentCharacter == '.')
                {
                    // More than one decimal point
                    if (decimalPointFound)
                    {
                        return false;
                    }

                    /*
                     * The decimal point cannot be the first or
                     * last character.
                     */
                    if (i == 0 || i == lexeme.Length - 1)
                    {
                        return false;
                    }

                    decimalPointFound = true;
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}