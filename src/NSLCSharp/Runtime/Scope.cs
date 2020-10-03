using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL.Runtime
{
    public partial class Runner
    {
        public class Scope
        {
            protected Dictionary<string, IValue> variables = new Dictionary<string, IValue>();
            public Scope Parent { get; protected set; } = null;
            public string Name { get; protected set; }

            public void Set(string name, IValue value)
            {
                variables[name] = value;
            }

            public bool Replace(string name, IValue value)
            {
                if (variables.ContainsKey(name))
                {
                    variables[name] = value;
                    return true;
                }
                else if (Parent != null)
                {
                    return Parent.Replace(name, value);
                }
                else return false;
            }

            public IValue Get(string name)
            {
                if (variables.TryGetValue(name, out IValue value))
                {
                    return value;
                }
                else if (Parent != null)
                {
                    return Parent.Get(name);
                }
                else return null;
            }

            public IEnumerable<(string key, IValue value)> GetAllVariables() => variables.Select(v => (v.Key, v.Value));

            public Scope(string name, Scope parent)
            {
                this.Parent = parent;
                Name = name;
            }
        }
    }
}