namespace NSL
{
    public partial class FunctionRegistry
    {
        private static void RegisterOperator(FunctionRegistry registry)
        {
            registry.AddOperator("_->_", "index", -2);

            registry.AddOperator("_++", "incPost", -1);
            registry.AddOperator("_--", "decPost", -1);
            registry.AddOperator("_~~", "invPost", -1);
            registry.AddOperator("++_", "incPrev", -1);
            registry.AddOperator("--_", "decPrev", -1);
            registry.AddOperator("~~_", "invPrev", -1);

            registry.AddOperator("!_", "not", 0);
            registry.AddOperator("~_", "not", 0);

            registry.AddOperator("_**_", "pow", 1);

            registry.AddOperator("_*_", "mul", 1);
            registry.AddOperator("_/_", "div", 1);
            registry.AddOperator("_%_", "mod", 1);

            registry.AddOperator("_+_", "add", 4);
            registry.AddOperator("_-_", "sub", 4);

            registry.AddOperator("_<<_", "shl", 5);
            registry.AddOperator("_>>_", "shr", 5);
            registry.AddOperator("-_", "neg", 5);

            registry.AddOperator("_>_", "gt", 6);
            registry.AddOperator("_<_", "lt", 6);
            registry.AddOperator("_>=_", "gte", 6);
            registry.AddOperator("_<=_", "lte", 6);

            registry.AddOperator("_==_", "eq", 7);
            registry.AddOperator("_!=_", "neq", 7);

            registry.AddOperator("_&_", "and", 8);

            registry.AddOperator("_^_", "xor", 9);

            // Skip bitwise or because it collides with pipe, you can just use logical or

            registry.AddOperator("_&&_", "and", 11);

            registry.AddOperator("_||_", "or", 12);

            registry.AddOperator("_=_", "set", 13);
            registry.AddOperator("_+=_", "addSet", 13);
            registry.AddOperator("_-=_", "subSet", 13);
            registry.AddOperator("_*=_", "mulSet", 13);
            registry.AddOperator("_/=_", "divSet", 13);
            registry.AddOperator("_**=_", "powSet", 13);
            registry.AddOperator("_%=_", "modSet", 13);
            registry.AddOperator("_&=_", "andSet", 13);
            registry.AddOperator("_|=_", "orSet", 13);
            registry.AddOperator("_^=_", "xorSet", 13);
            registry.AddOperator("_<<=_", "shlSet", 13);
            registry.AddOperator("_>>=_", "shrSet", 13);
            registry.AddOperator("_~>_", "set", 13, reverse: true);
        }
    }
}