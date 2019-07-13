using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harpy.Evaluation
{
    public class Value
    {
        public static readonly Value VOID = new Value(BatchType.Void, "");

        [Flags]
        public enum BatchType
        {
            Int = 1,
            String = 2,
            Bool = 4,
            Void = 8,
            Array = 16,
            Indeterminate = 32,
        }

        public static bool TypeEquals(BatchType a, BatchType b)
        {
            if(a == BatchType.Indeterminate && b != BatchType.Void)
            {
                //return true;
            }

            if (b == BatchType.Indeterminate && a != BatchType.Void)
            {
                //return true;
            }

            return a == b;
        }

        public static bool TypeConflict(BatchType a, BatchType b)
        {
            return !TypeEquals(a, b);
        }

        public static BatchType ParseType(string type)
        {
            if (type == "pointer") return BatchType.String;
            return (BatchType)Enum.Parse(typeof(BatchType), Evaluator.ToTitleCase(type));
        }

        public string content;
        public bool literal = true;
        public BatchType type;

        public Value(BatchType type, string value, bool literal = false)
        {
            this.content = value;
            this.type = type;
            this.literal = literal;
        }

        public override string ToString()
        {
            return content;
        }
    }
}
