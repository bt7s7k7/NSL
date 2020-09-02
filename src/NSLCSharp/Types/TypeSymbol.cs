using System;
using NSL.Types.Values;

namespace NSL.Types
{
    public class TypeSymbol
    {
        public string Name { get; protected set; }

        override public string ToString()
        {
            return Name;
        }

        public TypeSymbol(string name)
        {
            this.Name = name;
        }

        public ArrayTypeSymbol ToArray() => new ArrayTypeSymbol(this);
        public OptionalTypeSymbol ToOptional() => new OptionalTypeSymbol(this);

        public IValue Instantiate(object? value) => new SimpleValue(value, this);

        override public bool Equals(object? obj)
        {
            return obj != null && obj is TypeSymbol symbol && symbol.ToString() == this.ToString();
        }

        public static bool operator ==(TypeSymbol? a, TypeSymbol? b) => a?.Equals(b) ?? Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null);
        public static bool operator !=(TypeSymbol? a, TypeSymbol? b) => !(a == b);

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}