using System;
using CSCommon;
using NSL;
using NSL.Runtime;
using NSL.Parsing;
using NSL.Executable;
using System.Linq;

namespace NSLCSharp
{
    static class REPLRunner
    {
        private static void Work(string? evalString, bool printOutputs)
        {
            FunctionRegistry functions = FunctionRegistry.GetStandardFunctionRegistry();
            CommonFunctions.RegisterCommonFunctions(functions);
            Runner runner = new Runner(functions);
            var tokenizer = runner.Tokenizer;

            void run(string text)
            {
                var result = Emitter.Emit(Parser.Parse(tokenizer.Tokenize(text), functions), functions, runnerRootScope: runner.RootScope);

                ILogger.instance = new ConsoleLogger();
                foreach (var diagnostic in result.diagnostics)
                {
                    diagnostic.Log();
                }


                if (result.diagnostics.Count() == 0)
                {
                    try
                    {
                        var runResult = runner.Run(result.program);
                        if (printOutputs) ILogger.instance?.Name(runResult.ToString()).End();
                    }
                    catch (UserNSLException err)
                    {
                        err.Log();
                        if (!printOutputs) Environment.Exit(1);
                    }
                }

                ILogger.instance = null;
            }

            if (evalString != null)
            {
                run(evalString);
                return;
            }

            while (true)
            {
                Console.Write("> ");
                var text = ReadLine.Read();

                run(text);
            }
        }

        public static void Start()
        {
            Console.WriteLine("NSL :: (C) Branislav Trstenský 2020");
            Console.WriteLine("Type 'exit' to exit");
            Work(null, true);
        }

        public static void Run(string command, bool printResult)
        {
            Work(command, printResult);
        }
    }
}
