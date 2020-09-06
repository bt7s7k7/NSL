using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NSL;
using NSL.Executable;
using NSL.Parsing;
using NSL.Runtime;
using NSL.Tokenization;
using NSL.Types;
using NUnit.Framework;

namespace NSLCSharpNUnit
{
    [TestFixture]
    public static class FileTests
    {
        public class FileSettings
        {
            public string type = "T";
            public string[] output = new string[0];
        }

        public static string[] files = Directory.GetFiles("../../../Units/", "*.nsl").Select(v => Path.GetFullPath(v)).ToArray();
        public static FunctionRegistry functions = FunctionRegistry.GetStandardFunctionRegistry();
        public static NSLTokenizer tokenizer = new NSLTokenizer(functions);
        public static List<string> output = new List<string>();

        [TestCaseSource(nameof(files))]
        public static void TestFile(string file)
        {
            output.Clear();
            string? script = null;

            using (var reader = new StreamReader(file))
            {
                script = reader.ReadToEnd();
            }

            var typeString = script[1];
            var expectedOutput = new string[0];
            if (script[1] == '{')
            {
                //var settings = JsonSerializer.Deserialize<FileSettings>();
                var settings = JsonConvert.DeserializeObject<FileSettings>(script.Substring(1, script.IndexOf("\n")));
                typeString = settings.type[0];
                expectedOutput = settings.output;
            }


            var runner = new Runner(functions);
            try
            {
                var (result, diagnostics) = runner.RunScript(script, Path.GetFullPath(file));

                var success = diagnostics.Count() == 0;

                if (!success && typeString == 'T') Assert.Fail(String.Join('\n', diagnostics));

                if (success)
                {
                    success = Enumerable.SequenceEqual(expectedOutput, output);
                    if (!success && typeString == 'T') Assert.Fail($"Expected:\n  {JsonConvert.SerializeObject(expectedOutput)}\nGot:\n  {JsonConvert.SerializeObject(output)}");
                }

                if ((typeString == 'T') == success)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail("An error was expected");
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

        static FileTests()
        {
            functions.Add(NSLFunction.MakeAuto<Action<string>>("output", (val) => output.Add(val)));
        }
    }
}