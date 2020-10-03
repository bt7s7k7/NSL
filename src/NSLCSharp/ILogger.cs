using System;
using NSL.Tokenization.General;

namespace NSL
{
    public interface ILogger
    {
        public static ILogger instance = null;

        ILogger Error();
        ILogger Source(string text);
        ILogger Message(string text);
        ILogger Name(string text);
        ILogger Pos(Position pos);
        ILogger End();
        ILogger Object(object text);
    }
}