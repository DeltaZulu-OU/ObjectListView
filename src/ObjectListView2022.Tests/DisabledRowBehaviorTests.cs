using System.Linq;
using System.Reflection;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DisabledRowBehaviorTests
    {
        [TestMethod]
        public void DisableObject_ExistingRowBecomesDisabledAndDeselected()
        {
            using var listView = CreateList(new ObjectListView());
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });
            listView.SelectObject(first);

            listView.DisableObject(first);

            Assert.IsTrue(listView.IsDisabled(first));
            Assert.IsFalse(GetItem(listView, first).Enabled);
            Assert.IsFalse(GetItem(listView, first).Selected);
            Assert.IsNull(listView.SelectedObject);
        }

        [TestMethod]
        public void EnableObject_DisabledRowBecomesEnabledAgain()
        {
            using var listView = CreateList(new ObjectListView());
            var model = new Model("model");
            listView.SetObjects(new[] { model });
            listView.DisableObject(model);

            listView.EnableObject(model);

            Assert.IsFalse(listView.IsDisabled(model));
            Assert.IsTrue(GetItem(listView, model).Enabled);
        }

        [TestMethod]
        public void SelectObject_DoesNotSelectDisabledOrdinaryRow()
        {
            using var listView = CreateList(new ObjectListView());
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });
            listView.DisableObject(second);

            listView.SelectObject(second);

            Assert.IsFalse(GetItem(listView, second).Selected);
            Assert.IsNull(listView.SelectedObject);
        }

        [TestMethod]
        public void DisabledRow_CanBeFocusedWithoutBeingSelected()
        {
            using var listView = CreateList(new ObjectListView());
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });
            listView.DisableObject(second);
            _ = listView.Handle;
            var item = GetItem(listView, second);

            item.Focused = true;

            Assert.IsTrue(item.Focused);
            Assert.IsFalse(item.Selected);
        }

        [TestMethod]
        public void EditSubItem_DoesNotStartEditingDisabledRow()
        {
            using var listView = CreateList(new RecordingObjectListView());
            var model = new Model("model");
            listView.SetObjects(new[] { model });
            listView.DisableObject(model);

            listView.EditSubItem(GetItem(listView, model), 0);

            Assert.AreEqual(0, listView.StartCellEditCount);
        }

        [TestMethod]
        public void DisabledState_SurvivesBuildListAndFiltering()
        {
            using var listView = CreateList(new ObjectListView());
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });
            listView.DisableObject(second);

            listView.BuildList(true);
            Assert.IsFalse(GetItem(listView, second).Enabled);

            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ReferenceEquals(x, first));
            Assert.AreEqual(-1, listView.IndexOf(second));

            listView.ModelFilter = new ModelFilter(_ => true);

            Assert.IsTrue(listView.IsDisabled(second));
            Assert.IsFalse(GetItem(listView, second).Enabled);
        }

        [TestMethod]
        public void FastObjectListView_DisabledRowRemainsDisabledAfterRebuild()
        {
            using var listView = CreateList(new FastObjectListView());
            var first = new Model("first");
            var second = new Model("second");
            listView.SetObjects(new[] { first, second });
            listView.DisableObject(second);

            listView.BuildList(true);

            Assert.IsTrue(listView.IsDisabled(second));
            Assert.IsFalse(GetItem(listView, second).Enabled);
        }

        [TestMethod]
        public void FastObjectListView_NativeSelectAllLeavesDisabledRowUnselected()
        {
            using var listView = CreateList(new FastObjectListView());
            var first = new Model("first");
            var second = new Model("second");
            var third = new Model("third");
            listView.SetObjects(new[] { first, second, third });
            listView.DisableObject(second);
            _ = listView.Handle;

            InvokeNativeSelectAll(listView);

            Assert.IsTrue(listView.SelectedIndices.Contains(0));
            Assert.IsFalse(listView.SelectedIndices.Contains(1));
            Assert.IsTrue(listView.SelectedIndices.Contains(2));
        }

        [TestMethod]
        public void TreeListView_DisabledChildSurvivesCollapseAndExpand()
        {
            using var tree = new TreeListView();
            tree.Columns.Add(new OLVColumn("Name", nameof(Node.Name)));
            var child = new Node("child");
            var root = new Node("root", child);
            tree.CanExpandGetter = x => ((Node)x).Children.Length > 0;
            tree.ChildrenGetter = x => ((Node)x).Children;
            tree.Roots = new[] { root };
            tree.Expand(root);
            tree.DisableObject(child);

            Assert.IsFalse(GetItem(tree, child).Enabled);

            tree.Collapse(root);
            tree.Expand(root);

            Assert.IsTrue(tree.IsDisabled(child));
            Assert.IsFalse(GetItem(tree, child).Enabled);
        }

        [TestMethod]
        public void Reset_ClearsDisabledObjectState()
        {
            using var listView = CreateList(new ObjectListView());
            var model = new Model("model");
            listView.SetObjects(new[] { model });
            listView.DisableObject(model);

            listView.Reset();

            Assert.IsFalse(listView.IsDisabled(model));
            Assert.AreEqual(0, listView.DisabledObjects.Cast<object>().Count());
        }

        private static T CreateList<T>(T listView) where T : ObjectListView
        {
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            return listView;
        }

        private static OLVListItem GetItem(ObjectListView listView, object model)
        {
            var index = listView.IndexOf(model);
            Assert.IsGreaterThanOrEqualTo(0, index);
            var item = listView.GetItem(index);
            Assert.IsNotNull(item);
            return item;
        }

        private static void InvokeNativeSelectAll(ObjectListView listView)
        {
            var nativeMethods = typeof(ObjectListView).Assembly.GetType(
                "BrightIdeasSoftware.Implementation.NativeMethods",
                true);
            var method = nativeMethods.GetMethod(
                "SelectAllItems",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { listView });
        }

        private sealed class RecordingObjectListView : ObjectListView
        {
            public int StartCellEditCount { get; private set; }

            public override void StartCellEdit(OLVListItem item, int subItemIndex)
            {
                StartCellEditCount++;
            }
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
                Children = children;
            }

            public string Name { get; }
            public Node[] Children { get; }
        }
    }
}
