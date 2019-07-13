using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harpy.Evaluation;

namespace Harpy
{
    public class UserFunction
    {
        public string name;
        public string body;
        public bool inline = false;

        public Value.BatchType returnType = Value.BatchType.Void;
        public List<Value.BatchType> argumentTypes = new List<Value.BatchType>();
        public List<string> argumentNames = new List<string>();

        public List<UserFunction> calls = new List<UserFunction>(); // to prevent recursion
    }
}
