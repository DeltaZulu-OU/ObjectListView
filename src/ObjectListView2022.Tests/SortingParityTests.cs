using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class SortingParityTests
    {
        [TestMethod]
        public void ColumnAndModelComparers_AgreeOnSecondaryOrdering()
        {
            var groupColumn = new OLVColumn("Group", nameof(Model.Group));
            var rankColumn = new OLVColumn("Rank", nameof(Model.Rank));
            var first = new Model { Group = "same", Rank = 1 };
            var second = new Model { Group = "same", Rank = 2 };

            var columnComparer = new ColumnComparer(
                groupColumn,
                SortOrder.Ascending,
                rankColumn,
                SortOrder.Ascending);
            var modelComparer = new ModelObjectComparer(
                groupColumn,
                SortOrder.Ascending,
                rankColumn,
                SortOrder.Ascending);

            var itemResult = columnComparer.Compare(new OLVListItem(first), new OLVListItem(second));
            var modelResult = modelComparer.Compare(first, second);

            Assert.IsLessThan(0, itemResult);
            Assert.IsLessThan(0, modelResult);
            Assert.AreEqual(Math.Sign(itemResult), Math.Sign(modelResult));
        }

        [TestMethod]
        public void ColumnAndModelComparers_HandleNullAndDbNullConsistently()
        {
            var column = new OLVColumn("Value", nameof(Model.Value));
            var nullModel = new Model { Value = null };
            var dbNullModel = new Model { Value = DBNull.Value };
            var valueModel = new Model { Value = 10 };

            var columnComparer = new ColumnComparer(column, SortOrder.Ascending);
            var modelComparer = new ModelObjectComparer(column, SortOrder.Ascending);

            Assert.AreEqual(0, columnComparer.Compare(new OLVListItem(nullModel), new OLVListItem(dbNullModel)));
            Assert.AreEqual(0, modelComparer.Compare(nullModel, dbNullModel));

            var itemResult = columnComparer.Compare(new OLVListItem(nullModel), new OLVListItem(valueModel));
            var modelResult = modelComparer.Compare(nullModel, valueModel);
            Assert.AreEqual(Math.Sign(itemResult), Math.Sign(modelResult));
        }

        [TestMethod]
        public void DescendingOrder_ReversesPrimaryComparison()
        {
            var column = new OLVColumn("Rank", nameof(Model.Rank));
            var lower = new Model { Rank = 1 };
            var higher = new Model { Rank = 2 };

            var ascending = new ModelObjectComparer(column, SortOrder.Ascending);
            var descending = new ModelObjectComparer(column, SortOrder.Descending);

            Assert.AreEqual(-Math.Sign(ascending.Compare(lower, higher)), Math.Sign(descending.Compare(lower, higher)));
        }

        [TestMethod]
        public void GroupComparer_FallsBackToHeaderWhenSortValueIsMissing()
        {
            var alpha = new OLVGroup("alpha");
            var beta = new OLVGroup("beta");
            var comparer = new OLVGroupComparer(SortOrder.Ascending);

            Assert.IsLessThan(0, comparer.Compare(alpha, beta));
            Assert.IsGreaterThan(0, comparer.Compare(beta, alpha));
        }

        [TestMethod]
        public void GroupComparer_PrefersSortValueOverHeader()
        {
            var first = new OLVGroup("zulu") { SortValue = 1 };
            var second = new OLVGroup("alpha") { SortValue = 2 };
            var comparer = new OLVGroupComparer(SortOrder.Ascending);

            Assert.IsLessThan(0, comparer.Compare(first, second));
        }

        private sealed class Model
        {
            public string Group { get; set; }
            public int Rank { get; set; }
            public object Value { get; set; }
        }
    }
}
