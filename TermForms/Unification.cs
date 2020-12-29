using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TermRewritingV2;
using TermRewritingV3;

namespace TermForms
{
    public partial class Unification : Form
    {
        private UnificationSystem3 _system;
        public Unification()
        {
            InitializeComponent();
            signaturesBox.Text = "f/2\r\ng/2\r\na/0\r\nb/0\r\nc/0";
            //identitiesBox.Text = "f(x,y) = f(y,x)";
            //equationsBox.Text = "f(f(a, x), z) = f(f(y, b), f(a, b))";
            identitiesBox.Text = @"
f(x,y) = f(y,x)
g(x,y) = g(y,x)
f(f(x,y),z) = f(x,f(y,z))
g(g(x,y),z) = g(x,g(y,z))
g(x,f(y,z)) = f(g(x,y), g(x,z))";

            equationsBox.Text = @"
g(f(x,y),f(x,y)) = f(g(a,a), f(g(a,b), f(g(a,b), g(b,b)))";
        }

        private async void unifyButton_Click(object sender, EventArgs e)
        {
            var signatures = signaturesBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var identities = identitiesBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var terms = equationsBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            solutionBox.Text = "Thinking ...";

            await Task.Run(() => _system = new UnificationSystem3(signatures, identities, terms));

            solutionBox.Text = _system.Solution;
        }
    }
}
