using System;
using System.Reflection;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class HeaderMarshallingTests
    {
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

        private static MethodInfo GetTryMarshalStructure()
        {
            var method = typeof(ObjectListView).GetMethod(
                "TryMarshalStructure",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            Assert.IsTrue(method.IsGenericMethodDefinition);
            return method;
        }
    }
}
