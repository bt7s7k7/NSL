using NSL.Tokenization.General;
using System.Collections.Generic;

namespace NSL.Tokenization
{
    public static class TokenizerFactory
    {
        public enum TokenType
        {
            Keyword,
            String,
            Number,
            Pipe,
            PipeForEach,
            ActionStart,
            PipeStart,
            PipeForEachStart,
            BlockEnd,
            InlineStart,
            InlineEnd,
            StatementEnd
        }

        public enum StateType
        {

        }

        public static Tokenizer<TokenType, StateType> Build()
        {
            return new Tokenizer<TokenType, StateType>(new Dictionary<string, List<TokenDefinition<TokenType, StateType>>> { });
        }
    }
}