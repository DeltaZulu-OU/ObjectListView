using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class NativeUnicodeInteropTests
    {
        [TestMethod]
        public void LvGroupStructures_UseExplicitUnicodeMarshalling()
        {
            foreach (var name in new[] { "LVGROUP", "LVGROUP2" })
            {
                var type = GetNativeStructure(name);
                var layout = type.StructLayoutAttribute;

                Assert.IsNotNull(layout, $"{name} must declare StructLayout.");
                Assert.AreEqual(
                    CharSet.Unicode,
                    layout.CharSet,
                    $"{name} contains LPWSTR fields and must be marshalled as Unicode.");
            }
        }

        [TestMethod]
        public void LvGroupStructures_PreserveUnicodeHeaderAcrossNativeMarshalling()
        {
            const string expected = "Töögrupp – İstanbul – 東京 – 😀";

            foreach (var name in new[] { "LVGROUP", "LVGROUP2" })
            {
                var type = GetNativeStructure(name);
                var headerField = type.GetField("pszHeader", BindingFlags.Instance | BindingFlags.Public);
                Assert.IsNotNull(headerField, $"{name}.pszHeader was not found.");

                var value = Activator.CreateInstance(type);
                headerField.SetValue(value, expected);

                var pointer = Marshal.AllocHGlobal(Marshal.SizeOf(type));
                var structureCreated = false;
                try
                {
                    Marshal.StructureToPtr(value, pointer, false);
                    structureCreated = true;

                    var roundTripped = Marshal.PtrToStructure(pointer, type);
                    Assert.AreEqual(
                        expected,
                        (string)headerField.GetValue(roundTripped),
                        $"{name}.pszHeader did not survive managed/native marshalling unchanged.");
                }
                finally
                {
                    if (structureCreated)
                    {
                        Marshal.DestroyStructure(pointer, type);
                    }
                    Marshal.FreeHGlobal(pointer);
                }
            }
        }

        private static Type GetNativeStructure(string name)
        {
            var nativeMethods = typeof(ObjectListView).Assembly.GetType(
                "BrightIdeasSoftware.NativeMethods",
                throwOnError: true);

            var type = nativeMethods.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(type, $"NativeMethods.{name} was not found.");
            return type;
        }
    }
}
