using System.Collections;
using NSL.Runtime;
using NSL.Tokenization.General;
using NSL.Types;

namespace NSL.Executable.Instructions
{
    public class ForEachInvokeInstruction : InstructionBase
    {
        protected string argVarName;
        protected string actionVarName;
        protected string arrayVarName;

        override public int GetIndentDiff() => 0;
        override public string ToString() => $"for {arrayVarName} : {argVarName} => {actionVarName}";
        public override void Execute(Runner.State state)
        {
            var scope = state.GetTopScope();
            var actionVariable = scope.Get(actionVarName);
            if (actionVariable == null) throw new InternalNSLExcpetion($"Failed to find action variable {actionVarName}");
            if (actionVariable.GetValue() is NSLAction action)
            {

                var arrayVariable = scope.Get(arrayVarName);
                if (arrayVariable == null) throw new InternalNSLExcpetion($"Failed to find array variable {arrayVarName}");
                var arrayObject = arrayVariable.GetValue();
                if (arrayObject is IEnumerable arrayEnum)
                {
                    var argumentVariable = action.ArgumentVariable.type.Instantiate(null);
                    action.Scope.Set(action.ArgumentVariable.varName, argumentVariable);
                    foreach (var element in arrayEnum)
                    {
                        argumentVariable.SetValue(element);
                        state.Runner.RunAction(action);
                    }
                }
                else
                {
                    throw new InternalNSLExcpetion($"Supposed array typed ({arrayVariable.GetTypeSymbol()}) variable's object is not IEnumerable {arrayObject?.GetType().Name ?? "null"}");
                }
            }
            else
            {
                throw new InternalNSLExcpetion($"Supposed action typed ({actionVariable.GetTypeSymbol()}) variable's object is not an NSLAction {actionVariable.GetValue()?.GetType().Name ?? "null"}");
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