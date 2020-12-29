using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV3
{
    public class Context : Dictionary<string, Term>
    {
        public ICollection<Term> AssignedVariables { get; } = new List<Term>();

        public void UpdateIndexes()
        {
            foreach (var element in this.ToList())
            {
                var representation = element.Value.ToString();

                // element was updated
                if (element.Key != representation)
                {
                    Remove(element.Key);

                    if (element.Value.IsVariable && element.Value.IsVariable)
                        AssignedVariables.Add(element.Value);

                    if (!ContainsKey(representation))
                        Add(representation, element.Value);
                }
            }
        }
    }

    public class Problem
    {
        private readonly IReadOnlyList<Definition> _signature;

        public Context Context { get; } = new Context();
        public List<Pair> Equations { get; }

        public Problem(string[] input, IReadOnlyList<Definition> signature)
        {
            _signature = signature;
            Equations = Parse<Pair>(input);
        }

        private Problem(ICollection<Pair> equations, IReadOnlyList<Definition> signature)
        {
            Equations = equations.ToList();
            _signature = signature;
        }

        public bool Unify(IReadOnlyList<Pair> identities)
        {
            var problems = new List<Problem> { this };
            var processed = new List<string> { string.Join("|", Equations.Select(x => x.ToString()).OrderBy(x => x)) };
            var done = false;
            do
            {
                foreach (var problem in problems.ToList())
                {
                    foreach (var equation in problem.Equations.ToList())
                    {
                        if (!Unify(equation.Left, equation.Right, identities, out var alternatives))
                        {
                            problems.Remove(problem);
                            if (alternatives?.Count == 1)
                            {
                                done = true;
                                break;
                            }
                            foreach(var alt in alternatives)
                            {
                                var newEq = problem.Equations.Except(new[] { equation }).Append(alt).ToList();
                                var rep = string.Join("|", newEq.Select(x => x.ToString()).OrderBy(x => x));
                                if (processed.Contains(rep))
                                    continue;

                                problems.Add(new Problem(newEq, _signature));
                                processed.Add(rep);
                            }
                        }
                        else
                        {
                            // JACKPOT?
                        }
                    }
                }
                //foreach (var equation in Equations.ToList())
                //{
                //    if (!Unify(equation.Left, equation.Right, identities, out var alternatives) && alternatives != null)
                //    {
                //        Equations.Remove(equation);
                //        // these are new problems ... Equations.AddRange(alternatives);
                //    }
                //}
                //done = Equations.All()
            } while (!done);
            return false;
        }

        public bool Unify()
        {
            return Equations.All(x => Unify(x.Left, x.Right));
        }

        private bool Unify(Term left, Term right, IReadOnlyList<Pair> identities, out List<Pair> alternatives)
        {
            alternatives = null;
            if (Unify(left, right))
                return true;

            var ctx = new Context();
            var (initLeft, initRight) = (left.Clone(ctx), right.Clone(ctx));

            left.Reset();
            right.Reset();

            var lefts = new List<Term>() { left.Clone() };
            var rights = new List<Term>() { right.Clone() };

            foreach (var id in identities)
            {
                if (Unify(left, id.Left))
                {
                    lefts.Add(id.Right.Clone());
                }
                left.Reset();
                id.Left.Reset();

                if (Unify(right, id.Left))
                {
                    rights.Add(id.Right.Clone());
                }
                right.Reset();
                id.Left.Reset();

                if (Unify(left, id.Right))
                {
                    lefts.Add(id.Left.Clone());
                }
                left.Reset();
                id.Right.Reset();

                if (Unify(right, id.Right))
                {
                    rights.Add(id.Left.Clone());
                }
                right.Reset();
                id.Right.Reset();
            }

            alternatives = lefts.SelectMany(l => rights.Select(r =>
            {
                var c = new Context();
                return new Pair(l.Clone(c), r.Clone(c));
            })).ToList();

            return false;
        }

        private bool Unify(Term left, Term right)
        {
            var t1 = left.Value;
            var t2 = right.Value;

            if (t1.IsVariable && t2.IsVariable)
            {
                if (string.CompareOrdinal(t1.Definition.Name, t2.Definition.Name) > 0)
                    Substitute(t1, t2);
                else
                    Substitute(t2, t1);
                return true;
            }

            if (t1.IsVariable && !t2.IsVariable)
            {
                if (t2.Variables().Contains(t1))
                    return false;

                Substitute(t1, t2);
                return true;
            }

            if (!t1.IsVariable && t2.IsVariable)
            {
                return Unify(t2, t1);
            }

            if (t1.Definition == t2.Definition && t1.Subterms.Count == t2.Subterms.Count)
                return t1.Subterms.Select((st, index) => Unify(st, t2.Subterms[index])).All(x => x); // All = true for empty list

            return false;
        }

        public bool Substitute(Term target, Term substitution)
        {
            if (!target.Definition.IsVariable || target.IsAssignedVariable)
                return false;

            target.Union(substitution, true);

            return true;
        }

        private List<T> Parse<T>(string[] input) where T : Pair, new()
        {
            return input.Select(Parse<T>).ToList();
        }

        private T Parse<T>(string input) where T : Pair, new()
        {
            if (string.IsNullOrEmpty(input) || input.Count(x => x == '=') != 1)
                throw new ArgumentException("Invalid input");

            var split = input.Split('=');
            var terms = Term.Parse(split, _signature, Context);

            return new T
            {
                Left = terms.First(),
                Right = terms.Last()
            };
        }
    }

}
