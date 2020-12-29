using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV3
{
    public class Term
    {
        private static int _counter = 0;

        private readonly int id = _counter++;

        private Term _variableValue;
        public bool IsAssignedVariable { get; private set; } = false;

        private Context _context { get; }
        private IReadOnlyCollection<Definition> _signatures;

        public Definition Definition { get; set; }
        public bool IsVariable => Definition.IsVariable;

        public Term Value => IsVariable && IsAssignedVariable ? _variableValue.Value : this;

        public List<Term> Subterms { get; private set; }

        public ICollection<Term> Variables(bool includeAssigned = false)
        {
            if (Definition.IsVariable)
                if (!IsAssignedVariable || includeAssigned)
                    return new[] { this };
                else return new List<Term>();

            return Subterms
                .SelectMany(t => t.Variables(includeAssigned))
                .Distinct()
                .ToList();
        }

        public void Union(Term with, bool skipContextValidation)
        {
            if (!IsVariable)
                throw new Exception("Illegal");

            if (!skipContextValidation && _context != with._context)
                throw new Exception("Different contexts!");

            if (this == with)
                return;

            if (with.Variables().Contains(this))
            {
                throw new Exception("Occours check");
                _context.Remove(ToString());
                var cloned = Clone(_context);

                Definition = with.Definition;
                Subterms = with.Subterms.Select(x => x == this ? cloned : x).ToList();
            }
            else
            {
                _variableValue = with;
                IsAssignedVariable = true;
            }
            _context.UpdateIndexes();
        }

        public void Reset()
        {
            if (IsAssignedVariable)
            {
                _variableValue = null;
                IsAssignedVariable = false;
            }

            foreach (var term in Subterms)
                term.Reset();
        }

        private Term(Context context)
        {
            _context = context;
        }
        
        public Term Clone(Context context = null)
        {
            context = context ?? new Context();

            if (context.TryGetValue(ToString(), out var result))
                return result;

            result = new Term(context)
            {
                Definition = Definition,
                Subterms = Subterms.Select(x => x.Value.Clone(context)).ToList(),
                _signatures = new List<Definition>(_signatures),
            };

            context.Add(ToString(), result);

            return result;
        }

        public override string ToString()
        {
            if (IsAssignedVariable)
                return _variableValue.ToString();

            var ch = Subterms.Count == 0 ? string.Empty :
                $"({string.Join(", ", Subterms.Select(x => x.Value.ToString()))})";

            return $"{Definition.Name}{ch}";
        }

        public string Display()
        {
            if (IsAssignedVariable)
                return Definition.Name;

            return ToString();
        }

        public static Term Parse(string input, IReadOnlyCollection<Definition> signature, Context context = null)
        {
            context = context ?? new Context();

            var current = new Builder(null);
            var root = current;
            var trimmed = input.Replace(" ", string.Empty);

            var builders = new List<Builder>();

            foreach (var c in trimmed)
            {
                switch (c)
                {
                    case '(':
                        current.AddToFormula(c);
                        current = new Builder(current);
                        builders.Add(current);
                        current.Parent?.Children.Add(current);
                        break;
                    case ',':
                        current.AddToFormula(c);
                        current = new Builder(current.Parent);
                        builders.Add(current);
                        current.Parent.Children.Add(current);
                        break;
                    case ')':
                        current = current.Parent;
                        current.AddToFormula(c);
                        break;
                    default:
                        current.AddToFormula(c);
                        current.Name += c;
                        break;
                }
            }

            foreach (var builder in builders)
                builder.Formula = builder.Formula.TrimEnd(',');

            return root.Build(signature, new List<Definition>(), context);
        }

        public static ICollection<Term> Parse(string[] inputs, IReadOnlyCollection<Definition> signature, Context context = null)
        {
            context = context ?? new Context();
            return inputs.Select(x => Parse(x, signature, context)).ToList();
        }

        private class Builder
        {
            public string Name { get; set; }
            public List<Builder> Children { get; } = new List<Builder>();
            public Builder Parent { get; }
            public string Formula { get; set; }

            public Builder(Builder parent)
            {
                Parent = parent;
            }

            public void AddToFormula(char c)
            {
                Formula += c;
                Parent?.AddToFormula(c);
            }

            public int Size => Children.Sum(c => c.Size) + 1;

            public Term Build(IReadOnlyCollection<Definition> definitions, ICollection<Definition> variables, Context context)
            {
                if (context.TryGetValue(Formula, out var term))
                    return term;

                if (!TryGetOrAddDefinition(definitions, variables, out var definition))
                    throw new ArgumentException($"Invalid parameters for {Name}");

                term = new Term(context)
                {
                    Definition = definition,
                    _signatures = definitions
                };

                term.Subterms = Children.Select(x => x.Build(definitions, variables, context)).ToList();

                if (definition.Arity != term.Subterms.Count)
                    throw new ArgumentException($"{term.Definition.Name}: Invalid number of children");

                context.Add(Formula, term);

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
    }
}
