using System;

namespace NSL.Tokenization.General
{
    public class SimpleTokenDefinition<T, S> : ITokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public Func<Tokenizer<T, S>.TokenizationState, bool> executor;

        public SimpleTokenDefinition(Func<Tokenizer<T, S>.TokenizationState, bool> executor)
        {
            this.executor = executor;
        }

        public bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            return executor(state);
        }
    }
}