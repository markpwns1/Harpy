using System;

namespace Harpy.Evaluation
{
    public class Operation
    {
        public enum Association
        {
            Left,
            Right,
            None
        }

        public string operatorToken;

        public Func<Value, Value, Value> binaryFunction;
        // (left, right) => { }

        public bool eatOperator = true;
        public bool canBeGlobal = false;

        public Association association; // only applies to unary functions
        public Func<Value, Value> unaryFunction;
        // If left associative
        // (left) => { }

        // If right associative
        // (right) => { }


        public Operation(string tok)
        {
            operatorToken = tok;
        }

        public bool IsBinary
        {
            get { return unaryFunction == null; }
        }
    }
}
