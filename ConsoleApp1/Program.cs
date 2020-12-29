using System;
using System.Collections.Generic;
using System.Linq;
using TermRewritingV3;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var signatures = "f/2\r\ng/2\r\na/0\r\nb/0\r\nc/0"
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split('/'))
                .Select(x => Definition.Function(x[0], uint.Parse(x[1])))
                .ToList();


            //var zzz = Term.Parse("f(g(a,a), f(g(a,b), f(g(a,b), g(b,b)))", signatures);
            var t1 = Term.Parse("f(x,f(a,g(y,x)))", signatures);

            var t2 = Term.Parse("f(f(x,y), g(b,b))", signatures);
        }
    }
}
