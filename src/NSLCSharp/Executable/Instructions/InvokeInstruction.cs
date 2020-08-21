using System;
using System.Collections.Generic;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class InvokeInstruction : ExeInstruction
    {
        protected string? retVarName;
        protected string funcName;
        protected IEnumerable<string> arguments;

        override public int GetIndentDiff() => 0;
        override public string ToString() => $"invoke {retVarName ?? "_"} = {funcName} [{String.Join(',', arguments)}]";

        public void RemoveRetVarName() => retVarName = null;
        public string? GetRetVarName() => retVarName;

        public InvokeInstruction(Position start, Position end, string? retVarName, string funcName, IEnumerable<string> arguments) : base(start, end)
        {
            this.retVarName = retVarName;
            this.funcName = funcName;
            this.arguments = arguments;
        }
    }
}