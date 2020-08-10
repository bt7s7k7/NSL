using System.Text.RegularExpressions;
using System;

namespace NSL.Tokenization.General
{
    public class RegexTokenDefinition<T, S> : TokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public Regex expr;
        public T? type;
        public S? resultState;
        public Action<Token<T>, Tokenizer<T, S>.TokenizationState>? processor;

        public RegexTokenDefinition(Regex expr, T? type = null, S? resultState = null, Action<Token<T>, Tokenizer<T, S>.TokenizationState>? processor = null)
        {
            this.expr = expr;
            this.type = type;
            this.resultState = resultState;
            this.processor = processor;
        }

        public override bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            var start = state.position;
            if (state.Match(expr, out string? text))
            {
                if (type != null)
                {
                    var token = new Token<T>((T)(type), text, null, start, state.position);

                    if (processor != null)
                    {
                        processor(token, state);
                    }

                    state.PushToken(token);

                }

                if (resultState != null)
                {
                    state.state = (S)resultState!;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}