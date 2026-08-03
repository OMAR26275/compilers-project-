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
        public string lex;
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
            // Keywords are recognized without case sensitivity.
            ReservedWords =
                new Dictionary<string, Token_Class>(
                    StringComparer.OrdinalIgnoreCase);

            Operators = new Dictionary<string, Token_Class>();
            Symbols = new Dictionary<string, Token_Class>();

            InitializeReservedWords();
            InitializeOperators();
            InitializeSymbols();
        }

        private void InitializeReservedWords()
        {
            // Datatypes
            ReservedWords.Add("int", Token_Class.Integer);
            ReservedWords.Add("float", Token_Class.Real);
            ReservedWords.Add("string", Token_Class.StringType);

            // Input and output
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("endl", Token_Class.Endl);

            // Repetition
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);

            // Conditional statements
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("end", Token_Class.End);

            // Functions
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("main", Token_Class.Main);
        }

        private void InitializeOperators()
        {
            // Arithmetic
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);

            // Assignment
            Operators.Add(":=", Token_Class.AssignmentOp);

            // Conditions
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);

            // Boolean
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

        public void StartScanning(string sourceCode)
        {
            Tokens.Clear();
            Errors.ClearErrors();

            if (sourceCode == null)
            {
                Errors.AddError("Source code cannot be null.");
                JASON_Compiler.TokenStream = Tokens;
                return;
            }

            for (int i = 0; i < sourceCode.Length; i++)
            {
                char currentCharacter = sourceCode[i];

                // Ignore spaces, tabs and line endings.
                if (char.IsWhiteSpace(currentCharacter))
                {
                    continue;
                }

                // Check comments before the division operator.
                if (currentCharacter == '/' &&
                    i + 1 < sourceCode.Length &&
                    sourceCode[i + 1] == '*')
                {
                    ScanComment(sourceCode, ref i);
                    continue;
                }

                // String literal.
                if (currentCharacter == '"')
                {
                    ScanString(sourceCode, ref i);
                    continue;
                }

                // Identifier or reserved keyword.
                if (IsEnglishLetter(currentCharacter))
                {
                    string lexeme = ScanIdentifier(sourceCode, ref i);
                    FindTokenClass(lexeme);
                    continue;
                }

                // Integer, decimal number or invalid digit-starting identifier.
                if (char.IsDigit(currentCharacter))
                {
                    ScanNumber(sourceCode, ref i);
                    continue;
                }

                // Two-character operators must be checked first.
                if (i + 1 < sourceCode.Length)
                {
                    string twoCharacters = sourceCode.Substring(i, 2);

                    if (Operators.ContainsKey(twoCharacters))
                    {
                        AddToken(
                            twoCharacters,
                            Operators[twoCharacters]);

                        i++;
                        continue;
                    }
                }

                string currentLexeme = currentCharacter.ToString();

                // Single-character operators.
                if (Operators.ContainsKey(currentLexeme))
                {
                    AddToken(
                        currentLexeme,
                        Operators[currentLexeme]);

                    continue;
                }

                // Symbols.
                if (Symbols.ContainsKey(currentLexeme))
                {
                    AddToken(
                        currentLexeme,
                        Symbols[currentLexeme]);

                    continue;
                }

                // Unknown Tiny-language character.
                Errors.AddError(
                    "Unknown symbol: " + currentCharacter);
            }

            JASON_Compiler.TokenStream = Tokens;
        }

        private string ScanIdentifier(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Identifier = letter(letter | digit)*
            while (index < sourceCode.Length &&
                   (IsEnglishLetter(sourceCode[index]) ||
                    char.IsDigit(sourceCode[index])))
            {
                index++;
            }

            string lexeme = sourceCode.Substring(
                startIndex,
                index - startIndex);

            index--;
            return lexeme;
        }

        private void ScanNumber(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Read the integer part.
            while (index < sourceCode.Length &&
                   char.IsDigit(sourceCode[index]))
            {
                index++;
            }

            /*
             * Detect a malformed identifier that starts with digits.
             * Example: 123x
             */
            if (index < sourceCode.Length &&
                IsEnglishLetter(sourceCode[index]))
            {
                while (index < sourceCode.Length &&
                       (IsEnglishLetter(sourceCode[index]) ||
                        char.IsDigit(sourceCode[index])))
                {
                    index++;
                }

                string invalidIdentifier = sourceCode.Substring(
                    startIndex,
                    index - startIndex);

                Errors.AddError(
                    "Invalid identifier: " + invalidIdentifier);

                index--;
                return;
            }

            // Optional decimal part.
            if (index < sourceCode.Length &&
                sourceCode[index] == '.')
            {
                index++;

                // A decimal point must be followed by a digit.
                if (index >= sourceCode.Length ||
                    !char.IsDigit(sourceCode[index]))
                {
                    string invalidNumber = sourceCode.Substring(
                        startIndex,
                        index - startIndex);

                    Errors.AddError(
                        "Invalid number: " + invalidNumber);

                    index--;
                    return;
                }

                while (index < sourceCode.Length &&
                       char.IsDigit(sourceCode[index]))
                {
                    index++;
                }
            }

            // Detect another decimal point, such as 12.5.8.
            if (index < sourceCode.Length &&
                sourceCode[index] == '.')
            {
                while (index < sourceCode.Length &&
                       (char.IsDigit(sourceCode[index]) ||
                        sourceCode[index] == '.'))
                {
                    index++;
                }

                string invalidNumber = sourceCode.Substring(
                    startIndex,
                    index - startIndex);

                Errors.AddError(
                    "Invalid number: " + invalidNumber);

                index--;
                return;
            }

            // Detect letters after a decimal value, such as 12.5abc.
            if (index < sourceCode.Length &&
                IsEnglishLetter(sourceCode[index]))
            {
                while (index < sourceCode.Length &&
                       (IsEnglishLetter(sourceCode[index]) ||
                        char.IsDigit(sourceCode[index])))
                {
                    index++;
                }

                string invalidToken = sourceCode.Substring(
                    startIndex,
                    index - startIndex);

                Errors.AddError(
                    "Invalid token: " + invalidToken);

                index--;
                return;
            }

            string lexeme = sourceCode.Substring(
                startIndex,
                index - startIndex);

            AddToken(lexeme, Token_Class.Constant);

            index--;
        }

        private void ScanString(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Skip the opening quote.
            index++;

            while (index < sourceCode.Length &&
                   sourceCode[index] != '"' &&
                   sourceCode[index] != '\r' &&
                   sourceCode[index] != '\n')
            {
                index++;
            }

            // The line ended before a closing quote.
            if (index >= sourceCode.Length ||
                sourceCode[index] == '\r' ||
                sourceCode[index] == '\n')
            {
                string invalidString = sourceCode.Substring(
                    startIndex,
                    index - startIndex);

                Errors.AddError(
                    "Unclosed string: " + invalidString);

                /*
                 * Move back one position so that the main loop
                 * processes the line-ending character normally.
                 */
                index--;
                return;
            }

            // Include both double quotes in the string lexeme.
            string lexeme = sourceCode.Substring(
                startIndex,
                index - startIndex + 1);

            AddToken(
                lexeme,
                Token_Class.StringLiteral);
        }

        private void ScanComment(
            string sourceCode,
            ref int index)
        {
            int startIndex = index;

            // Skip /*
            index += 2;

            bool commentClosed = false;

            while (index < sourceCode.Length - 1)
            {
                if (sourceCode[index] == '*' &&
                    sourceCode[index + 1] == '/')
                {
                    commentClosed = true;

                    // Move to the '/' of the closing sequence.
                    index++;
                    break;
                }

                index++;
            }

            if (!commentClosed)
            {
                string invalidComment =
                    sourceCode.Substring(startIndex);

                Errors.AddError(
                    "Unclosed comment: " + invalidComment);

                /*
                 * An unfinished comment owns the remainder of
                 * the source program, so scanning ends here.
                 */
                index = sourceCode.Length;
            }

            // Valid comments are ignored and produce no token.
        }

        private void FindTokenClass(string lexeme)
        {
            if (string.IsNullOrEmpty(lexeme))
            {
                return;
            }

            // Keywords must be checked before identifiers.
            if (ReservedWords.ContainsKey(lexeme))
            {
                AddToken(
                    lexeme,
                    ReservedWords[lexeme]);

                return;
            }

            if (Operators.ContainsKey(lexeme))
            {
                AddToken(
                    lexeme,
                    Operators[lexeme]);

                return;
            }

            if (Symbols.ContainsKey(lexeme))
            {
                AddToken(
                    lexeme,
                    Symbols[lexeme]);

                return;
            }

            if (IsIdentifier(lexeme))
            {
                AddToken(
                    lexeme,
                    Token_Class.Identifier);

                return;
            }

            if (IsConstant(lexeme))
            {
                AddToken(
                    lexeme,
                    Token_Class.Constant);

                return;
            }

            Errors.AddError(
                "Invalid token: " + lexeme);
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

            if (!IsEnglishLetter(lexeme[0]))
            {
                return false;
            }

            for (int i = 1; i < lexeme.Length; i++)
            {
                if (!IsEnglishLetter(lexeme[i]) &&
                    !char.IsDigit(lexeme[i]))
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
                    if (decimalPointFound)
                    {
                        return false;
                    }

                    // A dot cannot start or end the number.
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

        private bool IsEnglishLetter(char character)
        {
            return (character >= 'a' && character <= 'z') ||
                   (character >= 'A' && character <= 'Z');
        }
    }
}