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
            Operator,
            ActionArgument,
            DirectPipe
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
        private static Action<Token<TokenType>, TokenizationState> NumberProcessorFactory(int baseNum = 10, bool cut = false)
        {
            return (token, state) =>
            {
                var text = token.content;
                if (cut) text = text.Substring(2);

                var parsed = Double.NaN;

                try
                {
                    if (baseNum == 10) parsed = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                    else parsed = Convert.ToInt32(text, baseNum);
                }
                catch (FormatException err)
                {
                    state.diagnostics.Add(new Diagnostic($"Invalid number format: {err.Message}", token.start, token.end));
                    ILogger.instance?.Source("TOK").Error().Message($"Invalid number format: {err.Message}").Pos(token.start).End();
                }
                catch (OverflowException)
                {
                    state.diagnostics.Add(new Diagnostic($"Number doesn't fit in a number type", token.start, token.end));
                    ILogger.instance?.Source("TOK").Error().Message($"Number doesn't fit in a number type").Pos(token.start).End();
                }

                token.value = PrimitiveTypes.numberType.Instantiate(parsed).MakeConstexpr();
            };
        }

        protected FunctionRegistry functions;

        public NSLTokenizer(FunctionRegistry functions) : base(
            new Dictionary<StateType, List<ITokenDefinition<TokenType, StateType>>> {
                { StateType.Default, new List<ITokenDefinition<TokenType, StateType>>{
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\\\n"),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\\\r\n"),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\n",type: TokenType.StatementEnd),
                    new WhitespaceTokenDefinition<TokenType,StateType>(),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "true",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.boolType.Instantiate(true).MakeConstexpr()),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "false",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.boolType.Instantiate(false).MakeConstexpr()),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "NaN",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.numberType.Instantiate(Double.NaN).MakeConstexpr()),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "Infinity",type: TokenType.Literal, processor: (token, state) => token.value = PrimitiveTypes.numberType.Instantiate(Double.PositiveInfinity).MakeConstexpr()),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "var",type: TokenType.VariableDecl),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "const",type: TokenType.VariableDecl),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "]", resultState: StateType.String, type: TokenType.Literal),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|>{",type: TokenType.PipeForEachStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|{",type: TokenType.PipeStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|>",type: TokenType.PipeForEach),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "=>",type: TokenType.ActionArgument),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "|",type: TokenType.Pipe),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: ".",type: TokenType.DirectPipe),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "!{",type: TokenType.ActionStart),
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "{",type: TokenType.ActionStart),
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
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\$?[a-zA-Z][a-zA-Z0-9]*", RegexOptions.Compiled),type: TokenType.Keyword),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^0x[\da-f]+", RegexOptions.Compiled),type: TokenType.Literal, processor: NumberProcessorFactory(baseNum: 16, cut: true)),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^0b[10]+", RegexOptions.Compiled),type: TokenType.Literal, processor: NumberProcessorFactory(baseNum: 2, cut: true)),
                    new RegexTokenDefinition<TokenType, StateType>(expr: new Regex(@"^\d+(\.\d+)?([eE][\+\-]?\d+)?", RegexOptions.Compiled),type: TokenType.Literal, verifier: (c) => Char.IsDigit(c), processor: NumberProcessorFactory()),
                } },
                { StateType.String, new List<ITokenDefinition<TokenType, StateType>>{
                    new SimpleTokenDefinition<TokenType, StateType>((state) => {
                        var stringToken = state.tokens[state.tokens.Count - 1];
                        if (stringStack.Count == 0) {
                            state.diagnostics.Add(new Diagnostic("Unexpected end of a template string embed, not in a template string", state.position, state.position));
                            stringToken.value = PrimitiveTypes.stringType.Instantiate("").MakeConstexpr();
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

                        stringToken.value = PrimitiveTypes.stringType.Instantiate(stringBuilder.ToString()).MakeConstexpr();
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
                    new RegexTokenDefinition<TokenType, StateType>(pattern: "\\\r\n"),
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
