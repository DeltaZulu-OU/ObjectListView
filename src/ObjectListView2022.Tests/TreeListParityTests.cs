using System.Collections.Generic;
using System.Linq;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class TreeListParityTests
    {
        [TestMethod]
        public void CollapseAndExpand_RebuildVisibleObjectIndexMapping()
        {
            using var tree = CreateTree(out var root, out var firstChild, out var secondChild);

            tree.Expand(root);
            Assert.AreEqual(3, tree.GetItemCount());
            Assert.AreEqual(1, tree.IndexOf(firstChild));
            Assert.AreEqual(2, tree.IndexOf(secondChild));

            tree.Collapse(root);
            Assert.AreEqual(1, tree.GetItemCount());
            Assert.AreEqual(-1, tree.IndexOf(firstChild));
            Assert.AreEqual(-1, tree.IndexOf(secondChild));

            tree.Expand(root);
            Assert.AreEqual(3, tree.GetItemCount());
            Assert.AreEqual(1, tree.IndexOf(firstChild));
            Assert.AreEqual(2, tree.IndexOf(secondChild));
        }

        [TestMethod]
        public void RefreshObject_UpdatesExpandedBranchAfterChildrenChange()
        {
            using var tree = CreateTree(out var root, out var firstChild, out var secondChild);
            tree.Expand(root);
            Assert.AreEqual(3, tree.GetItemCount());

            root.Children.Remove(secondChild);
            tree.RefreshObject(root);

            Assert.AreEqual(2, tree.GetItemCount());
            Assert.AreSame(root, tree.GetModelObject(0));
            Assert.AreSame(firstChild, tree.GetModelObject(1));
            Assert.AreEqual(-1, tree.IndexOf(secondChild));
        }

        [TestMethod]
        public void CheckedObjects_IncludesCheckedModelsInCollapsedBranches()
        {
            using var tree = CreateTree(out var root, out var firstChild, out _);
            tree.CheckBoxes = true;
            tree.Expand(root);
            tree.CheckObject(firstChild);

            tree.Collapse(root);

            Assert.IsTrue(tree.IsChecked(firstChild));
            Assert.AreSequenceEqual(
                new object[] { firstChild },
                tree.CheckedObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void ExpandedObjects_CanRestoreExpansionDuringRebuild()
        {
            using var tree = CreateTree(out var root, out _, out _);
            tree.ExpandedObjects = new[] { root };

            tree.RebuildAll(true);

            Assert.IsTrue(tree.IsExpanded(root));
            Assert.AreEqual(3, tree.GetItemCount());
        }

        private static TreeListView CreateTree(out Node root, out Node firstChild, out Node secondChild)
        {
            firstChild = new Node("child-1");
            secondChild = new Node("child-2");
            root = new Node("root", firstChild, secondChild);

            var tree = new TreeListView();
            tree.Columns.Add(new OLVColumn("Name", nameof(Node.Name)));
            tree.CanExpandGetter = x => ((Node)x).Children.Count > 0;
            tree.ChildrenGetter = x => ((Node)x).Children;
            tree.Roots = new[] { root };
            return tree;
        }

        private sealed class Node
        {
            public Node(string name, params Node[] children)
            {
                Name = name;
                Children = children.ToList();
            }

            public string Name { get; }
            public List<Node> Children { get; }
        }
    }
}
