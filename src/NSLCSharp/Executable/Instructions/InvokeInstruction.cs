using System;
using System.Collections.Generic;
using NSL.Runtime;
using NSL.Tokenization.General;
using System.Linq;

namespace NSL.Executable.Instructions
{
    public class InvokeInstruction : InstructionBase
    {
        protected string? retVarName;
        protected string funcName;
        protected IEnumerable<string> arguments;

        override public int GetIndentDiff() => 0;
        override public string ToString() => $"invoke {retVarName ?? "_"} = {funcName} [{String.Join(',', arguments)}]";
        public override void Execute(Runner.State state)
        {
            if (funcName[0] == '$')
            {
                var variable = state.GetTopScope().Get(funcName) ?? throw new InternalNSLExcpetion($"Failed to find invoked variable '{funcName}'");

                var argumentValues = arguments.Select(v => state.GetTopScope().Get(v) ?? throw new InternalNSLExcpetion($"Failed to find variable '{v}'"));
                if (argumentValues.Count() == 1)
                {
                    variable.SetValue(argumentValues.First().GetValue());
                }

                if (retVarName != null)
                {
                    var retVariable = state.GetTopScope().Get(retVarName) ?? throw new InternalNSLExcpetion($"Failed to find return variable '{retVarName}'");
                    retVariable.SetValue(variable.GetValue());
                }
            }
            else
            {
                var function = state.FunctionRegistry.Find(funcName);
                if (function != null)
                {
                    var argumentValues = arguments.Select(v => state.GetTopScope().Get(v) ?? throw new InternalNSLExcpetion($"Failed to find variable '{v}'"));
                    var returnValue = function.Invoke(argumentValues);
                    if (retVarName != null)
                    {
                        var retVariable = state.GetTopScope().Get(retVarName) ?? throw new InternalNSLExcpetion($"Failed to find return variable '{retVarName}'");
                        retVariable.SetValue(returnValue.GetValue());
                    }
                }
                else
                {
                    throw new InternalNSLExcpetion($"Failed to find function '{funcName}'");
                }
            }
        }

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