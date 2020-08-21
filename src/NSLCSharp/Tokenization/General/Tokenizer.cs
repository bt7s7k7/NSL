using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NSL.Tokenization.General
{
    public class Tokenizer<T, S>
        where T : struct, IComparable
        where S : struct, IComparable
    {

        public class TokenizationResult
        {
            public List<Token<T>> tokens;
            public List<Diagnostic> diagnostics;

            public TokenizationResult()
            {
                tokens = new List<Token<T>>();
                diagnostics = new List<Diagnostic>();
            }
        }

        public class TokenizationState : TokenizationResult
        {
            public string code;
            public Position position;
            public S state;
            public bool isEnd;

            public TokenizationState(string code, string file)
            {
                this.code = code;
                this.state = (S)(object)0;
                this.isEnd = false;
                this.position = new Position(0, 0, file, 0, new Code(code));
            }

            public bool EatSpace()
            {
                if (Char.IsWhiteSpace(code[position.index]))
                {
                    Next();
                    return true;
                }
                else return false;
            }

            public bool Next()
            {
                var curr = code[position.index];
                if (curr == '\n')
                {
                    position.line++;
                    position.col = 0;
                    position.index++;
                    if (position.index == code.Length - 1)
                    {
                        isEnd = true;
                    }
                    return true;
                }
                position.col++;
                position.index++;
                if (position.index == code.Length - 1)
                {
                    isEnd = true;
                }
                return false;
            }

            public bool Match(Regex pattern, [NotNullWhen(true)] out string? text)
            {
                var match = pattern.Match(code.Substring(position.index));
                if (match.Success)
                {
                    text = match.Value;
                    for (int i = 0, len = text.Length; i < len; i++) Next();
                    return true;
                }
                else
                {
                    text = null;
                    return false;
                }
            }

            public bool MatchString(string pattern)
            {
                for (int i = 0, len = pattern.Length; i < len; i++)
                {
                    if (code[position.index + i] != pattern[i])
                    {
                        return false;
                    }
                }

                for (int i = 0, len = pattern.Length; i < len; i++) Next();
                return true;
            }

            public void PushToken(Token<T> token)
            {
                tokens.Add(token);
                ILogger.instance?
                    .Source("TOK")
                    .Message("Found token")
                    .Name(token.type.ToString()!)
                    .Object(token.content)
                    .Message("=")
                    .Object(token.value)
                    .Pos(token.start)
                    .End();
            }
        }

        protected Dictionary<S, List<ITokenDefinition<T, S>>> grammar;

        public Tokenizer(Dictionary<S, List<ITokenDefinition<T, S>>> grammar)
        {
            this.grammar = grammar;
        }

        public TokenizationResult Tokenize(string code, string file = "anon")
        {
            ILogger.instance?.Source("TOK").Message($"Starting tokenization in '{file}'").End();

            code = code + "\n";

            var state = new TokenizationState(code, file);

            while (!state.isEnd)
            {
                if (grammar.TryGetValue(state.state, out List<ITokenDefinition<T, S>>? defsInState))
                {
                    if (defsInState == null) throw new TokenDefinitionExcpetion("Token list in state {state.state} is null");
                    ITokenDefinition<T, S>? found = null;
                    Position lastPosition = state.position;

                    foreach (var def in defsInState)
                    {
                        if (def.Execute(state))
                        {
                            found = def;
                            break;
                        }
                    }

                    if (found == null)
                    {
                        state.diagnostics.Add(new Diagnostic($"Failed to trigger any token definition in {state.state}", lastPosition, state.position));
                        ILogger.instance?.Source("TOK").Error().Message("Failed to trigger any token definition in").Name(state.state.ToString()!).Pos(state.position).End();
                        state.Next();
                    }
                    else if (!state.isEnd && state.position.Equals(lastPosition)) throw new TokenDefinitionExcpetion($"Token definition {found} failed to increment position at {state.position}");
                }
                else
                {
                    throw new TokenDefinitionExcpetion($"There are no token definitions for state {state.state}");
                }

            }

            return state;
        }

    }
}
