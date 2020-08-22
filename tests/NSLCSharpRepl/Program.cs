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
            var tokenizer = new NSLTokenizer();
            var functions = FunctionRegistry.GetStandardFunctionRegistry();
            CommonFunctions.RegisterCommonFunctions(functions);
            var runner = new Runner(functions);

            while (true)
            {
                Console.Write("> ");
                var text = Console.ReadLine();
                var result = Emitter.Emit(Parser.Parse(tokenizer.Tokenize(text)), functions, runnerRootScope: runner.GetRootScope());

                ILogger.instance = new ConsoleLogger();
                foreach (var diagnostic in result.diagnostics)
                {
                    diagnostic.Log();
                }

                if (result.diagnostics.Count() == 0) ILogger.instance?.Name(runner.Run(result.program).ToString()).End();
                ILogger.instance = null;
            }
        }
    }
}
