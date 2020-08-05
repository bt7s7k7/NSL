using System;

namespace NSL.Tokenization.General
{
    public struct Position
    {
        public int line;
        public int col;
        public string file;

        override public string ToString()
        {
            return $"{file}({line + 1},{col + 1})";
        }

        public Position(int line, int col, string file)
        {
            this.line = line;
            this.col = col;
            this.file = file;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Position other)
            {
                return line == other.line
                && col == other.col
                && file == other.file;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(line, col, file);
        }
    }
}