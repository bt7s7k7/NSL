﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using NSL;
using NSL.Parsing;
using NSL.Tokenization;
using NSL.Tokenization.General;

namespace NSLCSharpConsole
{
    class Program
    {
        private const string FILE_PATH = "../Examples/test1.nsl";

        protected Timer tokenizerNewTime = new Timer();
        protected Timer tokenizationTime = new Timer();
        protected Timer parsingTime = new Timer();
        protected string code;

        static void Main(string[] args)
        {
            var runConfig = args.Length > 0 ? args[0] : null;
            var program = new Program();

            if (runConfig == null)
            {
                program.RunSimple();
            }
            else if (runConfig == "time")
            {
                program.RunTime();
            }
            else if (runConfig == "timeLog")
            {
                program.RunTimeLogger();
            }
            else
            {
                Console.WriteLine("Invalid run configuration");
            }
        }

        protected void Run(bool tokenizationLogger, bool parsingLogger)
        {
            Logger.instance = null;
            if (tokenizationLogger) Logger.instance = new ConsoleLogger();

            tokenizerNewTime.Start();
            var tokenizer = new NSLTokenizer();
            tokenizerNewTime.End();

            tokenizationTime.Start();
            var tokenizationResult = tokenizer.Tokenize(code, Path.GetFullPath(FILE_PATH));
            tokenizationTime.End();

            Console.WriteLine("");

            if (parsingLogger) Logger.instance = new ConsoleLogger();

            parsingTime.Start();
            var parsingResult = Parser.Parse(tokenizationResult);
            parsingTime.End();


            Console.WriteLine("");

            Logger.instance = new ConsoleLogger();

            foreach (var diagnostic in parsingResult.diagnostics)
            {
                diagnostic.Log();
            }

            Console.WriteLine("");
            Console.WriteLine(parsingResult.rootNode.ToString());
            Console.WriteLine("");

            if (parsingResult.diagnostics.Count > 0)
            {
                Environment.ExitCode = 1;
            }
        }

        public void RunSimple()
        {
            Run(false, true);
            WriteTimes();
        }

        public void RunTime()
        {
            for (var i = 0; i < 20; i++)
            {
                Console.Write($"${i} / 20\r");
                Run(false, false);
            }
            WriteTimes();
        }

        public void RunTimeLogger()
        {
            for (var i = 0; i < 20; i++)
            {
                Console.Write($"${i} / 20\r");
                Run(true, true);
            }
            WriteTimes();
        }

        public Program()
        {
            string? script = null;

            using (var reader = new StreamReader(FILE_PATH))
            {
                script = reader.ReadToEnd();
            }

            this.code = script;
        }

        protected void WriteTimes()
        {
            Console.WriteLine("");
            Console.WriteLine("Done!");
            Console.WriteLine($"  newT : {tokenizerNewTime}");
            Console.WriteLine($"  T()  : {tokenizationTime}");
            Console.WriteLine($"  P()  : {parsingTime}");
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
