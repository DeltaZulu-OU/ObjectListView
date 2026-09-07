using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class KeyboardContextMenuTests
    {
        [TestMethod]
        public void ContextMenuKey_RaisesCellRightClickForFocusedItem()
        {
            using var listView = CreateListView(out var model);
            CellRightClickEventArgs received = null;
            listView.CellRightClick += (sender, args) =>
            {
                received = args;
                args.Handled = true;
            };

            Assert.IsTrue(listView.ProcessKey(Keys.Apps));
            Assert.IsNotNull(received);
            Assert.AreSame(model, received.Model);
            Assert.AreEqual(0, received.ColumnIndex);
        }

        [TestMethod]
        public void ShiftF10_RaisesCellRightClickForFocusedItem()
        {
            using var listView = CreateListView(out _);
            var eventCount = 0;
            listView.CellRightClick += (sender, args) =>
            {
                eventCount++;
                args.Handled = true;
            };

            Assert.IsTrue(listView.ProcessKey(Keys.Shift | Keys.F10));
            Assert.AreEqual(1, eventCount);
        }

        [TestMethod]
        public void KeyboardContextMenu_WithoutCustomHandlingFallsBackToWinForms()
        {
            using var listView = CreateListView(out _);
            var eventCount = 0;
            listView.CellRightClick += (sender, args) => eventCount++;

            Assert.IsFalse(listView.ProcessKeyboardContextMenu());
            Assert.AreEqual(1, eventCount);
        }

        private static TestObjectListView CreateListView(out Model model)
        {
            var listView = new TestObjectListView
            {
                View = View.Details
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            model = new Model("focused");
            listView.SetObjects(new[] { model });

            listView.GetItem(0).Focused = true;
            return listView;
        }

        private sealed class TestObjectListView : ObjectListView
        {
            public bool ProcessKey(Keys keyData)
            {
                var message = Message.Create(Handle, 0, System.IntPtr.Zero, System.IntPtr.Zero);
                return ProcessCmdKey(ref message, keyData);
            }

            public bool ProcessKeyboardContextMenu() => HandleKeyboardContextMenu();
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
