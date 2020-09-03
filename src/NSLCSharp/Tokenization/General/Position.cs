using System;
using System.Text;

namespace NSL.Tokenization.General
{
    public struct Position
    {
        public int line;
        public int col;
        public int index;
        public string file;
        public Code code;

        override public string ToString()
        {
            return $"{file}({line + 1},{col + 1})";
        }

        public Position(int line, int col, string file, int index, Code code)
        {
            this.line = line;
            this.col = col;
            this.file = file;
            this.index = index;
            this.code = code;
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

        public string GetDebugLineArrow(int indent = 0)
        {
            var builder = new StringBuilder();
            var lineNum = (line + 1) + ":";
            var currIndex = index;

            if (currIndex >= code.code.Length) currIndex = code.code.Length - 1;

            if (code.code[currIndex] == '\n')
            {
                currIndex--;
            }

            var lineStart = code.code.LastIndexOf('\n', currIndex);
            var lineEnd = code.code.IndexOf('\n', currIndex);

            if (lineStart == -1) lineStart = 0;
            if (lineEnd == -1) lineEnd = code.code.Length - 1;

            var lineText = code.code.Substring(lineStart, lineEnd - lineStart).Replace("\n", "").Replace("\r", "").Replace("\t", " ");

            builder.Append(new String(' ', indent * 2));
            builder.Append(lineNum);
            builder.Append(' ');
            builder.Append(lineText);
            builder.Append('\n');
            builder.Append(new String(' ', indent * 2 + lineNum.Length + 1));
            builder.Append(new String(' ', col));
            builder.Append('↑');

            return builder.ToString();
        }
    }
}