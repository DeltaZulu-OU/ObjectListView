using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class FastListParityTests
    {
        [TestMethod]
        public void Filtering_KeepsFullAndFilteredCollectionsSeparate()
        {
            using var listView = CreateList();
            var models = new[] {
                new Model("alpha", 1),
                new Model("beta", 2),
                new Model("atom", 3)
            };
            listView.SetObjects(models);
            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ((Model)x).Name.StartsWith("a"));

            Assert.AreSequenceEqual(
                new[] { "alpha", "atom" },
                listView.FilteredObjects.Cast<Model>().Select(x => x.Name).ToArray());
            Assert.AreSequenceEqual(
                new[] { "alpha", "beta", "atom" },
                listView.Objects.Cast<Model>().Select(x => x.Name).ToArray());
        }

        [TestMethod]
        public void RemovingFilteredOutObject_UpdatesFullCollectionWithoutChangingVisibleMatches()
        {
            using var listView = CreateList();
            var alpha = new Model("alpha", 1);
            var beta = new Model("beta", 2);
            var atom = new Model("atom", 3);
            listView.SetObjects(new[] { alpha, beta, atom });
            listView.UseFiltering = true;
            listView.ModelFilter = new ModelFilter(x => ((Model)x).Name.StartsWith("a"));

            listView.RemoveObjects(new[] { beta });

            Assert.AreSequenceEqual(
                new[] { "alpha", "atom" },
                listView.Objects.Cast<Model>().Select(x => x.Name).ToArray());
            Assert.AreSequenceEqual(
                new[] { "alpha", "atom" },
                listView.FilteredObjects.Cast<Model>().Select(x => x.Name).ToArray());
        }

        [TestMethod]
        public void DataSourceSort_RebuildsObjectIndexMapping()
        {
            using var listView = CreateList();
            var alpha = new Model("alpha", 1);
            var beta = new Model("beta", 2);
            var gamma = new Model("gamma", 3);
            listView.SetObjects(new[] { beta, gamma, alpha });

            var source = (FastObjectListDataSource)listView.VirtualListDataSource;
            var rankColumn = new OLVColumn("Rank", nameof(Model.Rank));
            source.Sort(rankColumn, SortOrder.Descending);

            Assert.AreSame(gamma, source.GetNthObject(0));
            Assert.AreSame(beta, source.GetNthObject(1));
            Assert.AreSame(alpha, source.GetNthObject(2));
            Assert.AreEqual(0, source.GetObjectIndex(gamma));
            Assert.AreEqual(1, source.GetObjectIndex(beta));
            Assert.AreEqual(2, source.GetObjectIndex(alpha));
        }

        [TestMethod]
        public void ApplyingListAndModelFilters_PreservesFilterOrder()
        {
            using var listView = CreateList();
            listView.SetObjects(new[] {
                new Model("alpha", 1),
                new Model("atom", 2),
                new Model("amber", 3),
                new Model("beta", 4)
            });
            listView.UseFiltering = true;
            listView.ListFilter = new ListFilter(items => items.Cast<Model>().Take(3).ToArray());
            listView.ModelFilter = new ModelFilter(x => ((Model)x).Name.Contains("m") || ((Model)x).Name.Contains("t"));

            Assert.AreSequenceEqual(
                new[] { "atom", "amber" },
                listView.FilteredObjects.Cast<Model>().Select(x => x.Name).ToArray());
        }

        private static FastObjectListView CreateList()
        {
            var listView = new FastObjectListView();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            return listView;
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
    }
}
