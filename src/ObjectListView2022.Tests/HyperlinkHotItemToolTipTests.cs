using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class HyperlinkHotItemToolTipTests
    {
        [TestMethod]
        public void IsHyperlink_CanReplaceGeneratedUrl()
        {
            var model = new Model("first", "display-text");
            using var listView = CreateHyperlinkList(new[] { model });
            var eventCount = 0;
            listView.IsHyperlink += (_, e) => {
                eventCount++;
                Assert.AreSame(model, e.Model);
                Assert.AreSame(listView.GetColumn(1), e.Column);
                Assert.AreEqual("display-text", e.Text);
                Assert.AreEqual("display-text", e.Url);
                e.Url = "https://example.test/replaced";
            };

            listView.SetObjects(new[] { model });

            Assert.AreEqual(1, eventCount);
            Assert.AreEqual("https://example.test/replaced", GetSubItem(listView, model, 1).Url);
        }

        [TestMethod]
        public void IsHyperlink_CanDisableHyperlinkForIndividualCell()
        {
            var model = new Model("first", "https://example.test/original");
            using var listView = CreateHyperlinkList(new[] { model });
            listView.IsHyperlink += (_, e) => e.IsHyperlink = false;

            listView.SetObjects(new[] { model });

            Assert.IsNull(GetSubItem(listView, model, 1).Url);
        }

        [TestMethod]
        public void DisabledModel_IsNotHyperlinkByDefaultWhenRowIsBuilt()
        {
            var model = new Model("first", "https://example.test/original");
            using var listView = CreateHyperlinkList(System.Array.Empty<Model>());
            listView.DisableObject(model);

            listView.SetObjects(new[] { model });

            Assert.IsTrue(listView.IsDisabled(model));
            Assert.IsNull(GetSubItem(listView, model, 1).Url);
        }

        [TestMethod]
        public void GetCellToolTip_CustomGetterTakesPrecedenceOverHyperlinkFallback()
        {
            var model = new Model("first", "display-text");
            using var listView = CreateHyperlinkList(new[] { model });
            listView.IsHyperlink += (_, e) => e.Url = "https://example.test/different";
            listView.CellToolTipGetter = (column, rowObject) =>
                $"custom:{column.Text}:{((Model)rowObject).Name}";
            listView.SetObjects(new[] { model });
            listView.SetHotCellLocation(HitTestLocation.Text);

            var text = listView.GetCellToolTip(1, 0);

            Assert.AreEqual("custom:Link:first", text);
        }

        [TestMethod]
        public void GetCellToolTip_ShowsDistinctHyperlinkUrlOnlyOverText()
        {
            var model = new Model("first", "display-text");
            using var listView = CreateHyperlinkList(new[] { model });
            listView.IsHyperlink += (_, e) => e.Url = "https://example.test/different";
            listView.SetObjects(new[] { model });

            listView.SetHotCellLocation(HitTestLocation.Text);
            Assert.AreEqual("https://example.test/different", listView.GetCellToolTip(1, 0));

            listView.SetHotCellLocation(HitTestLocation.InCell);
            Assert.IsNull(listView.GetCellToolTip(1, 0));
        }

        [TestMethod]
        public void HotItemChanged_ReportsOldAndNewStateBeforeControlStateChanges()
        {
            var first = new Model("first", null);
            var second = new Model("second", null);
            using var listView = CreateHotList(View.Details, first, second);

            listView.ApplyHotItem(MakeHit(listView, first, 0, HitTestLocation.InCell));

            HotItemChangedEventArgs observed = null;
            var hotRowDuringEvent = -1;
            listView.HotItemChanged += (_, e) => {
                if (e.HotRowIndex != 1)
                {
                    return;
                }

                observed = e;
                hotRowDuringEvent = listView.HotRowIndex;
            };

            listView.ApplyHotItem(MakeHit(listView, second, 0, HitTestLocation.Text));

            Assert.IsNotNull(observed);
            Assert.AreEqual(0, observed.OldHotRowIndex);
            Assert.AreEqual(1, observed.HotRowIndex);
            Assert.AreEqual(HitTestLocation.InCell, observed.OldHotCellHitLocation);
            Assert.AreEqual(HitTestLocation.Text, observed.HotCellHitLocation);
            Assert.AreEqual(0, hotRowDuringEvent);
            Assert.AreEqual(1, listView.HotRowIndex);
        }

        [TestMethod]
        public void HotItemChanged_NonDetailsViewNormalizesHitToColumnZero()
        {
            var first = new Model("first", "one");
            var second = new Model("second", "two");
            using var listView = CreateHotList(View.LargeIcon, first, second);

            HotItemChangedEventArgs observed = null;
            listView.HotItemChanged += (_, e) => {
                if (e.HotRowIndex == 1)
                {
                    observed = e;
                }
            };

            listView.ApplyHotItem(MakeHit(listView, second, 1, HitTestLocation.Text));

            Assert.IsNotNull(observed);
            Assert.AreEqual(0, observed.HotColumnIndex);
            Assert.AreEqual(0, listView.HotColumnIndex);
        }

        [TestMethod]
        public void HyperlinksKeepHotTrackingActiveWithoutHotItemStyling()
        {
            var first = new Model("first", "one");
            var second = new Model("second", "two");
            using var listView = CreateHyperlinkList(new[] { first, second });
            listView.UseHotItem = false;
            listView.SetObjects(new[] { first, second });

            listView.ApplyHotItem(MakeHit(listView, second, 1, HitTestLocation.Text));

            Assert.AreEqual(1, listView.HotRowIndex);
            Assert.AreEqual(1, listView.HotColumnIndex);
            Assert.AreEqual(HitTestLocation.Text, listView.HotCellHitLocation);
        }

        [TestMethod]
        public void FastObjectListView_NonDetailsHyperlinkItemRetainsUrl()
        {
            var model = new Model("https://example.test/fast", null);
            using var listView = new FastObjectListView {
                UseHyperlinks = true,
                ShowGroups = false,
                View = View.LargeIcon
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)) { Hyperlink = true });

            listView.SetObjects(new[] { model });

            var item = listView.GetItem(0);
            Assert.IsNotNull(item);
            Assert.AreEqual("https://example.test/fast", item.GetSubItem(0).Url);
        }

        private static ProbeObjectListView CreateHyperlinkList(IEnumerable<Model> models)
        {
            var listView = new ProbeObjectListView {
                UseHyperlinks = true,
                ShowGroups = false,
                View = View.Details
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            listView.Columns.Add(new OLVColumn("Link", nameof(Model.Link)) { Hyperlink = true });
            listView.SetObjects(models);
            return listView;
        }

        private static ProbeObjectListView CreateHotList(View view, params Model[] models)
        {
            var listView = new ProbeObjectListView {
                UseHotItem = true,
                ShowGroups = false,
                View = view
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            listView.Columns.Add(new OLVColumn("Link", nameof(Model.Link)));
            listView.SetObjects(models);
            return listView;
        }

        private static OlvListViewHitTestInfo MakeHit(
            ObjectListView listView,
            Model model,
            int columnIndex,
            HitTestLocation location)
        {
            var item = GetItem(listView, model);
            var hit = new OlvListViewHitTestInfo(
                item,
                item.GetSubItem(columnIndex),
                0,
                null,
                columnIndex)
            {
                HitTestLocation = location
            };
            return hit;
        }

        private static OLVListItem GetItem(ObjectListView listView, object model)
        {
            var index = listView.IndexOf(model);
            Assert.IsGreaterThanOrEqualTo(0, index);
            var item = listView.GetItem(index);
            Assert.IsNotNull(item);
            return item;
        }

        private static OLVListSubItem GetSubItem(ObjectListView listView, object model, int columnIndex)
        {
            var subItem = GetItem(listView, model).GetSubItem(columnIndex);
            Assert.IsNotNull(subItem);
            return subItem;
        }

        private sealed class ProbeObjectListView : ObjectListView
        {
            public void ApplyHotItem(OlvListViewHitTestInfo hitTest) => UpdateHotItem(hitTest);

            public void SetHotCellLocation(HitTestLocation location) => HotCellHitLocation = location;
        }

        private sealed class Model
        {
            public Model(string name, string link)
            {
                Name = name;
                Link = link;
            }

            public string Name { get; }
            public string Link { get; }
        }
    }
}
