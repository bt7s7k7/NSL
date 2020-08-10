using System;
using System.Collections.Generic;
namespace NSL.Types
{
    public class NSLFunction
    {
        protected Func<List<TypeSymbol>, Signature> signatureGenerator;
        protected Func<List<NSLValue>, NSLValue> impl;
        protected string name;

        public struct Signature
        {
            public List<TypeSymbol> arguments;
            public TypeSymbol result;
        }

        public Signature GetSignature(List<TypeSymbol> providedArguments) => signatureGenerator(providedArguments);

        public NSLValue Invoke(List<NSLValue> arguments) => impl(arguments);

        public NSLFunction(string name, Func<List<TypeSymbol>, Signature> signatureGenerator, Func<List<NSLValue>, NSLValue> impl)
        {
            this.signatureGenerator = signatureGenerator;
            this.impl = impl;
            this.name = name;
        }

        public static NSLFunction MakeSimple(string name, List<TypeSymbol> arguments, TypeSymbol result, Func<List<NSLValue>, NSLValue> impl) => new NSLFunction(
            name,
            _ => new Signature
            {
                arguments = arguments,
                result = result
            },
            impl
        );

        public string GetName() => name;
        public override string ToString() => name;
    }
}