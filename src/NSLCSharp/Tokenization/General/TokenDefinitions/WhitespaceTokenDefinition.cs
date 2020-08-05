using System;

namespace NSL.Tokenization.General
{
    public class WhitespaceTokenDefinition<T, S> : TokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public override bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            return state.EatSpace();
        }
    }
}