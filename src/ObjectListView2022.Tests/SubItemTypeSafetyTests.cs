using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class SubItemTypeSafetyTests
    {
        [TestMethod]
        public void HasAnyHyperlinks_IgnoresPlainListViewSubItems()
        {
            var item = new OLVListItem(new object());
            item.SubItems.Add(new ListViewItem.ListViewSubItem(null, "plain"));

            Assert.IsFalse(item.HasAnyHyperlinks);
        }

        [TestMethod]
        public void HasAnyHyperlinks_DetectsOlvSubItemUrl()
        {
            var item = new OLVListItem(new object());
            var subItem = new OLVListSubItem(null, "link", null) { Url = "https://example.invalid/" };
            item.SubItems.Add(subItem);

            Assert.IsTrue(item.HasAnyHyperlinks);
        }

        [TestMethod]
        public void GetSubItem_ReturnsNullForPlainListViewSubItem()
        {
            var item = new OLVListItem(new object());
            item.SubItems.Add(new ListViewItem.ListViewSubItem(null, "plain"));

            Assert.IsNull(item.GetSubItem(1));
        }

        [TestMethod]
        public void GetSubItem_ReturnsOlvListSubItem()
        {
            var item = new OLVListItem(new object());
            var subItem = new OLVListSubItem(null, "typed", null);
            item.SubItems.Add(subItem);

            Assert.AreSame(subItem, item.GetSubItem(1));
        }

        [TestMethod]
        public void DrawAllDecorations_SkipsPlainSubItemsAndDrawsTypedDecoration()
        {
            using var listView = new DecorationProbeObjectListView();
            var item = new OLVListItem(new object());
            var subItem = new OLVListSubItem(null, "typed", null);
            var decoration = new RecordingDecoration();
            subItem.Decoration = decoration;
            item.SubItems.Add(new ListViewItem.ListViewSubItem(null, "plain"));
            item.SubItems.Add(subItem);
            using var bitmap = new Bitmap(16, 16);
            using var graphics = Graphics.FromImage(bitmap);

            listView.DrawDecorations(graphics, new List<OLVListItem> { item });

            Assert.AreEqual(1, decoration.DrawCallCount);
            Assert.AreSame(item, decoration.ListItem);
            Assert.AreSame(subItem, decoration.SubItem);
        }

        private sealed class DecorationProbeObjectListView : ObjectListView
        {
            public void DrawDecorations(Graphics graphics, List<OLVListItem> items) =>
                DrawAllDecorations(graphics, items);
        }

        private sealed class RecordingDecoration : AbstractDecoration
        {
            public int DrawCallCount { get; private set; }

            public override void Draw(ObjectListView olv, Graphics g, Rectangle r) => DrawCallCount++;
        }
    }
}
