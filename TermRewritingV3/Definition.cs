namespace TermRewritingV3
{
    public class Definition
    {
        private static int _functionIndex = 0;
        private static int _variableIndex = 0;

        public int Order { get; }
        public string Name { get; private set; }
        public uint Arity { get; }
        public TermType Type { get; }

        public bool IsVariable => Type == TermType.Variable;

        private Definition(string name, uint arity, TermType type)
        {
            Name = name;
            Arity = arity;
            Type = type == TermType.Function && arity == 0 ? TermType.Constant : type;
            Order = type == TermType.Function ? _functionIndex++ : _variableIndex++;
        }

        public override string ToString()
            => $"[{Type}] {Name} / {Arity}";

        public static Definition Function(string name, uint arity)
            => new Definition(name, arity, TermType.Function);

        public static Definition Variable(string name)
            => new Definition(name, 0, TermType.Variable);
    }

    public enum TermType
    {
        Function,
        Constant,
        Variable
    }
}
