using System;
using System.Linq;
using System.Collections.Generic;
using NSL.Executable.Instructions;
using NSL.Parsing;
using NSL.Parsing.Nodes;
using NSL.Types;
using NSL.Runtime;

namespace NSL.Executable
{
    public static class Emitter
    {
        public class Result
        {
            public IEnumerable<Diagnostic> diagnostics;
            public NSLProgram program;

            public Result(List<Diagnostic> diagnostics, IEnumerable<IInstruction> instructions, IProgram.ReturnVariable? returnVariable)
            {
                this.diagnostics = diagnostics;
                this.program = new NSLProgram(instructions, returnVariable);
            }
        }

        protected interface IInstructionContainer
        {
            public void Add(EmittedInstruction instruction);
        }

        protected class EmittedInstruction
        {
            public IInstruction instruction;
            public Context context;

            public EmittedInstruction(IInstruction instruction, Context context)
            {
                this.instruction = instruction;
                this.context = context;
            }
        }

        protected class State : IInstructionContainer
        {
            public List<EmittedInstruction> instructions = new List<EmittedInstruction>();
            public List<Diagnostic> diagnostics;

            public State(List<Diagnostic> diagnostics)
            {
                this.diagnostics = diagnostics;
            }

            public void Add(EmittedInstruction instruction)
            {
                instructions.Add(instruction);
            }

            public void Add(IInstruction instruction, Context context)
            {
                Add(new EmittedInstruction(instruction, context));
            }

            public IEnumerable<IInstruction> FinishInstructions() => instructions.Select(inst =>
            {
                if (inst.instruction is InvokeInstruction invoke)
                {
                    var varName = invoke.GetRetVarName();
                    if (varName != null)
                    {
                        var varType = inst.context.scope.Get(varName);
                        if (varType == null)
                        {
                            invoke.RemoveRetVarName();
                        }
                    }
                }

                return inst.instruction;
            });
        }

        protected struct Context
        {
            public int? scopeId;
            public Scope scope;

            public Context(int? scope)
            {
                this.scopeId = scope;
                this.scope = new Scope(null);
            }

            public Context UpdateScope(int? scopeId)
            {
                var copy = this;
                copy.scopeId = scopeId;
                copy.scope = new Scope(this.scope);
                return copy;
            }
        }

        protected class Emission : IInstructionContainer
        {
            public TypeSymbol? type;
            public List<EmittedInstruction> instructions = new List<EmittedInstruction>();
            public string varName;

            public Emission(string varName, IASTNode node)
            {
                this.varName = varName;
                this.node = node;
            }

            public Action<IInstructionContainer, Emission>? emit;
            public IASTNode node;

            public void Add(EmittedInstruction instruction)
            {
                ILogger.instance?.Source("EMT").Message("Added").Name(instruction.GetType().Name).Object(instruction.instruction.ToString()).Pos(instruction.instruction.Start).End();
                instructions.Add(instruction);
            }

            public void Add(IInstruction instruction, Context context)
            {
                Add(new EmittedInstruction(instruction, context));
            }

            public void EmitTo(IInstructionContainer instCont, Context context)
            {
                if (emit != null)
                {
                    emit(instCont, this);
                }
                else
                {
                    foreach (var inst in instructions)
                    {
                        instCont.Add(inst);
                    }
                }
            }
        }

        protected class Scope
        {
            protected Dictionary<string, TypeSymbol> variables = new Dictionary<string, TypeSymbol>();
            protected Scope? parent = null;

            public void Add(string name, TypeSymbol value)
            {
                variables.Add(name, value);
            }

            public TypeSymbol? Get(string name)
            {
                if (variables.TryGetValue(name, out TypeSymbol? value))
                {
                    return value;
                }
                else if (parent != null)
                {
                    return parent.Get(name);
                }
                else return null;
            }

            public Scope(Scope? parent)
            {
                this.parent = parent;
            }
        }

        private static int globalScopeId = 0;
        private static int globalVariableId = 0;

