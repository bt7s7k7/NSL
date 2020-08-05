namespace NSL.Tokenization.General
{
    public struct Position
    {
        public int line;
        public int col;
        public string file;

        override public string ToString()
        {
            return $"{file}({line},{col})";
        }

        public Position(int line, int col, string file)
        {
            this.line = line;
            this.col = col;
            this.file = file;
        }
    }
}