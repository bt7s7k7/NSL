using System;
using System.IO;
using System.Linq;
using NSL;
using NSL.Executable;
using NSL.Parsing;
using NSL.Runtime;
using NSL.Tokenization;
using NUnit.Framework;

namespace NSLCSharpNUnit
{
    [TestFixture]
    public static class FileTests
    {
        public static string[] files = Directory.GetFiles("../../../Units/", "*.nsl");
        public static NSLTokenizer tokenizer = new NSLTokenizer();
        public static FunctionRegistry functions = FunctionRegistry.GetStandardFunctionRegistry();


        [TestCaseSource(nameof(files))]
        public static void TestFile(string file)
        {
            string? script = null;

            using (var reader = new StreamReader(file))
            {
                script = reader.ReadToEnd();
            }

            var typeString = script[1];

            var runner = new Runner(functions);
            try
            {
                var (result, diagnostics) = runner.RunScript(script, Path.GetFullPath(file));

                if (diagnostics.Count() == 0)
                {
                    if (typeString == 'E')
                    {
                        Assert.Fail("An error was expected");
                    }
                    else
                    {
                        Assert.Pass();
                    }
                }
                else
                {
                    Assert.Fail(String.Join('\n', diagnostics));
                }
            }
            catch (UserNSLException err)
            {
                if (typeString == 'E')
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail(err.GetNSLStacktrace());
                }
            }
        }
    }
}