using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class CheckboxSemanticsTests
    {
        [TestMethod]
        public void HierarchicalCheckboxes_EnableCheckboxesAndDisableTriStateCycling()
        {
            using var tree = new TreeListView { TriStateCheckBoxes = true };

            tree.HierarchicalCheckboxes = true;

            Assert.IsTrue(tree.CheckBoxes);
            Assert.IsFalse(tree.TriStateCheckBoxes);
        }

        [TestMethod]
        public void HierarchicalCheckboxes_DeepLeafChangesRecalculateAllAncestors()
        {
            using var tree = CreateHierarchicalTree(
                out var root,
                out var branch,
                out var firstLeaf,
                out var secondLeaf,
                out var rootLeaf);

            tree.CheckObject(firstLeaf);

            AssertCheckState(tree, firstLeaf, CheckState.Checked);
            AssertCheckState(tree, secondLeaf, CheckState.Unchecked);
            AssertCheckState(tree, branch, CheckState.Indeterminate);
            AssertCheckState(tree, root, CheckState.Indeterminate);

            tree.CheckObject(secondLeaf);

            AssertCheckState(tree, branch, CheckState.Checked);
            AssertCheckState(tree, root, CheckState.Indeterminate);

            tree.CheckObject(rootLeaf);

            AssertCheckState(tree, root, CheckState.Checked);

            tree.UncheckObject(firstLeaf);

            AssertCheckState(tree, branch, CheckState.Indeterminate);
            AssertCheckState(tree, root, CheckState.Indeterminate);
        }

        [TestMethod]
        public void HierarchicalCheckboxes_CheckingBranchPropagatesToKnownDescendants()
        {
            using var tree = CreateHierarchicalTree(
                out var root,
                out var branch,
                out var firstLeaf,
                out var secondLeaf,
                out var rootLeaf);

            tree.CheckObject(branch);

            AssertCheckState(tree, branch, CheckState.Checked);
            AssertCheckState(tree, firstLeaf, CheckState.Checked);
            AssertCheckState(tree, secondLeaf, CheckState.Checked);
            AssertCheckState(tree, rootLeaf, CheckState.Unchecked);
            AssertCheckState(tree, root, CheckState.Indeterminate);

            tree.CheckObject(root);

            AssertCheckState(tree, root, CheckState.Checked);
            AssertCheckState(tree, branch, CheckState.Checked);
            AssertCheckState(tree, firstLeaf, CheckState.Checked);
            AssertCheckState(tree, secondLeaf, CheckState.Checked);
            AssertCheckState(tree, rootLeaf, CheckState.Checked);
        }

        [TestMethod]
        public void HierarchicalCheckboxes_CheckedObjectsSetterRecalculatesAncestors()
        {
            using var tree = CreateHierarchicalTree(
                out var root,
                out var branch,
                out var firstLeaf,
                out var secondLeaf,
                out var rootLeaf);

            tree.CheckedObjects = new object[] { firstLeaf };

            AssertCheckState(tree, firstLeaf, CheckState.Checked);
            AssertCheckState(tree, secondLeaf, CheckState.Unchecked);
            AssertCheckState(tree, branch, CheckState.Indeterminate);
            AssertCheckState(tree, rootLeaf, CheckState.Unchecked);
            AssertCheckState(tree, root, CheckState.Indeterminate);
            Assert.AreSequenceEqual(
                new[] { firstLeaf.Name },
                tree.CheckedObjects.Cast<Node>().Select(x => x.Name).OrderBy(x => x).ToArray());

            tree.CheckedObjects = new object[] { firstLeaf, secondLeaf, rootLeaf };

            AssertCheckState(tree, branch, CheckState.Checked);
            AssertCheckState(tree, root, CheckState.Checked);
            Assert.AreSequenceEqual(
                new[] { branch.Name, firstLeaf.Name, root.Name, rootLeaf.Name, secondLeaf.Name }.OrderBy(x => x).ToArray(),
                tree.CheckedObjects.Cast<Node>().Select(x => x.Name).OrderBy(x => x).ToArray());
        }

        [TestMethod]
        public void HierarchicalCheckboxes_RebuildAllPreservesDeepCheckState()
        {
            using var tree = CreateHierarchicalTree(
                out var root,
                out var branch,
                out var firstLeaf,
                out var secondLeaf,
                out var rootLeaf);
            tree.CheckObject(firstLeaf);
            tree.CheckObject(secondLeaf);
            tree.Collapse(branch);

            tree.RebuildAll(true);

            AssertCheckState(tree, firstLeaf, CheckState.Checked);
            AssertCheckState(tree, secondLeaf, CheckState.Checked);
            AssertCheckState(tree, branch, CheckState.Checked);
            AssertCheckState(tree, rootLeaf, CheckState.Unchecked);
            AssertCheckState(tree, root, CheckState.Indeterminate);
            Assert.IsTrue(tree.IsChecked(firstLeaf));
            Assert.IsTrue(tree.IsChecked(secondLeaf));
        }

        [TestMethod]
        public void FastObjectListView_CheckedStateFollowsModelAcrossSortAndRebuild()
        {
            using var listView = CreateFastList(out var rankColumn);
            var first = new Model("first", 1);
            var second = new Model("second", 3);
            var third = new Model("third", 2);
            listView.SetObjects(new[] { first, second, third });
            listView.CheckObject(second);

            listView.Sort(rankColumn, SortOrder.Descending);
            listView.BuildList(true);

            Assert.AreEqual(0, listView.IndexOf(second));
            Assert.IsTrue(listView.IsChecked(second));
            Assert.AreSequenceEqual(
                new object[] { second },
                listView.CheckedObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void FastObjectListView_CheckedObjectsExcludesFilteredModelUntilItReturns()
        {
            using var listView = CreateFastList(out _);
            var first = new Model("first", 1);
            var second = new Model("second", 2);
            listView.SetObjects(new[] { first, second });
            listView.CheckObject(second);

            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ReferenceEquals(x, first));

            Assert.IsEmpty(listView.CheckedObjects.Cast<object>());
            Assert.Contains(
                second,
                listView.GetAllObjectsWithMappedCheckState(CheckState.Checked).Cast<object>().ToArray());

            listView.ModelFilter = new ModelFilter(_ => true);

            Assert.IsTrue(listView.IsChecked(second));
            Assert.AreSequenceEqual(
                new object[] { second },
                listView.CheckedObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void FastObjectListView_CheckedObjectsSetterReplacesVirtualCheckState()
        {
            using var listView = CreateFastList(out _);
            var first = new Model("first", 1);
            var second = new Model("second", 2);
            listView.SetObjects(new[] { first, second });
            listView.CheckObject(first);

            listView.CheckedObjects = new object[] { second };

            Assert.IsFalse(listView.IsChecked(first));
            Assert.IsTrue(listView.IsChecked(second));
            Assert.AreSequenceEqual(
                new object[] { second },
                listView.CheckedObjects.Cast<object>().ToArray());
        }

        private static InspectableTreeListView CreateHierarchicalTree(
            out Node root,
            out Node branch,
            out Node firstLeaf,
            out Node secondLeaf,
            out Node rootLeaf)
        {
            firstLeaf = new Node("leaf-1");
            secondLeaf = new Node("leaf-2");
            branch = new Node("branch", firstLeaf, secondLeaf);
            rootLeaf = new Node("root-leaf");
            root = new Node("root", branch, rootLeaf);

            var tree = new InspectableTreeListView {
                CanExpandGetter = x => ((Node)x).Children.Count > 0,
                ChildrenGetter = x => ((Node)x).Children,
                ParentGetter = x => ((Node)x).Parent,
                HierarchicalCheckboxes = true
            };
            tree.Columns.Add(new OLVColumn("Name", nameof(Node.Name)));
            tree.Roots = new[] { root };
            tree.ExpandAll();
            return tree;
        }

        private static FastObjectListView CreateFastList(out OLVColumn rankColumn)
        {
            var listView = new FastObjectListView {
                CheckBoxes = true,
                ShowGroups = false
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            rankColumn = new OLVColumn("Rank", nameof(Model.Rank));
            listView.Columns.Add(rankColumn);
            return listView;
        }

        private static void AssertCheckState(InspectableTreeListView tree, Node model, CheckState expected)
        {
            var actual = tree.GetModelCheckState(model);
            Assert.IsTrue(actual.HasValue, model.Name);
            Assert.AreEqual(expected, actual.Value, model.Name);
        }

        private sealed class InspectableTreeListView : TreeListView
        {
            public CheckState? GetModelCheckState(object model) => GetCheckState(model);
        }

        private sealed class Model
        {
            public Model(string name, int rank)
            {
                Name = name;
                Rank = rank;
            }

            public string Name { get; }
            public int Rank { get; }
        }

        private sealed class Node
        {
            public Node(string name, params Node[] children)
            {
                Name = name;
                Children = children.ToList();
                foreach (var child in Children)
                {
                    child.Parent = this;
                }
            }

            public string Name { get; }
            public Node Parent { get; private set; }
            public List<Node> Children { get; }
        }
    }
}
