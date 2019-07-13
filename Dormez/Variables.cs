using System.Collections.Generic;
using Harpy.Evaluation;
using System.Linq;
namespace Harpy
{
    public static class Variables
    {
        public static Parser parser;
        public static string suffix = "";

        #region Temporary Variables
        private static int temporaryVariableCount = 0;

        public static void ClearTemporaryVariables()
        {
            temporaryVariableCount = 0;
        }

        public static string FlexibleTemporary(Value value)
        {
            if (value.type == Value.BatchType.Int)
                return CreateTemporaryInt(value.content);

            return CreateTemporary(value.content);
        }

        public static string CreateTemporary(string value)
        {
            parser.output += "set \"temp_" + temporaryVariableCount + "=" + value + "\"\n";
            return "%temp_" + temporaryVariableCount++ + "%";
        }

        public static string CreateTemporaryInt(string value)
        {
            parser.output += "set /a \"temp_" + temporaryVariableCount + "=" + value + "\"\n";
            return "%temp_" + temporaryVariableCount++ + "%";
        }

        public static string CreateTemporaryInput(string value)
        {
            parser.output += "set /p \"temp_" + temporaryVariableCount + "=" + value + "\"\n";
            return "%temp_" + temporaryVariableCount++ + "%";
        }
        
        #endregion

        public static List<Variable> variables = new List<Variable>();

        public static bool Exists(string name)
        {
            if (!variables.Exists(x => x.name == name + suffix))
            {
                if (variables.Exists(x => x.name == name))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public static Variable Get(string name)
        {
            if (name.StartsWith("%var_"))
            {
                return new Variable(name, Value.BatchType.Indeterminate);
            }

            if (!variables.Exists(x => x.name == name + suffix))
            {
                if(variables.Exists(x => x.name == name))
                {
                    return variables.Find(x => x.name == name);
                }

                throw parser.Exception("No such variable exists: " + name);
            }

            return variables.Find(x => x.name == name + suffix);
        }

        public static Value GetReference(string name)
        {
            var v = Get(name);
            return new Value(v.type, "!var_" + v.name + (v.depth > 0 ? "" : "") + "!");
        }

        public static string NameFromReference(string reference)
        {
            if(reference.StartsWith("!var_") && reference.EndsWith("!"))
            {
                return reference.Remove(reference.Length - 1).Substring(5);
            }
            else
            {
                throw new System.ArgumentException("Improperly formatted reference");
            }
        }

        public static string NameFromTempReference(string reference)
        {
            if (reference.StartsWith("%temp_") && reference.EndsWith("%"))
            {
                return reference.Remove(reference.Length - 1).Substring(1);
            }
            else
            {
                throw new System.ArgumentException("Improperly formatted reference");
            }
        }

        public static Value Create(string name, Value value)
        {
            variables.Add(new Variable(name, value.type));
            parser.output += "set " + (value.type == Value.BatchType.Int ? "/a " : "") + "\"var_" + name + "=" + value.content + "\"\n";
            return GetReference(name);
        }

        public static void Create(string name, Value.BatchType type)
        {
            variables.Add(new Variable(name, type));
        }

        public static Value Create(string name, Value value, int depth)
        {
            variables.Add(new Variable(name + suffix, value.type, depth));
            parser.output += "set " + (value.type == Value.BatchType.Int ? "/a " : "") + "\"var_" + name + (depth > 0 ? suffix : "") + "=" + value.content + "\"\n";
            return GetReference(name);
        }

        public static void Create(string name, Value.BatchType type, int depth)
        {
            variables.Add(new Variable(name + suffix, type, depth));
        }

        public static Value Assign(string name, Value value)
        {
            var found = Get(name);

            if(Value.TypeConflict(found.type, value.type) && found.type != Value.BatchType.Indeterminate)
            {
                throw parser.Exception("Cannot assign value of type " + found.type + " to a variable of type " + value.type);
            }

            parser.output += "set " + (value.type == Value.BatchType.Int ? "/a " : "") + "\"var_" + name + (found.depth > 0 ? suffix : "") + "=" + value.content + "\"\n";
            return GetReference(name);
        }

        public static void Delete(string name, bool addSuffix = false)
        {
            var v = Get(name);
            variables.Remove(v);
            parser.output += "set \"var_" + name + (v.depth > 0 && addSuffix ? suffix : "") + "=\"\n";
        }

        public static void DeleteOutOfScopeVariables()
        {
            List<string> toDelete = new List<string>();

            for (int i = 0; i < variables.Count; i++)
            {
                if(variables[i].depth > parser.depth)
                {
                    toDelete.Add(variables[i].name);
                }
            }

            foreach (var variable in toDelete)
            {
                Delete(variable);
            }
        }

        public static void VerifyType(string name, params Value.BatchType[] types)
        {
            var v = Get(name);
            foreach (var type in types)
            {
                if (v.type == type) return;
            }

            throw parser.Exception("Variable is of type " + v.type + ". Expected any of the following: " + string.Join(", ", types.Select(x => x.ToString())));
        }
    }
}
