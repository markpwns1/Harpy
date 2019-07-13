using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harpy.Evaluation;

namespace Harpy
{
    public class Variable
    {
        public readonly string name;
        public readonly Value.BatchType type;
        public readonly int depth = 0;
        public readonly bool readOnly = false;

        public Variable(string name, Value.BatchType type)
        {
            this.name = name;
            this.type = type;
        }

        public Variable(string name, Value.BatchType type, int depth)
        {
            this.name = name;
            this.type = type;
            this.depth = depth;
        }

        public Variable(string name, Value.BatchType type, bool readOnly)
        {
            this.name = name;
            this.type = type;
            this.readOnly = readOnly;
        }

        public Variable(string name, Value.BatchType type, int depth, bool readOnly)
        {
            this.name = name;
            this.type = type;
            this.depth = depth;
            this.readOnly = readOnly;
        }
    }
}
