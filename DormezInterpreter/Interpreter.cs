using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harpy;
using Harpy.Evaluation;
namespace HarpyCompiler
{
    class Program
    {
        public const bool DEBUG_MODE = false;

        static void Main(string[] args)
        {
            if(args.Length > 2 || args.Length < 1)
            {
                Console.WriteLine("Proper usage: harpy.exe <filename> [output filename]");
                return;
            }
            
            string filename = args[0];
            string outputFile = args.Length > 1 ? args[1] : filename.Remove(filename.LastIndexOf('.')) + ".bat";
            
            if(!File.Exists(filename))
            {
                Console.WriteLine("File does not exist: " + filename);
            }

            Lexer.inputText = File.ReadAllText(args[0]);
            Console.WriteLine("File read successfully");


            Parser p = new Parser();
            Console.WriteLine("Successfully initialized parser");

            Variables.parser = p;
            StandardLibrary.p = p;

            StandardLibrary.Init();
            Console.WriteLine("Successfully initialized internal standard library");

            UserFunctions.p = p;

            Evaluator eval = new Evaluator(p);
            p.evaluator = eval;
            Console.WriteLine("Successfully initialized evaluator");
            Console.WriteLine("Compiling...");
            
            string output;
            if(DEBUG_MODE)
            {
                output = p.Compile();
            }
            else
            {
                try
                {
                    output = p.Compile();
                }
                catch (Exception e)
                {
                    Console.WriteLine("COMPILATION ERROR:");
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            Console.WriteLine("Compilation successful.");

            for (int i = 0; i < 10; i++)
            {
                output = output.Replace("\n\n", "\n");
            }

            output = output.Replace("<NL>", "\n");
            output = output.Replace("\n", "\r\n");

            Console.WriteLine("Compiled code formatted successfully");

            File.WriteAllText(outputFile, output);
            Console.WriteLine("Wrote to file '" + outputFile + "' successfully");

            if(DEBUG_MODE) System.Diagnostics.Process.Start(outputFile);

            Console.WriteLine("Compilation complete");
        }
    }
}