        public static Result Emit(Parser.ParsingResult parsingResult, FunctionRegistry functions, Runner.Scope? runnerRootScope = null)
        {
            var state = new State(parsingResult.diagnostics);

            if (parsingResult.diagnostics.Count > 0)
            {
                return new Result(state.diagnostics, state.FinishInstructions(), null);
            }

            string makeVarName()
            {
                return "$_" + globalVariableId++;
            }

            Emission visitBlock(StatementRootNode rootNode, Context context, int? overrideScopeId = null)
            {
                ILogger.instance?.Source("EMT").Message("Visiting block").Pos(rootNode.Start).End();
                var result = new Emission("", rootNode);

                var innerContext = context.UpdateScope(overrideScopeId ?? globalScopeId++);
                result.Add(new PushInstruction(rootNode.Start, rootNode.Start, (int)innerContext.scopeId!, context.scopeId), innerContext);
                Emission? lastEmission = null;
                bool lastEmissionDefined = false;

                foreach (var node in rootNode.Children)
                {
                    if (node is VariableNode variableNode)
                    {
                        var varName = variableNode.varName ?? throw new InternalNSLExcpetion("Variable node has .varName == null");
                        var wrapperEmission = new Emission(varName, variableNode);

                        lastEmission = visitStatement(variableNode, innerContext);

                        wrapperEmission.Add(new DefInstruction(variableNode.Start, variableNode.End, varName, lastEmission.type!, null), innerContext);
                        wrapperEmission.type = lastEmission.type!;
                        innerContext.scope.Add(varName, lastEmission.type!);
                        lastEmission.EmitTo(wrapperEmission, innerContext);

                        lastEmission = wrapperEmission;
                        lastEmission.EmitTo(result, innerContext);
                        lastEmissionDefined = true;
                    }
                    else if (node is StatementNode statementNode)
                    {
                        lastEmission = visitStatement(statementNode, innerContext);
                        lastEmission.EmitTo(result, innerContext);
                        lastEmissionDefined = false;
                    }
                    else if (node is ForEachNode forEachNode)
                    {
                        lastEmission = visitForEachStatement(forEachNode, innerContext);
                        lastEmission.EmitTo(result, innerContext);
                        lastEmissionDefined = true;
                    }
                    else if (node is DistributionNode distributionNode)
                    {
                        lastEmission = visitDistributionNode(distributionNode, innerContext);
                        lastEmission.EmitTo(result, innerContext);
                        lastEmissionDefined = true;
                    }
                    else
                    {
                        throw new InternalNSLExcpetion($"Unexpected {node.GetType()} in statement root node children");
                    }
                }

                if (lastEmission != null)
                {
                    if (lastEmissionDefined)
                    {
                        result.varName = makeVarName();
                        result.type = lastEmission.type;
                        result.Add(new InvokeInstruction(rootNode.Start, rootNode.End, result.varName, lastEmission.varName, new string[] { }), innerContext);
                    }
                    else
                    {
                        result.varName = lastEmission.varName;
                        result.type = lastEmission.type;
                    }
                }
                else
                {
                    result.varName = makeVarName();
                    result.type = PrimitiveTypes.voidType;
                }

                result.Add(new PopInstruction(rootNode.End, rootNode.End), innerContext);

                return result;
            }

            Emission visitStatement(StatementNode node, Context context)
            {
                var variableNode = node as VariableNode;

                IEnumerable<NSLFunction> makeVariableAssignmentFunction()
                {
                    var name = node.name;
                    var type = context.scope.Get(name);
                    if (type != null)
                    {
                        return new[] {
                            NSLFunction.MakeSimple(name, new [] {type!}, type!, (argsEnum, state) => PrimitiveTypes.voidType.Instantiate(null)),
                            NSLFunction.MakeSimple(name, new TypeSymbol[] {}, type!, (argsEnum, state) => PrimitiveTypes.voidType.Instantiate(null))
                        };
                    }
                    else
                    {
                        state!.diagnostics.Add(new Diagnostic($"Variable '{node.name}' not found", node.Start, node.End));
                        return new[] {
                            NSLFunction.MakeSimple(name, new [] {PrimitiveTypes.neverType}, PrimitiveTypes.neverType, (argsEnum, state) => PrimitiveTypes.voidType.Instantiate(null)),
                            NSLFunction.MakeSimple(name, new TypeSymbol[] {}, PrimitiveTypes.neverType, (argsEnum, state) => PrimitiveTypes.voidType.Instantiate(null))
                        };
                    }
                }

                var foundFunctions = variableNode != null
                    ? new[] { FunctionRegistry.MakeVariableDefinitionFunction(variableNode.varName ?? throw new InternalNSLExcpetion("Variable node has .varName == null")) }
                    : node.name[0] == '$' ? makeVariableAssignmentFunction()
                    : functions.Find(node.name);

                if (variableNode != null)
                {
                    node = (StatementNode)variableNode.Children[0];
                }

                if (foundFunctions.Count() == 0)
                {
                    var emission = new Emission(makeVarName(), node);
                    emission.type = PrimitiveTypes.neverType;
                    emission.Add(new DefInstruction(node.Start, node.End, emission.varName, PrimitiveTypes.neverType, null), context);
                    context.scope.Add(emission.varName, PrimitiveTypes.neverType);
                    state!.diagnostics.Add(new Diagnostic($"Function '{node.name}' not found", node.Start, node.End));
                    return emission;
                }
                else
                {
                    var arguments = new List<Emission>();
                    var emission = new Emission(variableNode != null ? foundFunctions.First().GetName() : makeVarName(), node);

                    var innerContext = context.UpdateScope(globalScopeId++);
                    emission.Add(new PushInstruction(node.Start, node.Start, (int)innerContext.scopeId!, context.scopeId), innerContext);

                    foreach (var child in node.Children)
                    {
                        if (child is LiteralNode literal)
                        {
                            var litEmission = new Emission(makeVarName(), literal);
                            arguments.Add(litEmission);
                            litEmission.Add(new DefInstruction(literal.Start, literal.End, litEmission.varName, literal.value.GetTypeSymbol(), literal.value.GetValue()), innerContext);
                            context.scope.Add(litEmission.varName, literal.value.GetTypeSymbol());
                            litEmission.type = literal.value.GetTypeSymbol();
                        }
                        else if (child is StatementNode statement)
                        {
                            if (statement.name[0] != '$')
                            {
                                var statementEmission = visitStatement(statement, innerContext);
                                if (statementEmission.type == null) throw new InternalNSLExcpetion("The result of 'visitStatement' has .type == null");
                                var wrapperEmission = new Emission(statementEmission.varName, statementEmission.node);
                                wrapperEmission.Add(new DefInstruction(statementEmission.node.Start, statementEmission.node.End, statementEmission.varName, statementEmission.type, null), innerContext);
                                innerContext.scope.Add(statementEmission.varName, statementEmission.type);
                                wrapperEmission.type = statementEmission.type;

                                statementEmission.EmitTo(wrapperEmission, innerContext);
                                arguments.Add(wrapperEmission);
                            }
                            else
                            {
                                var varType = context.scope.Get(statement.name);
                                var statementEmission = new Emission(statement.name, statement);

                                if (varType != null)
                                {
                                    statementEmission.type = varType;
                                }
                                else
                                {
                                    state!.diagnostics.Add(new Diagnostic($"Failed to find variable {statement.name}", statement.Start, statement.End));
                                    statementEmission.type = PrimitiveTypes.neverType;
                                }

                                arguments.Add(statementEmission);
                            }
                        }
                        else if (child is StatementRootNode rootNode)
                        {
                            var rootEmission = visitBlock(rootNode, innerContext);
                            if (rootEmission.type == null) throw new InternalNSLExcpetion("The result of 'visitBlock' has .type == null");
                            var wrapperEmission = new Emission(rootEmission.varName, rootEmission.node);
                            wrapperEmission.Add(new DefInstruction(rootEmission.node.Start, rootEmission.node.End, rootEmission.varName, rootEmission.type, null), innerContext);
                            innerContext.scope.Add(rootEmission.varName, rootEmission.type);
                            wrapperEmission.type = rootEmission.type;

                            rootEmission.EmitTo(wrapperEmission, innerContext);
                            arguments.Add(wrapperEmission);
                        }
                        else if (child is ActionNode actionNode)
                        {
                            var blockNode = (StatementBlockNode)actionNode.Children[0];
                            var actionEmission = makeAction(blockNode, innerContext, "$v");

                            arguments.Add(actionEmission);

                        }
                        else throw new InternalNSLExcpetion($"Unexpected {child.GetType()} in statement node children");
                    }

                    var providedArgs = arguments.Select(v => v.type).ToArray();

                    NSLFunction.Signature signature;

                    try
                    {
                        IEnumerable<Emission> finalArguments = arguments;

                        (_, signature) = NSLFunction.GetMatchingFunction(foundFunctions, providedArgs, (index, argumentType) =>
                        {
                            var actionEmission = arguments[index];
                            actionEmission.instructions[0].context.scope.Add("$v", argumentType);
                            actionEmission.EmitTo(emission, innerContext);

                            finalArguments = finalArguments.Where(v => v != actionEmission);

                            return actionEmission.type ?? throw new InternalNSLExcpetion("Action emission did not set a type");
                        });

                        foreach (var argument in finalArguments)
                        {
                            argument.EmitTo(emission, innerContext);
                        }
                    }
                    catch (OverloadNotFoundNSLException err)
                    {
                        state!.diagnostics.Add(new Diagnostic(err.Message, node.Start, node.End));
                        signature = new NSLFunction.Signature
                        {
                            arguments = new TypeSymbol[] { },
                            name = err.FunctionName,
                            result = err.ReturnType
                        };
                    }

                    emission.type = signature.result;

                    emission.Add(new InvokeInstruction(node.Start, node.End, emission.varName, signature.name, arguments.Select(v => v.varName)), innerContext);

                    emission.Add(new PopInstruction(node.End, node.End), innerContext);

                    return emission;
                }
            }

            Emission makeAction(StatementRootNode blockNode, Context context, string argVarName)
            {
                var result = new Emission(makeVarName(), blockNode);
                var innerContext = context.UpdateScope(context.scopeId);

                result.Add(new EmittedInstruction(new InvokeInstruction(blockNode.Start, blockNode.End, null, "echo", new[] { "$v" }), innerContext));

                result.emit = (target, emission) =>
                {
                    var blockEmission = visitBlock(blockNode, innerContext);

                    IProgram.ReturnVariable returnVariable = new IProgram.ReturnVariable(blockEmission.type!, blockEmission.varName);
                    IProgram.ReturnVariable argumentVariable = new IProgram.ReturnVariable(
                        innerContext.scope.Get(argVarName) ?? throw new InternalNSLExcpetion($"Failed to find argument variable {argVarName}"),
                        argVarName);
                    target.Add(new EmittedInstruction(new ActionInstruction(parsingResult.rootNode.Start, parsingResult.rootNode.End, result.varName, returnVariable, argumentVariable), innerContext));

                    if (blockEmission.type != PrimitiveTypes.voidType)
                    {
                        innerContext.scope.Add(blockEmission.varName, blockEmission.type!);
                    }
                    blockEmission.EmitTo(target, innerContext);

                    target.Add(new EmittedInstruction(new EndInstruction(parsingResult.rootNode.End, parsingResult.rootNode.End), innerContext));

                    emission.type = blockEmission.type;
                };


                return result;
            }

            Emission visitForEachStatement(ForEachNode node, Context context)
            {
                var innerContext = context.UpdateScope(globalScopeId++);
                var sourceNode = node.Children[0] as StatementNode;
                var targetNode = node.Children[1];

                var sourceEmission = visitStatement(sourceNode!, innerContext);
                if (sourceEmission.type == null) throw new InternalNSLExcpetion("The result of 'visitStatement' has .type == null");
                if (sourceEmission.type is ArrayTypeSymbol arrayType)
                {
                    var result = new Emission(sourceEmission.varName, sourceEmission.node);
                    result.type = sourceEmission.type;

                    result.Add(new DefInstruction(sourceEmission.node.Start, sourceEmission.node.End, sourceEmission.varName, sourceEmission.type, null), innerContext);
                    innerContext.scope.Add(sourceEmission.varName, sourceEmission.type);

                    result.Add(new PushInstruction(node.Start, node.Start, (int)innerContext.scopeId!, context.scopeId), innerContext);

                    TypeSymbol itemType = arrayType.GetItemType();
                    result.Add(new DefInstruction(sourceEmission.node.Start, sourceEmission.node.End, "$_a", itemType, null), innerContext);
                    innerContext.scope.Add("$_a", itemType);

                    sourceEmission.EmitTo(result, innerContext);

                    if (targetNode is StatementNode statementTargetNode)
                    {
                        var block = new StatementBlockNode(false, false, statementTargetNode.Start, statementTargetNode.End);
                        block.AddChild(statementTargetNode);

                        var actionEmission = makeAction(block, innerContext, "$_a");

                        actionEmission.EmitTo(result, innerContext);

                        result.Add(new ForEachInvokeInstruction(node.Start, node.End, sourceEmission.varName, "$_a", actionEmission.varName), innerContext);
                    }
                    else if (targetNode is StatementBlockNode statementBlockTargetNode)
                    {
                        var actionEmission = makeAction(statementBlockTargetNode, innerContext, "$_a");

                        actionEmission.EmitTo(result, innerContext);

                        result.Add(new ForEachInvokeInstruction(node.Start, node.End, sourceEmission.varName, "$_a", actionEmission.varName), innerContext);
                    }
                    else
                    {
                        throw new InternalNSLExcpetion($"Unexpected {targetNode.GetType()} in target slot (1) in for each node");
                    }

                    result.Add(new PopInstruction(node.End, node.End), innerContext);

                    return result;
                }
                else
                {
                    state!.diagnostics.Add(new Diagnostic("Foreach source type is not an array", node.Start, node.End));
                    var emission = new Emission(makeVarName(), node);
                    emission.type = PrimitiveTypes.neverType;
                    emission.Add(new DefInstruction(node.Start, node.End, emission.varName, emission.type, null), innerContext);
                    return emission;
                }
            }

            Emission visitDistributionNode(DistributionNode node, Context context)
            {
                var innerContext = context.UpdateScope(globalScopeId++);
                var sourceNode = node.Children[0] as StatementNode;
                var targetNode = node.Children[1] as StatementBlockNode;

                var sourceEmission = visitStatement(sourceNode!, innerContext);
                if (sourceEmission.type == null) throw new InternalNSLExcpetion("The result of 'visitStatement' has .type == null");

                var result = new Emission(sourceEmission.varName, sourceEmission.node);
                result.type = sourceEmission.type;

                result.Add(new DefInstruction(node.Start, node.End, sourceEmission.varName, sourceEmission.type, null), innerContext);
                innerContext.scope.Add(sourceEmission.varName, sourceEmission.type);

                result.Add(new PushInstruction(node.Start, node.Start, (int)innerContext.scopeId!, context.scopeId), innerContext);

                var pushedVarName = targetNode!.pushedVarName!;

                result.Add(new DefInstruction(node.Start, node.End, pushedVarName, sourceEmission.type, null), innerContext);
                innerContext.scope.Add(pushedVarName, sourceEmission.type);

                sourceEmission.EmitTo(result, innerContext);

                result.Add(new InvokeInstruction(node.Start, node.End, pushedVarName, sourceEmission.varName, new string[] { }), innerContext);

                visitBlock(targetNode!, innerContext).EmitTo(result, innerContext);

                result.Add(new PopInstruction(node.End, node.End), innerContext);

                return result;
            }

            Context context = new Context(scope: null);

            if (runnerRootScope != null) foreach (var (key, value) in runnerRootScope.GetAllVariables())
                {
                    context.scope.Add(key, value.GetTypeSymbol());
                }

            var result = visitBlock(parsingResult.rootNode, context, overrideScopeId: -1);

            IProgram.ReturnVariable? returnVariable = null;
            if (result.type != PrimitiveTypes.voidType)
            {
                context.scope.Add(result.varName, result.type!);
                result.EmitTo(state, context);
                returnVariable = new IProgram.ReturnVariable(result.type!, result.varName);
            }
            else
            {
                result.EmitTo(state, context);
            }

            return new Result(state.diagnostics, state.FinishInstructions(), returnVariable);
        }
    }
}