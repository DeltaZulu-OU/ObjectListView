using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class HeaderMarshallingTests
    {
        private const int HdnItemChangingW = -320;
        private const int HdnTrackW = -328;

        [TestMethod]
        public void TryMarshalStructure_ReturnsFalseForArgumentOutOfRangeException()
        {
            var method = GetTryMarshalStructure().MakeGenericMethod(typeof(int));
            object[] arguments = {
                new Func<int>(() => throw new ArgumentOutOfRangeException()),
                0
            };

            var result = (bool)method.Invoke(null, arguments);

            Assert.IsFalse(result);
            Assert.AreEqual(0, arguments[1]);
        }

        [TestMethod]
        public void TryMarshalStructure_DoesNotSuppressOtherExceptions()
        {
            var method = GetTryMarshalStructure().MakeGenericMethod(typeof(int));
            object[] arguments = {
                new Func<int>(() => throw new InvalidOperationException()),
                0
            };

            try
            {
                method.Invoke(null, arguments);
                Assert.Fail("Expected InvalidOperationException to propagate through reflection.");
            }
            catch (TargetInvocationException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(InvalidOperationException));
            }
        }

        [TestMethod]
        public void HandleNotify_TrackMarshalsHeaderItemAndClampsWidth()
        {
            using var listView = new HeaderNotifyProbeObjectListView();
            listView.Columns.Add(new OLVColumn("Name", "Name") { MinimumWidth = 100 });
            using var nativeMessage = new NativeHeaderMessage(HdnTrackW, 0, 25, 1);
            var message = nativeMessage.Message;

            var handled = listView.InvokeHandleNotify(ref message);

            Assert.IsFalse(handled);
            Assert.AreEqual(100, nativeMessage.ReadWidth());
        }

        [TestMethod]
        public void HandleNotify_ItemChangingMarshalsHeaderItemAndRejectsOutOfRangeWidth()
        {
            using var listView = new HeaderNotifyProbeObjectListView();
            listView.Columns.Add(new OLVColumn("Name", "Name") { MinimumWidth = 100 });
            using var nativeMessage = new NativeHeaderMessage(HdnItemChangingW, 0, 25, 1);
            var message = nativeMessage.Message;

            var handled = listView.InvokeHandleNotify(ref message);

            Assert.IsTrue(handled);
            Assert.AreEqual(new IntPtr(1), message.Result);
        }

        [TestMethod]
        public void HandleNotify_InvalidColumnSkipsHeaderItemMarshalling()
        {
            using var listView = new HeaderNotifyProbeObjectListView();
            listView.Columns.Add(new OLVColumn("Name", "Name"));
            using var nativeMessage = new NativeHeaderMessage(HdnTrackW, 1, 25, 1, allocateHeaderItem: false);
            var message = nativeMessage.Message;

            var handled = listView.InvokeHandleNotify(ref message);

            Assert.IsFalse(handled);
        }

        private static MethodInfo GetTryMarshalStructure()
        {
            var method = typeof(ObjectListView).GetMethod(
                "TryMarshalStructure",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            Assert.IsTrue(method.IsGenericMethodDefinition);
            return method;
        }

        private sealed class HeaderNotifyProbeObjectListView : ObjectListView
        {
            public bool InvokeHandleNotify(ref Message message) => HandleNotify(ref message);
        }

        private sealed class NativeHeaderMessage : IDisposable
        {
            private readonly Type hdItemType;
            private readonly IntPtr headerPointer;
            private readonly IntPtr hdItemPointer;

            public NativeHeaderMessage(int code, int itemIndex, int width, int mask, bool allocateHeaderItem = true)
            {
                var assembly = typeof(ObjectListView).Assembly;
                var nmhdrType = assembly.GetType("BrightIdeasSoftware.Implementation.NativeMethods+NMHDR", true);
                var nmheaderType = assembly.GetType("BrightIdeasSoftware.Implementation.NativeMethods+NMHEADER", true);
                hdItemType = assembly.GetType("BrightIdeasSoftware.Implementation.NativeMethods+HDITEM", true);

                if (allocateHeaderItem)
                {
                    var hdItem = Activator.CreateInstance(hdItemType);
                    hdItemType.GetField("mask").SetValue(hdItem, mask);
                    hdItemType.GetField("cxy").SetValue(hdItem, width);
                    hdItemPointer = Marshal.AllocHGlobal(Marshal.SizeOf(hdItemType));
                    Marshal.StructureToPtr(hdItem, hdItemPointer, false);
                }

                var nmhdr = Activator.CreateInstance(nmhdrType);
                nmhdrType.GetField("code").SetValue(nmhdr, code);

                var nmheader = Activator.CreateInstance(nmheaderType);
                nmheaderType.GetField("nhdr").SetValue(nmheader, nmhdr);
                nmheaderType.GetField("iItem").SetValue(nmheader, itemIndex);
                nmheaderType.GetField("pHDITEM").SetValue(nmheader, hdItemPointer);

                headerPointer = Marshal.AllocHGlobal(Marshal.SizeOf(nmheaderType));
                Marshal.StructureToPtr(nmheader, headerPointer, false);
                Message = Message.Create(IntPtr.Zero, 0x004E, IntPtr.Zero, headerPointer);
            }

            public Message Message { get; }

            public int ReadWidth()
            {
                var hdItem = Marshal.PtrToStructure(hdItemPointer, hdItemType);
                return (int)hdItemType.GetField("cxy").GetValue(hdItem);
            }

            public void Dispose()
            {
                if (headerPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(headerPointer);
                }
                if (hdItemPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(hdItemPointer);
                }
            }
        }
    }
}
