using System;
using System.Collections.Generic;

namespace NSL.Tokenization.General
{
    public class Tokenizer<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public class TokenizationState
        {
            public List<Token<T>> tokens;
            public string code;
            public Position position;
            public S state;
        }

        protected Dictionary<string, List<TokenDefinition<T, S>>> grammar;

        public Tokenizer(Dictionary<string, List<TokenDefinition<T, S>>> grammar)
        {
            this.grammar = grammar;
        }

        public List<Token<T>> Tokenize(string code, string file = "anon")
        {
            var state = new TokenizationState
            {
                code = code,
                position = new Position(0, 0, file),
                state = (S)(object)0,
                tokens = new List<Token<T>>()
            };

            return state.tokens;
        }
    }
}
