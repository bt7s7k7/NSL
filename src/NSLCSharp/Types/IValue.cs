using System;

namespace NSL.Types
{
    public interface IValue
    {
        object? Value { get; set; }
        TypeSymbol TypeSymbol { get; }

        T GetValue<T>();
        string ToString();
    }

    public abstract class NSLValueBase : IValue
    {
        public abstract object? Value { get; set; }
        public abstract TypeSymbol TypeSymbol { get; protected set; }

        public T GetValue<T>() => (T)(Value ?? throw new NullReferenceException());
        public override string ToString() => $"{ToStringUtil.ToString(Value)} : {TypeSymbol}";
    }
}