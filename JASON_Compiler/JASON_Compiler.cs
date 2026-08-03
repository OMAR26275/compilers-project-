using System.Collections.Generic;

namespace JASON_Compiler
{
    public static class JASON_Compiler
    {
        public static Scanner Jason_Scanner =
            new Scanner();

        public static List<Token> TokenStream =
            new List<Token>();

        public static void Start_Compiling(string sourceCode)
        {
            Jason_Scanner.StartScanning(sourceCode);
            TokenStream = Jason_Scanner.Tokens;
        }
    }
}