namespace Harpy
{
    public class Token
    {
        public CodeLocation Location { get; }
        public string Type { get; }
        public object Value { get; }

        public Token(string t, int ln = -1, int col = -1, object cont = null)
        {
            this.Type = t.Trim().ToLower();
            this.Location = new CodeLocation(ln, col);
            this.Value = cont;
        }

        public Token(string t, CodeLocation loc, object cont = null)
        {
            this.Type = t.Trim().ToLower();
            this.Location = loc;
            this.Value = cont;
        }

        public Token(string t, object cont)
        {
            this.Type = t.Trim().ToLower();
            this.Location = new CodeLocation(-1, -1);
            this.Value = cont;
        }

        public override string ToString()
        {
            return "Token (" + Type + ", " + (Value == null ? "null" : Value.ToString()) + ") @ " + Location.ToString();
        }

        public static bool operator ==(Token t1, string t2)
        {
            return t1.Type.ToLower().Trim() == t2.ToLower().Trim();
        }

        public static bool operator !=(Token t1, string t2)
        {
            return !(t1 == t2);
        }

        public static bool operator ==(Token t1, CodeLocation t2)
        {
            return t1.Location == t2;
        }

        public static bool operator !=(Token t1, CodeLocation t2)
        {
            return !(t1 == t2);
        }
    }
}
