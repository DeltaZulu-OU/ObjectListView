using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class ToolTipLifecycleTests
    {
        [TestMethod]
        public void HandleDestroy_AfterNativeHandleIsGoneDoesNotScheduleTooltipWork()
        {
            using var listView = new ToolTipProbeObjectListView();
            listView.CreateControl();
            _ = listView.CellToolTip;

            listView.DestroyHandleForTest();
            listView.InvokeHandleDestroyForTest();

            Assert.AreEqual(0, listView.UpdateCellToolTipHandleCallCount);
        }

        [TestMethod]
        public void RecreateHandle_RestoresCellToolTipDuringHandleCreation()
        {
            using var form = new Form();
            using var listView = new ToolTipProbeObjectListView();
            form.Controls.Add(listView);
            form.CreateControl();
            listView.CreateControl();
            _ = listView.CellToolTip;
            listView.ResetUpdateCount();

            listView.RecreateHandleForTest();

            Assert.AreEqual(1, listView.UpdateCellToolTipHandleCallCount);
        }

        [TestMethod]
        public void RecreateHandle_TwiceRestoresCellToolTipOncePerHandle()
        {
            using var form = new Form();
            using var listView = new ToolTipProbeObjectListView();
            form.Controls.Add(listView);
            form.CreateControl();
            listView.CreateControl();
            _ = listView.CellToolTip;
            listView.ResetUpdateCount();

            listView.RecreateHandleForTest();
            listView.RecreateHandleForTest();

            Assert.AreEqual(2, listView.UpdateCellToolTipHandleCallCount);
        }

        [TestMethod]
        public void RecreateHandle_WithoutCellToolTipDoesNotAttemptRestoration()
        {
            using var form = new Form();
            using var listView = new ToolTipProbeObjectListView();
            form.Controls.Add(listView);
            form.CreateControl();
            listView.CreateControl();
            listView.ResetUpdateCount();

            listView.RecreateHandleForTest();

            Assert.AreEqual(0, listView.UpdateCellToolTipHandleCallCount);
        }

        [TestMethod]
        public void Dispose_WithCellToolTip_DoesNotRestoreDestroyedHandle()
        {
            using var form = new Form();
            var listView = new ToolTipProbeObjectListView();
            form.Controls.Add(listView);
            form.CreateControl();
            listView.CreateControl();
            _ = listView.CellToolTip;
            listView.ResetUpdateCount();

            listView.Dispose();
            Application.DoEvents();

            Assert.AreEqual(0, listView.UpdateCellToolTipHandleCallCount);
        }

        private sealed class ToolTipProbeObjectListView : ObjectListView
        {
            public int UpdateCellToolTipHandleCallCount { get; private set; }

            public void DestroyHandleForTest() => DestroyHandle();

            public void InvokeHandleDestroyForTest()
            {
                var message = Message.Create(IntPtr.Zero, 0x0002, IntPtr.Zero, IntPtr.Zero);
                base.HandleDestroy(ref message);
            }

            public void RecreateHandleForTest() => RecreateHandle();

            public void ResetUpdateCount() => UpdateCellToolTipHandleCallCount = 0;

            protected override void UpdateCellToolTipHandle()
            {
                UpdateCellToolTipHandleCallCount++;
                base.UpdateCellToolTipHandle();
            }
        }
    }
}
