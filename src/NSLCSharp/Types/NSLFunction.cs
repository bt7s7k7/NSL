using System;
using System.Collections.Generic;
namespace NSL.Types
{
    public class NSLFunction
    {
        protected Func<IEnumerable<TypeSymbol?>, Signature> signatureGenerator;
        protected Func<IEnumerable<NSLValue>, NSLValue> impl;
        protected string name;

        public struct Signature
        {
            public IEnumerable<TypeSymbol> arguments;
            public TypeSymbol result;
            public string name;

            public override string ToString() => $"{name} {String.Join(' ', arguments)} : {result}";
        }

        public Signature GetSignature(IEnumerable<TypeSymbol?> providedArguments) => signatureGenerator(providedArguments);

        public NSLValue Invoke(IEnumerable<NSLValue> arguments) => impl(arguments);

        public NSLFunction(string name, Func<IEnumerable<TypeSymbol?>, Signature> signatureGenerator, Func<IEnumerable<NSLValue>, NSLValue> impl)
        {
            this.signatureGenerator = signatureGenerator;
            this.impl = impl;
            this.name = name;
        }

        public static NSLFunction MakeSimple(string name, IEnumerable<TypeSymbol> arguments, TypeSymbol result, Func<IEnumerable<NSLValue>, NSLValue> impl) => new NSLFunction(
            name,
            _ => new Signature
            {
                arguments = arguments,
                result = result,
                name = name
            },
            impl
        );

        public string GetName() => name;
        public override string ToString() => name;
    }
}