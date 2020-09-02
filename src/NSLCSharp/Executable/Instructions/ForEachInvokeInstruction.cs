using System.Collections;
using NSL.Runtime;
using NSL.Tokenization.General;
using NSL.Types;

namespace NSL.Executable.Instructions
{
    public class ForEachInvokeInstruction : InstructionBase
    {
        override public int IndentDiff => 0;

        protected string argVarName;
        protected string actionVarName;
        protected string arrayVarName;

        override public string ToString() => $"for {arrayVarName} : {argVarName} => {actionVarName}";
        public override void Execute(Runner.State state)
        {
            var scope = state.TopScope;
            var actionVariable = scope.Get(actionVarName);
            if (actionVariable == null) throw new InternalNSLExcpetion($"Failed to find action variable {actionVarName}");
            if (actionVariable.Value is NSLAction action)
            {

                var arrayVariable = scope.Get(arrayVarName);
                if (arrayVariable == null) throw new InternalNSLExcpetion($"Failed to find array variable {arrayVarName}");
                var arrayObject = arrayVariable.Value;
                if (arrayObject is IEnumerable arrayEnum)
                {
                    var argumentVariable = action.ArgumentVariable.type.Instantiate(null);
                    action.Scope.Set(action.ArgumentVariable.varName, argumentVariable);
                    foreach (var element in arrayEnum)
                    {
                        argumentVariable.Value = element;
                        state.Runner.RunAction(action);
                    }
                }
                else
                {
                    throw new InternalNSLExcpetion($"Supposed array typed ({arrayVariable.TypeSymbol}) variable's object is not IEnumerable {arrayObject?.GetType().Name ?? "null"}");
                }
            }
            else
            {
                throw new InternalNSLExcpetion($"Supposed action typed ({actionVariable.TypeSymbol}) variable's object is not an NSLAction {actionVariable.Value?.GetType().Name ?? "null"}");
            }
        }

        public ForEachInvokeInstruction(Position start, Position end, string arrayVarName, string argVarName, string actionVarName) : base(start, end)
        {
            this.argVarName = argVarName;
            this.actionVarName = actionVarName;
            this.arrayVarName = arrayVarName;
        }
    }
}