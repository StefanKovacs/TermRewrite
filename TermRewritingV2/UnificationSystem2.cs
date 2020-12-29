using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV2
{
    public class UnificationSystem2
    {
        private static string ResultSeparator =
            Environment.NewLine + Environment.NewLine +
            string.Concat(Enumerable.Repeat("-", 55)) +
            Environment.NewLine + Environment.NewLine;

        private static readonly List<string> VariableNames = new List<string> { "x", "y", "z", "t", "u", "v", "w" };
        private readonly IReadOnlyList<Definition> _signature;
        private readonly List<Identity> _identities;
        private readonly List<List<Unifier>> _solution;
        private readonly List<string> _identifiedProblems = new List<string>();

        public string Solution => _solution.Count == 0 ? "No solution found" :
                string.Join(ResultSeparator, _solution.Select(sln => string.Join(Environment.NewLine, sln)));

        public UnificationSystem2(string[] signatures, string[] identities, string[] terms)
        {
            _signature = signatures
                .Select(x => x.Split('/'))
                .Select(x => Definition.Function(x[0], uint.Parse(x[1])))
                .ToList();

            _identities = identities.Select(Parse<Identity>).Distinct().ToList();
            var problem = terms.Select(Parse<Pair>).Distinct().ToList();
            //Solve2(_problem);
            _solution = new List<List<Unifier>> {Solve2(problem)};
        }

        private List<Unifier> Solve2(List<Pair> originalProblem)
        {
            var problems = new List<Problem>
            {
                new Problem(originalProblem)
            };

            do
            {
                foreach (var problem in problems.Where(x => !x.IsSolvedForm).ToList())
                {
                    if (problem.Equations.Any(eq => OccursCheck(eq.Left, eq.Right) || Clash(eq.Left, eq.Right, true)))
                    {
                        problems.Remove(problem);
                        continue;
                    }

                    var unOrientedPairs = problem.Equations.Where(x => x.Right.IsVariable && !x.Left.IsVariable).ToList();

                    foreach (var unOrientedPair in unOrientedPairs)
                    {
                        problem.Equations.Remove(unOrientedPair);
                        problem.Equations.Add(new Pair(unOrientedPair.Right, unOrientedPair.Left));
                    }

                    var eliminations = problem.Equations.Where(x => x.Left.IsVariable && !x.Right.IsVariable).ToList();

                    var altered = false;

                    foreach (var elimination in eliminations)
                    {
                        var (left, right) = elimination;
                        foreach (var (l, r) in problem.Equations.Where(eq => eq != elimination))
                        {
                            var lSub = l.Substitute(left.RootSymbol, right);
                            var rSub = r.Substitute(left.RootSymbol, right);
                            altered = altered || lSub || rSub;
                        }
                    }

                    if (altered)
                        break;

                    var dec = false;

                    foreach (var equation in problem.Equations.ToList())
                    {
                        var variants = MatchWithIdentities(equation);

                        if (variants.Any(v => v.Left == v.Right))
                        {
                            problem.Equations.Remove(equation);
                        }

                        var decomposed = variants
                            .Select(Decompose)
                            .Where(x => x.Count > 0)
                            .ToList();

                        if (decomposed.Count >= 1)
                        {
                            dec = true;
                            var subProblems = decomposed.Select(d => problem.Equations.Except(new[] { equation }).Concat(d).ToList())
                                .Where(FirstTimeEncountered)
                                .Select(x => new Problem(x))
                                .ToList();

                            problems.Remove(problem);
                            problems.AddRange(subProblems);
                            break;
                        }
                    }

                    if (problem.IsSolvedForm)
                        return problem.Equations.Select(eq => new Unifier(eq.Left, eq.Right)).ToList();

                    if (dec)
                        break;

                    if (!problem.IsSolvedForm && problem.Equations.Any(eq => Clash(eq.Left, eq.Right)))
                        problems.Remove(problem);
                }
            } while (problems.Any());

            return new List<Unifier>();
        }

        private List<List<Unifier>> Solve(List<Pair> problem)
        {
            var result = new List<List<Unifier>>();

            foreach (var equation in problem)
            {
                var decomposed = MatchWithIdentities(equation)
                                .Select(Decompose)
                                .Where(x => x.Count > 0)
                                .ToList();

                var subProblems = decomposed.Select(d => problem.Except(new[] { equation }).Concat(d).ToList())
                                            .Where(FirstTimeEncountered);

                var subSolutions = subProblems.SelectMany(Solve).ToList();
                result.AddRange(subSolutions);
            }

            if (Unify(problem, out var solution))
                result.Add(solution);

            return result;
        }

        private bool FirstTimeEncountered(List<Pair> problem)
        {
            var rep = string.Join(", ", problem.Select(x => x.ToString()).OrderBy(x => x));

            if (_identifiedProblems.Contains(rep))
                return false;

            _identifiedProblems.Add(rep);
            return true;
        }

        private List<Pair> Decompose(Pair pair)
        {
            var result = new List<Pair>();

            var (left, right) = pair;

            if (!left.IsVariable && !right.IsVariable)
            {
                if (left.Definition == right.Definition)
                {
                    var decomposed = left.Children
                        .Zip(right.Children, (l, r) => r.IsVariable && !l.IsVariable ? new Pair(r, l) : new Pair(l, r))
                        .Where(x => x.Left != x.Right)
                        .ToList();

                    result.AddRange(decomposed);
                }

                else if (left.Children.Count > 0 && right.Children.Count > 0)
                    result.Add(pair);
            }

            return result;
        }

        private List<Pair> MatchWithIdentities(Pair pair)
        {
            var (left, right) = pair;

            var leftInstances = new List<Term> { Term.Clone(left) };
            var rightInstances = new List<Term> { Term.Clone(right) };

            foreach (var identity in _identities)
            {
                if (Match(left, identity.Left, out var subs))
                    leftInstances.Add(identity.Right.Substitute(subs));
                if (Match(left, identity.Right, out subs))
                    leftInstances.Add(identity.Left.Substitute(subs));

                if (Match(right, identity.Left, out subs))
                    rightInstances.Add(identity.Right.Substitute(subs));
                if (Match(right, identity.Right, out subs))
                    rightInstances.Add(identity.Left.Substitute(subs));
            }

            var result = leftInstances.SelectMany(l => rightInstances.Select(r => new Pair(l, r))).ToList();

            return result.GroupBy(x => x.ToString()).Select(x => x.First()).ToList();
        }

        private bool Match(Term term1, Term term2, out List<Pair> results)
        {
            results = new List<Pair>();

            var terms = DistinctVariables(new[] { Term.Clone(term1), Term.Clone(term2) }, out var map);
            var target = terms.First();
            var pattern = terms.Last();

            var changedPositions = new List<string>();
            var done = true;
            do
            {
                foreach (var pos in new Dictionary<string, Term>(pattern.Positions))
                {
                    var targetPos = target[pos.Key];

                    if (targetPos.IsVariable && !pos.Value.IsVariable)
                        return false;

                    if (!targetPos.IsVariable && !pos.Value.IsVariable)
                    {
                        if (targetPos.Definition != pos.Value.Definition)
                            return false;
                        continue;
                    }

                    if (targetPos.IsVariable && pos.Value.IsVariable && targetPos.RootSymbol == pos.Value.RootSymbol)
                    {
                        continue;
                    }

                    if (!changedPositions.Contains(pos.Key))
                    {
                        var left = pos.Value.Substitute(map[1]);
                        var right = targetPos.Substitute(map[0]);
                        results.Add(new Pair(left, right));
                        pattern.Substitute(pos.Value.RootSymbol, targetPos);
                        changedPositions.Add(pos.Key);
                        break;
                    }

                    return false;
                }
                done = target == pattern;
            } while (!done);

            return true;
        }

        private bool Unify(List<Pair> problem, out List<Unifier> solution)
        {
            solution = null;

            var change = false;
            var pairs = problem.Select(u => new Unifier(Term.Clone(u.Left), Term.Clone(u.Right))).ToList();

            do
            {
                change = false;
                foreach (var pair in pairs.ToList())
                {
                    var (left, right) = pair;

                    if (Clash(left, right) || OccursCheck(left, right))
                        return false;

                    // Orient
                    if (!left.IsVariable && right.IsVariable)
                    {
                        pairs.Remove(pair);
                        pairs.Add(new Unifier(right, left));
                        change = true;
                        break;
                    }

                    if (!left.IsVariable && !right.IsVariable)
                    {
                        // Decompose
                        var add = left.Children
                            .Zip(right.Children, (l, r) => r.IsVariable && !l.IsVariable ? new Unifier(r, l) : new Unifier(l, r))
                            .Where(x => x.Left != x.Right);
                        pairs.AddRange(add);

                        pairs.Remove(pair);

                        change = true;
                        break;
                    }

                    // Eliminate
                    var sub = false;
                    foreach (var (l, r) in pairs.Where(p => p != pair))
                    {
                        var lSub = l.Substitute(left.RootSymbol, right);
                        var rSub = r.Substitute(left.RootSymbol, right);
                        sub = sub || lSub || rSub;
                    }
                    if (sub)
                    {
                        change = true;
                        break;
                    }
                }

            } while (change);

            solution = pairs;
            return true;
        }

        private static bool OccursCheck(Term left, Term right)
        {
            return left.Variables.Any(v => v.Name == right.RootSymbol)
                || right.Variables.Any(v => v.Name == left.RootSymbol);
        }

        private static bool Clash(Term left, Term right, bool ignoreFunctions = false)
        {
            if (!ignoreFunctions)
                return !left.IsVariable && !right.IsVariable && left.RootSymbol != right.RootSymbol;

            return !left.IsVariable && !right.IsVariable && (!left.Children.Any() || !right.Children.Any());
        }

        private List<Term> DistinctVariables(Term[] terms, out List<List<Pair>> map)
        {
            var clones = terms.Select(NormalizeVariables).ToList();
            for (var i = 0; i < clones.Count; i++)
            {
                SuffixVariables(clones[i], i.ToString());
            }
            map = clones.Select((t, i) => t.Positions.Where(x => x.Value.IsVariable)
                .GroupBy(x => x.Value.RootSymbol)
                .Select(x => x.First())
                .Select(x => new Pair(Term.Parse(_signature, x.Value.RootSymbol), Term.Parse(_signature, terms[i][x.Key].RootSymbol))).ToList())
                .ToList();
            return clones;
        }

        private Term NormalizeVariables(Term term)
        {
            var clone = Term.Clone(term);
            foreach (var var in clone.Variables)
            {
                clone.Substitute(var.Name, Term.Parse(_signature, var.Name + Guid.NewGuid()));
            }

            var existing = clone.Variables;
            for (var i = 0; i < existing.Count; i++)
            {
                clone.Substitute(existing[i].Name, Term.Parse(_signature, VariableNames[i]));
            }
            return clone;
        }

        private void SuffixVariables(Term term, string suffix)
        {
            foreach (var var in term.Variables)
            {
                term.Substitute(var.Name, Term.Parse(_signature, var.Name + suffix));
            }
        }

        private T Parse<T>(string input) where T : Pair, new()
        {
            if (string.IsNullOrEmpty(input) || input.Count(x => x == '=') != 1)
                throw new ArgumentException("Invalid input");

            var split = input.Split('=');
            var terms = Term.Parse(_signature, split);

            return new T
            {
                Left = terms.First(),
                Right = terms.Last()
            };
        }
    }

    internal class Problem
    {
        public Problem(IEnumerable<Pair> equations)
        {
            Equations = equations.ToList();
        }
        public List<Pair> Equations { get;}

        public bool IsSolvedForm => Equations.All(eq =>
            eq.Left.IsVariable && Equations.All(r => eq.Left.RootSymbol != r.Right.RootSymbol));
    }
}
