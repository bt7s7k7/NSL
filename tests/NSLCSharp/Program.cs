using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using Process = System.Diagnostics.Process;

namespace NSLCSharp
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine($"procId = {Process.GetCurrentProcess().Id}");
            }

            var replCommand = new Command("--interactive", "Run a REPL");
            replCommand.AddAlias("-i");

            replCommand.Handler = CommandHandler.Create(() =>
            {
                REPLRunner.Start();
            });

            var evalCommand = new Command("--eval", "Run the provided string") {
                new Argument<string>("code", "Code to run"),
                new Option<bool>(new string[] {"-p", "--print"}, () => false, "Prints the result of the command")
            };
            evalCommand.AddAlias("-e");

            evalCommand.Handler = CommandHandler.Create<string, bool>((code, printResult) =>
            {
                REPLRunner.Run(code, printResult: printResult);
            });

            var rootCommand = new RootCommand
            {
                replCommand,
                evalCommand,
                new Argument<FileInfo>("file", () => null, "File to run"),
                new Option<int>(new string[] {"--repeat", "-r"}, () => 1, "How many times to run the file"),
                new Option<bool>(new string[] {"--time", "-t"}, () => false, "Profile the timing of the execution"),
                new Option<bool>(new string[] {"--verbose", "-v"}, () => false, "Print detailed compiler output"),
            };

            rootCommand.Handler = CommandHandler.Create<FileInfo, int, bool, bool>((file, repeat, time, verbose) =>
            {
                if (file == null)
                {
                    REPLRunner.Start();
                }
                else
                {
                    var fileRunner = new FileRunner(
                        filePath: file,
                        verbose: verbose,
                        repeats: repeat,
                        doTime: time
                    );

                    fileRunner.Invoke();
                }
            });

            rootCommand.Invoke(args);
        }
    }
}
