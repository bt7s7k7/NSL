using System;

namespace NSL.Tokenization.General
{
    public class WhitespaceTokenDefinition<T, S> : ITokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            return state.EatSpace();
        }
    }
}