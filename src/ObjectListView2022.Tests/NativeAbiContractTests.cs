using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class NativeAbiContractTests
    {
        [TestMethod]
        public void NativeNotificationStructures_UsePointerSizedHandleFields()
        {
            AssertFieldType("NMHDR", "hwndFrom", typeof(IntPtr));
            AssertFieldType("NMHDR", "idFrom", typeof(IntPtr));
            AssertFieldType("NMHEADER", "pHDITEM", typeof(IntPtr));
            AssertFieldType("NMCUSTOMDRAW", "hdc", typeof(IntPtr));
            AssertFieldType("NMCUSTOMDRAW", "dwItemSpec", typeof(IntPtr));
            AssertFieldType("NMCUSTOMDRAW", "lItemlParam", typeof(IntPtr));
            AssertFieldType("NMLISTVIEW", "lParam", typeof(IntPtr));
            AssertFieldType("LVITEM", "lParam", typeof(IntPtr));
            AssertFieldType("LVITEM", "puColumns", typeof(IntPtr));
            AssertFieldType("WINDOWPOS", "hwnd", typeof(IntPtr));
            AssertFieldType("WINDOWPOS", "hwndInsertAfter", typeof(IntPtr));
        }

        [TestMethod]
        public void NativeNotificationStructures_HaveExpectedPointerAwareLayout()
        {
            var nmhdr = GetNativeType("NMHDR");
            Assert.AreEqual(IntPtr.Zero, Marshal.OffsetOf(nmhdr, "hwndFrom"));
            Assert.AreEqual(new IntPtr(IntPtr.Size), Marshal.OffsetOf(nmhdr, "idFrom"));
            Assert.AreEqual(new IntPtr(IntPtr.Size * 2), Marshal.OffsetOf(nmhdr, "code"));
            Assert.AreEqual(IntPtr.Size == 8 ? 24 : 12, Marshal.SizeOf(nmhdr));

            var nmheader = GetNativeType("NMHEADER");
            var nmhdrSize = Marshal.SizeOf(nmhdr);
            Assert.AreEqual(new IntPtr(nmhdrSize), Marshal.OffsetOf(nmheader, "iItem"));
            Assert.AreEqual(new IntPtr(nmhdrSize + sizeof(int)), Marshal.OffsetOf(nmheader, "iButton"));
            Assert.AreEqual(new IntPtr(nmhdrSize + (2 * sizeof(int))), Marshal.OffsetOf(nmheader, "pHDITEM"));
            Assert.AreEqual(IntPtr.Size == 8 ? 40 : 24, Marshal.SizeOf(nmheader));
        }

        [TestMethod]
        public void CoreNativeEntryPoints_UseWin32CompatibleReturnTypes()
        {
            AssertPInvokeReturnType("DeleteObject", typeof(bool), "gdi32.dll");
            AssertPInvokeReturnType("SetWindowPos", typeof(bool), "user32.dll");
            AssertPInvokeReturnType("InvalidateRect", typeof(bool), "user32.dll");
            AssertPInvokeReturnType("GetClientRect", typeof(bool), "user32.dll");
        }

        [TestMethod]
        public void SetWindowLongPtr64_UsesPointerSizedValueParameter()
        {
            var method = GetNativeMethodsType().GetMethod(
                "SetWindowLongPtr64",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(IntPtr), method.ReturnType);

            var parameters = method.GetParameters();
            Assert.HasCount(3, parameters);
            Assert.AreEqual(typeof(IntPtr), parameters[0].ParameterType);
            Assert.AreEqual(typeof(int), parameters[1].ParameterType);
            Assert.AreEqual(
                typeof(IntPtr),
                parameters[2].ParameterType,
                "SetWindowLongPtr receives a LONG_PTR value, which must remain pointer-sized on 64-bit Windows.");

            var import = method.GetCustomAttribute<DllImportAttribute>();
            Assert.IsNotNull(import);
            Assert.AreEqual("user32.dll", import.Value, ignoreCase: true);
            Assert.AreEqual("SetWindowLongPtr", import.EntryPoint);
        }

        private static void AssertFieldType(string structureName, string fieldName, Type expectedType)
        {
            var field = GetNativeType(structureName).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(field, $"NativeMethods.{structureName}.{fieldName} was not found.");
            Assert.AreEqual(expectedType, field.FieldType, $"{structureName}.{fieldName} must be pointer-sized.");
        }

        private static void AssertPInvokeReturnType(string methodName, Type expectedReturnType, string expectedLibrary)
        {
            var methods = GetNativeMethodsType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name == methodName)
                .ToArray();

            Assert.HasCount(1, methods, $"Expected a single NativeMethods.{methodName} entry point.");
            var method = methods[0];
            Assert.AreEqual(expectedReturnType, method.ReturnType, $"NativeMethods.{methodName} has the wrong Win32 return type.");

            var import = method.GetCustomAttribute<DllImportAttribute>();
            Assert.IsNotNull(import, $"NativeMethods.{methodName} must be a P/Invoke entry point.");
            Assert.AreEqual(expectedLibrary, import.Value, ignoreCase: true);
        }

        private static Type GetNativeType(string name)
        {
            var type = GetNativeMethodsType().GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(type, $"NativeMethods.{name} was not found.");
            return type;
        }

        private static Type GetNativeMethodsType() =>
            typeof(ObjectListView).Assembly.GetType(
                "BrightIdeasSoftware.NativeMethods",
                throwOnError: true);
    }
}
