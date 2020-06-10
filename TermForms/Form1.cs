using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using TermRewritingV2;
using static TermRewritingV2.HistoryLog;

namespace TermForms
{
    public partial class Form1 : Form
    {
        private RewriteSystem _system;
        private Term _selection;
        public Form1()
        {
            InitializeComponent();
            SignaturesInput.Text = "f/2;i/1;e/0";
            //SignaturesInput.Text = "f/2;h/2;c/0;";
            TermInput.Text = "f(x, f(y, z))";
        }

        private void inputButton1_Click(object sender, EventArgs e)
        {
            try
            {
                _system = new RewriteSystem(SignaturesInput.Text);
                var term = Term.Parse(_system.Definitions, TermInput.Text);
                PopulateTreeView(preview, term);
                UpdateIdentities();
                UpdateRules();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PopulateTreeView(TreeView tree, Term term)
        {
            _selection = term;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            tree.Nodes.Add(Transform(term, term.Positions()));
            tree.EndUpdate();
        }

        private static TreeNode Transform(Term node, Dictionary<string, Term> positions)
        {
            var position = positions.FirstOrDefault(p => p.Value == node).Key;
            var representation = $"({node.Definition.Type}) {node.Definition.Name} | {position}";

            var tree = new TreeNode(representation) { Tag = node };
            tree.Nodes.AddRange(node.Children.Select(c => Transform(c, positions)).ToArray());
            tree.Expand();
            return tree;
        }

        private void addIdentity_Click(object sender, EventArgs e)
        {
            try
            {
                _system.AddIdentity(TermInput.Text);
                UpdateIdentities();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateIdentities(string[] identities = null)
        {
            if (identities == null)
                identities = _system.Identities.ToArray();
            identitiesBox.BeginUpdate();
            identitiesBox.Items.Clear();
            identitiesBox.Items.AddRange(identities);
            identitiesBox.EndUpdate();
        }

        private void UpdateRules(string[] rules = null)
        {
            if (rules == null)
                rules = _system.Rules.ToArray();
            rulesBox.BeginUpdate();
            rulesBox.Items.Clear();
            rulesBox.Items.AddRange(rules);
            rulesBox.EndUpdate();
        }

        private void UpdateLog()
        {
            logBox.BeginUpdate();
            logBox.Items.Clear();
            foreach (var item in _system.History.History)
            {
                logBox.Items.Add(item);
            }
            logBox.EndUpdate();
        }

        private void completeButton_Click(object sender, EventArgs e)
        {
            try
            {
                _system.CompleteHuet();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                UpdateRules();
                UpdateIdentities();
                UpdateLog();
            }
        }

        private void logBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (Log)logBox.SelectedItem;
            UpdateIdentities(selected.Identities);
            UpdateRules(selected.Rules);
            if (selected.Term1 != null)
                PopulateTreeView(preview, selected.Term1);
            if (selected.Term2 != null)
                PopulateTreeView(previewRight, selected.Term2);
        }
    }
}
