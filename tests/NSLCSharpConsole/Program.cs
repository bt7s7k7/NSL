﻿using System;
using System.IO;
using System.Text.Json;
using NSL;
using NSL.Tokenization;
using NSL.Tokenization.General;

namespace NSLCSharpConsole
{
    class Program
    {
        protected const string FILE_PATH = "../Examples/test1.nsl";

        static void Main(string[] args)
        {
            Logger.instance = new ConsoleLogger();

            string? script = null;

            using (var reader = new StreamReader(FILE_PATH))
            {
                script = reader.ReadToEnd();
            }

            var tokenizer = TokenizerFactory.Build();
            var tokenizationResult = tokenizer.Tokenize(script, Path.GetFullPath(FILE_PATH));

            Console.WriteLine("");
            Console.WriteLine("");

            foreach (var diagnostic in tokenizationResult.diagnostics)
            {
                Logger.instance.Error().Message(diagnostic.ToString()).End();
            }

        }
    }

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
            Console.Write("at " + pos.ToString() + " ");
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
