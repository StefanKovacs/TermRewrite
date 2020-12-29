using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV3
{
    public class UnificationSystem3
    {
        private readonly IReadOnlyList<Definition> _signature;
        private readonly IReadOnlyList<Identity> _identities;

        private readonly List<Unifier> _solution;
        private bool _success;

        public string Solution => _solution.Count > 0 ? string.Join(Environment.NewLine, _solution)
                                           : _success ? "Unification Successful, but not variables" : "No solution found";


        public UnificationSystem3(string[] signatures, string[] identities, string[] terms)
        {
            _signature = signatures
                .Select(x => x.Split('/'))
                .Select(x => Definition.Function(x[0], uint.Parse(x[1])))
                .ToList();

            _identities = identities.Select(Parse<Identity>).Distinct().ToList();
            _solution = Solve(terms);
        }

        private List<Unifier> Solve(string[] input)
        {
            var problems = new List<Problem> { new Problem(input, _signature) };
            Problem solved = null;

            do
            {
                foreach (var problem in problems.ToList())
                {
                    _success = problem.Unify(_identities);

                    if (_success)
                    {
                        solved = problem;
                        break;
                    }
                    FindAlternativeForms(problem.Equations[0]);

                }
            } while (!_success);

            if (solved is null)
                return new List<Unifier>();

            return solved.Context.AssignedVariables.Distinct().Select(v => new Unifier(v, v.Value)).ToList();
        }

        private List<Pair> FindAlternativeForms(Pair pair)
        {
            var (left, right) = pair;


            foreach (var id in _identities)
            {
                if(Match(left, id.Left, out var t, out var p))
                {
                    return null;
                }
            }

            return null;
        }

        private bool Match(Term term, Term pattern, out List<Term> termVariables, out List<Term> patternVariables)
        {
            var initialTerm = term.Clone();
            var initialPattern = pattern.Clone();
            termVariables = patternVariables = null;

            var pairs = new List<(Term, Term)> { (initialTerm, initialPattern) };

            var done = false;
            do
            {
                foreach (var pair in pairs.ToList())
                {
                    var (left, right) = pair;
                    if (!left.Value.IsVariable && !right.Value.IsVariable)
                    {
                        if (left.Value.Definition != right.Value.Definition
                            || left.Value.Subterms.Count != right.Value.Subterms.Count)
                            return false;

                        pairs.AddRange(left.Value.Subterms.Select((s, i) => (s, right.Value.Subterms[i])));

                        pairs.Remove(pair);
                        continue;
                    }

                    if (!left.Value.IsVariable && right.Value.IsVariable)
                    {
                        //right.Union(left.Value, true);
                        continue;
                    }
                    if (!right.Value.IsVariable && left.Value.IsVariable)
                    {
                        //left.Union(right.Value, true);
                        continue;
                    }

                    return false;

                }
                done = AreMatched(initialTerm, initialPattern);
            } while (!done);
            termVariables = initialTerm.Variables(true).ToList();
            patternVariables = initialPattern.Variables(true).ToList();
            return true;
        }

        private bool AreMatched(Term left, Term right)
        {
            if (!left.IsVariable && !right.Value.IsVariable && left.Definition != right.Value.Definition)
                return false;

            if (left.Value.Subterms.Count != right.Value.Subterms.Count)
                return false;

            if (left.Value.Subterms.Count > 0)
                return left.Value.Subterms.Select((s, i) => AreMatched(s, right.Value.Subterms[i])).All(x => x);

            return true;
        }

        private T Parse<T>(string input) where T : Pair, new()
        {
            if (string.IsNullOrEmpty(input) || input.Count(x => x == '=') != 1)
                throw new ArgumentException("Invalid input");

            var split = input.Split('=');
            var terms = Term.Parse(split, _signature);

            return new T
            {
                Left = terms.First(),
                Right = terms.Last()
            };
        }

        private static readonly string ResultSeparator =
            Environment.NewLine + Environment.NewLine +
            string.Concat(Enumerable.Repeat("-", 55)) +
            Environment.NewLine + Environment.NewLine;


        private static bool OccursCheck(Term left, Term right)
        {
            return left.Variables().Contains(right)
                || right.Variables().Contains(left);
        }

        private static bool Clash(Term left, Term right, bool ignoreFunctions = false)
        {
            if (!ignoreFunctions)
                return !left.Value.Definition.IsVariable && !right.Value.Definition.IsVariable
                    && left.Value.Definition.Name != right.Value.Definition.Name;

            return !left.Value.Definition.IsVariable && !right.Value.Definition.IsVariable
                && (!left.Value.Subterms.Any() || !right.Value.Subterms.Any());
        }
    }
}
