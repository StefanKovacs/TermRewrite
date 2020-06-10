using System;
using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV2
{
    public class RewriteSystem : IRewriteSystem
    {
        private static List<string> _variableNames = new List<string> { "x", "y", "z", "t", "u", "v", "w" };
        public IReadOnlyList<Definition> Definitions { get; }
        private readonly List<Identity> _identities = new List<Identity>();
        private readonly List<Rule> _rules = new List<Rule>();
        public HistoryLog History { get; }
        public IReadOnlyCollection<string> Identities => _identities.Select(p => p.ToString()).ToList();
        public IReadOnlyCollection<string> Rules => _rules.Select(r => r.ToString()).ToList();

        private List<string> _usedCriticalPairs = new List<string>();

        public RewriteSystem(string definitions)
        {
            History = new HistoryLog(this);
            Definitions = definitions
                .Split(';')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Split('/'))
                .Select(x => Definition.Function(x[0], uint.Parse(x[1])))
                .ToList();

            Init712();
        }
        private void Init712()
        {
            AddIdentity("f(f(x,y),z) = f(x,f(y,z))");
            AddIdentity("f(x, i(x)) = e");
            AddIdentity("f(e, x) = x");
        }

        public void AddIdentity(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Count(x => x == '=') != 1)
                throw new ArgumentException("Invalid input");

            var split = input.Split('=');
            var terms = Term.Parse(Definitions, split);

            var pair = new Identity(terms.First(), terms.Last());

            if (pair.Left == pair.Right ||
                _identities.Any(x => x.Left == pair.Left && x.Right == pair.Right) ||
                _identities.Any(x => x.Right == pair.Left && x.Left == pair.Right))
                return;

            _identities.Add(pair);
        }

        public void CompleteNaive()
        {
            foreach (var rule in _rules)
            {
                rule.IsMarked = false;
            }

            while (_identities.Any() || _rules.Any(r => !r.IsMarked))
            {
                while (_identities.Any())
                {
                    var identity = _identities.OrderBy(id => id.Left.Size + id.Right.Size).First();
                    Log($"(a) Chose {identity}", identity.Left, identity.Right);

                    var (left, right) = identity;
                    var leftNormalForm = Reduce(left, _rules).First();
                    var rightNormalForm = Reduce(right, _rules).First();
                    Log($"(b) lhs reduced to {leftNormalForm}", left, leftNormalForm);
                    Log($"(b) rhs reduced to {rightNormalForm}", right, rightNormalForm);


                    if (leftNormalForm == rightNormalForm)
                    {
                        _identities.Remove(identity);
                        Log($"(c) normal forms are equal, removed identity", leftNormalForm, rightNormalForm);
                        continue;
                    }
                    var newRule = Compare(leftNormalForm, rightNormalForm);
                    if (newRule == null)
                    {
                        Log($"(d) FAIL! Cannot orient {leftNormalForm} and {rightNormalForm}", leftNormalForm, rightNormalForm);
                        throw new Exception($"Cannot orient {leftNormalForm} and {rightNormalForm}");
                    }
                    Log($"(e) Oriented {identity} to {newRule}", newRule.Left, newRule.Right);

                    foreach (var rule in _rules.ToList())
                    {
                        var (lhs, rhs) = rule;
                        var rules = _rules.Append(rule).ToList();
                        var reducedLhs = Reduce(lhs, new[] { newRule }).First();
                        var reducedRhs = Reduce(rhs, rules).First();

                        if (lhs == reducedLhs && rhs != reducedRhs)
                        {
                            Log($"(e) For rule {rule}, rhs reduced to {reducedRhs}", rhs, reducedRhs);
                            _rules.Remove(rule);
                            var replace = Compare(lhs, reducedRhs);
                            replace.IsMarked = rule.IsMarked;
                            _rules.Add(replace);
                            Log($"(e)    Replaced with rule {replace}", null, null);
                        }
                        else
                        if (lhs != reducedLhs && rhs == reducedRhs)
                        {
                            var id = new Identity(reducedLhs, rhs);
                            _identities.Add(id);
                            _rules.Remove(rule);
                            Log($"(e) For rule {rule}, lhs can be reduced to {reducedLhs}", lhs, reducedLhs);
                            Log($"(e)    Added new identity {id}", id.Left, id.Right);
                            Log($"(e)    Removed rule {rule}", rule.Left, rule.Right);
                        }
                    }

                    _rules.Add(newRule);
                    Log($"(e) Added {newRule} to set of rules", newRule.Left, newRule.Right);
                    _identities.Remove(identity);
                    Log($"(e) Removed {identity} from set of identities", identity.Left, identity.Right);
                }

                var cp = FindCriticalPair();
                if (cp != null)
                {
                    var id = new Identity(cp.Left, cp.Right);
                    _identities.Add(id);
                    Log($"Added {id} to the set of identities", cp.Left, cp.Right);
                }
                else
                {
                    Log("No critical pair found .. STOP", null, null);
                    if (!_identities.Any())
                        break;
                }
            }
        }

        public void CompleteHuet()
        {
            foreach (var rule in _rules)
            {
                rule.IsMarked = false;
            }

            while (_identities.Any() || _rules.Any(r => !r.IsMarked))
            {
                Log(" ------------------------------ ");
                Log("Starting outer loop with:");
                Log("    * Identities:");
                if (!_identities.Any())
                    Log("    none");
                else
                {
                    foreach (var id in _identities)
                        Log($"    {id}", id.Left, id.Right);
                }
              Log("    * Rules:");
                if (!_rules.Any())
                    Log("    none");
                else
                {
                    foreach (var rule in _rules)
                        Log($"    {rule}", rule.Left, rule.Right);
                }
                while (_identities.Any())
                {
                    var identity = _identities.OrderBy(id => id.Left.Size + id.Right.Size).First();
                    Log($"(a) Chose {identity}", identity.Left, identity.Right);

                    var (left, right) = identity;
                    var leftNormalForm = Reduce(left, _rules).First();
                    var rightNormalForm = Reduce(right, _rules).First();
                    Log($"(b) lhs reduced to {leftNormalForm}", left, leftNormalForm);
                    Log($"(b) rhs reduced to {rightNormalForm}", right, rightNormalForm);


                    if (leftNormalForm == rightNormalForm)
                    {
                        _identities.Remove(identity);
                        Log($"(c) normal forms are equal, removed identity", leftNormalForm, rightNormalForm);
                        continue;
                    }
                    var newRule = Compare(leftNormalForm, rightNormalForm);
                    if (newRule == null)
                    {
                        Log($"(d) FAIL! Cannot orient {leftNormalForm} and {rightNormalForm}", leftNormalForm, rightNormalForm);
                        throw new Exception($"Cannot orient {leftNormalForm} and {rightNormalForm}");
                    }
                    Log($"(e) Oriented {identity} to {newRule}", newRule.Left, newRule.Right);

                    foreach (var rule in _rules.ToList())
                    {
                        var (lhs, rhs) = rule;
                        var rules = _rules.Append(rule).ToList();
                        var reducedLhs = Reduce(lhs, new[] { newRule }).First();
                        var reducedRhs = Reduce(rhs, rules).First();

                        if (lhs == reducedLhs && rhs != reducedRhs)
                        {
                            Log($"(e) For rule {rule}, rhs reduced to {reducedRhs}", rhs, reducedRhs);
                            _rules.Remove(rule);
                            var replace = Compare(lhs, reducedRhs);
                            replace.IsMarked = rule.IsMarked;
                            _rules.Add(replace);
                            Log($"(e)    Replaced with rule {replace}", null, null);
                        }
                        else
                        if (lhs != reducedLhs && rhs == reducedRhs)
                        {
                            var id = new Identity(reducedLhs, rhs);
                            _identities.Add(id);
                            _rules.Remove(rule);
                            Log($"(e) For rule {rule}, lhs can be reduced to {reducedLhs}", lhs, reducedLhs);
                            Log($"(e)    Added new identity {id}", id.Left, id.Right);
                            Log($"(e)    Removed rule {rule}", rule.Left, rule.Right);
                        }
                    }

                    _rules.Add(newRule);
                    Log($"(e) Added {newRule} to set of rules", newRule.Left, newRule.Right);
                    _identities.Remove(identity);
                    Log($"(e) Removed {identity} from set of identities", identity.Left, identity.Right);
                }

                var pairs = FindCriticalPairs();
                if (pairs.Count > 0)
                {
                    foreach (var cp in pairs)
                    {
                        var id = new Identity(cp.Left, cp.Right);
                        _identities.Add(id);
                        Log($"Added {id} to the set of identities", cp.Left, cp.Right);
                    }
                }
                else
                {
                    Log("No critical pair found .. ");
                }
            }
            Log("Stopping since there the set of identities is empty and there are no unmarked rules.");
        }

        private List<Pair> FindCriticalPairs()
        {
            var allPairs = new List<(Pair pair, Rule rule, (Rule rule1, Rule rule2) other, string mgu)>();
            var results = new List<Pair>();
            var orderedRules = _rules
                .Where(r => !r.IsMarked)
                .OrderBy(r => r.Left.Size + r.Right.Size).ToList();

            if (!orderedRules.Any())
                return results;

            var rule = orderedRules.First();
            var pairs = _rules
                .Where(r => r.IsMarked).Append(rule)
                .Select(r => DistinctVariables(r, rule)).ToList();

            Log($"Looking for critical pairs using rule {rule}", rule.Left, rule.Right);

            foreach (var (rule1, rule2) in pairs)
            {
                var lPos = rule1.Left.Positions().Where(x => !x.Value.IsVariable && !string.IsNullOrEmpty(x.Key)).ToList();
                foreach (var pos in lPos)
                {
                    if (Unify(pos.Value, rule2.Left, out var mgu))
                    {
                        var r1 = PerformSubstitutions(rule1.Right, mgu);
                        var r2 = PerformSubstitutions(rule1.Left, mgu);
                        r2.Replace(pos.Key, PerformSubstitutions(rule2.Right, mgu));
                        var (n1, n2) = NormalizeVariables(r1, r2);
                        var pair = new Pair(n1, n2);

                        if (n1 != n2 && !_rules.Any(r => r.Left == n1 && r.Right == n2 || r.Right == n1 && r.Left == n2))
                        {
                            allPairs.Add((pair, rule, (rule1, rule2), string.Join(" ; ", mgu) + $"  POS = {pos.Key}"));
                        }
                    }
                }
                var rPos = rule2.Left.Positions().Where(x => !x.Value.IsVariable && !string.IsNullOrEmpty(x.Key)).ToList();
                foreach (var pos in rPos)
                {
                    if (Unify(pos.Value, rule1.Left, out var mgu))
                    {
                        var r1 = PerformSubstitutions(rule2.Right, mgu);
                        var r2 = PerformSubstitutions(rule2.Left, mgu);
                        r2.Replace(pos.Key, PerformSubstitutions(rule1.Right, mgu));
                        var (n1, n2) = NormalizeVariables(r1, r2);
                        var pair = new Pair(n1, n2);

                        if (n1 != n2 && !_rules.Any(r => r.Left == n1 && r.Right == n2 || r.Right == n1 && r.Left == n2))
                        {
                            allPairs.Add((pair, rule, (rule2, rule1), string.Join(" ; ", mgu) + $"  POS = {pos.Key}"));
                        }
                    }
                }
            }

            foreach (var result in allPairs)
            {
                if (result.rule != null)
                {
                    Log($"Found critical pair {result.pair}", result.pair.Left, result.pair.Right);
                    Log($"    From rules {result.other.rule1}", result.other.rule1.Left, result.other.rule1.Right);
                    Log($"           And {result.other.rule2}", result.other.rule2.Left, result.other.rule2.Right);
                    Log($"           MGU = {result.mgu}", result.other.rule1.Left, null);
                    results.Add(result.pair);
                }
            }

            if (!allPairs.Any())
            {
                Log($"No critical pairs found using rule {rule}", rule.Left, rule.Right);
            }

            Log($"Marked rule {rule}", rule.Left, rule.Right);
            rule.IsMarked = true;
            return results;
        }

        private List<Term> Reduce(Term term, ICollection<Rule> rules)
        {
            var results = new List<Term>();

            var matchingPositions = term.Positions()
                    .OrderByDescending(x => int.TryParse(x.Key, out var k) ? k : 0)
                    .Where(p => !p.Value.IsVariable)
                    .SelectMany(pos => rules.Select(
                            rule => (pos, rule, match: Match(pos.Value, rule.Left, out var subs), subs)))
                    .Where(x => x.match)
                    .ToList();

            if (!matchingPositions.Any())
            {
                var t = Term.Clone(term);
                results.Add(t);
                return results;
            }

            foreach (var match in matchingPositions)
            {
                var clone = Term.Clone(term);
                var replacement = PerformSubstitutions(Term.Clone(match.rule.Right), match.subs);

                clone.Replace(match.pos.Key, replacement);
                results.AddRange(Reduce(clone, rules));
            }

            return results
                .Select(r => (r, r.Representation()))
                .GroupBy(r => r.Item2)
                .Select(g => g.First().r)
                .ToList();
        }

        private bool Unify(Term term1, Term term2, out List<Unifier> mgu)
        {
            mgu = null;
            var t1 = Term.Clone(term1);
            var t2 = Term.Clone(term2);

            var change = false;
            var pairs = new List<Unifier> { new Unifier(t1, t2) };

            do
            {
                change = false;
                foreach (var pair in pairs.ToList())
                {
                    var (left, right) = pair;

                    if (!left.IsVariable && right.IsVariable)
                    {
                        pairs.Remove(pair);
                        pairs.Add(new Unifier(right, left));
                        change = true;
                        break;
                    }

                    if (!left.IsVariable && !right.IsVariable)
                    {
                        if (left.Definition.Name != right.Definition.Name)
                            return false;

                        var add = left.Children
                            .Zip(right.Children, (l, r) => r.IsVariable && !l.IsVariable ? new Unifier(r, l) : new Unifier(l, r))
                            .Where(x => x.Left != x.Right);
                        pairs.AddRange(add);

                        pairs.Remove(pair);

                        change = true;
                        break;
                    }

                    if (right.Variables.Any(v => v.Name == left.Definition.Name))
                        return false;

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
            mgu = pairs.ToList();
            return true;
        }

        private Pair FindCriticalPair()
        {
            var allPairs = new List<(Pair pair, Rule rule, (Rule rule1, Rule rule2) other, string mgu)>();
            var orderedRules = _rules
                .OrderBy(r => r.Left.Size + r.Right.Size).ToList();

            foreach (var rule in orderedRules)
            {
                var pairs = _rules
                    .Select(r => DistinctVariables(r, rule)).ToList();

                foreach (var (rule1, rule2) in pairs)
                {
                    var lPos = rule1.Left.Positions().Where(x => !x.Value.IsVariable && !string.IsNullOrEmpty(x.Key)).ToList();
                    foreach (var pos in lPos)
                    {
                        if (Unify(pos.Value, rule2.Left, out var mgu))
                        {
                            var r1 = PerformSubstitutions(rule1.Right, mgu);
                            var r2 = PerformSubstitutions(rule1.Left, mgu);
                            r2.Replace(pos.Key, PerformSubstitutions(rule2.Right, mgu));
                            var (n1, n2) = NormalizeVariables(r1, r2);
                            var pair = new Pair(n1, n2);

                            if (n1 != n2 && !_rules.Any(r => r.Left == n1 && r.Right == n2 || r.Right == n1 && r.Left == n2)
                                && _usedCriticalPairs.All(p => p != pair.ToString()))
                            {
                                allPairs.Add((pair, rule, (rule1, rule2), string.Join(" ; ", mgu) + $"  POS = {pos.Key}"));
                            }
                        }
                    }
                    var rPos = rule2.Left.Positions().Where(x => !x.Value.IsVariable && !string.IsNullOrEmpty(x.Key)).ToList();
                    foreach (var pos in rPos)
                    {
                        if (Unify(pos.Value, rule1.Left, out var mgu))
                        {
                            var r1 = PerformSubstitutions(rule2.Right, mgu);
                            var r2 = PerformSubstitutions(rule2.Left, mgu);
                            r2.Replace(pos.Key, PerformSubstitutions(rule1.Right, mgu));
                            var (n1, n2) = NormalizeVariables(r1, r2);
                            var pair = new Pair(n1, n2);

                            if (n1 != n2 && !_rules.Any(r => r.Left == n1 && r.Right == n2 || r.Right == n1 && r.Left == n2)
                                && _usedCriticalPairs.All(p => p != pair.ToString()))
                            {
                                allPairs.Add((pair, rule, (rule2, rule1), string.Join(" ; ", mgu) + $"  POS = {pos.Key}"));
                            }
                        }
                    }
                }
            }
            var smallest = allPairs.OrderBy(x => x.pair.Left.Size + x.pair.Right.Size).FirstOrDefault();
            if (smallest.rule != null)
            {
                //  smallest.rule.IsMarked = true;
                Log($"Found critical pair {smallest.pair}", smallest.pair.Left, smallest.pair.Right);
                Log($"    From rules {smallest.other.rule1}", smallest.other.rule1.Left, smallest.other.rule1.Right);
                Log($"           And {smallest.other.rule2}", smallest.other.rule2.Left, smallest.other.rule2.Right);
                Log($"           MGU = {smallest.mgu}", smallest.other.rule1.Left, null);
                //  Log($"Marked rule {smallest.rule}", smallest.rule.Left, smallest.rule.Right);
                _usedCriticalPairs.Add(smallest.pair.ToString());
            }
            return smallest.pair;
        }

        private Term PerformSubstitutions(Term t, List<Unifier> substitutions)
        {
            return PerformSubstitutions(t, substitutions.Cast<Pair>().ToList());
        }


        private Term PerformSubstitutions(Term t, List<Pair> substitutions)
        {
            var result = Term.Clone(t);
            var targets = result.Positions().Where(x => x.Value.IsVariable)
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
            map = clones.Select((t, i) => t.Positions().Where(x => x.Value.IsVariable)
                .GroupBy(x => x.Value.Definition.Name)
                .Select(x => x.First())
                .Select(x => new Pair(Term.Parse(Definitions, x.Value.Definition.Name), Term.Parse(Definitions, terms[i][x.Key].Definition.Name))).ToList())
                .ToList();
            return clones;
        }

        private (Rule l, Rule r) DistinctVariables(Rule l, Rule r)
        {
            var lClone = new[] { Term.Clone(l.Left), Term.Clone(l.Right) };
            var rClone = new[] { Term.Clone(r.Left), Term.Clone(r.Right) };
            var clones = new[] { lClone, rClone };
            for (var i = 0; i < clones.Length; i++)
            {
                foreach (var term in clones[i])
                    SuffixVariables(term, i.ToString());
            }
            return (new Rule(lClone[0], lClone[1]), new Rule(rClone[0], rClone[1]));
        }

        private (Term t1, Term t2) NormalizeVariables(Term t1, Term t2)
        {
            var c1 = Term.Clone(t1);
            var c2 = Term.Clone(t2);
            var variables = c1.Variables.Concat(c2.Variables).Select(x => x.Name).Distinct().ToList();

            foreach (var var in variables)
            {
                var uid = Term.Parse(Definitions, var + Guid.NewGuid());
                c1.Substitute(var, uid);
                c2.Substitute(var, uid);
            }

            variables = c1.Variables.Concat(c2.Variables).Select(x => x.Name).Distinct().ToList();
            for (var i = 0; i < variables.Count; i++)
            {
                var normalized = Term.Parse(Definitions, _variableNames[i]);
                c1.Substitute(variables[i], normalized);
                c2.Substitute(variables[i], normalized);
            }

            return (c1, c2);
        }

        private Term NormalizeVariables(Term term)
        {
            var clone = Term.Clone(term);
            foreach (var var in clone.Variables)
            {
                clone.Substitute(var.Name, Term.Parse(Definitions, var.Name + Guid.NewGuid()));
            }

            var existing = clone.Variables;
            for (var i = 0; i < existing.Count; i++)
            {
                clone.Substitute(existing[i].Name, Term.Parse(Definitions, _variableNames[i]));
            }
            return clone;
        }

        private void SuffixVariables(Term term, string suffix)
        {
            foreach (var var in term.Variables)
            {
                term.Substitute(var.Name, Term.Parse(Definitions, var.Name + suffix));
            }
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
                foreach (var pos in new Dictionary<string, Term>(pattern.Positions()))
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

        private void Log(string what, Term t = null, Term u = null) => History.Save(what, t, u);

        private Rule Compare(Term left, Term right)
        {
            if (left > right)
                return new Rule(left, right);

            if (right > left)
                return new Rule(right, left);

            return null;
        }

        private class Pair
        {
            public Term Left { get; protected set; }
            public Term Right { get; protected set; }
            public void Deconstruct(out Term left, out Term right)
            {
                left = Left;
                right = Right;
            }
            public Pair(Term left, Term right)
            {
                Left = left;
                Right = right;
            }
            public override string ToString()
             => $"{Left.ToString()} {Symbol} {Right.ToString()}";
            protected virtual string Symbol => ",";
        }

        private class Unifier : Pair
        {
            public Unifier(Term left, Term right) : base(left, right) { }
            protected override string Symbol => "→";
        }

        private class Identity : Pair
        {
            public Identity(Term left, Term right) : base(left, right) { }
            protected override string Symbol => "≈";
        }

        private class Rule : Pair
        {
            public Rule(Term left, Term right) : base(left, right) { }
            protected override string Symbol => "→";
            public bool IsMarked { get; set; }
            public override string ToString()
                => $"{base.ToString()} {(IsMarked ? "*" : "")}";

        }
    }

    public interface IRewriteSystem
    {
        IReadOnlyCollection<string> Identities { get; }
        IReadOnlyCollection<string> Rules { get; }
    }
}
