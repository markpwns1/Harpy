namespace Harpy
{
    public struct CodeLocation
    {
        public readonly int line;
        public readonly int column;

        public CodeLocation(int line, int col)
        {
            this.line = line;
            this.column = col;
        }

        public override string ToString()
        {
            return "ln " + line + ", col " + column;
        }

        public static bool operator ==(CodeLocation c1, CodeLocation c2)
        {
            return c1.line == c2.line && c1.column == c2.column;
        }

        public static bool operator !=(CodeLocation c1, CodeLocation c2)
        {
            return !(c1 == c2);
        }
    }
}
