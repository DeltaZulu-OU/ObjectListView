using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class NativeCallbackLifecycleTests
    {
        private const int HdmLayout = 0x1205;

        [TestMethod]
        public void HeaderLayoutCallback_DoesNotInvalidateReplacementHandle()
        {
            using var form = new Form();
            using var listView = CreateListView();
            form.Controls.Add(listView);
            form.CreateControl();
            _ = listView.Handle;

            var firstHeader = listView.HeaderControl;
            QueueHeaderLayoutCallback(firstHeader, listView.ClientRectangle);

            listView.RecreateHandleForTest();
            var secondHeader = listView.HeaderControl;
            Assert.AreNotSame(firstHeader, secondHeader);

            listView.ResetTrackedInvalidations();
            listView.TrackInvalidations = true;

            var markerRan = false;
            listView.BeginInvoke((MethodInvoker)(() => markerRan = true));
            Application.DoEvents();

            listView.TrackInvalidations = false;

            Assert.IsTrue(markerRan, "The replacement handle must remain able to process queued callbacks.");
            Assert.AreEqual(
                0,
                listView.TrackedInvalidations,
                "A header-layout callback queued for the destroyed handle must not invalidate the replacement ListView handle.");
        }

        private static CallbackProbeObjectListView CreateListView()
        {
            var listView = new CallbackProbeObjectListView
            {
                View = View.Details,
                Size = new Size(320, 180)
            };
            listView.Columns.Add(new OLVColumn("Name", "Name") { Width = 160 });
            return listView;
        }

        private static void QueueHeaderLayoutCallback(HeaderControl header, Rectangle clientRectangle)
        {
            var rectPointer = IntPtr.Zero;
            var windowPosPointer = IntPtr.Zero;
            var layoutPointer = IntPtr.Zero;

            try
            {
                var rect = new NativeRect
                {
                    left = clientRectangle.Left,
                    top = clientRectangle.Top,
                    right = clientRectangle.Right,
                    bottom = clientRectangle.Bottom
                };
                var windowPos = new NativeWindowPos();

                rectPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeRect)));
                windowPosPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeWindowPos)));
                layoutPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeHeaderLayout)));

                Marshal.StructureToPtr(rect, rectPointer, false);
                Marshal.StructureToPtr(windowPos, windowPosPointer, false);
                Marshal.StructureToPtr(
                    new NativeHeaderLayout
                    {
                        prc = rectPointer,
                        pwpos = windowPosPointer
                    },
                    layoutPointer,
                    false);

                var message = Message.Create(header.Handle, HdmLayout, IntPtr.Zero, layoutPointer);
                var method = typeof(HeaderControl).GetMethod(
                    "HandleLayout",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.IsNotNull(method);
                var arguments = new object[] { message };
                var result = (bool)method.Invoke(header, arguments);
                Assert.IsFalse(result, "HeaderControl.HandleLayout should consume HDM_LAYOUT after applying its custom height.");
            }
            finally
            {
                if (layoutPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(layoutPointer);
                }

                if (windowPosPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(windowPosPointer);
                }

                if (rectPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(rectPointer);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeHeaderLayout
        {
            public IntPtr prc;
            public IntPtr pwpos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeWindowPos
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        private sealed class CallbackProbeObjectListView : ObjectListView
        {
            public bool TrackInvalidations { get; set; }

            public int TrackedInvalidations { get; private set; }

            public void RecreateHandleForTest() => RecreateHandle();

            public void ResetTrackedInvalidations() => TrackedInvalidations = 0;

            protected override void OnInvalidated(InvalidateEventArgs e)
            {
                if (TrackInvalidations)
                {
                    TrackedInvalidations++;
                }

                base.OnInvalidated(e);
            }
        }
    }
}
