using System;
using System.IO;
using System.Linq;
using CSCommon;
using NSL;
using NSL.Executable;
using NSL.Parsing;
using NSL.Tokenization;

namespace NSLCSharpConsole
{
    class Program
    {
        protected enum LoggerStartLocation
        {
            Tokenization,
            Parsing,
            Emitting,
            Running,
            None
        }

        private const string FILE_PATH = "../Examples/emitTest.nsl";

        protected Timer tokenizerNewTime = new Timer();
        protected Timer tokenizationTime = new Timer();
        protected Timer parsingTime = new Timer();
        protected Timer emittingTime = new Timer();
        protected Timer runningTime = new Timer();
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

        protected void Run(LoggerStartLocation loggerLocation)
        {
            ILogger.instance = null;

            // Tokenization
            if (loggerLocation == LoggerStartLocation.Tokenization) ILogger.instance = new ConsoleLogger();
            tokenizerNewTime.Start();
            var tokenizer = new NSLTokenizer();
            tokenizerNewTime.End();

            tokenizationTime.Start();
            var tokenizationResult = tokenizer.Tokenize(code, Path.GetFullPath(FILE_PATH));
            tokenizationTime.End();
            Console.WriteLine("");

            // Parsing
            if (loggerLocation == LoggerStartLocation.Parsing) ILogger.instance = new ConsoleLogger();

            parsingTime.Start();
            var parsingResult = Parser.Parse(tokenizationResult);
            parsingTime.End();

            Console.WriteLine("");
            Console.WriteLine(parsingResult.rootNode.ToString());
            Console.WriteLine("");

            // Emitting
            if (loggerLocation == LoggerStartLocation.Emitting) ILogger.instance = new ConsoleLogger();
            var funcs = FunctionRegistry.GetStandardFunctionRegistry();
            CommonFunctions.RegisterCommonFunctions(funcs);

            emittingTime.Start();
            var emittingResult = Emitter.Emit(parsingResult, funcs);
            emittingTime.End();

            Console.WriteLine("");

            emittingResult.program.Log();
            var returnVariable = emittingResult.program.GetReturnVariable();
            Console.WriteLine(returnVariable == null ? ": _" : $": {returnVariable.varName} = {returnVariable.type}");
            Console.WriteLine("");

            // Running
            if (loggerLocation == LoggerStartLocation.Running) ILogger.instance = new ConsoleLogger();
            runningTime.Start();
            runningTime.End();


            // Finish
            ILogger.instance = new ConsoleLogger();
            foreach (var diagnostic in emittingResult.diagnostics)
            {
                diagnostic.Log();
            }
            Console.WriteLine("");

            if (emittingResult.diagnostics.Count() > 0)
            {
                Environment.ExitCode = 1;
            }
        }

        public void RunTokens()
        {
            Run(LoggerStartLocation.Tokenization);
            WriteTimes();
        }

        public void RunSimple()
        {
            Run(LoggerStartLocation.Running);
            WriteTimes();
        }

        public void RunTime()
        {
            for (var i = 0; i < 20; i++)
            {
                Console.Write($"${i} / 20\r");
                Run(LoggerStartLocation.None);
            }
            WriteTimes();
        }

        public void RunTimeLogger()
        {
            for (var i = 0; i < 20; i++)
            {
                Console.Write($"${i} / 20\r");
                Run(LoggerStartLocation.Tokenization);
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
            Console.WriteLine($"  R()  : {runningTime}");
        }
    }
}
