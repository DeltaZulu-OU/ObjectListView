using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DisposalLifecycleTests
    {
        [TestMethod]
        public void BuildList_AfterDisposeSkipsExtendedStyleWork()
        {
            var listView = new ProbeObjectListView();
            listView.Dispose();

            listView.BuildList(true);

            Assert.IsFalse(listView.ApplyExtendedStylesCalled);
        }

        [TestMethod]
        public void ClearHotItem_AfterDisposeSkipsHotItemUpdate()
        {
            var listView = new ProbeObjectListView();
            listView.Dispose();

            listView.ClearHotItem();

            Assert.IsFalse(listView.HotItemUpdateCalled);
        }

        [TestMethod]
        public void RefreshHotItem_AfterDisposeSkipsHotItemUpdate()
        {
            var listView = new ProbeObjectListView();
            listView.Dispose();

            listView.RefreshHotItem();

            Assert.IsFalse(listView.HotItemUpdateCalled);
        }

        [TestMethod]
        public void UpdateColumnFiltering_AfterDisposeSkipsFilterRebuild()
        {
            var listView = new ProbeObjectListView();
            listView.Dispose();

            listView.UpdateColumnFiltering();

            Assert.IsFalse(listView.CreateColumnFilterCalled);
        }

        [TestMethod]
        public void ColumnClick_WhenSortDisposesControlDoesNotFailDuringTeardown()
        {
            var listView = new ProbeObjectListView();
            listView.Columns.Add(new OLVColumn("Name", "Name"));
            listView.DisposeDuringSort = true;

            listView.TriggerColumnClick(0);

            Assert.IsTrue(listView.SortCalled);
            Assert.IsTrue(listView.IsDisposed);
        }

        private sealed class ProbeObjectListView : ObjectListView
        {
            public bool ApplyExtendedStylesCalled { get; private set; }
            public bool CreateColumnFilterCalled { get; private set; }
            public bool HotItemUpdateCalled { get; private set; }
            public bool SortCalled { get; private set; }
            public bool DisposeDuringSort { get; set; }

            protected override void ApplyExtendedStyles()
            {
                ApplyExtendedStylesCalled = true;
                base.ApplyExtendedStyles();
            }

            public override IModelFilter CreateColumnFilter()
            {
                CreateColumnFilterCalled = true;
                return base.CreateColumnFilter();
            }

            protected override void UpdateHotItem(Point pt)
            {
                HotItemUpdateCalled = true;
                base.UpdateHotItem(pt);
            }

            public override void Sort(int columnToSortIndex)
            {
                SortCalled = true;
                if (DisposeDuringSort)
                {
                    Dispose();
                    return;
                }

                base.Sort(columnToSortIndex);
            }

            public void TriggerColumnClick(int columnIndex) =>
                base.HandleColumnClick(this, new ColumnClickEventArgs(columnIndex));
        }
    }
}
