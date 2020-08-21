using System;

namespace NSL.Tokenization.General
{
    public interface ITokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        bool Execute(Tokenizer<T, S>.TokenizationState state);
    }
}