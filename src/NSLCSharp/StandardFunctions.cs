using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NSL.Executable;
using System.Text;
using NSL.Types;
using NSL.Types.Values;

namespace NSL
{
    public partial class FunctionRegistry
    {
        public static FunctionRegistry GetStandardFunctionRegistry()
        {
            var registry = new FunctionRegistry();

            RegisterBasic(registry);
            RegisterErrorHandling(registry);
            RegisterString(registry);
            RegisterNumber(registry);
            RegisterBoolean(registry);
            RegisterArray(registry);
            RegisterOperator(registry);
            RegisterType(registry);

            return registry;
        }
    }
}