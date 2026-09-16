using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class GroupingTransitionTests
    {
        [TestMethod]
        public void ObjectListView_GroupCreationEventsFollowLifecycleOrder()
        {
            using var listView = CreateObjectList(out var categoryColumn);
            listView.SetObjects(CreateModels());
            listView.ShowGroups = true;
            var events = new List<string>();
            listView.BeforeCreatingGroups += (_, _) => events.Add("before");
            listView.AboutToCreateGroups += (_, _) => events.Add("about-to");
            listView.AfterCreatingGroups += (_, _) => events.Add("after");

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.AreSequenceEqual(
                new[] { "before", "about-to", "after" },
                events.ToArray());
        }

        [TestMethod]
        public void ObjectListView_CollapsedGroupsCanBeRestoredByKeyAfterRebuild()
        {
            using var listView = CreateObjectList(out var categoryColumn);
            listView.HasCollapsibleGroups = true;
            _ = listView.Handle;
            listView.SetObjects(CreateModels());
            listView.ShowGroups = true;
            listView.BuildGroups(categoryColumn, SortOrder.Ascending);
            var originalGroupA = listView.OLVGroups.Single(x => Equals(x.Key, "A"));

            listView.CollapsedGroups = new[] { originalGroupA };

            var savedCollapsedGroups = listView.CollapsedGroups.ToArray();
            Assert.AreSequenceEqual(
                new object[] { "A" },
                savedCollapsedGroups.Select(x => x.Key).ToArray());

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);
            var rebuiltGroupA = listView.OLVGroups.Single(x => Equals(x.Key, "A"));
            Assert.AreNotSame(originalGroupA, rebuiltGroupA);

            listView.CollapsedGroups = savedCollapsedGroups;

            Assert.IsTrue(rebuiltGroupA.Collapsed);
            Assert.AreSequenceEqual(
                new object[] { "A" },
                listView.CollapsedGroups.Select(x => x.Key).ToArray());
        }

        [TestMethod]
        public void ObjectListView_GroupedUngroupedRoundTripPreservesSelection()
        {
            using var listView = CreateObjectList(out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.SelectedObject = models[2];
            listView.PrimarySortColumn = categoryColumn;
            listView.PrimarySortOrder = SortOrder.Ascending;

            listView.ShowGroups = true;
            listView.BuildList(true);
            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.OLVGroups.Select(x => x.Key).ToArray());
            Assert.AreSame(models[2], listView.SelectedObject);

            listView.ShowGroups = false;
            listView.BuildList(true);

            Assert.IsFalse(listView.ShowGroups);
            Assert.AreEqual(models.Length, listView.GetItemCount());
            Assert.AreSame(models[2], listView.SelectedObject);

            listView.ShowGroups = true;
            listView.BuildList(true);

            Assert.IsTrue(listView.ShowGroups);
            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.OLVGroups.Select(x => x.Key).ToArray());
            Assert.AreSame(models[2], listView.SelectedObject);
        }

        [TestMethod]
        public void FastObjectListView_GroupedUngroupedRoundTripRebuildsVirtualGroups()
        {
            using var listView = CreateFastList(out var categoryColumn);
            var models = CreateModels();
            listView.SetObjects(models);
            listView.SelectedObject = models[2];
            listView.PrimarySortColumn = categoryColumn;
            listView.PrimarySortOrder = SortOrder.Ascending;

            listView.ShowGroups = true;
            listView.BuildList(true);

            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.OLVGroups.Select(x => x.Key).ToArray());
            Assert.AreSame(models[2], listView.SelectedObject);

            listView.ShowGroups = false;
            listView.BuildList(true);

            Assert.IsFalse(listView.ShowGroups);
            Assert.AreEqual(models.Length, listView.GetItemCount());
            Assert.AreSame(models[2], listView.SelectedObject);

            listView.ShowGroups = true;
            listView.BuildList(true);

            Assert.IsTrue(listView.ShowGroups);
            Assert.AreSequenceEqual(
                new object[] { "A", "B" },
                listView.OLVGroups.Select(x => x.Key).ToArray());
            Assert.AreSame(models[2], listView.SelectedObject);
            Assert.AreSequenceEqual(
                new[] { 2, 2 },
                listView.OLVGroups.Select(x => x.VirtualItemCount).ToArray());
        }

        [TestMethod]
        public void FastObjectListView_GeneratedGroupsHonorCollapsibleSetting()
        {
            using var listView = CreateFastList(out var categoryColumn);
            listView.HasCollapsibleGroups = true;
            listView.SetObjects(CreateModels());
            listView.ShowGroups = true;

            listView.BuildGroups(categoryColumn, SortOrder.Ascending);

            Assert.IsTrue(listView.OLVGroups.All(x => x.Collapsible));
        }

        private static ObjectListView CreateObjectList(out OLVColumn categoryColumn)
        {
            var listView = new ObjectListView {
                ShowGroups = false,
                View = View.Details
            };
            AddColumns(listView, out categoryColumn);
            return listView;
        }

        private static FastObjectListView CreateFastList(out OLVColumn categoryColumn)
        {
            var listView = new FastObjectListView {
                ShowGroups = false,
                View = View.Details
            };
            AddColumns(listView, out categoryColumn);
            return listView;
        }

        private static void AddColumns(ObjectListView listView, out OLVColumn categoryColumn)
        {
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            categoryColumn = new OLVColumn("Category", nameof(Model.Category)) {
                GroupKeyGetter = x => ((Model)x).Category,
                GroupKeyToTitleConverter = key => "Category " + key
            };
            listView.Columns.Add(categoryColumn);
            listView.Columns.Add(new OLVColumn("Rank", nameof(Model.Rank)));
        }

        private static Model[] CreateModels() => new[] {
            new Model("a-low", "A", 1),
            new Model("b-mid", "B", 2),
            new Model("a-high", "A", 3),
            new Model("b-top", "B", 4)
        };

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
