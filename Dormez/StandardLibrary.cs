using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harpy.Evaluation;

namespace Harpy
{
    public static class StandardLibrary
    {
        public static Parser p;

        public static List<KeyValuePair<string, Value.BatchType[]>> functionInputs = new List<KeyValuePair<string, Value.BatchType[]>>();
        public static List<KeyValuePair<string, Func<Value[], Value>>> functions = new List<KeyValuePair<string, Func<Value[], Value>>>();

        private static void VerifyType(Value v, params Value.BatchType[] types)
        {
            foreach (var type in types)
            {
                if (v.type == type) return;
            }

            throw p.Exception("Variable is of type " + v.type + ". Expected any of the following: " + string.Join(", ", types.Select(x => x.ToString())));
        }

        private static void VerifyArgs(Value[] args, params Value.BatchType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                VerifyType(args[i], types[i]);
            }
        }

        private static void RegisterFunction(string name, Func<Value[], Value> func, params Value.BatchType[] argumentTypes)
        {
            functionInputs.Add(new KeyValuePair<string, Value.BatchType[]>(name, argumentTypes));
            functions.Add(new KeyValuePair<string, Func<Value[], Value>>(name, func));
        }
        
        public static bool Exists(string name)
        {
            return functions.Exists(x => x.Key == name);
        }

