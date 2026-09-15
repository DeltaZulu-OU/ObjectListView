using System.Drawing;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DpiScalingTests
    {
        [TestMethod]
        public void RescaleConstantsForDpi_RoundTripDoesNotCompoundFontScaling()
        {
            using var font = new Font(SystemFonts.DefaultFont.FontFamily, 10.0f);
            using var listView = new DpiTestObjectListView { Font = font };
            listView.CreateControl();

            listView.Rescale(96, 144);
            Assert.AreEqual(15.0f, listView.Font.Size, 0.11f);

            listView.Rescale(144, 192);
            Assert.AreEqual(20.0f, listView.Font.Size, 0.11f);

            listView.Rescale(192, 96);
            Assert.AreEqual(10.0f, listView.Font.Size, 0.11f);
        }

        [TestMethod]
        public void RescaleConstantsForDpi_PreservesColumnWidths()
        {
            using var font = new Font(SystemFonts.DefaultFont.FontFamily, 10.0f);
            using var listView = new DpiTestObjectListView { Font = font };
            var first = new OLVColumn("First", "First") { Width = 173 };
            var second = new OLVColumn("Second", "Second") { Width = 211 };
            listView.Columns.AddRange(new[] { first, second });
            listView.CreateControl();

            listView.Rescale(96, 144);

            Assert.AreEqual(173, first.Width);
            Assert.AreEqual(211, second.Width);
        }

        [TestMethod]
        public void RescaleConstantsForDpi_RefreshesOrdinaryItems()
        {
            using var listView = new DpiTestObjectListView();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            listView.SetObjects(new[] {
                new Model("first"),
                new Model("second"),
                new Model("third")
            });
            listView.CreateControl();
            listView.ResetRefreshItemCallCount();

            listView.Rescale(96, 96);

            Assert.AreEqual(3, listView.RefreshItemCallCount);
        }

        [TestMethod]
        public void RescaleConstantsForDpi_WorksWithVirtualLists()
        {
            using var font = new Font(SystemFonts.DefaultFont.FontFamily, 10.0f);
            using var listView = new DpiTestFastObjectListView { Font = font };
            listView.Columns.Add(new OLVColumn("Name", "Name"));
            listView.SetObjects(new[] {
                new Model("first"),
                new Model("second"),
                new Model("third")
            });
            listView.CreateControl();

            listView.Rescale(96, 144);

            Assert.AreEqual(15.0f, listView.Font.Size, 0.11f);
            Assert.AreEqual(3, listView.GetItemCount());
        }

        [TestMethod]
        public void RescaleConstantsForDpi_ClearsVirtualItemCache()
        {
            using var listView = new DpiTestFastObjectListView();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            listView.SetObjects(new[] {
                new Model("first"),
                new Model("second")
            });
            listView.CreateControl();
            listView.ResetClearCachedInfoCallCount();

            listView.Rescale(96, 96);

            Assert.AreEqual(1, listView.ClearCachedInfoCallCount);
        }

        [TestMethod]
        public void RescaleConstantsForDpi_UsesExplicitFontAsNewBaseline()
        {
            using var initialFont = new Font(SystemFonts.DefaultFont.FontFamily, 10.0f);
            using var replacementFont = new Font(SystemFonts.DefaultFont.FontFamily, 12.0f, FontStyle.Bold);
            using var listView = new DpiTestObjectListView { Font = initialFont };
            listView.CreateControl();
            listView.Rescale(96, 144);

            listView.Font = replacementFont;
            listView.Rescale(144, 192);

            Assert.AreEqual(16.0f, listView.Font.Size, 0.11f);
            Assert.AreEqual(FontStyle.Bold, listView.Font.Style);
        }

        private sealed class DpiTestObjectListView : ObjectListView
        {
            public int RefreshItemCallCount { get; private set; }

            public void Rescale(int oldDpi, int newDpi) => RescaleConstantsForDpi(oldDpi, newDpi);

            public void ResetRefreshItemCallCount() => RefreshItemCallCount = 0;

            public override void RefreshItem(OLVListItem olvi)
            {
                RefreshItemCallCount++;
                base.RefreshItem(olvi);
            }
        }

        private sealed class DpiTestFastObjectListView : FastObjectListView
        {
            public int ClearCachedInfoCallCount { get; private set; }

            public void Rescale(int oldDpi, int newDpi) => RescaleConstantsForDpi(oldDpi, newDpi);

            public void ResetClearCachedInfoCallCount() => ClearCachedInfoCallCount = 0;

            public override void ClearCachedInfo()
            {
                ClearCachedInfoCallCount++;
                base.ClearCachedInfo();
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
    }
}
