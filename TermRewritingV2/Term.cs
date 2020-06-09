using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV2
{
    public class Term
    {
        private static int counter = 0;
        private readonly int id = counter++;
        private Term _parent;
        private IReadOnlyCollection<Definition> _signatures;

        public List<Term> Children { get; private set; }

        public Definition Definition { get; private set; }

        public Term this[string pos] => Position(pos);

        public int Size => Positions().Count;

        public bool IsVariable => Definition.IsVariable;

        public void Replace(string position, Term term)
        {
            if (term == null)
                throw new Exception("Cannot replace with empty term");
            var target = this[position];
            target.AssignFrom(term);
        }

        public bool Substitute(string variableName, Term term)
        {
            if (string.IsNullOrWhiteSpace(variableName))
                throw new Exception("Invalid substitution input");

            var target = Root(this).Variables.FirstOrDefault(v => v.Name == variableName);
            if (target == null)
                return false;
            var targetPositions = Positions().Values.Where(x => x.Definition.Name == target.Name);

            foreach (var position in targetPositions)
            {
                position.AssignFrom(term);
            }

            return targetPositions.Any();
        }

        public Dictionary<string, Term> Positions()
            => PositionsInner().ToDictionary(x => x.Item1, x => x.Item2);

        public override string ToString()
            => $"{Definition.Name}{(Children.Count > 0 ? $"({string.Join(", ", Children.Select(x => x.ToString()))})" : string.Empty)}";

        public string Representation(List<Definition> context = null)
        {
            context = context ?? new List<Definition>();

            if (IsVariable)
            {
                var index = context.IndexOf(Definition) + 1;
                if (index == 0)
                {
                    context.Add(Definition);
                    index = context.Count;
                }

                return new string(Enumerable.Repeat('#', index).ToArray());
            }

            return $"{Definition.Name}{(Children.Count > 0 ? $"({string.Join(", ", Children.Select(x => x.Representation(context)))}" : string.Empty)})";
        }

        private void AssignFrom(Term term)
        {
            var newTerm = Clone(term);

            Children.Clear();
            Children.AddRange(newTerm.Children);
            Children.ForEach(c => c._parent = this);
            Definition = newTerm.Definition;
        }

        public List<Definition> Variables
            => Flatten(this)
                .Where(x => x.Definition.Type == TermType.Variable)
                .Select(x => x.Definition)
                .GroupBy(x => x.Name)
                .Select(g => g.First())
                .ToList();

        private static Term Root(Term child)
            => child._parent == null ? child : Root(child._parent);

        private static List<Term> Flatten(Term t)
            => new[] { t }.Concat(t.Children.SelectMany(Flatten)).ToList();

        private Term Position(string position)
        {
            if (string.IsNullOrEmpty(position))
                return this;

            var index = int.Parse(position[0].ToString()) - 1;
            if (index >= Children.Count)
                throw new Exception($"Invalid Position {position[0]} for {ToString()}");

            return Children[index].Position(position.Substring(1, position.Length - 1));
        }

        private ICollection<(string, Term)> PositionsInner(string start = "")
            => new[] { (start, this) }.Concat(Children.SelectMany((c, i) => c.PositionsInner($"{start}{i + 1}"))).ToList();

        public static Term Clone(Term other)
            => Parse(other._signatures, other.ToString());

        public static Term Parse(IReadOnlyCollection<Definition> signature, string input)
        {
            return Parse(signature, new[] { input }).FirstOrDefault();
        }

        public static List<Term> Parse(IReadOnlyCollection<Definition> signature, params string[] inputs)
        {
            var context = new List<Definition>();
            return inputs.Select(x => Parse(x, signature, context)).ToList();
        }

        private static Term Parse(string input, IReadOnlyCollection<Definition> signature, ICollection<Definition> variables)
        {
            var current = new Builder(null);
            var root = current;
            var trimmed = input.Replace(" ", string.Empty);

            foreach (var c in trimmed)
            {
                switch (c)
                {
                    case '(':
                        current = new Builder(current);
                        current.Parent?.Children.Add(current);
                        break;
                    case ',':
                        current = new Builder(current.Parent);
                        current.Parent.Children.Add(current);
                        break;
                    case ')':
                        current = current.Parent;
                        break;
                    default:
                        current.Name += c;
                        break;
                }
            }

            return root.Build(signature, variables);
        }

        private class Builder
        {
            public string Name { get; set; }
            public Builder Parent { get; }
            public List<Builder> Children { get; } = new List<Builder>();

            public Builder(Builder parent)
            {
                Parent = parent;
            }

            public override string ToString()
                => $"{Name} ({Children.Count})";

            public Term Build(IReadOnlyCollection<Definition> definitions, ICollection<Definition> variables, Term parent = null)
            {
                if (!TryGetOrAddDefinition(definitions, variables, out var definition))
                    throw new ArgumentException($"Invalid parameters for {Name}");

                var term = new Term
                {
                    Definition = definition,
                    _signatures = definitions,
                    _parent = parent
                };
                term.Children = Children.Select(x => x.Build(definitions, variables, term)).ToList();

                if (definition.Arity != term.Children.Count)
                    throw new ArgumentException($"{term.Definition.Name}: Invalid number of children at position");

                return term;
            }

            private bool TryGetOrAddDefinition(IReadOnlyCollection<Definition> definitions, ICollection<Definition> variables, out Definition def)
            {
                def = definitions.FirstOrDefault(x => x.Name == Name);

                if (def != null)
                    return true;

                if (Children.Count > 0)
                    return false;

                def = variables.FirstOrDefault(x => x.Name == Name);

                if (def == null)
                {
                    def = Definition.Variable(Name);
                    variables.Add(def);
                }

                return true;
            }
        }

        public static bool operator >(Term first, Term second)
        {
            // LPO 1
            if (first.Variables.Any(v => v.Name == second.Definition.Name) && first != second)
                return true;

            // LPO 2
            if (first.Definition.Type != TermType.Variable && second.Definition.Type != TermType.Variable)
            {
                if (first.Definition.Name != second.Definition.Name)
                {
                    //LPO 2a / || for reflexive closure
                    if (first.Children.Any(x => x > second || x.Definition.Name == second.Definition.Name))
                        return true;

                    //LPO 2b (smaller order means larger)
                    if (first.Definition.Order < second.Definition.Order &&
                        second.Children.All(c => first > c))
                        return true;
                }
                // LPO 2c
                else if (first.Definition.Order == second.Definition.Order && second.Children.All(c => first > c))
                {
                    for (int i = 0; i < first.Children.Count; i++)
                    {
                        var fchild = first.Children[i];
                        var schild = second.Children[i];
                        if (fchild == schild)
                            continue;

                        return first.Children[i] > second.Children[i];
                    }
                }
            }
            return false;
        }

        public static bool operator <(Term t1, Term t2)
            => t2 > t1;
        public static bool operator ==(Term t1, Term t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;

            if (t1 is null || t2 is null)
                return false;

            return t1.ToString() == t2.ToString();
        }

        public static bool operator !=(Term t1, Term t2) => !(t1 == t2);

    }
}
