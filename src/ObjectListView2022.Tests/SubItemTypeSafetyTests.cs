using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
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
            item.SubItems.Clear();
            item.SubItems.Add(new ListViewItem.ListViewSubItem(null, "plain"));

            Assert.IsFalse(item.HasAnyHyperlinks);
        }

        [TestMethod]
        public void GetSubItem_ReturnsNullForPlainListViewSubItem()
        {
            var item = new OLVListItem(new object());
            item.SubItems.Clear();
            item.SubItems.Add(new ListViewItem.ListViewSubItem(null, "plain"));

            Assert.IsNull(item.GetSubItem(0));
        }

        [TestMethod]
        public void DrawAllDecorations_IgnoresPlainListViewSubItems()
        {
            using var listView = new DecorationProbeObjectListView();
            var item = new OLVListItem(new object());
            item.SubItems.Clear();
            item.SubItems.Add(new ListViewItem.ListViewSubItem(null, "plain"));
            using var bitmap = new Bitmap(16, 16);
            using var graphics = Graphics.FromImage(bitmap);

            listView.DrawDecorations(graphics, new List<OLVListItem> { item });

            Assert.IsTrue(true);
        }

        private sealed class DecorationProbeObjectListView : ObjectListView
        {
            public void DrawDecorations(Graphics graphics, List<OLVListItem> items) =>
                DrawAllDecorations(graphics, items);
        }
    }
}
