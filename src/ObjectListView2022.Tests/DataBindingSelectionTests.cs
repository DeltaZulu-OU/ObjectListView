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
            using var listView = new RecordingDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();

            listView.DataSource = rows;

            Assert.AreEqual(0, listView.SelectionAssignmentCount);
            Assert.IsNull(listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void DataListView_PositionChangeAfterBindingUpdatesSelection()
        {
            using var listView = new RecordingDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];
            var targetPosition = currencyManager.Position == 1 ? 0 : 1;

            currencyManager.Position = targetPosition;

            Assert.AreEqual(1, listView.SelectionAssignmentCount);
            Assert.AreSame(rows[targetPosition], listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void DataListView_RebindingWithExistingSelectionAppliesNewCurrentPosition()
        {
            using var listView = new RecordingDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var firstRows = CreateRows();
            var secondRows = new BindingList<Row>
            {
                new Row { Name = "third" },
                new Row { Name = "fourth" }
            };
            listView.DataSource = firstRows;
            listView.SelectedObject = firstRows[1];
            listView.ResetSelectionAssignmentCount();

            listView.DataSource = secondRows;

            Assert.IsGreaterThanOrEqualTo(1, listView.SelectionAssignmentCount);
            Assert.AreSame(secondRows[0], listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void FastDataListView_InitialBindingPreservesEmptySelection()
        {
            using var listView = new RecordingFastDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();

            listView.DataSource = rows;

            Assert.AreEqual(0, listView.SelectionAssignmentCount);
            Assert.IsNull(listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void FastDataListView_PositionChangeAfterBindingUpdatesSelection()
        {
            using var listView = new RecordingFastDataListView
            {
                BindingContext = new BindingContext(),
                AutoGenerateColumns = false
            };
            var rows = CreateRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];
            var targetPosition = currencyManager.Position == 1 ? 0 : 1;

            currencyManager.Position = targetPosition;

            Assert.AreEqual(1, listView.SelectionAssignmentCount);
            Assert.AreSame(rows[targetPosition], listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void DataTreeListView_InitialBindingPreservesEmptySelection()
        {
            using var listView = CreateTreeListView();
            var rows = CreateTreeRows();

            listView.DataSource = rows;

            Assert.AreEqual(0, listView.SelectionAssignmentCount);
            Assert.IsNull(listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void DataTreeListView_PositionChangeAfterBindingUpdatesSelection()
        {
            using var listView = CreateTreeListView();
            var rows = CreateTreeRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];
            var targetPosition = currencyManager.Position == 1 ? 0 : 1;

            currencyManager.Position = targetPosition;

            Assert.AreEqual(1, listView.SelectionAssignmentCount);
            Assert.AreSame(rows[targetPosition], listView.RecordedSelectedObject);
        }

        [TestMethod]
        public void DataTreeListView_PositionChangeToCollapsedChildExpandsAncestor()
        {
            using var listView = CreateTreeListView();
            var rows = CreateTreeRows();
            listView.DataSource = rows;
            var currencyManager = (CurrencyManager)listView.BindingContext[rows];

            Assert.IsFalse(listView.IsExpanded(rows[0]));
            currencyManager.Position = 1;

            Assert.IsTrue(listView.IsExpanded(rows[0]));
            Assert.AreSame(rows[1], listView.RecordedSelectedObject);
        }

        private static RecordingDataTreeListView CreateTreeListView() => new RecordingDataTreeListView
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

        private sealed class RecordingDataListView : DataListView
        {
            public int SelectionAssignmentCount { get; private set; }
            public object RecordedSelectedObject { get; private set; }

            public override object SelectedObject {
                get => RecordedSelectedObject;
                set {
                    RecordedSelectedObject = value;
                    SelectionAssignmentCount++;
                }
            }

            public void ResetSelectionAssignmentCount() => SelectionAssignmentCount = 0;
        }

        private sealed class RecordingFastDataListView : FastDataListView
        {
            public int SelectionAssignmentCount { get; private set; }
            public object RecordedSelectedObject { get; private set; }

            public override object SelectedObject {
                get => RecordedSelectedObject;
                set {
                    RecordedSelectedObject = value;
                    SelectionAssignmentCount++;
                }
            }
        }

        private sealed class RecordingDataTreeListView : DataTreeListView
        {
            public int SelectionAssignmentCount { get; private set; }
            public object RecordedSelectedObject { get; private set; }

            public override object SelectedObject {
                get => RecordedSelectedObject;
                set {
                    RecordedSelectedObject = value;
                    SelectionAssignmentCount++;
                }
            }
        }

        private sealed class Row
        {
            public int Id { get; set; }
            public int? ParentId { get; set; }
            public string Name { get; set; }
        }
    }
}
