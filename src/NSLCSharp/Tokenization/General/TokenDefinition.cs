using System;

namespace NSL.Tokenization.General
{
    abstract public class TokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        abstract public bool Execute(Tokenizer<T, S>.TokenizationState state);
    }
}