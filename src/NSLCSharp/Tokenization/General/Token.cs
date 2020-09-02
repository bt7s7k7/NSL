using System;
using NSL.Types;

namespace NSL.Tokenization.General
{
    public class Token<T>
        where T : struct, IComparable
    {
        public T type;
        public string content;
        public IValue? value;
        public Position start;
        public Position end;

        public Token(T type, string content, IValue? value, Position start, Position end)
        {
            this.type = type;
            this.content = content;
            this.value = value;
            this.start = start;
            this.end = end;
        }
    }
}