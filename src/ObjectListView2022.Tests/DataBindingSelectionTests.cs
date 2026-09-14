using System.ComponentModel;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DataBindingSelectionTests
    {
        [TestMethod]
        public void DataListView_InitialBindingPreservesEmptySelection()
        {
            using var listView = new DataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();

            listView.DataSource = rows;

            Assert.IsNull(listView.SelectedObject);
            Assert.AreEqual(0, listView.SelectedIndices.Count);
        }

        [TestMethod]
        public void DataListView_PositionChangeAfterBindingUpdatesSelection()
        {
            using var listView = new DataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];

            currencyManager.Position = 1;

            Assert.AreSame(rows[1], listView.SelectedObject);
        }

        [TestMethod]
        public void FastDataListView_InitialBindingPreservesEmptySelection()
        {
            using var listView = new FastDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();

            listView.DataSource = rows;

            Assert.IsNull(listView.SelectedObject);
            Assert.AreEqual(0, listView.SelectedIndices.Count);
        }

        [TestMethod]
        public void FastDataListView_PositionChangeAfterBindingUpdatesSelection()
        {
            using var listView = new FastDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];

            currencyManager.Position = 1;

            Assert.AreSame(rows[1], listView.SelectedObject);
        }

        [TestMethod]
        public void DataTreeListView_InitialBindingPreservesEmptySelection()
        {
            using var listView = CreateTreeListView();
            var rows = CreateTreeRows();

            listView.DataSource = rows;

            Assert.IsNull(listView.SelectedObject);
            Assert.AreEqual(0, listView.SelectedIndices.Count);
        }

        [TestMethod]
        public void DataTreeListView_PositionChangeAfterBindingUpdatesSelection()
        {
            using var listView = CreateTreeListView();
            var rows = CreateTreeRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];

            currencyManager.Position = 1;

            Assert.AreSame(rows[1], listView.SelectedObject);
        }

        private static DataTreeListView CreateTreeListView() => new DataTreeListView
        {
            BindingContext = new BindingContext(),
            AutoGenerateColumns = false,
            KeyAspectName = nameof(Row.Id),
            ParentKeyAspectName = nameof(Row.ParentId)
        };

        private static BindingList<Row> CreateRows() => new BindingList<Row>
        {
            new Row { Name = "first" },
            new Row { Name = "second" }
        };

        private static BindingList<Row> CreateTreeRows() => new BindingList<Row>
        {
            new Row { Id = 1, Name = "root" },
            new Row { Id = 2, ParentId = 1, Name = "child" }
        };

        private sealed class Row
        {
            public int Id { get; set; }
            public int? ParentId { get; set; }
            public string Name { get; set; }
        }
    }
}
