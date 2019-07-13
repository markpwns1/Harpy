using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harpy.Evaluation;

namespace Harpy
{
    public static class UserFunctions
    {
        public static Parser p;

        public static List<UserFunction> userFuncs = new List<UserFunction>();

        public static bool Exists(string name)
        {
            return userFuncs.Exists(x => x.name == name);
        }

        public static bool Exists(string name, List<Value.BatchType> args)
        {
            return userFuncs.Exists(x => x.name == name && ArgumentsValid(args, x.argumentTypes));
        }

        public static bool Exists(string name, List<Value.BatchType> args, Value.BatchType returnType)
        {
            return userFuncs.Exists(x => x.name == name && ArgumentsValid(args, x.argumentTypes) && x.returnType == returnType);
        }

        public static bool ArgumentsValid(List<Value.BatchType> inputted, List<Value.BatchType> required)
        {
            if(inputted.Count != required.Count)
            {
                return false;
            }

            bool valid = true;
            for (int i = 0; i < required.Count; i++)
            {
                if (required[i] == Value.BatchType.Indeterminate) continue;
                if (required[i] != inputted[i]) valid = false;
            }
            return valid;
        }

        public static UserFunction Get(string name, List<Value.BatchType> args)
        {
            var func = userFuncs.Find(x => x.name == name && ArgumentsValid(args, x.argumentTypes));
            if (func == null)
            {
                throw p.Exception("Cannot find the following function: " + GetSignature(name, args));
            }

            return func;
        }

        public static string GetSignature(string name, List<Value.BatchType> args)
        {
            return name + "(" + string.Join(", ", args.Select(x => x.ToString().ToLower())) + ")";
        }

        public static string GetSignature(UserFunction func)
        {
            return func.inline ? "inline " : "" + func.name + "(" + string.Join(", ", func.argumentTypes.Select(x => x.ToString().ToLower())) + "): " + func.returnType.ToString().ToLower();
        }

        public static int GetID(UserFunction func)
        {
            return userFuncs.IndexOf(func);
        }

        public static UserFunction Register(string name, List<Value.BatchType> args, Value.BatchType returnType = Value.BatchType.Void, bool inline = false)
        {
            if (Exists(name, args))
                throw p.Exception("Function has already been declared: " + GetSignature(name, args));

            var func = new UserFunction() { name = name, argumentTypes = args, returnType = returnType, inline = inline };
            userFuncs.Add(func);
            return func;
        }

        public static bool IsRecursive(UserFunction func, UserFunction test)
        {
            if(test.calls.Contains(func))
            {
                return true;
            }

            foreach (var f in test.calls)
            {
                if(IsRecursive(func, f))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
