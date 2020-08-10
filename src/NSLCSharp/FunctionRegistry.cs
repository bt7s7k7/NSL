using System.Collections.Generic;
using NSL.Types;

namespace NSL
{
    public class FunctionRegistry
    {
        protected Dictionary<string, NSLFunction> functions = new Dictionary<string, NSLFunction>();

        public void Add(NSLFunction function)
        {
            functions.Add(function.GetName(), function);
        }

        public NSLFunction? Get(string name)
        {
            if (functions.TryGetValue(name, out NSLFunction? function))
            {
                return function;
            }
            else return null;
        }
    }
}