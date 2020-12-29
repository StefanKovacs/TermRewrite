using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV2
{
    public class UnificationSystem
    {
        private static string ResultSeparator =>
            Environment.NewLine + Environment.NewLine +
            string.Concat(Enumerable.Repeat("-", 55)) +
            Environment.NewLine + Environment.NewLine;

        private readonly IReadOnlyList<Definition> _signature;
        private readonly List<Identity> _identities = new List<Identity>();
        private readonly List<Pair> _problem = new List<Pair>();
        private readonly List<List<Unifier>> _solution;
        private static readonly List<string> _variableNames = new List<string> { "x", "y", "z", "t", "u", "v", "w" };

        public string Solution => _solution.Count == 0 ? "No solution found" :
                string.Join(ResultSeparator, _solution.Select(sln => string.Join(Environment.NewLine, sln)));

        public UnificationSystem(string[] signatures, string[] identities, string[] terms)
        {
            _signature = signatures
                .Select(x => x.Split('/'))
                .Select(x => Definition.Function(x[0], uint.Parse(x[1])))
                .ToList();

            _identities = identities.Select(Parse<Identity>).Distinct().ToList();
            _problem = terms.Select(Parse<Pair>).Distinct().ToList();
            _solution = UnifyGeneric();
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

        private List<List<Unifier>> UnifyGeneric()
        {
            var solutions = new List<List<Unifier>>();

            var problems = new List<List<Unifier>> { _problem.Select(p => new Unifier(Term.Clone(p.Left), Term.Clone(p.Right))).ToList() };

            var change = false;

            do
            {
                change = false;

                var pairProblems = problems.SelectMany(pb => pb.Select(pair => (pb, pair))).ToList();

                foreach (var (problem, pair) in pairProblems)
                {
                    var (left, right) = pair;

                    if (OccursCheck(left, right) || Clash(left, right))
                    {
                        problems.Remove(problem);
                        change = true;
                        break;
                    }

                    if (GenericDecompose(pair, problem, problems))
                    {
                        change = true;
                        break;
                    }

                    if (Unify(problem, out var solution))
                    {
                        problems.Remove(problem);
                        solutions.Add(solution);
                        change = true;
                        break;
                    }
                }

            } while (change);

            solutions = solutions.Select(sol => sol.OrderBy(x => x.Left.ToString()).ToList())
                                 .GroupBy(x => string.Concat(x.Select(v => v.ToString())))
                                 .Select(x => x.First())
                                 .ToList();

            return solutions;
        }

        private bool GenericDecompose(Unifier pair, List<Unifier> problem, List<List<Unifier>> problems)
        {
            var (left, right) = pair;

            if (left.IsVariable || right.IsVariable)
                return false;

            var prob = MatchIdentities(pair);

            if (prob.Count > 1)
            {
                problem.Remove(pair);
                var decomposed = Decompose(prob);
                problems.Remove(problem);

                decomposed.ForEach(dec => dec.AddRange(problem));

                problems.AddRange(decomposed);

                return true;
            }
            return false;
        }

        private List<List<Unifier>> Decompose(List<Unifier> problem)
        {
            var result = new List<List<Unifier>>();

            foreach (var subProblem in problem)
            {
                var (left, right) = subProblem;

                if (left.Definition == right.Definition && !left.IsVariable)
                {
                    var decomposed = left.Children
                        .Zip(right.Children, (l, r) => r.IsVariable && !l.IsVariable ? new Unifier(r, l) : new Unifier(l, r))
                        .Where(x => x.Left != x.Right)
                        .ToList();

                    result.Add(decomposed);
                }
            }

            return result;
        }

        private List<Unifier> MatchIdentities(Unifier pair)
        {
            var result = new List<Unifier>();
            var (left, right) = pair;

            foreach (var identity in _identities)
            {
                var leftInstances = new List<Term> { Term.Clone(left) };
                if (Match(left, identity.Left, out var subs))
                    leftInstances.Add(PerformSubstitutions(identity.Right, subs));
                if (Match(left, identity.Right, out subs))
                    leftInstances.Add(PerformSubstitutions(identity.Left, subs));

                var rightInstances = new List<Term> { Term.Clone(right) };
                if (Match(right, identity.Left, out subs))
                    rightInstances.Add(PerformSubstitutions(identity.Right, subs));
                if (Match(right, identity.Right, out subs))
                    rightInstances.Add(PerformSubstitutions(identity.Left, subs));

                result.AddRange(leftInstances.SelectMany(l => rightInstances.Select(r => new Unifier(l, r))));
            }

            return result.GroupBy(x => x.ToString()).Select(x => x.First()).ToList();
        }

        private List<List<Unifier>> C_Unify()
        {
            var solutions = new List<List<Unifier>>();

            var problems = new List<List<Unifier>> { _problem.Select(p => new Unifier(Term.Clone(p.Left), Term.Clone(p.Right))).ToList() };

            var change = false;

            do
            {
                change = false;

                var pairProblems = problems.SelectMany(pb => pb.Select(pair => (pb, pair))).ToList();

                foreach (var (problem, pair) in pairProblems)
                {
                    var (left, right) = pair;

                    // C-Decompose
                    if (C_Decompose(pair, problem, problems))
                    {
                        change = true;
                        break;
                    }

                    if (Unify(problem, out var solution))
                    {
                        problems.Remove(problem);
                        solutions.Add(solution);
                        change = true;
                        break;
                    }
                }

            } while (change);

            return solutions;
        }

        private bool C_Decompose(Unifier pair, List<Unifier> problem, List<List<Unifier>> problems)
        {
            var (left, right) = pair;

            if (left.Definition == right.Definition && !left.IsVariable && left.Definition.Arity > 1)
            {
                problems.Remove(problem);

                problem.Remove(pair);

                // decompose the problem and add the remaining equations (if any)
                var permutations = GetPermutations(left.Children.Count);

                var subProblems = permutations.Select(p => p.Select((lPos, rPos) => new Unifier(left.Children[lPos], right.Children[rPos])).ToList()).ToList();
                subProblems.ForEach(sp => sp.AddRange(problem));

                problems.AddRange(subProblems);

                return true;
            }

            return false;
        }

        private static ICollection<ICollection<int>> GetPermutations(int length, ICollection<int> list = null)
        {
            list = list ?? Enumerable.Range(0, length).ToArray();

            if (length == 1) return list.Select(t => new int[] { t }).ToArray();

            return GetPermutations(length - 1, list)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new int[] { t2 }).ToArray()).ToArray();
        }

        private bool Unify(List<Unifier> problem, out List<Unifier> solution)
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
                        // Clash
                        if (Clash(left, right))
                            return false;

                        // Decompose
                        var add = left.Children
                            .Zip(right.Children, (l, r) => r.IsVariable && !l.IsVariable ? new Unifier(r, l) : new Unifier(l, r))
                            .Where(x => x.Left != x.Right);
                        pairs.AddRange(add);

                        pairs.Remove(pair);

                        change = true;
                        break;
                    }

                    if (OccursCheck(left, right))
                        return false;

                    // Eliminate
                    var sub = false;
                    foreach (var (l, r) in pairs.Where(p => p != pair))
                    {
                        var lSub = l.Substitute(left.Definition.Name, right);
                        var rSub = r.Substitute(left.Definition.Name, right);
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
            return left.Variables.Any(v => v.Name == right.Definition.Name)
                || right.Variables.Any(v => v.Name == left.Definition.Name);
        }
   
        private static bool Clash(Term left, Term right)
        {
            return !left.IsVariable && !right.IsVariable && left.Definition.Name != right.Definition.Name;
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
                done = true;
                foreach (var pos in new Dictionary<string, Term>(pattern.Positions))
                {
                    var targetPos = target[pos.Key];

                    if (targetPos.IsVariable && !pos.Value.IsVariable)
                        return false;

                    if (!targetPos.IsVariable && !pos.Value.IsVariable)
                    {
                        if (targetPos.Definition != pos.Value.Definition)
                            return false;
                        else continue;
                    }

                    if (targetPos.IsVariable && pos.Value.IsVariable && targetPos.Definition.Name == pos.Value.Definition.Name)
                    {
                        continue;
                    }
                    if (!changedPositions.Contains(pos.Key))
                    {
                        var left = PerformSubstitutions(pos.Value, map[1]);
                        var right = PerformSubstitutions(targetPos, map[0]);
                        results.Add(new Pair(left, right));
                        pattern.Substitute(pos.Value.Definition.Name, targetPos);
                        changedPositions.Add(pos.Key);
                        done = false;
                        break;
                    }
                    else
                        return false;
                }
                done = target == pattern;
            } while (!done);
            return true;
        }

        private Term PerformSubstitutions(Term t, List<Pair> substitutions)
        {
            var result = Term.Clone(t);
            var targets = result.Positions.Where(x => x.Value.IsVariable)
                .Join(substitutions,
                    pos => pos.Value.Definition.Name,
                    sub => sub.Left.Definition.Name,
                    (pos, sub) => (pos, sub));

            foreach (var (pos, sub) in targets)
            {
                result.Replace(pos.Key, sub.Right);
            }

            return result;
        }

        private List<Term> DistinctVariables(Term[] terms, out List<List<Pair>> map)
        {
            var clones = terms.Select(NormalizeVariables).ToList();
            for (var i = 0; i < clones.Count; i++)
            {
                SuffixVariables(clones[i], i.ToString());
            }
            map = clones.Select((t, i) => t.Positions.Where(x => x.Value.IsVariable)
                .GroupBy(x => x.Value.Definition.Name)
                .Select(x => x.First())
                .Select(x => new Pair(Term.Parse(_signature, x.Value.Definition.Name), Term.Parse(_signature, terms[i][x.Key].Definition.Name))).ToList())
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
                clone.Substitute(existing[i].Name, Term.Parse(_signature, _variableNames[i]));
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
    }
}
