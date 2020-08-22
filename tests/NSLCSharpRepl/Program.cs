using System;
using CSCommon;
using NSL;
using NSL.Runtime;
using NSL.Tokenization;
using NSL.Parsing;
using NSL.Executable;
using System.Linq;

namespace NSLCSharpRepl
{
    class Program
    {
        static void Main(string[] args)
        {
            NSLTokenizer tokenizer = new NSLTokenizer();
            FunctionRegistry functions = FunctionRegistry.GetStandardFunctionRegistry();
            CommonFunctions.RegisterCommonFunctions(functions);
            Runner runner = new Runner(functions);

            void run(string text)
            {
                var result = Emitter.Emit(Parser.Parse(tokenizer.Tokenize(text)), functions, runnerRootScope: runner.GetRootScope());

                ILogger.instance = new ConsoleLogger();
                foreach (var diagnostic in result.diagnostics)
                {
                    diagnostic.Log();
                }

                if (result.diagnostics.Count() == 0) ILogger.instance?.Name(runner.Run(result.program).ToString()).End();
                ILogger.instance = null;
            }

            foreach (var arg in args)
            {
                run(arg);
            }

            while (true)
            {
                Console.Write("> ");
                var text = Console.ReadLine();

                run(text);
            }
        }
    }
}
