using System;
using System.Collections.Generic;

namespace JASON_Compiler
{
    public static class Errors
    {
        // Stores all lexical errors found during scanning
        public static List<string> Error_List = new List<string>();

        // Add a new error message
        public static void AddError(string message)
        {
            Error_List.Add(message);
        }

        // Clear all previous errors before scanning a new program
        public static void ClearErrors()
        {
            Error_List.Clear();
        }
    }
}