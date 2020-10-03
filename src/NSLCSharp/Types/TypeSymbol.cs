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
        public TypeSymbol NotConstexpr() => this is ConstexprTypeSymbol constexpr ? constexpr.Base : this;

        public virtual IValue Instantiate(object value) => new SimpleValue(value, this);

        override public bool Equals(object obj)
        {
            var one = this;
            if (obj is TypeSymbol other)
            {
                if (one is ConstexprTypeSymbol constOne && other is ConstexprTypeSymbol constOther)
                {
                    return constOne.ToString() == constOther.ToString();
                }
                else
                {
                    if (one is ConstexprTypeSymbol oneConst) one = oneConst.Base;
                    if (other is ConstexprTypeSymbol otherConst) one = otherConst.Base;

                    return one.ToString() == other.ToString();
                }
            }
            else return false;
        }

        public static bool operator ==(TypeSymbol a, TypeSymbol b) => a?.Equals(b) ?? Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null);
        public static bool operator !=(TypeSymbol a, TypeSymbol b) => !(a == b);

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static readonly TypeSymbol typeSymbol = new TypeSymbol("Type");
    }
}