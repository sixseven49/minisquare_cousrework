using System;
using System.Collections.Generic;

namespace Compiler.IO
{
    /// <summary>
    /// An object for reporting errors in the compilation process
    /// </summary>
    public class ErrorReporter
    {


        //public bool HasErrors { set; get; }

        //public List<string> ErrorPositions { set; get; } = new List<string>();

        /// <summary>
        /// Whether or not any errors have been encountered
        /// </summary>
        public bool TokenizerHasErrors { set; get; }

        public List<string> TokenizerErrorPositions { set; get; } = new List<string>();

        public bool ParserHasErrors { set; get; }

        public List<string> ParserErrorPositions { set; get; } = new List<string>();

        public bool IdentifierHasErrors { set; get; }

        public List<string> IdentifierErrorPositions { set; get; } = new List<string>();
        

        public bool CheckingHasErrors { set; get; }

        public List<string> CheckingErrorPositions { set; get; } = new List<string>();

        public bool ExportHasErrors { set; get; }

        public List<string> ExportErrorPositions { set; get; } = new List<string>();

        public bool anyErrors()
        {
            return (TokenizerHasErrors || ParserHasErrors || IdentifierHasErrors || CheckingHasErrors || ExportHasErrors);
        }

    }
}