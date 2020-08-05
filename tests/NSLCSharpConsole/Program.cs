using System;
using System.IO;
using NSL.Tokenization;

namespace NSLCSharpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string script = null;

            using (var reader = new StreamReader("../Examples/test1.nsl"))
            {
                script = reader.ReadToEnd();
            }

            var tokenizer = TokenizerFactory.Build();
            var tokens = tokenizer.Tokenize(script);

            foreach (var token in tokens)
            {
                Console.WriteLine($"{token.type} at ${token.start}");
                Console.WriteLine($"  {token.content}");
                if (token.value != null) Console.WriteLine($"  {token.value}");
            }
        }
    }
}
