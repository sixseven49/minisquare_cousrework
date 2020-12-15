using Compiler.CodeGeneration;
using Compiler.IO;
using Compiler.Nodes;
using Compiler.SemanticAnalysis;
using Compiler.SyntacticAnalysis;
using Compiler.Tokenization;
using System.Collections.Generic;
using System.IO;
using static System.Console;

namespace Compiler
{
    /// <summary>
    /// Compiler for code in a source file
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// The file reader
        /// </summary>
        public IFileReader Reader { get; }

        /// <summary>
        /// The tokenizer
        /// </summary>
        public Tokenizer Tokenizer { get; }

        /// <summary>
        /// The parser
        /// </summary>
        public Parser Parser { get; }

        /// <summary>
        /// The identifier
        /// </summary>
        public DeclarationIdentifier Identifier { get; }

        /// <summary>
        /// The type checker
        /// </summary>
        public TypeChecker Checker { get; }

        /// <summary>
        /// The code generator
        /// </summary>
        public CodeGenerator Generator { get; }

        /// <summary>
        /// The target code writer
        /// </summary>
        public TargetCodeWriter Writer { get; }

        /// <summary>
        /// Creates a new compiler
        /// </summary>
        /// <param name="inputFile">The file containing the source code</param>
        /// <param name="binaryOutputFile">The file to write the binary target code to</param>
        /// <param name="textOutputFile">The file to write the text asembly code to</param>
        public Compiler(string inputFile, string binaryOutputFile, string textOutputFile)
        {
            Reporter = new ErrorReporter();
            Reader = new FileReader(inputFile);
            Tokenizer = new Tokenizer(Reader, Reporter);
            Parser = new Parser(Reporter);
            Identifier = new DeclarationIdentifier(Reporter);
            Checker = new TypeChecker(Reporter);
            Generator = new CodeGenerator(Reporter);
            Writer = new TargetCodeWriter(binaryOutputFile, textOutputFile, Reporter);
        }

        /// <summary>
        /// Performs the compilation process
        /// </summary>
        public void Compile()
        {
            // Tokenize
            Write("Tokenising...");
            List<Token> tokens = Tokenizer.GetAllTokens();
            // changed this to spit out wats up rather than just killing the process
            //if (Reporter.TokenizerHasErrors) return;
            WriteLine("Done");

            // Parse
            Write("Parsing...");
            ProgramNode tree = Parser.Parse(tokens);
            // by returning it here it kills the process
            //if (Reporter.ParserHasErrors) return;
            WriteLine("Done");

            // Identify
            Write("Identifying...");
            Identifier.PerformIdentification(tree);
            //if (Reporter.ParserHasErrors) return;
            WriteLine("Done");

            // Type check
            Write("Type Checking...");
            Checker.PerformTypeChecking(tree);
            //if (Reporter.CheckingHasErrors) return;
            WriteLine("Done");

            WriteLine(TreePrinter.ToString(tree));
            // Code generation
            Write("Generating code...");
            TargetCode targetCode = Generator.GenerateCodeFor(tree);
            //if (Reporter.HasErrors) return;
            WriteLine("Done");

            // Output
            Write("Writing to file...");
            Writer.WriteToFiles(targetCode);
            //if (Reporter.HasErrors) return;
            WriteLine("Done");
        }

        private void printLoop(List<string> errorList)
        {
           for (int p = 0; p < errorList.Count; p++)
           {
                WriteLine($"Error: {errorList[p]}");
            }
        }
        /// <summary>
        /// Writes a message reporting on the success of compilation
        /// just prints the errors found at the end NICEEEE
        /// </summary>
        private void WriteFinalMessage()
        {
            if (Reporter.anyErrors())
            {
                //Write output to tell the user whether it worked or not here
                if (Reporter.TokenizerHasErrors)
                {
                    WriteLine("List presents the errors found in the current file in thr tokenizer stage:");
                    // prints out all the found errors
                    printLoop(Reporter.TokenizerErrorPositions);
                }
                if (Reporter.ParserHasErrors)
                {
                    WriteLine("List presents the errors found in the current file in thr Parser stage:");
                    // prints out all the found errors
                    printLoop(Reporter.ParserErrorPositions);
                }
                if (Reporter.IdentifierHasErrors)
                {
                    WriteLine("List presents the errors found in the current file in thr Identifier stage:");
                    // prints out all the found errors
                    printLoop(Reporter.IdentifierErrorPositions);
                }
                if (Reporter.CheckingHasErrors)
                {
                    WriteLine("List presents the errors found in the current file in thr Checking stage:");
                    // prints out all the found errors
                    printLoop(Reporter.CheckingErrorPositions);
                }
            }
            else
            {
                WriteLine("No errors where found, Good job");
            }
        }

       


        /// <summary>
        /// Compiles the code in a file
        /// </summary>
        /// <param name="args">Should be one argument, the input file (*.tri)</param>
        public static void Main(string[] args)
        {
            string inputFile = "/Users/christinechau/Documents/Languages nd Compiler_CM4106/coursework/example.tri";
            string binaryOutputFile = "/Users/christinechau/Documents/Languages nd Compiler_CM4106/coursework/example.tam";
            string textOutputFile = "/Users/christinechau/Documents/Languages nd Compiler_CM4106/coursework/out.txt";
            if (!inputFile.Contains(".tri"))
            {
                WriteLine("ERROR: Must call the program with exactly one argument, the input file (*.tri)");
            }
            else if (inputFile == "")
                WriteLine($"ERROR: The input file \"{Path.GetFullPath(args[0])}\" does not exist");
            else
            {
                Compiler compiler = new Compiler(inputFile, binaryOutputFile, textOutputFile);
                WriteLine("Compiling...");
                compiler.Compile();
                compiler.WriteFinalMessage();
            }
        }
    }
}
