using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Filtering;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class GroupingLifecycleTests
    {
        [TestMethod]
        public void ObjectListView_GroupingUsesKeysTitlesAndRequestedGroupOrder()
        {
            using var listView = CreateObjectList(out _, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.ShowGroups = true;

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            Assert.AreSequenceEqual(
                new[] { "Category A", "Category B" },
                listView.CapturedGroups.Select(x => x.Header).ToArray());
            Assert.AreSequenceEqual(
                new[] { "a-high", "a-low" },
                GetOrdinaryGroupModels(listView.CapturedGroups[0]).Select(x => x.Name).OrderBy(x => x).ToArray());

            listView.BuildGroups(categoryColumn, SortOrder.Descending);

            Assert.AreSequenceEqual(
                new object[] { "B", "A" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
        }

        [TestMethod]
        public void ObjectListView_NonGroupableSortColumnKeepsExistingGrouping()
        {
            using var listView = CreateObjectList(out var rankColumn, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.ShowGroups = true;
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);
            rankColumn.Groupable = false;

            listView.BuildGroups(rankColumn, SortOrder.Descending);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            Assert.AreSequenceEqual(
                new[] { 3, 1 },
                GetOrdinaryGroupModels(listView.CapturedGroups[0]).Select(x => x.Rank).ToArray());
        }

        [TestMethod]
        public void ObjectListView_AlwaysGroupByColumnSurvivesSortChanges()
        {
            using var listView = CreateObjectList(out var rankColumn, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.ShowGroups = true;
            listView.AlwaysGroupByColumn = categoryColumn;
            listView.AlwaysGroupBySortOrder = SortOrder.Descending;

            listView.Sort(rankColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "B", "A" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            var groupA = listView.CapturedGroups.Single(x => Equals(x.Key, "A"));
            Assert.AreSequenceEqual(
                new[] { 1, 3 },
                GetOrdinaryGroupModels(groupA).Select(x => x.Rank).ToArray());
        }

        [TestMethod]
        public void ObjectListView_FilteringRebuildsGroupsFromVisibleRows()
        {
            using var listView = CreateObjectList(out _, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.ShowGroups = true;
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ((Model)x).Category == "A");
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "A" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            Assert.AreEqual(2, listView.CapturedGroups[0].Items.Count);

            listView.ModelFilter = new ModelFilter(_ => true);
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
        }

        [TestMethod]
        public void ObjectListView_BeforeCreatingGroupsCanReplaceDefaultGroups()
        {
            using var listView = CreateObjectList(out _, out var categoryColumn);
            listView.SetObjects(CreateModels());
            listView.ShowGroups = true;
            listView.BeforeCreatingGroups += (_, e) =>
                e.Groups = new List<OLVGroup> {
                    new OLVGroup("Injected") { Key = "injected" }
                };

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreEqual(1, listView.CapturedGroups.Count);
            Assert.AreEqual("Injected", listView.CapturedGroups[0].Header);
            Assert.AreEqual("injected", listView.CapturedGroups[0].Key);
        }

        [TestMethod]
        public void ObjectListView_AboutToCreateGroupsCanMutateGeneratedGroups()
        {
            using var listView = CreateObjectList(out _, out var categoryColumn);
            listView.SetObjects(CreateModels());
            listView.ShowGroups = true;
            listView.AboutToCreateGroups += (_, e) => {
                foreach (var group in e.Groups)
                {
                    group.Header = "Modified " + group.Header;
                }
            };

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new[] { "Modified Category A", "Modified Category B" },
                listView.CapturedGroups.Select(x => x.Header).ToArray());
        }

        [TestMethod]
        public void ObjectListView_GroupedBuildListPreservesSelection()
        {
            using var listView = CreateObjectList(out _, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.SelectedObject = models[1];
            listView.ShowGroups = true;
            listView.PrimarySortColumn = categoryColumn;
            listView.PrimarySortOrder = SortOrder.Ascending;

            listView.BuildList(true);

            Assert.AreSame(models[1], listView.SelectedObject);
            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
        }

        [TestMethod]
        public void FastObjectListView_GroupingUsesSameKeysTitlesAndMembership()
        {
            using var listView = CreateFastList(out _, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.ShowGroups = true;

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            Assert.AreSequenceEqual(
                new[] { "Category A", "Category B" },
                listView.CapturedGroups.Select(x => x.Header).ToArray());
            Assert.AreEqual(2, listView.CapturedGroups[0].VirtualItemCount);
            Assert.AreSequenceEqual(
                new[] { "a-high", "a-low" },
                GetVirtualGroupModels(listView, listView.CapturedGroups[0]).Select(x => x.Name).OrderBy(x => x).ToArray());
        }

        [TestMethod]
        public void FastObjectListView_NonGroupableSortColumnKeepsExistingGrouping()
        {
            using var listView = CreateFastList(out var rankColumn, out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.ShowGroups = true;
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);
            rankColumn.Groupable = false;

            listView.BuildGroups(rankColumn, SortOrder.Descending);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            var groupA = listView.CapturedGroups.Single(x => Equals(x.Key, "A"));
            Assert.AreSequenceEqual(
                new[] { 3, 1 },
                GetVirtualGroupModels(listView, groupA).Select(x => x.Rank).ToArray());
        }

        [TestMethod]
        public void FastObjectListView_FilteringRebuildsVirtualGroupMembership()
        {
            using var listView = CreateFastList(out _, out var categoryColumn);
            listView.SetObjects(CreateModels());
            listView.ShowGroups = true;
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ((Model)x).Category == "B");
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
            Assert.AreEqual(2, listView.CapturedGroups[0].VirtualItemCount);
            Assert.AreSequenceEqual(
                new[] { "b-mid", "b-top" },
                GetVirtualGroupModels(listView, listView.CapturedGroups[0]).Select(x => x.Name).OrderBy(x => x).ToArray());

            listView.ModelFilter = new ModelFilter(_ => true);
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.CapturedGroups.Select(x => x.Key).ToArray());
        }

        private static RecordingObjectListView CreateObjectList(out OLVColumn rankColumn, out OLVColumn categoryColumn)
        {
            var listView = new RecordingObjectListView { ShowGroups = false };
            AddGroupingColumns(listView, out rankColumn, out categoryColumn);
            return listView;
        }

        private static RecordingFastObjectListView CreateFastList(out OLVColumn rankColumn, out OLVColumn categoryColumn)
        {
            var listView = new RecordingFastObjectListView { ShowGroups = false };
            AddGroupingColumns(listView, out rankColumn, out categoryColumn);
            return listView;
        }

        private static void AddGroupingColumns(ObjectListView listView, out OLVColumn rankColumn, out OLVColumn categoryColumn)
        {
            rankColumn = new OLVColumn("Rank", nameof(Model.Rank));
            categoryColumn = new OLVColumn("Category", nameof(Model.Category)) {
                GroupKeyGetter = x => ((Model)x).Category,
                GroupKeyToTitleConverter = key => "Category " + key
            };
            listView.Columns.Add(rankColumn);
            listView.Columns.Add(categoryColumn);
        }

        private static Model[] CreateModels() => new[] {
            new Model("a-low", "A", 1),
            new Model("b-mid", "B", 2),
            new Model("a-high", "A", 3),
            new Model("b-top", "B", 4)
        };

        private static IEnumerable<Model> GetOrdinaryGroupModels(OLVGroup group) =>
            group.Items.Select(x => (Model)x.RowObject);

        private static IEnumerable<Model> GetVirtualGroupModels(FastObjectListView listView, OLVGroup group) =>
            group.Contents.Cast<int>().Select(index => (Model)listView.GetModelObject(index));

        private sealed class RecordingObjectListView : ObjectListView
        {
            public IList<OLVGroup> CapturedGroups { get; private set; } = new List<OLVGroup>();

            protected override void CreateGroups(IEnumerable<OLVGroup> groups)
            {
                CapturedGroups = groups.ToList();
            }
        }

        private sealed class RecordingFastObjectListView : FastObjectListView
        {
            public IList<OLVGroup> CapturedGroups { get; private set; } = new List<OLVGroup>();

            protected override void CreateGroups(IEnumerable<OLVGroup> groups)
            {
                CapturedGroups = groups.ToList();
            }
        }

        private sealed class Model
        {
            public Model(string name, string category, int rank)
            {
                Name = name;
                Category = category;
                Rank = rank;
            }

            public string Name { get; }
            public string Category { get; }
            public int Rank { get; }
        }
    }
}
