using System;

namespace NSL.Tokenization.General
{
    public class Token<T>
        where T : struct, IComparable
    {
        public T type;
        public string content;
        public object? value;
        public Position start;
        public Position end;

        public Token(T type, string content, object? value, Position start, Position end)
        {
            this.type = type;
            this.content = content;
            this.value = value;
            this.start = start;
            this.end = end;
        }
    }
}