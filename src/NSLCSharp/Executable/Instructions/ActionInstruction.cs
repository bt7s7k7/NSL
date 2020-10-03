using System;
using System.Collections.Generic;
using System.Linq;
using NSL.Runtime;
using NSL.Tokenization.General;

namespace NSL.Executable.Instructions
{
    public class ActionInstruction : InstructionBase
    {
        override public int IndentDiff => 1;

        public string Name { get; protected set; }
        public VariableDefinition ReturnVariable { get; protected set; }
        public IEnumerable<VariableDefinition> ArgumentVariables { get; protected set; }

        override public string ToString()
        {
            var arguments = String.Join(", ", ArgumentVariables.Select(v => $"{v.varName} : {v.type}"));
            var returnType = ReturnVariable == null ? "void" : $"{ReturnVariable.varName} = {ReturnVariable.type}";
            return $"action {Name} : ({arguments}) => {returnType}";
        }

        public override void Execute(Runner.State state)
        {
            throw new System.NotImplementedException();
        }

        public ActionInstruction(Position start, Position end, string name, VariableDefinition returnVariable, IEnumerable<VariableDefinition> argumentVariables) : base(start, end)
        {
            Name = name;
            ReturnVariable = returnVariable;
            ArgumentVariables = argumentVariables;
        }
    }
}