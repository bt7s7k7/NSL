using System.Security.AccessControl;
using System;
using System.Linq;
using System.Collections.Generic;
using NSL.Executable.Instructions;
using NSL.Parsing;
using NSL.Parsing.Nodes;
using NSL.Types;

namespace NSL.Executable
{
    public static class Emitter
    {
        public class Result
        {
            public List<ExeInstruction> instructions = new List<ExeInstruction>();
            public List<Diagnostic> diagnostics;

            public Result(List<Diagnostic> diagnostics)
            {
                this.diagnostics = diagnostics;
            }
        }

        public interface IInstructionContainer
        {
            public void Add(ExeInstruction instruction);
        }

        public class State : Result, IInstructionContainer
        {
            public State(List<Diagnostic> diagnostics) : base(diagnostics) { }

            public void Add(ExeInstruction instruction)
            {
                instructions.Add(instruction);
            }
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
            public List<ExeInstruction> instructions = new List<ExeInstruction>();
            public string varName;

            public Emission(string varName, ASTNode node)
            {
                this.varName = varName;
                this.node = node;
            }

            public Action<IInstructionContainer, Emission>? emit;
            public ASTNode node;

            public void Add(ExeInstruction instruction)
            {
                Logger.instance?.Source("EMT").Message("Added").Name(instruction.GetType().Name).Object(instruction.ToString()).Pos(instruction.start).End();
                instructions.Add(instruction);
            }

            public void EmitTo(IInstructionContainer instCont)
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

        public static Result Emit(Parser.ParsingResult parsingResult, FunctionRegistry functions)
        {
            var state = new State(parsingResult.diagnostics);

            if (parsingResult.diagnostics.Count > 0)
            {
                return state;
            }

            var globalScopeId = 0;
            var globalVariableId = 0;

            string makeVarName()
            {
                return "$_" + globalVariableId++;
            }

            Emission visitBlock(StatementRootNode rootNode, Context context)
            {
                Logger.instance?.Source("EMT").Message("Visiting block").Pos(rootNode.start).End();
                var result = new Emission("", rootNode);

                var innerContext = context.UpdateScope(globalScopeId++);
                result.Add(new PushInstruction(rootNode.start, rootNode.start, (int)innerContext.scopeId!, context.scopeId));
                Emission? lastEmission = null;

                foreach (var node in rootNode.children)
                {
                    if (node is StatementNode statementNode)
                    {
                        lastEmission = visitStatement(statementNode, innerContext);
                        lastEmission.EmitTo(result);
                    }
                    else
                    {
                        throw new InternalNSLExcpetion($"Unexpected {node.GetType()} in statement root node children");
                    }
                }

                if (lastEmission != null)
                {
                    result.varName = lastEmission.varName;
                    result.type = lastEmission.type;
                }
                else
                {
                    result.varName = makeVarName();
                    result.type = PrimitiveTypes.voidType;
                }

                result.Add(new PopInstruction(rootNode.end, rootNode.end));

                return result;
            }

            Emission visitStatement(StatementNode node, Context context)
            {
                var function = functions.Find(node.name);

                if (function == null)
                {
                    var emission = new Emission(makeVarName(), node);
                    emission.type = PrimitiveTypes.voidType;
                    emission.Add(new DefInstruction(node.start, node.end, emission.varName, PrimitiveTypes.voidType, null));
                    return emission;
                }
                else
                {
                    var arguments = new List<Emission>();
                    var emission = new Emission(makeVarName(), node);

                    var innerContext = context.UpdateScope(globalScopeId++);
                    emission.Add(new PushInstruction(node.start, node.start, (int)innerContext.scopeId!, context.scopeId));

                    foreach (var child in node.children)
                    {
                        if (child is LiteralNode literal)
                        {
                            var litEmission = new Emission(makeVarName(), literal);
                            arguments.Add(litEmission);
                            litEmission.Add(new DefInstruction(literal.start, literal.end, litEmission.varName, literal.value.GetTypeSymbol(), literal.value.GetValue()));
                            litEmission.type = literal.value.GetTypeSymbol();
                        }
                        else if (child is StatementNode statement)
                        {
                            var statementEmission = visitStatement(statement, innerContext);
                            if (statementEmission.type == null) throw new InternalNSLExcpetion("The result of 'visitStatement' has .type == null");
                            var wrapperEmission = new Emission(statementEmission.varName, statementEmission.node);
                            wrapperEmission.Add(new DefInstruction(statementEmission.node.start, statementEmission.node.end, statementEmission.varName, statementEmission.type, null));
                            wrapperEmission.type = statementEmission.type;

                            statementEmission.EmitTo(wrapperEmission);
                            arguments.Add(wrapperEmission);
                        }
                        else if (child is StatementRootNode rootNode)
                        {
                            var rootEmission = visitBlock(rootNode, innerContext);
                            if (rootEmission.type == null) throw new InternalNSLExcpetion("The result of 'visitBlock' has .type == null");
                            var wrapperEmission = new Emission(rootEmission.varName, rootEmission.node);
                            wrapperEmission.Add(new DefInstruction(rootEmission.node.start, rootEmission.node.end, rootEmission.varName, rootEmission.type, null));
                            wrapperEmission.type = rootEmission.type;

                            rootEmission.EmitTo(wrapperEmission);
                            arguments.Add(wrapperEmission);
                        }
                        else throw new InternalNSLExcpetion($"Unexpected {node.GetType()} in statement node children");
                    }

                    var providedArgs = arguments.Select(v => v.type).ToArray();

                    var signature = function.GetSignature(providedArgs);
                    var wantedArgs = signature.arguments.ToArray();

                    if (providedArgs.Length < wantedArgs.Length)
                    {
                        state.diagnostics.Add(new Diagnostic($"Wrong argument count for '{signature}', expected: '{wantedArgs.Length}', got: '{providedArgs.Length}'", node.start, node.end));
                    }
                    else
                    {
                        for (int i = 0, len = providedArgs.Count(); i < len; i++)
                        {
                            var provided = providedArgs[i];
                            var wanted = wantedArgs[i];
                            var argumentEmission = arguments[i];

                            if (provided == wanted)
                            {
                                argumentEmission.EmitTo(emission);
                            }
                            else
                            {
                                state.diagnostics.Add(new Diagnostic($"Wrong argument type for '{signature}', expected: '{wanted}', got: '{provided}'", argumentEmission.node.start, argumentEmission.node.end));
                            }
                        }
                    }

                    emission.type = signature.result;

                    emission.Add(new InvokeInstruction(node.start, node.end, emission.varName, signature.name, arguments.Select(v => v.varName)));

                    emission.Add(new PopInstruction(node.end, node.end));

                    return emission;
                }
            }

            state.Add(new ActionInstruction(parsingResult.rootNode.start, parsingResult.rootNode.end, makeVarName()));

            var result = visitBlock(parsingResult.rootNode, new Context(
                scope: null
            ));

            result.EmitTo(state);

            state.Add(new EndInstruction(parsingResult.rootNode.end, parsingResult.rootNode.end));

            return state;
        }
    }
}