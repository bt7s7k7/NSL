using System;
using System.IO;
using CSCommon;
using NSL;
using NSL.Executable;
using NSL.Parsing;
using NSL.Tokenization;

namespace NSLCSharpConsole
{
    class Program
    {
        private const string FILE_PATH = "../Examples/emitTest.nsl";

        protected Timer tokenizerNewTime = new Timer();
        protected Timer tokenizationTime = new Timer();
        protected Timer parsingTime = new Timer();
        protected Timer emittingTime = new Timer();
        protected string code;

        static void Main(string[] args)
        {
            var runConfig = args.Length > 0 ? args[0] : null;

            if (runConfig != null && runConfig[0] == '+')
            {
                runConfig = runConfig.Substring(1);
                if (runConfig.Length == 0) runConfig = null;
                Console.ReadLine();
            }

            var program = new Program();

            if (runConfig == null)
            {
                program.RunSimple();
            }
            else if (runConfig == "token")
            {
                program.RunTokens();
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

        protected void Run(bool tokenizationLogger, bool parsingLogger, bool emittingLogger)
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

            if (emittingLogger) Logger.instance = new ConsoleLogger();
            var funcs = FunctionRegistry.GetStandardFunctionRegistry();
            CommonFunctions.RegisterCommonFunctions(funcs);

            emittingTime.Start();
            var emittingResult = Emitter.Emit(parsingResult, funcs);
            emittingTime.End();

            Console.WriteLine("");

            Logger.instance = new ConsoleLogger();

            foreach (var diagnostic in emittingResult.diagnostics)
            {
                diagnostic.Log();
            }

            Console.WriteLine("");
            // Console.WriteLine(parsingResult.rootNode.ToString());
            var indent = 0;
            foreach (var inst in emittingResult.instructions)
            {
                var indentDelta = inst.GetIndentDiff();
                if (indentDelta < 0) indent += indentDelta;
                Logger.instance?.Message(new String(' ', indent * 2)).Message(inst.ToString() ?? inst.GetType().Name).Pos(inst.start).End();
                if (indentDelta > 0) indent += indentDelta;
            }
            Console.WriteLine("");

            if (parsingResult.diagnostics.Count > 0)
            {
                Environment.ExitCode = 1;
            }
        }

        public void RunTokens()
        {
            Run(true, true, true);
            WriteTimes();
        }

        public void RunSimple()
        {
            Run(false, false, true);
            WriteTimes();
        }

        public void RunTime()
        {
            for (var i = 0; i < 20; i++)
            {
                Console.Write($"${i} / 20\r");
                Run(false, false, false);
            }
            WriteTimes();
        }

        public void RunTimeLogger()
        {
            for (var i = 0; i < 20; i++)
            {
                Console.Write($"${i} / 20\r");
                Run(true, true, true);
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
            Console.WriteLine($"  E()  : {emittingTime}");
        }
    }
}
