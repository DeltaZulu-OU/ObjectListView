using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class NativeHandleLifecycleTests
    {
        [TestMethod]
        public void RecreateHandle_ReplacesHeaderControlWrapperAndPreservesWordWrap()
        {
            using var form = new Form();
            using var listView = CreateListView();
            listView.HeaderWordWrap = true;
            form.Controls.Add(listView);
            form.CreateControl();
            _ = listView.Handle;

            var firstHeader = listView.HeaderControl;
            Assert.IsTrue(firstHeader.WordWrap);
            Assert.AreNotEqual(IntPtr.Zero, firstHeader.Handle);

            listView.RecreateHandleForTest();

            var secondHeader = listView.HeaderControl;
            Assert.AreNotSame(firstHeader, secondHeader);
            Assert.IsTrue(secondHeader.WordWrap);
            Assert.AreNotEqual(IntPtr.Zero, secondHeader.Handle);
        }

        [TestMethod]
        public void RecreateHandle_RepeatedlyKeepsCurrentHeaderUsable()
        {
            using var form = new Form();
            using var listView = CreateListView();
            form.Controls.Add(listView);
            form.CreateControl();
            _ = listView.Handle;

            var seenHeaders = new List<HeaderControl>();
            for (var i = 0; i < 5; i++)
            {
                var header = listView.HeaderControl;
                Assert.AreNotEqual(IntPtr.Zero, header.Handle);
                Assert.DoesNotContain(header, seenHeaders);
                seenHeaders.Add(header);

                listView.RecreateHandleForTest();
            }

            var currentHeader = listView.HeaderControl;
            Assert.AreNotEqual(IntPtr.Zero, currentHeader.Handle);
            Assert.DoesNotContain(currentHeader, seenHeaders);
            Assert.IsGreaterThanOrEqualTo(0, currentHeader.ClientRectangle.Width);
            Assert.IsGreaterThanOrEqualTo(0, currentHeader.ClientRectangle.Height);
        }

        [TestMethod]
        public void RecreateHandle_RecreatesHeaderToolTipWithCurrentHeader()
        {
            using var form = new Form();
            using var listView = CreateListView();
            form.Controls.Add(listView);
            form.CreateControl();
            _ = listView.Handle;

            var firstHeader = listView.HeaderControl;
            var firstToolTip = firstHeader.ToolTip;
            Assert.AreNotEqual(IntPtr.Zero, firstToolTip.Handle);

            listView.RecreateHandleForTest();

            var secondHeader = listView.HeaderControl;
            var secondToolTip = secondHeader.ToolTip;
            Assert.AreNotSame(firstHeader, secondHeader);
            Assert.AreNotSame(firstToolTip, secondToolTip);
            Assert.AreNotEqual(IntPtr.Zero, secondHeader.Handle);
            Assert.AreNotEqual(IntPtr.Zero, secondToolTip.Handle);
        }

        private static HandleProbeObjectListView CreateListView()
        {
            var listView = new HandleProbeObjectListView
            {
                View = View.Details,
                Size = new System.Drawing.Size(320, 180)
            };
            listView.Columns.Add(new OLVColumn("Name", "Name") { Width = 160 });
            return listView;
        }

        private sealed class HandleProbeObjectListView : ObjectListView
        {
            public void RecreateHandleForTest() => RecreateHandle();
        }
    }
}
