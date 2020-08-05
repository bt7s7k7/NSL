using System;

namespace NSL.Tokenization.General
{
    public class SimpleTokenDefinition<T, S> : TokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public Func<Tokenizer<T, S>.TokenizationState, bool> executor;

        public override bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            return executor(state);
        }
    }
}