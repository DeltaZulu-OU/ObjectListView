using System.Collections.Generic;
using System.Linq;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class WinFormsBehaviorTests
    {
        [TestMethod]
        public void CheckedObjects_TracksCheckedModels()
        {
            using var listView = new ObjectListView { CheckBoxes = true };
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });

            listView.CheckObject(second);

            Assert.AreSequenceEqual(new object[] { second }, listView.CheckedObjects.Cast<object>().ToArray());
            Assert.IsFalse(listView.IsChecked(first));
            Assert.IsTrue(listView.IsChecked(second));
        }

        [TestMethod]
        public void PersistentCheckState_SurvivesListRebuild()
        {
            using var listView = new ObjectListView { CheckBoxes = true, PersistentCheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            var model = new Model("model");
            listView.SetObjects(new[] { model });
            listView.CheckObject(model);

            listView.BuildList(true);

            Assert.IsTrue(listView.IsChecked(model));
            Assert.AreSequenceEqual(new object[] { model }, listView.CheckedObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void MappedCheckState_IncludesFilteredObjects()
        {
            using var listView = new ObjectListView { CheckBoxes = true, PersistentCheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });
            listView.CheckObject(second);

            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ReferenceEquals(x, first));

            Assert.AreEqual(1, listView.GetItemCount());
            Assert.AreSequenceEqual(new object[] { second },
                listView.GetAllObjectsWithMappedCheckState(System.Windows.Forms.CheckState.Checked).Cast<object>().ToArray());
        }

        [TestMethod]
        public void FastObjectListView_CheckedObjectsTracksVirtualItems()
        {
            using var listView = new FastObjectListView { CheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });

            listView.CheckObject(second);

            Assert.AreSequenceEqual(new object[] { second }, listView.CheckedObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void FastObjectListView_PreservesObjectIndexMapping()
        {
            using var listView = new FastObjectListView();
            var models = new[] {
                    new Model("first"),
                    new Model("second"),
                    new Model("third")
                };

            listView.SetObjects(models);

            Assert.AreSame(models[1], listView.GetModelObject(1));
            Assert.AreEqual(1, listView.IndexOf(models[1]));
            Assert.AreEqual(-1, listView.IndexOf(new Model("missing")));
        }

        [TestMethod]
        public void TreeListView_ExpandsChildrenIntoVisibleModelList()
        {
            using var tree = new TreeListView();
            var child1 = new Node("child-1");
            var child2 = new Node("child-2");
            var root = new Node("root", child1, child2);

            tree.CanExpandGetter = x => ((Node)x).Children.Count > 0;
            tree.ChildrenGetter = x => ((Node)x).Children;
            tree.Roots = new[] { root };

            Assert.AreEqual(1, tree.GetItemCount());

            tree.Expand(root);

            Assert.AreEqual(3, tree.GetItemCount());
            Assert.AreSame(root, tree.GetModelObject(0));
            Assert.AreSame(child1, tree.GetModelObject(1));
            Assert.AreSame(child2, tree.GetModelObject(2));
        }

        [TestMethod]
        public void PossibleFinishCellEditing_ReturnsTrueWhenNoEditorIsActive()
        {
            using var listView = new ObjectListView();
            Assert.IsTrue(listView.PossibleFinishCellEditing());
        }

        private sealed class Model
        {
            public Model(string name)
            {
                Name = name;
            }

            public string Name { get; }
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
