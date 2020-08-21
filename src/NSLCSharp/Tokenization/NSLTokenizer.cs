using NSL.Tokenization.General;
using NSL.Types;
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
            VariableDecl
        }

        public enum StateType
        {
            Default,
            String,
            Comment
        }

        public NSLTokenizer() : base(
            new Dictionary<StateType, List<ITokenDefinition<TokenType, StateType>>> {
                { StateType.Default, new List<ITokenDefinition<TokenType, StateType>>{
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\n",type: TokenType.StatementEnd),
                    new WhitespaceTokenDefinition<TokenType,StateType>(),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "true",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.boolType.Instantiate(true)),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "false",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.boolType.Instantiate(false)),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "var",type: TokenType.VariableDecl),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|>{",type: TokenType.PipeForEachStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|{",type: TokenType.PipeStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|>",type: TokenType.PipeForEach),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|",type: TokenType.Pipe),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "!{",type: TokenType.ActionStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "}",type: TokenType.BlockEnd),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "#",resultState: StateType.Comment),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "(",type: TokenType.InlineStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: ")",type: TokenType.InlineEnd),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: ";",type: TokenType.StatementEnd),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\"",resultState: StateType.String, type: TokenType.Literal),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^[a-z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Keyword, verifier: (c) => 'a' <= c && 'z' >= c),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\$[a-z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Keyword),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\d+(\.\d+)?", RegexOptions.Compiled),type: TokenType.Literal, verifier: (c) => Char.IsDigit(c), processor: (token, state) => {
                        try {
                            var parsed = Double.Parse(token.content, NumberStyles.Float);
                            token.value = PrimitiveTypes.numberType.Instantiate(parsed);
                        } catch (FormatException err) {
                            state.diagnostics.Add(new Diagnostic($"Invalid number format: {err.Message}", token.start, token.end));
                            ILogger.instance?.Source("TOK").Error().Message($"Invalid number format: {err.Message}").Pos(token.start).End();
                        }
                    }),
                } },
                { StateType.String, new List<ITokenDefinition<TokenType, StateType>>{
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

                        for (var curr = state.code[state.position.index];; curr = state.code[state.position.index]) {
                            if (curr == '\\') {
                                if (next()) return true;
                                curr = state.code[state.position.index];
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

                        token.value = PrimitiveTypes.stringType.Instantiate(builder.ToString());
                        token.end = state.position;

                        state.state = StateType.Default;
                        state.Next();

                        ILogger.instance?.Message("      → ").Object(token.value).End();

                        return true;
                    })
                } },
                { StateType.Comment, new List<ITokenDefinition<TokenType, StateType>> {
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\n",resultState: StateType.Default),
                    new RegexTokenDefinition<TokenType, StateType>()
                } }
             }
        )
        { }

    }
}
