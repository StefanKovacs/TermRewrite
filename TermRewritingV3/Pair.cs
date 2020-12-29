namespace TermRewritingV3
{
    public class Pair
    {
        public Term Left { get; internal set; }
        public Term Right { get; internal set; }
        public void Deconstruct(out Term left, out Term right)
        {
            left = Left;
            right = Right;
        }

        public Pair() { }

        public Pair(Term left, Term right)
        {
            Left = left;
            Right = right;
        }
        public override string ToString()
         => $"{Left.Display()} {Symbol} {Right.ToString()}";

        protected virtual string Symbol => ",";
    }

    internal class Unifier : Pair
    {
        public Unifier() : base() { }
        public Unifier(Term left, Term right) : base(left, right) { }
        protected override string Symbol => "→";
    }

    internal class Identity : Pair
    {
        public Identity() : base() { }
        public Identity(Term left, Term right) : base(left, right) { }
        protected override string Symbol => "≈";
    }
}