        public static Value CallFunction(string name, Value[] args)
        {
            for (int i = 0; i < functionInputs.Count; i++)
            {
                var input = functionInputs[i];

                if (name != input.Key)
                {
                    continue;
                }

                if (args.Length != input.Value.Length)
                {
                    continue;
                }

                bool invalid = false;
                for (int j = 0; j < input.Value.Length; j++)
                {
                    if (args[j].type != input.Value[j])
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                {
                    continue;
                }

                return functions[i].Value.Invoke(args);
            }

            throw p.Exception("Function '" + name + "' does not accept the following parameters: " + string.Join(", ", args.Select(x => x.type.ToString())));
        }

        #region Functions
        public static Value Prompt(Value[] args)
        {
            string prompt = args.Length > 0 ? args[0].content : "";
            return new Value(Value.BatchType.String, Variables.CreateTemporaryInput(prompt));
        }
        
        public static Value ToInt(Value[] args)
        {
            return new Value(Value.BatchType.Int, args[0].content);
        }

        public static Value Clear(Value[] args)
        {
            return new Value(Value.BatchType.Void, "cls");
        }
        
        public static Value IntToString(Value[] args)
        {
            return new Value(Value.BatchType.String, Variables.CreateTemporaryInt(args[0].content));
        }

        public static Value BoolToString(Value[] args)
        {
            string reference = Variables.CreateTemporary("");
            string name = Variables.NameFromTempReference(reference);
            string temp = Variables.CreateTemporary(args[0].content);
            p.EmitLn("if " + temp + " (set " + name + "=true) else (set " + name + "=false)");
            return new Value(Value.BatchType.String, Variables.CreateTemporary(reference));
        }

        public static Value ArrayToString(Value[] args)
        {
            string name = Variables.NameFromReference(args[0].content);
            return new Value(Value.BatchType.String, "Array length: " + Variables.GetReference(name + "_length"));
        }

        public static Value CreateVariable(Value[] args)
        {
            //Console.WriteLine("ADDED VARIABLE: " + args[0].value);
            Value.BatchType type = (Value.BatchType)Enum.Parse(typeof(Value.BatchType), Evaluator.ToTitleCase(args[1].content));
            Variables.Create(args[0].content, type, 0);
            return Value.VOID;
        }

        public static Value DeclareVariable(Value[] args)
        {
            Value.BatchType type = (Value.BatchType)Enum.Parse(typeof(Value.BatchType), Evaluator.ToTitleCase(args[1].content));
            Variables.Create(args[0].content, new Value(type, args[2].content), 0);
            return Value.VOID;
        }
        

        public static Value RawVoid(Value[] args)
        {
            return new Value(Value.BatchType.Indeterminate, args[0].content);
        }

        public static Value SetTitle(Value[] args)
        {
            p.EmitLn("set \"window_title=" + args[0].content + "\"");
            return new Value(Value.BatchType.Void, "title " + args[0].content);
        }

        public static Value GetTitle(Value[] args)
        {
            return new Value(Value.BatchType.String, "%window_title%");
        }

        public static Value NewLine(Value[] args)
        {
            return new Value(Value.BatchType.Void, "echo:");
        }

        public static Value Write(Value[] args)
        {
            return new Value(Value.BatchType.Void, "echo | set /p ^=" + Lexer.Sanitize(args[0].content));
        }

        public static Value Print(Value[] args)
        {
            return new Value(Value.BatchType.Void, "echo:" + Lexer.Sanitize(args[0].content));
        }

        public static Value IsNumber(Value[] args)
        {
            string tempBool = Variables.CreateTemporary("1==0");
            string name = Variables.NameFromTempReference(tempBool);
            string temp = Variables.CreateTemporary(args[0].content);
            p.EmitLn("if " + temp + " equ +" + temp + " (set " + name + "=1==1)");
            return new Value(Value.BatchType.Bool, tempBool);
        }
        
        public static Value ChangeType(Value[] args)
        {
            return new Value(Value.ParseType(args[0].content), args[1].content);
        }

        public static Value GetVarType(Value[] args)
        {
            return new Value(Value.BatchType.String, Variables.Get(args[0].content).type.ToString().ToLower());
        }

        public static Value IsNull(Value[] args)
        {
            string tempBool = Variables.CreateTemporary("1==0");
            string name = Variables.NameFromTempReference(tempBool);
            p.EmitLn("if defined var_" + args[0].content + " set " + name + "=1==1");
            return new Value(Value.BatchType.Bool, tempBool);
        }

        public static Value VarExists(Value[] args)
        {
            return new Value(Value.BatchType.Bool, Variables.Exists(args[0].content) ? "1==1" : "1==0");
        }

        public static Value Pause(Value[] args)
        {
            return new Value(Value.BatchType.Void, "pause");
        }

        public static Value Error(Value[] args)
        {
            p.EmitLn("echo ERROR: " + args[0].content);
            p.EmitLn("pause");
            p.EmitLn("exit");
            return Value.VOID;
        }
        #endregion

        public static void Init()
        {
            // raw(content: string): void
            RegisterFunction("raw", RawVoid, Value.BatchType.String);
            // unsafe_cast(T: string, value: indeterminate): T
            RegisterFunction("type", ChangeType, Value.BatchType.String, Value.BatchType.Indeterminate);

            // var_defined(pointer: pointer): bool
            RegisterFunction("var_exists", IsNull, Value.BatchType.String);
            // var_type(pointer: string): string
            RegisterFunction("var_type", GetVarType, Value.BatchType.String);

            RegisterFunction("to_string", IntToString, Value.BatchType.Int);
            RegisterFunction("to_string", BoolToString, Value.BatchType.Bool);
            RegisterFunction("to_string", ArrayToString, Value.BatchType.Array);

            RegisterFunction("is_valid_number", IsNumber, Value.BatchType.String);
            RegisterFunction("to_int", ToInt, Value.BatchType.String);

            RegisterFunction("title", SetTitle, Value.BatchType.String);
            RegisterFunction("title", GetTitle);
            RegisterFunction("clear", Clear);
            RegisterFunction("input", Prompt);
            RegisterFunction("input", Prompt, Value.BatchType.String);
            RegisterFunction("print", NewLine);
            RegisterFunction("print", Print, Value.BatchType.String);
            RegisterFunction("write", Write, Value.BatchType.String);
            RegisterFunction("pause", Pause);
            RegisterFunction("error", Error, Value.BatchType.String);
        }
    }
}
