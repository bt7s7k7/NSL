using System;
using System.Text.Json;
using NSL;
using NSL.Tokenization.General;

namespace CSCommon
{
    class ConsoleLogger : ILogger
    {
        public ILogger End()
        {
            Console.Write("\n");
            SetColor();
            return this;
        }

        public ILogger Message(string source)
        {
            SetColor();
            Console.Write(source + " ");
            SetColor();
            return this;
        }

        public ILogger Name(string source)
        {
            SetColor(ConsoleColor.Green);
            Console.Write(source + " ");
            SetColor();
            return this;
        }

        public ILogger Pos(Position pos)
        {
            SetColor(ConsoleColor.DarkGray);
            Console.Write("at " + pos.ToString() + " " + "\n");
            Console.Write(pos.GetDebugLineArrow(3));
            SetColor();
            return this;
        }

        public ILogger Source(string source)
        {
            SetColor();
            Console.Write("[");
            SetColor(ConsoleColor.Blue);
            Console.Write(source);
            SetColor();
            Console.Write("] ");
            SetColor();

            return this;
        }

        public ILogger Error()
        {
            SetColor();
            Console.Write("[");
            SetColor(ConsoleColor.Red);
            Console.Write("ERR!");
            SetColor();
            Console.Write("] ");
            SetColor();

            return this;
        }

        public ILogger Object(object text)
        {
            SetColor(ConsoleColor.DarkYellow);
            Console.Write(JsonSerializer.Serialize(text) + " ");
            SetColor();

            return this;
        }

        protected void SetColor(ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
        }
    }
}