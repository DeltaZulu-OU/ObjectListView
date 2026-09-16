using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DragDropBehaviorTests
    {
        [TestMethod]
        public void SimpleDragSource_StartDragOnlyAcceptsLeftMouseButton()
        {
            using var listView = CreateList(new Model("one"), new Model("two"));
            listView.SelectedObject = listView.GetModelObject(1);
            var source = new SimpleDragSource();

            Assert.IsNull(source.StartDrag(listView, MouseButtons.Right, listView.GetItem(1)));
            Assert.IsNull(source.StartDrag(listView, MouseButtons.Middle, listView.GetItem(1)));

            var dragObject = source.StartDrag(listView, MouseButtons.Left, listView.GetItem(1));
            Assert.IsInstanceOfType<OLVDataObject>(dragObject);

            var data = (OLVDataObject)dragObject;
            Assert.AreSame(listView, data.ListView);
            Assert.AreSequenceEqual(
                new object[] { listView.GetModelObject(1) },
                data.ModelObjects.Cast<object>().ToArray());
        }

        [TestMethod]
        public void SimpleDragSource_DefaultAllowedEffectsIncludeLink()
        {
            var source = new SimpleDragSource();

            var effects = source.GetAllowedEffects(new object());

            Assert.AreEqual(DragDropEffects.All | DragDropEffects.Link, effects);
            Assert.AreEqual(DragDropEffects.Link, effects & DragDropEffects.Link);
        }

        [TestMethod]
        public void SimpleDropSink_DefaultConfigurationMatchesHistoricalContract()
        {
            var sink = new SimpleDropSink();

            Assert.IsTrue(sink.CanDropOnItem);
            Assert.IsFalse(sink.CanDropOnBackground);
            Assert.IsFalse(sink.CanDropBetween);
            Assert.IsFalse(sink.CanDropOnSubItem);
            Assert.IsTrue(sink.AcceptExternal);
            Assert.IsTrue(sink.AutoScroll);
            Assert.IsTrue(sink.EnableFeedback);
        }

        [TestMethod]
        public void SimpleDropSink_StandardDropActionFollowsModifierKeys()
        {
            var sink = new SimpleDropSink
            {
                KeyState = 0
            };
            Assert.AreEqual(DragDropEffects.Move, sink.CalculateStandardDropActionFromKeys());

            sink.KeyState = 8; // MK_CONTROL
            Assert.AreEqual(DragDropEffects.Copy, sink.CalculateStandardDropActionFromKeys());

            sink.KeyState = 8 | 4; // MK_CONTROL | MK_SHIFT
            Assert.AreEqual(DragDropEffects.Link, sink.CalculateStandardDropActionFromKeys());

            sink.KeyState = 4; // MK_SHIFT alone does not change the historical default
            Assert.AreEqual(DragDropEffects.Move, sink.CalculateStandardDropActionFromKeys());
        }

        [TestMethod]
        public void SimpleDropSink_CanDropRaisesModelEventBeforeGenericEvent()
        {
            using var sourceList = CreateList(new Model("source"));
            using var targetList = CreateList(new Model("target"));
            using var form = Host(sourceList, targetList);
            var sink = new SimpleDropSink
            {
                ListView = targetList,
                AutoScroll = false
            };
            var order = new List<string>();
            ModelDropEventArgs modelArgs = null;

            sink.ModelCanDrop += (sender, args) =>
            {
                order.Add("model");
                modelArgs = args;
                args.Effect = DragDropEffects.Copy;
            };
            sink.CanDrop += (sender, args) => order.Add("generic");

            var dragArgs = MakeDragEventArgs(sourceList, targetList, new[] { sourceList.GetModelObject(0) });
            sink.Enter(dragArgs);

            Assert.AreSequenceEqual(new[] { "model", "generic" }, order);
            Assert.IsNotNull(modelArgs);
            Assert.AreSame(sourceList, modelArgs.SourceListView);
            Assert.AreSame(targetList, modelArgs.ListView);
            Assert.AreSequenceEqual(
                new object[] { sourceList.GetModelObject(0) },
                modelArgs.SourceModels.Cast<object>().ToArray());
            Assert.AreEqual(DragDropEffects.Copy, dragArgs.Effect);
        }

        [TestMethod]
        public void SimpleDropSink_HandledModelCanDropSuppressesGenericCanDrop()
        {
            using var sourceList = CreateList(new Model("source"));
            using var targetList = CreateList(new Model("target"));
            using var form = Host(sourceList, targetList);
            var sink = new SimpleDropSink
            {
                ListView = targetList,
                AutoScroll = false
            };
            var genericCount = 0;

            sink.ModelCanDrop += (sender, args) =>
            {
                args.Effect = DragDropEffects.Link;
                args.Handled = true;
            };
            sink.CanDrop += (sender, args) => genericCount++;

            var dragArgs = MakeDragEventArgs(sourceList, targetList, new[] { sourceList.GetModelObject(0) });
            sink.Enter(dragArgs);

            Assert.AreEqual(0, genericCount);
            Assert.AreEqual(DragDropEffects.Link, dragArgs.Effect);
        }

        [TestMethod]
        public void SimpleDropSink_DroppedRaisesModelEventBeforeGenericEvent()
        {
            using var sourceList = CreateList(new Model("source"));
            using var targetList = CreateList(new Model("target"));
            using var form = Host(sourceList, targetList);
            var sink = new SimpleDropSink { ListView = targetList };
            var order = new List<string>();

            sink.ModelDropped += (sender, args) => order.Add("model");
            sink.Dropped += (sender, args) => order.Add("generic");

            var dragArgs = MakeDragEventArgs(sourceList, targetList, new[] { sourceList.GetModelObject(0) });
            sink.Enter(dragArgs);
            order.Clear();

            sink.Drop(dragArgs);

            Assert.AreSequenceEqual(new[] { "model", "generic" }, order);
        }

        [TestMethod]
        public void SimpleDropSink_HandledModelDroppedSuppressesGenericDropped()
        {
            using var sourceList = CreateList(new Model("source"));
            using var targetList = CreateList(new Model("target"));
            using var form = Host(sourceList, targetList);
            var sink = new SimpleDropSink { ListView = targetList };
            var genericCount = 0;

            sink.ModelDropped += (sender, args) => args.Handled = true;
            sink.Dropped += (sender, args) => genericCount++;

            var dragArgs = MakeDragEventArgs(sourceList, targetList, new[] { sourceList.GetModelObject(0) });
            sink.Enter(dragArgs);
            sink.Drop(dragArgs);

            Assert.AreEqual(0, genericCount);
        }

        [TestMethod]
        public void SimpleDropSink_RejectsExternalModelBeforeModelCanDropEventWhenDisabled()
        {
            using var sourceList = CreateList(new Model("source"));
            using var targetList = CreateList(new Model("target"));
            using var form = Host(sourceList, targetList);
            var sink = new SimpleDropSink
            {
                ListView = targetList,
                AcceptExternal = false,
                AutoScroll = false
            };
            var modelEventCount = 0;
            var genericEventCount = 0;

            sink.ModelCanDrop += (sender, args) => modelEventCount++;
            sink.CanDrop += (sender, args) => genericEventCount++;

            var dragArgs = MakeDragEventArgs(sourceList, targetList, new[] { sourceList.GetModelObject(0) });
            sink.Enter(dragArgs);

            Assert.AreEqual(0, modelEventCount);
            Assert.AreEqual(1, genericEventCount);
            Assert.AreEqual(DragDropEffects.None, dragArgs.Effect);
            Assert.AreEqual(DropTargetLocation.None, sink.DropTargetLocation);
        }

        [TestMethod]
        public void RearrangingDropSink_DefaultsToBetweenAndBackgroundTargets()
        {
            var sink = new RearrangingDropSink();

            Assert.IsTrue(sink.CanDropBetween);
            Assert.IsTrue(sink.CanDropOnBackground);
            Assert.IsFalse(sink.CanDropOnItem);
            Assert.IsTrue(sink.AcceptExternal);
        }

        [TestMethod]
        public void RearrangingDropSink_MovesModelsWithinSameObjectListView()
        {
            var first = new Model("first");
            var second = new Model("second");
            var third = new Model("third");
            using var listView = CreateList(first, second, third);
            using var form = Host(listView);
            var sink = new RearrangingDropSink { ListView = listView };

            sink.ModelCanDrop += (sender, args) =>
            {
                args.DropTargetLocation = DropTargetLocation.AboveItem;
                args.DropTargetIndex = 0;
            };

            var dragArgs = MakeDragEventArgs(listView, listView, new[] { second });
            sink.Enter(dragArgs);
            sink.Drop(dragArgs);

            Assert.AreSequenceEqual(
                new object[] { second, first, third },
                Enumerable.Range(0, listView.GetItemCount())
                    .Select(listView.GetModelObject)
                    .ToArray());
        }

        [TestMethod]
        public void RearrangingDropSink_ExternalBackgroundDropMovesModelsBetweenLists()
        {
            var first = new Model("first");
            var moved = new Model("moved");
            var target = new Model("target");
            using var sourceList = CreateList(first, moved);
            using var targetList = CreateList(target);
            using var form = Host(sourceList, targetList);
            var sink = new RearrangingDropSink(true) { ListView = targetList };

            sink.ModelCanDrop += (sender, args) =>
            {
                args.DropTargetLocation = DropTargetLocation.Background;
                args.DropTargetIndex = -1;
            };

            var dragArgs = MakeDragEventArgs(sourceList, targetList, new[] { moved });
            sink.Enter(dragArgs);
            sink.Drop(dragArgs);

            Assert.AreSequenceEqual(
                new object[] { first },
                Enumerable.Range(0, sourceList.GetItemCount())
                    .Select(sourceList.GetModelObject)
                    .ToArray());
            Assert.AreSequenceEqual(
                new object[] { target, moved },
                Enumerable.Range(0, targetList.GetItemCount())
                    .Select(targetList.GetModelObject)
                    .ToArray());
        }

        private static ObjectListView CreateList(params Model[] models)
        {
            var listView = new ObjectListView
            {
                View = View.Details,
                FullRowSelect = true,
                Size = new Size(240, 140)
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)) { Width = 180 });
            listView.SetObjects(models);
            return listView;
        }

        private static Form Host(params ObjectListView[] listViews)
        {
            var form = new Form { Size = new Size(600, 240) };
            var left = 10;
            foreach (var listView in listViews)
            {
                listView.Location = new Point(left, 10);
                form.Controls.Add(listView);
                left += listView.Width + 10;
            }

            form.CreateControl();
            foreach (var listView in listViews)
            {
                listView.CreateControl();
                _ = listView.Handle;
            }

            return form;
        }

        private static DragEventArgs MakeDragEventArgs(
            ObjectListView source,
            ObjectListView target,
            IEnumerable<object> models)
        {
            var data = new OLVDataObject(source, new ArrayList(models.ToArray()));
            var screenPoint = target.PointToScreen(new Point(10, target.ClientRectangle.Height - 10));
            return new DragEventArgs(
                data,
                0,
                screenPoint.X,
                screenPoint.Y,
                DragDropEffects.All | DragDropEffects.Link,
                DragDropEffects.None);
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
