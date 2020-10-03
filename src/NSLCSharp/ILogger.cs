using System;
using NSL.Tokenization.General;

namespace NSL
{
    public interface ILogger
    {
        ILogger Error();
        ILogger Source(string text);
        ILogger Message(string text);
        ILogger Name(string text);
        ILogger Pos(Position pos);
        ILogger End();
        ILogger Object(object text);
    }

    public static class LoggerProvider
    {
        public static ILogger instance = null;
    }
}