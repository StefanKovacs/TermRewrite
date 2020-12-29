using System.Collections.Generic;

namespace TermRewritingV2
{
    public  class Pair
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
         => $"{Left.ToString()} {Symbol} {Right.ToString()}";

        protected virtual string Symbol => ",";

        public static bool operator ==(Pair p1, Pair p2)
        {
            if (ReferenceEquals(p1, p2))
                return true;

            if (p1 is null || p2 is null)
                return false;

            return p1.Equals(p2);
        }

        public static bool operator !=(Pair left, Pair right)
            => !(left == right);
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

    internal class Rule : Pair
    {
        public Rule(Term left, Term right) : base(left, right) { }
        protected override string Symbol => "→";
        public bool IsMarked { get; set; }
        public override string ToString()
            => $"{base.ToString()} {(IsMarked ? "*" : "")}";

    }
}
