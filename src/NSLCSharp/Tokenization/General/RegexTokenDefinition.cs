using System.Text.RegularExpressions;
using System;

namespace NSL.Tokenization.General
{
    public class RegexTokenDefinition<T, S> : TokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public Regex expr;
        public Nullable<T> type;
        public string resultState;
        public Action<Token<T>> processor;

        public override bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            return false;
        }
    }
}