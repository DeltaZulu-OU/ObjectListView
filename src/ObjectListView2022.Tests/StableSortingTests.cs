using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class StableSortingTests
    {
        [TestMethod]
        public void ColumnComparer_PreservesCurrentOrderWhenSortKeysAreEqual()
        {
            using var listView = new ObjectListView();
            var column = new OLVColumn("Group", nameof(Model.Group));
            listView.Columns.Add(column);

            var first = new OLVListItem(new Model(1, "same"));
            var second = new OLVListItem(new Model(2, "same"));
            listView.Items.Add(first);
            listView.Items.Add(second);

            var comparer = new ColumnComparer(column, SortOrder.Ascending);

            Assert.IsLessThan(0, comparer.Compare(first, second));
            Assert.IsGreaterThan(0, comparer.Compare(second, first));
        }

        [TestMethod]
        public void FastObjectListDataSource_SortPreservesOrderOfEqualKeys()
        {
            using var listView = new FastObjectListView();
            var column = new OLVColumn("Group", nameof(Model.Group));
            listView.Columns.Add(column);
            var models = Enumerable.Range(0, 32)
                .Select(i => new Model(i, "same"))
                .ToArray();
            listView.SetObjects(models);

            var source = (FastObjectListDataSource)listView.VirtualListDataSource;
            source.Sort(column, SortOrder.Ascending);

            CollectionAssert.AreEqual(
                models.Select(x => x.Id).ToArray(),
                source.ObjectList.Cast<Model>().Select(x => x.Id).ToArray());
            CollectionAssert.AreEqual(
                models.Select(x => x.Id).ToArray(),
                source.FilteredObjectList.Cast<Model>().Select(x => x.Id).ToArray());
        }

        [TestMethod]
        public void TreeBranchSort_PreservesSiblingOrderWhenSortKeysAreEqual()
        {
            using var listView = new TreeListView();
            var tree = new TreeListView.Tree(listView);
            var parent = new TreeListView.Branch(null, tree, new Model(-1, "parent"));
            var models = Enumerable.Range(0, 32)
                .Select(i => new Model(i, "same"))
                .ToArray();

            foreach (var model in models)
            {
                parent.ChildBranches.Add(new TreeListView.Branch(parent, tree, model));
            }

            var column = new OLVColumn("Group", nameof(Model.Group));
            var comparer = new TreeListView.BranchComparer(
                new ModelObjectComparer(column, SortOrder.Ascending));

            parent.Sort(comparer);

            CollectionAssert.AreEqual(
                models.Select(x => x.Id).ToArray(),
                parent.ChildBranches.Select(x => ((Model)x.Model).Id).ToArray());
        }

        private sealed class Model
        {
            public Model(int id, string group)
            {
                Id = id;
                Group = group;
            }

            public int Id { get; }
            public string Group { get; }
        }
    }
}
