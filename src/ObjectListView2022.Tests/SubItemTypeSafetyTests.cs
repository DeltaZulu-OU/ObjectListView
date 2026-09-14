using System.Windows.Forms;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
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
    }
}
