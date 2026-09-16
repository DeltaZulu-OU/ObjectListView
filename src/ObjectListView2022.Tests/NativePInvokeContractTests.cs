using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class NativePInvokeContractTests
    {
        [TestMethod]
        public void SetWindowTheme_UsesHResultReturnType()
        {
            var method = GetPInvoke("SetWindowTheme");

            Assert.AreEqual(
                typeof(int),
                method.ReturnType,
                "SetWindowTheme returns HRESULT, which is a signed 32-bit value on both x86 and x64 Windows.");

            var parameters = method.GetParameters();
            Assert.HasCount(3, parameters);
            Assert.AreEqual(typeof(IntPtr), parameters[0].ParameterType);
            Assert.AreEqual(typeof(string), parameters[1].ParameterType);
            Assert.AreEqual(typeof(string), parameters[2].ParameterType);

            AssertLibrary(method, "uxtheme.dll");
        }

        [TestMethod]
        public void ValidateRect_UsesWin32BoolAndNativeRect()
        {
            var method = GetPInvoke("ValidatedRectInternal");

            Assert.AreEqual(
                typeof(bool),
                method.ReturnType,
                "ValidateRect returns a Win32 BOOL, not a pointer-sized value.");

            var parameters = method.GetParameters();
            Assert.HasCount(2, parameters);
            Assert.AreEqual(typeof(IntPtr), parameters[0].ParameterType);
            Assert.AreEqual(
                GetNativeRectType().MakeByRefType(),
                parameters[1].ParameterType,
                "ValidateRect must marshal RECT coordinates as left/top/right/bottom rather than Rectangle X/Y/Width/Height.");

            AssertLibrary(method, "user32.dll");
        }

        [TestMethod]
        public void GetWindowRect_UsesNativeRect()
        {
            var method = GetPInvoke("GetWindowRect");

            Assert.AreEqual(typeof(bool), method.ReturnType);

            var parameters = method.GetParameters();
            Assert.HasCount(2, parameters);
            Assert.AreEqual(typeof(IntPtr), parameters[0].ParameterType);
            Assert.AreEqual(
                GetNativeRectType().MakeByRefType(),
                parameters[1].ParameterType,
                "GetWindowRect returns left/top/right/bottom coordinates and must not marshal them as Rectangle width/height fields.");

            AssertLibrary(method, "user32.dll");
        }

        private static MethodInfo GetPInvoke(string name)
        {
            var method = GetNativeMethodsType().GetMethod(
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method, $"NativeMethods.{name} was not found.");
            Assert.IsNotNull(
                method.GetCustomAttribute<DllImportAttribute>(),
                $"NativeMethods.{name} must be a P/Invoke entry point.");
            return method;
        }

        private static void AssertLibrary(MethodInfo method, string expectedLibrary)
        {
            var import = method.GetCustomAttribute<DllImportAttribute>();
            Assert.IsNotNull(import);
            Assert.AreEqual(expectedLibrary, import.Value, ignoreCase: true);
        }

        private static Type GetNativeRectType()
        {
            var type = GetNativeMethodsType().GetNestedType(
                "RECT",
                BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(type, "NativeMethods.RECT was not found.");
            return type;
        }

        private static Type GetNativeMethodsType() =>
            typeof(ObjectListView).Assembly.GetType(
                "BrightIdeasSoftware.NativeMethods",
                throwOnError: true);
    }
}
