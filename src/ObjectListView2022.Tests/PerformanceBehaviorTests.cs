using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class PerformanceBehaviorTests
    {
        [TestMethod]
        public void CheckedObjects_SetterHandlesSparseSelection()
        {
            using var listView = new ObjectListView { CheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));

            var models = Enumerable.Range(0, 256)
                .Select(i => new Model("model-" + i))
                .ToArray();
            listView.SetObjects(models);

            listView.CheckedObjects = new ArrayList { models[3], models[200] };

            Assert.AreSequenceEqual(
                new object[] { models[3], models[200] },
                listView.CheckedObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void FastObjectListView_CheckedObjectsSetterHandlesSparseSelection()
        {
            using var listView = new FastObjectListView { CheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));

            var models = Enumerable.Range(0, 256)
                .Select(i => new Model("model-" + i))
                .ToArray();
            listView.SetObjects(models);

            listView.CheckedObjects = new ArrayList { models[3], models[200] };

            Assert.AreSequenceEqual(
                new object[] { models[3], models[200] }, listView.CheckedObjects.Cast<object>().ToArray(), Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
        }

        [TestMethod]
        public void UpdateHotItem_DoesNotRaiseChangedEventForIdenticalHit()
        {
            using var listView = new TestObjectListView { UseHotItem = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            var model = new Model("model");
            listView.SetObjects(new[] { model });

            var item = listView.GetItem(0);
            var hit = new OlvListViewHitTestInfo(item, item.GetSubItem(0), 0, null, 0)
            {
                HitTestLocation = HitTestLocation.InCell
            };
            var changeCount = 0;
            listView.HotItemChanged += (sender, args) => changeCount++;

            listView.ApplyHotItem(hit);
            listView.ApplyHotItem(hit);

            Assert.AreEqual(1, changeCount);
        }

        [TestMethod]
        public void MouseMove_StillRaisesCellOverEvents()
        {
            using var listView = new TestObjectListView();
            listView.Size = new Size(200, 100);
            _ = listView.Handle;

            var eventCount = 0;
            listView.CellOver += (sender, args) => eventCount++;

            listView.ApplyMouseMove(new Point(5, 5));

            Assert.AreEqual(1, eventCount);
        }

        private sealed class TestObjectListView : ObjectListView
        {
            public void ApplyHotItem(OlvListViewHitTestInfo hitTest) => UpdateHotItem(hitTest);

            public void ApplyMouseMove(Point location) =>
                OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, location.X, location.Y, 0));
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
