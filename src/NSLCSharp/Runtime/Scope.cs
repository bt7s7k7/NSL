using System.Collections.Generic;
using System.Linq;
using NSL.Types;

namespace NSL.Runtime
{
    public partial class Runner
    {
        public class Scope
        {
            protected Dictionary<string, NSLValue> variables = new Dictionary<string, NSLValue>();
            public Scope? Parent { get; protected set; } = null;
            public string Name { get; protected set; }

            public void Set(string name, NSLValue value)
            {
                variables[name] = value;
            }

            public NSLValue? Get(string name)
            {
                if (variables.TryGetValue(name, out NSLValue? value))
                {
                    return value;
                }
                else if (Parent != null)
                {
                    return Parent.Get(name);
                }
                else return null;
            }

            public IEnumerable<(string key, NSLValue value)> GetAllVariables() => variables.Select(v => (v.Key, v.Value));

            public Scope(string name, Scope? parent)
            {
                this.Parent = parent;
                Name = name;
            }
        }
    }
}