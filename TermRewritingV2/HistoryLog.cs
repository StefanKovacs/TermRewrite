using System.Collections.Generic;
using System.Linq;

namespace TermRewritingV2
{
    public class HistoryLog
    {
        private readonly IRewriteSystem _system;
        public readonly List<Log> History = new List<Log>();
        private int counter = 0;

        public HistoryLog(IRewriteSystem system)
        {
            _system = system;
        }

        public void Save(string what, Term term1, Term term2)
        {
            History.Add(new Log
            {
                Text = what,
                Term1 = term1 != null ? Term.Clone(term1) : null,
                Term2 = term2 != null ? Term.Clone(term2) : null,
                Identities = _system.Identities.ToArray(),
                Rules = _system.Rules.ToArray(),
                Index = ++counter
            });
        }

        public class Log
        {
            public int Index { get; set; }
            public string Text { get; set; }
            public string[] Identities { get; set; }
            public string[] Rules { get; set; }
            public Term Term1 { get; set; }
            public Term Term2 { get; set; }

            public override string ToString() => $"{Index}. {Text}";
        }
    }
}
