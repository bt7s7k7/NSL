using System;
using System.Collections.Generic;
using NSL.Runtime;
using NSL.Tokenization.General;
using System.Linq;
using NSL.Types;

namespace NSL.Executable.Instructions
{
    public class InvokeInstruction : InstructionBase
    {
        override public int IndentDiff => 0;

        protected string? retVarName;
        protected string? oldRetVarName = null;
        protected string funcName;
        protected IEnumerable<string> arguments;

        override public string ToString() => $"invoke {retVarName ?? "_"}{(oldRetVarName != null ? $" ({oldRetVarName})" : "")} = {funcName} [{String.Join(',', arguments)}]";
        public override void Execute(Runner.State state)
        {
            if (funcName[0] == '$')
            {
                var variable = state.TopScope.Get(funcName) ?? throw new InternalNSLExcpetion($"Failed to find invoked variable '{funcName}'");

                var argumentValues = arguments.Select(v => state.TopScope.Get(v) ?? throw new InternalNSLExcpetion($"Failed to find variable '{v}'"));
                if (argumentValues.Count() == 1)
                {
                    variable.Value = argumentValues.First().Value;
                }

                if (retVarName != null)
                {
                    state.TopScope.Replace(retVarName, variable);
                }
            }
            else
            {
                var argumentValues = arguments.Select(v => state.TopScope.Get(v) ?? throw new InternalNSLExcpetion($"Failed to find variable '{v}'"));
                var function = state.FunctionRegistry.FindSpecific(funcName);
                if (function != null)
                {
                    var returnValue = function.Invoke(argumentValues, state);
                    if (retVarName != null)
                    {
                        state.TopScope.Replace(retVarName, returnValue);
                    }
                }
                else
                {
                    throw new InternalNSLExcpetion($"Failed to find function '{funcName}'");
                }
            }
        }

        public void RemoveRetVarName()
        {
            if (retVarName != null)
            {
                oldRetVarName = retVarName;
                retVarName = null;
            }
        }
        public string? GetRetVarName() => retVarName;

        public InvokeInstruction(Position start, Position end, string? retVarName, string funcName, IEnumerable<string> arguments) : base(start, end)
        {
            this.retVarName = retVarName;
            this.funcName = funcName;
            this.arguments = arguments;
        }
    }
}