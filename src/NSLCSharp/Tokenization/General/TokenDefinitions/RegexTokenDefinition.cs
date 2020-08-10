using System.Text.RegularExpressions;
using System;

namespace NSL.Tokenization.General
{
    public class RegexTokenDefinition<T, S> : TokenDefinition<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {
        public Regex? expr;
        public string? pattern;
        public T? type;
        public S? resultState;
        public Action<Token<T>, Tokenizer<T, S>.TokenizationState>? processor;
        public Func<char, bool>? verifier;

        public RegexTokenDefinition(
            Regex? expr = null,
            string? pattern = null,
            T? type = null,
            S? resultState = null,
            Action<Token<T>,
            Tokenizer<T, S>.TokenizationState>? processor = null,
            Func<char, bool>? verifier = null
        )
        {
            this.expr = expr;
            this.type = type;
            this.resultState = resultState;
            this.processor = processor;
            this.pattern = pattern;
            this.verifier = verifier;
        }

        public override bool Execute(Tokenizer<T, S>.TokenizationState state)
        {
            var start = state.position;
            var match = false;
            string? text = null;

            if (verifier != null)
            {
                match = verifier(state.code[state.position.index]);
                if (match)
                {
                    if (expr != null)
                    {
                        match = state.Match(expr, out text);
                    }
                    else
                    {
                        text = new String(state.code[state.position.index], 1);
                    }
                }
            }
            else if (expr != null) match = state.Match(expr, out text);
            else if (pattern != null) match = state.MatchString(pattern);
            else
            {
                text = new String(state.code[state.position.index], 1);
                match = true;
                state.Next();
            }

            if (match)
            {
                if (type != null)
                {
                    var token = new Token<T>((T)(type), (text ?? pattern)!, null, start, state.position);

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