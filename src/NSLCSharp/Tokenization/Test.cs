using NSL.Tokenization.General;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NSL.Tokenization
{
    public class NSLTokenizer : Tokenizer<NSLTokenizer.TokenType, NSLTokenizer.StateType>
    {
        public enum TokenType
        {
            Keyword,
            Literal,
            Pipe,
            PipeForEach,
            ActionStart,
            PipeStart,
            PipeForEachStart,
            BlockEnd,
            InlineStart,
            InlineEnd,
            StatementEnd,
            Variable,
            Declaration
        }

        public enum StateType
        {
            Default,
            String
        }

        public NSLTokenizer() : base(
            new Dictionary<StateType, List<TokenDefinition<TokenType, StateType>>> {
                { StateType.Default, new List<TokenDefinition<TokenType, StateType>>{
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\n", RegexOptions.Compiled),type: TokenType.StatementEnd),
                    new WhitespaceTokenDefinition<TokenType,StateType>(),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^true", RegexOptions.Compiled),type: TokenType.Literal, processor: (token, state) => token.value = true),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^false", RegexOptions.Compiled),type: TokenType.Literal, processor: (token, state) => token.value = false),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^var", RegexOptions.Compiled),type: TokenType.Declaration),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^[a-z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Keyword),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\$[a-z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Variable),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\|>{", RegexOptions.Compiled),type: TokenType.PipeForEachStart),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\|{", RegexOptions.Compiled),type: TokenType.PipeStart),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\|>", RegexOptions.Compiled),type: TokenType.PipeForEach),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\|", RegexOptions.Compiled),type: TokenType.Pipe),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^!{", RegexOptions.Compiled),type: TokenType.ActionStart),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^}", RegexOptions.Compiled),type: TokenType.BlockEnd),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\(", RegexOptions.Compiled),type: TokenType.InlineStart),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\)", RegexOptions.Compiled),type: TokenType.InlineEnd),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^;", RegexOptions.Compiled),type: TokenType.StatementEnd),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\d+(\.\d+)?", RegexOptions.Compiled),type: TokenType.Literal, processor: (token, state) => {
                        try {
                            var parsed = Double.Parse(token.content, NumberStyles.Float);
                            token.value = parsed;
                        } catch (FormatException err) {
                            state.diagnostics.Add(new Diagnostic($"Invalid number format: {err.Message}", token.start, token.end));
                            Logger.instance?.Source("TOK").Error().Message($"Invalid number format: {err.Message}").Pos(token.start).End();
                        }
                    }),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex("^\"", RegexOptions.Compiled),resultState: StateType.String, type: TokenType.Literal)
                } },
                { StateType.String, new List<TokenDefinition<TokenType, StateType>>{
                    new SimpleTokenDefinition<TokenType, StateType>((state) => {
                        var token = state.tokens[state.tokens.Count - 1];
                        var builder = new StringBuilder();

                        bool next() {
                            if (state.Next()) {
                                state.diagnostics.Add(new Diagnostic("Unexpected EOF in the middle of a string", state.position, state.position));
                                return true;
                            }
                            return false;
                        }

                        for (var curr = state.code[state.index];; curr = state.code[state.index]) {
                            if (curr == '\\') {
                                if (next()) return true;
                                curr = state.code[state.index];
                                if (curr == 'n') builder.Append('\n');
                                else if (curr == 't') builder.Append('\t');
                                else if (curr == '"') builder.Append('"');
                                else if (curr == '\\') builder.Append('\\');
                                else {
                                    state.diagnostics.Add(new Diagnostic("Unknown escape sequence", state.position, state.position));
                                }
                            } else if (curr == '"') {
                                break;
                            } else {
                                builder.Append(curr);
                            }

                            if (next()) return true;
                        }

                        token.value = builder.ToString();
                        token.end = state.position;

                        state.state = StateType.Default;
                        state.Next();

                        Logger.instance?.Message("      → ").Object(token.value).End();

                        return true;
                    })
                } }
             }
        )
        { }

    }
}