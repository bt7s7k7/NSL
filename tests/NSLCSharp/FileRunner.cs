using System;
using System.IO;
using System.Linq;
using CSCommon;
using NSL;
using NSL.Executable;
using NSL.Parsing;
using NSL.Runtime;
using NSL.Tokenization;

namespace NSLCSharp
{
    class FileRunner
    {
        protected enum LoggerStartLocation
        {
            Tokenization,
            Parsing,
            Emitting,
            Running,
            None
        }

        protected Timer tokenizerNewTime = new Timer();
        protected Timer tokenizationTime = new Timer();
        protected Timer parsingTime = new Timer();
        protected Timer emittingTime = new Timer();
        protected Timer runningTime = new Timer();
        protected string code;
        protected LoggerStartLocation loggerLocation;
        protected bool doTime;
        protected int repeats;
        protected FileInfo filePath;

        protected void Run()
        {
            ILogger.instance = null;

            var funcs = FunctionRegistry.GetStandardFunctionRegistry();
            CommonFunctions.RegisterCommonFunctions(funcs);

            // Tokenization
            if (loggerLocation == LoggerStartLocation.Tokenization) ILogger.instance = new ConsoleLogger();
            tokenizerNewTime.Start();
            var tokenizer = new NSLTokenizer(funcs);
            tokenizerNewTime.End();

            tokenizationTime.Start();
            var tokenizationResult = tokenizer.Tokenize(code, filePath.FullName);
            tokenizationTime.End();
            ILogger.instance?.End();

            foreach (var token in tokenizationResult.tokens)
            {
                ILogger.instance?.Name(token.type.ToString()).Object(token.content).Pos(token.start).End();
            }
            ILogger.instance?.End();

            // Parsing
            if (loggerLocation == LoggerStartLocation.Parsing) ILogger.instance = new ConsoleLogger();

            parsingTime.Start();
            var parsingResult = Parser.Parse(tokenizationResult, funcs);
            parsingTime.End();

            ILogger.instance?.End()
                .Message(parsingResult.rootNode.ToString()).End()
                .End();

            // Emitting
            if (loggerLocation == LoggerStartLocation.Emitting) ILogger.instance = new ConsoleLogger();

            emittingTime.Start();
            var emittingResult = Emitter.Emit(parsingResult, funcs);
            emittingTime.End();

            ILogger.instance?.End();

            emittingResult.program.Log();
            var returnVariable = emittingResult.program.ReturnVariable;
            ILogger.instance?.Message(returnVariable == null ? ": _" : $": {returnVariable.type}").End().End();

            // Running
            if (emittingResult.diagnostics.Count() == 0)
            {
                try
                {
                    if (loggerLocation == LoggerStartLocation.Running) ILogger.instance = new ConsoleLogger();
                    runningTime.Start();
                    var runner = new Runner(funcs);
                    var runResult = runner.Run(emittingResult.program);
                    ILogger.instance?.Message(runResult.ToString()).End();
                    runningTime.End();
                }
                catch (UserNSLException err)
                {
                    ILogger.instance = new ConsoleLogger();
                    err.Log();
                    Environment.ExitCode = 1;
                }
            }

            // Finish
            ILogger.instance = new ConsoleLogger();
            foreach (var diagnostic in emittingResult.diagnostics)
            {
                diagnostic.Log();
            }
            if (emittingResult.diagnostics.Count() > 0) ILogger.instance?.End();

            if (emittingResult.diagnostics.Count() > 0)
            {
                Environment.ExitCode = 1;
            }
        }

        public void Invoke()
        {
            for (var i = 0; i < repeats; i++)
            {
                if (repeats > 1) Console.Write($"{i + 1} / {repeats}\r");
                Run();
                if (doTime && repeats != 1 && i == 0)
                {
                    tokenizerNewTime.Reset();
                    tokenizationTime.Reset();
                    parsingTime.Reset();
                    emittingTime.Reset();
                    runningTime.Reset();
                }
            }
            if (doTime) WriteTimes();
        }

        public FileRunner(FileInfo filePath, bool verbose, int repeats, bool doTime)
        {
            string script = null;

            using (var reader = new StreamReader(filePath.FullName))
            {
                script = reader.ReadToEnd();
            }

            this.code = script;
            this.repeats = repeats;
            this.doTime = doTime;
            this.loggerLocation = verbose ? LoggerStartLocation.Tokenization : LoggerStartLocation.None;
            this.filePath = filePath;
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
            Console.WriteLine($"  SUM  : {Timer.GetTotal(new Timer[] { tokenizerNewTime, tokenizationTime, parsingTime, emittingTime, runningTime })}");
        }
    }
}
