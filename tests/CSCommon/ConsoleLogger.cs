using System;
using System.Text.Json;
using NSL;
using NSL.Tokenization.General;

namespace CSCommon
{
    class ConsoleLogger : Logger
    {
        public override Logger End()
        {
            Console.Write("\n");
            SetColor();
            return this;
        }

        public override Logger Message(string source)
        {
            SetColor();
            Console.Write(source + " ");
            SetColor();
            return this;
        }

        public override Logger Name(string source)
        {
            SetColor(ConsoleColor.Green);
            Console.Write(source + " ");
            SetColor();
            return this;
        }

        public override Logger Pos(Position pos)
        {
            SetColor(ConsoleColor.DarkGray);
            Console.Write("at " + pos.ToString() + " " + "\n");
            Console.Write(pos.GetDebugLineArrow(3));
            SetColor();
            return this;
        }

        public override Logger Source(string source)
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

        public override Logger Error()
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

        public override Logger Object(object? text)
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