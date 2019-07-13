using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harpy.Evaluation;

namespace Harpy
{
    public class Function
    {
        public string name;
        public Value.BatchType returnType = Value.BatchType.Void;

        public List<Value.BatchType> argumentTypes = new List<Value.BatchType>();

    }
}
