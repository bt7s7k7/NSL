using System;
using NSL.Tokenization.General;

namespace NSL
{
    public abstract class Logger
    {
        public static Logger? instance = null;

        public abstract Logger Error();
        public abstract Logger Source(string text);
        public abstract Logger Message(string text);
        public abstract Logger Name(string text);
        public abstract Logger Pos(Position pos);
        public abstract Logger End();
        public abstract Logger Object(object? text);
    }
}