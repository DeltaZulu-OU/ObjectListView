using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class StatePersistenceTests
    {
        [TestMethod]
        public void SaveState_UsesXmlPayload()
        {
            using var listView = CreateList();

            var state = listView.SaveState();
            var text = Encoding.UTF8.GetString(state);

            Assert.Contains("<ObjectListViewState", text);
            Assert.Contains("<NumberOfColumns>2</NumberOfColumns>", text);
        }

        [TestMethod]
        public void SaveAndRestoreState_RoundTripsPersistedSettings()
        {
            using var source = CreateList();
            source.View = View.Details;
            source.AllColumns[0].Width = 123;
            source.AllColumns[0].LastDisplayIndex = 1;
            source.AllColumns[1].Width = 234;
            source.AllColumns[1].LastDisplayIndex = 0;
            source.AllColumns[1].IsVisible = false;
            source.PrimarySortColumn = source.AllColumns[0];
            source.PrimarySortOrder = SortOrder.Descending;
            source.ShowGroups = false;

            var state = source.SaveState();

            using var target = CreateList();
            target.View = View.Details;
            target.AllColumns[0].Width = 50;
            target.AllColumns[1].Width = 60;
            target.AllColumns[1].IsVisible = true;
            target.PrimarySortColumn = null;
            target.PrimarySortOrder = SortOrder.None;
            target.ShowGroups = true;

            Assert.IsTrue(target.RestoreState(state));
            Assert.AreEqual(View.Details, target.View);
            Assert.AreSame(target.AllColumns[0], target.PrimarySortColumn);
            Assert.AreEqual(SortOrder.Descending, target.PrimarySortOrder);
            Assert.AreEqual(123, target.AllColumns[0].Width);
            Assert.AreEqual(234, target.AllColumns[1].Width);
            Assert.AreEqual(1, target.AllColumns[0].LastDisplayIndex);
            Assert.AreEqual(0, target.AllColumns[1].LastDisplayIndex);
            Assert.IsFalse(target.AllColumns[1].IsVisible);
            Assert.IsFalse(target.ShowGroups);
        }

        [TestMethod]
        public void SaveAndRestoreState_RoundTripsView()
        {
            using var source = CreateList();
            source.View = View.Details;

            var state = source.SaveState();

            using var target = CreateList();
            target.View = View.LargeIcon;

            Assert.IsTrue(target.RestoreState(state));
            Assert.AreEqual(View.Details, target.View);
        }

        [TestMethod]
        public void RestoreState_RejectsBinaryPayload()
        {
            var binaryPayload = new byte[] { 0, 1, 0, 0, 0, 255, 255, 255, 255, 1, 0, 0, 0 };

            using var listView = CreateList();

            Assert.IsFalse(listView.RestoreState(binaryPayload));
        }

        [TestMethod]
        public void RestoreState_RejectsDtdPayload()
        {
            var state = Encoding.UTF8.GetBytes(
                "<!DOCTYPE ObjectListViewState [<!ENTITY xxe SYSTEM 'file:///c:/windows/win.ini'>]>" +
                "<ObjectListViewState><NumberOfColumns>2</NumberOfColumns></ObjectListViewState>");

            using var listView = CreateList();

            Assert.IsFalse(listView.RestoreState(state));
        }

        [TestMethod]
        public void RestoreState_RejectsStructurallyInconsistentXml()
        {
            var state = Encoding.UTF8.GetBytes(
                "<ObjectListViewState>" +
                "<VersionNumber>1</VersionNumber>" +
                "<NumberOfColumns>2</NumberOfColumns>" +
                "<CurrentView>Details</CurrentView>" +
                "<SortColumn>-1</SortColumn>" +
                "<IsShowingGroups>false</IsShowingGroups>" +
                "<LastSortOrder>None</LastSortOrder>" +
                "<ColumnIsVisible><boolean>true</boolean></ColumnIsVisible>" +
                "<ColumnDisplayIndicies><int>0</int><int>1</int></ColumnDisplayIndicies>" +
                "<ColumnWidths><int>100</int><int>200</int></ColumnWidths>" +
                "</ObjectListViewState>");

            using var listView = CreateList();

            Assert.IsFalse(listView.RestoreState(state));
        }

        private static ObjectListView CreateList()
        {
            var listView = new ObjectListView { View = View.Details };
            listView.AllColumns.Add(new OLVColumn("Name", "Name") { Width = 100, IsVisible = true });
            listView.AllColumns.Add(new OLVColumn("Value", "Value") { Width = 200, IsVisible = true });
            listView.RebuildColumns();
            return listView;
        }
    }
}
