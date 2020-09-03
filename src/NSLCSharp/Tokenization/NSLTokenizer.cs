using NSL.Tokenization.General;
using NSL.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            VariableDecl,
            Operator
        }

        public enum StateType
        {
            Default,
            String,
            Comment
        }

        private class StringState
        {
            public char stringCloseChar;
            public bool stringTemplate;
            public bool isFirst = true;

            public StringState(char stringCloseChar, bool stringTemplate)
            {
                this.stringCloseChar = stringCloseChar;
                this.stringTemplate = stringTemplate;
            }
        }

        private static Stack<StringState> stringStack = new Stack<StringState>();
        private static void PushString(char stringCloseChar, bool stringTemplate)
        {
            stringStack.Push(new StringState(stringCloseChar, stringTemplate));
        }

        protected FunctionRegistry functions;

        public NSLTokenizer(FunctionRegistry functions) : base(
            new Dictionary<StateType, List<ITokenDefinition<TokenType, StateType>>> {
                { StateType.Default, new List<ITokenDefinition<TokenType, StateType>>{
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\\\n"),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\n",type: TokenType.StatementEnd),
                    new WhitespaceTokenDefinition<TokenType,StateType>(),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "true",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.boolType.Instantiate(true)),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "false",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.boolType.Instantiate(false)),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "var",type: TokenType.VariableDecl),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "]", resultState: StateType.String, type: TokenType.Literal),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|>{",type: TokenType.PipeForEachStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|{",type: TokenType.PipeStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|>",type: TokenType.PipeForEach),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|",type: TokenType.Pipe),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: ".",type: TokenType.Pipe),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "!{",type: TokenType.ActionStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "}",type: TokenType.BlockEnd),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "#",resultState: StateType.Comment),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "(",type: TokenType.InlineStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: ")",type: TokenType.InlineEnd),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: ";",type: TokenType.StatementEnd),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\"",resultState: StateType.String, type: TokenType.Literal, processor: (token, state) => PushString('"', false)),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "`",resultState: StateType.String, type: TokenType.Literal, processor: (token, state) => PushString('`', false)),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "$\"",resultState: StateType.String, custom: (state) => {
                        PushString('"', true);
                        state.tokens.Add(new Token<TokenType>(TokenType.InlineStart, "(", null, state.position, state.position));
                        state.tokens.Add(new Token<TokenType>(TokenType.Keyword, "concat", null, state.position, state.position));
                        state.tokens.Add(new Token<TokenType>(TokenType.Literal, "", null, state.position, state.position));
                    }),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "$`",resultState: StateType.String, custom: (state) => {
                        PushString('`', true);
                        state.tokens.Add(new Token<TokenType>(TokenType.InlineStart, "(", null, state.position, state.position));
                        state.tokens.Add(new Token<TokenType>(TokenType.Keyword, "concat", null, state.position, state.position));
                        state.tokens.Add(new Token<TokenType>(TokenType.Literal, "", null, state.position, state.position));
                    }),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^[a-z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Keyword, verifier: (c) => 'a' <= c && 'z' >= c),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\$[a-z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Keyword),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^-?\d+(\.\d+)?", RegexOptions.Compiled),type: TokenType.Literal, verifier: (c) => Char.IsDigit(c) || c == '-', processor: (token, state) => {
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
                        var stringToken = state.tokens[state.tokens.Count - 1];
                        if (stringStack.Count == 0) {
                            state.diagnostics.Add(new Diagnostic("Unexpected end of a template string embed, not in a template string", state.position, state.position));
                            stringToken.value = PrimitiveTypes.stringType.Instantiate("");
                            stringToken.end = state.position;
                            state.state = StateType.Default;
                            return true;
                        }
                        var stringBuilder = new StringBuilder();
                        var breakForTemplate = false;
                        var stringState = stringStack.Peek();
                        if (!stringState.isFirst) {
                            state.tokens.Insert(state.tokens.Count - 1, new Token<TokenType>(TokenType.InlineEnd, ")", null, state.position, state.position));
                        }
                        stringState.isFirst = false;

                        bool next() {
                            state.Next();
                            return false;
                        }

                        for (var curr = state.code[state.position.index];; curr = state.code[state.position.index]) {
                            if (curr == '\\') {
                                if (next()) return true;
                                curr = state.code[state.position.index];
                                if (curr == 'n') stringBuilder.Append('\n');
                                else if (curr == 't') stringBuilder.Append('\t');
                                else if (curr == '"') stringBuilder.Append('"');
                                else if (curr == '`') stringBuilder.Append('`');
                                else if (curr == '\\') stringBuilder.Append('\\');
                                else {
                                    state.diagnostics.Add(new Diagnostic("Unknown escape sequence", state.position, state.position));
                                }
                            } else if (curr == '$' && stringState.stringTemplate) {
                                if (next()) return true;
                                curr = state.code[state.position.index];
                                if (curr != '[') {
                                    continue;
                                } else {
                                    breakForTemplate = true;
                                    break;
                                }
                            } else if (curr == stringState.stringCloseChar) {
                                breakForTemplate = false;
                                break;
                            } else {
                                stringBuilder.Append(curr);
                            }

                            if (next()) return true;
                        }

                        stringToken.value = PrimitiveTypes.stringType.Instantiate(stringBuilder.ToString());
                        stringToken.end = state.position;

                        state.state = StateType.Default;
                        state.Next();

                        ILogger.instance?.Message("      → ").Object(stringToken.value).End();

                        if (!breakForTemplate) {
                            stringStack.Pop();
                            if (stringState.stringTemplate) {
                                state.tokens.Add(new Token<TokenType>(TokenType.InlineEnd, ")", null, state.position, state.position));
                            }
                        } else {
                            state.tokens.Add(new Token<TokenType>(TokenType.InlineStart, "(", null, state.position, state.position));
                        }

                        return true;
                    })
                } },
                { StateType.Comment, new List<ITokenDefinition<TokenType, StateType>> {
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\\\n"),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\n",resultState: StateType.Default),
                    new RegexTokenDefinition<TokenType, StateType>()
                } }
             }
        )
        {
            this.functions = functions;

            this.grammar[StateType.Default].AddRange(this.functions.Operators.OrderByDescending(v => v.match.Length).Select(
                op => new RegexTokenDefinition<TokenType, StateType>(pattern: op.match, type: TokenType.Operator)
            ).ToArray());
        }
    }
}
