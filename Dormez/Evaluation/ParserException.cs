using System;

namespace Harpy.Evaluation
{
    public class ParserException : Exception
    {
        public CodeLocation location;
        public string message;

        public ParserException(Token token, string message)
            : base(/*location.ToString() + ": " + */ message)
        {
            this.location = token.Location;
            this.message = message;
        }

        public ParserException(CodeLocation location, string message) 
            : base(/*location.ToString() + ": " + */message)
        {
            this.location = location;
            this.message = message;
        }

        public ParserException(string message) : base(message) { }

        public override string ToString()
        {
            //return location.ToString() + ": " + message;
            return message;
        }
    }
}
