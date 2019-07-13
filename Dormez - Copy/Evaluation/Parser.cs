using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harpy;

namespace Harpy.Evaluation
{
    public class Parser
    {
        public static Parser current;

        //public List<Token> tokens = new List<Token>();
        public Evaluator evaluator;
        
        public int depth = 0;
        public int lastLexerPoint;

        public string output;

        public List<string> imported = new List<string>();

        public ParserException Exception(string message)
        {
            if(evaluator.currentContext != null)
            {
                message += "\nError occured in the function: " + UserFunctions.GetSignature(evaluator.currentContext);
            }
            else
            {
                message += "\nError occured in the global scope.";
            }
            return new ParserException(CurrentToken, message);
        }

        public Parser()
        {
            lastLexerPoint = 0;
            PreviousToken = null;
            CurrentToken = Lexer.GetNextToken();
        }
        
        public Token CurrentToken;
        public Token PreviousToken;
        
        public string GetIdentifier()
        {
            return Eat<string>("identifier");
        }

        public T Eat<T>(string type)
        {
            if (CurrentToken == type)
            {
                return Eat<T>();
            }

            throw Exception("Expected " + type + " but got " + CurrentToken.Type);
        }

        public object Eat(string type)
        {
            return Eat<object>(type);
        }

        public T Eat<T>()
        {
            if(CurrentToken == "l curly")
            {
                depth++;
            }
            else if(CurrentToken == "r curly")
            {
                depth--;
                Variables.DeleteOutOfScopeVariables();
            }

            if(depth < 0)
            {
                throw Exception("Depth is less than zero!");
            }

            //pointer++;

            lastLexerPoint = Lexer.pointer;
            PreviousToken = CurrentToken;
            CurrentToken = Lexer.GetNextToken();

            return (T) PreviousToken.Value;
        }

        public void Comment(string message)
        {
            EmitLn("REM -- " + message);
        }
        
        public void TryEat(string type)
        {
            if (CurrentToken == type)
            {
                Eat();
            }
        }

        public object Eat()
        {
            return Eat<object>();
        }

        public void EmitLn(string line)
        {
            output += line + "\n";
        }

        public void Emit(string text)
        {
            output += text;
        }
        
        /*public ParserLocation GetLocation()
        {
            return new ParserLocation()
            {
                pointer = pointer,
                depth = depth
            };
        }

        /// <summary>
        /// Goes to a location in code, changing scope and deleting unscoped variables
        /// </summary>
        /// <param name="location"></param>
        public void Goto(ParserLocation location)
        {
            pointer = location.pointer;
            depth = location.depth;
        }*/

        public PseudoStack<int> loopIDs = new PseudoStack<int>();

        public string PreEvaluateBlock()
        {
            int index = output.Length;
            string originalOutput = output;
            EvaluateBlock();
            string newOutput = output;
            output = originalOutput;
            return newOutput.Substring(index);
        }

        //public void EvaluateFunction()
        //{
        //    int d = depth;
        //    Eat("l curly");
        //    while (depth != d && !is)
        //    {
        //        string line = evaluator.Evaluate().value + "\n";
        //        output += line;
        //    }
        //}

        public void Reset()
        {
            lastLexerPoint = 0;
            Lexer.pointer = 0;
            PreviousToken = null;
            CurrentToken = Lexer.GetNextToken();
        }

        public void EvaluateBlock()
        {
            int d = depth;
            Eat("l curly");
            while(depth != d)
            {
                string line = evaluator.Evaluate().value + "\n";
                if (line.Trim().StartsWith("%")) continue;

                output += line;
            }
        }

        public void SkipBlock()
        {
            int d = depth;
            Eat("l curly");
            while (depth != d)
            {
                Eat();
            }
        }
        
        public string Compile()
        {
            EmitLn("@echo off");
            EmitLn("setlocal EnableExtensions");
            EmitLn("setlocal EnableDelayedExpansion<NL>");
            Comment("SETTING WINDOW TITLE");
            EmitLn("set window_title=Harpy Program");
            EmitLn("title Harpy Program<NL>");
            Comment("GLOBAL VARIABLE DECLARATION");

            /*for (int i = 0; i < 9; i++)
            {
                Variables.Create("arg_" + i, new Value(Value.BatchType.String, "%" + (i + 1)));
            }*/

            // import includes
            while (CurrentToken != "eof")
            {
                if (CurrentToken == "include")
                {
                    Eat();
                    evaluator.DoIncludes();
                    Reset();
                    //break;
                }
                else
                {
                    Eat();
                }
            }

            Reset();

            // register global variables
            while (CurrentToken != "eof")
            {
                if (CurrentToken == "function")
                {
                    Eat();

                    while (CurrentToken != "l curly")
                    {
                        Eat();
                    }

                    SkipBlock();
                }
                else if (CurrentToken == "var")
                {
                    Eat();
                    evaluator.RegisterGlobalVariable();
                }
                else
                {
                    Eat();
                }
            }

            Reset();

            // register functions
            while (CurrentToken != "eof")
            {
                if(CurrentToken == "function")
                {
                    Eat();
                    evaluator.RegisterFunction();
                }
                else
                {
                    Eat();
                }
            }

            Reset();

            //UserFunctions.userFuncs.Reverse();
            if(!UserFunctions.Exists("main"))
            {
                throw Exception("Program is missing an entry point marked by: main(...): void");
            }

            if(UserFunctions.userFuncs.Count(x => x.name == "main") > 1)
            {
                throw Exception("Cannot have multiple entry points");
            }

            var entryPoint = UserFunctions.userFuncs.Find(x => x.name == "main");
            var id = UserFunctions.GetID(entryPoint);

            foreach (var type in entryPoint.argumentTypes)
            {
                if(type != Value.BatchType.String)
                {
                    throw Exception("Entry point can only have parameters of type string");
                }
            }

            // compile program
            Lexer.mainCompile = true;

            while (CurrentToken != "eof")
            {
                if(CurrentToken != "function" && CurrentToken != "var" && CurrentToken != "semicolon")
                {
                    throw Exception("Unexpected token in the global scope: " + CurrentToken);
                }

                string line = evaluator.Evaluate().value + "\n";
                if (line.Trim().StartsWith("%")) continue;

                //Variables.ClearTemporaryVariables();

                output += line;
            }
            
            output += "<NL>";
            Comment("CALL TO ENTRY POINT");
            output += "call :func_" + id + " %1 %2 %3 %4 %5 %6 %7 %8 %9\n";

            // append function bodies
            output += "goto :EOF\n<NL>";
            foreach (var func in UserFunctions.userFuncs)
            {
                output += func.body + "<NL>";
            }
            
            Lexer.mainCompile = false;

            return output;
        }

    }
}
