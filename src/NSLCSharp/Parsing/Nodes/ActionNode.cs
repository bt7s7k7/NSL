using NSL.Executable;
using NSL.Tokenization.General;
using System;
using System.Collections.Generic;

namespace NSL.Parsing.Nodes
{
    public class ActionNode : ASTNodeBase
    {
        public bool HasBody => Children.Count != 0;
        public IEnumerable<string> Arguments => arguments;

        protected List<string> arguments = new List<string>();

        public void AddArgument(string newArgument)
        {
            arguments.Add(newArgument);
        }

        public override void Unbalanced(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("ActionNode node cannot be pushed on the stack");
        }

        public override void Execute(Parser.ParsingState state)
        {
            throw new InternalNSLExcpetion("ActionNode node cannot be pushed on the stack");
        }

        public override string GetAdditionalInfo()
        {
            return $"({String.Join(", ", Arguments)}) => _";
        }

        public ActionNode(Position start, Position end) : base(start, end) { }

    }
}