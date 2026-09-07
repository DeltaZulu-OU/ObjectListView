using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class MungerTests
    {
        [TestMethod]
        public void IgnoreMissingAspects_DefaultMatchesBuildConfiguration() =>
#if DEBUG
            Assert.IsFalse(Munger.IgnoreMissingAspects);
#else
            Assert.IsTrue(Munger.IgnoreMissingAspects);
#endif


        [TestMethod]
        public void GetValue_ReadsSimpleAspect()
        {
            var model = new RootModel { Name = "root" };
            var munger = new Munger(nameof(RootModel.Name));

            Assert.AreEqual("root", munger.GetValue(model));
        }

        [TestMethod]
        public void GetValue_ReadsNestedAspect()
        {
            var model = new RootModel { Child = new ChildModel { Value = "nested" } };
            var munger = new Munger("Child.Value");

            Assert.AreEqual("nested", munger.GetValue(model));
        }

        [TestMethod]
        public void GetValue_ReadsField()
        {
            var model = new RootModel { FieldValue = "field" };
            var munger = new Munger(nameof(RootModel.FieldValue));

            Assert.AreEqual("field", munger.GetValue(model));
        }

        [TestMethod]
        public void GetValue_InvokesParameterlessMethod()
        {
            var model = new RootModel { Name = "method" };
            var munger = new Munger(nameof(RootModel.GetName));

            Assert.AreEqual("method", munger.GetValue(model));
        }

        [TestMethod]
        public void GetValue_ReturnsNullWhenIntermediateAspectIsNull()
        {
            var model = new RootModel();
            var munger = new Munger("Child.Value");

            Assert.IsNull(munger.GetValue(model));
        }

        [TestMethod]
        public void GetValue_ReturnsNullForMissingAspectWhenIgnored()
        {
            var previous = Munger.IgnoreMissingAspects;
            try
            {
                Munger.IgnoreMissingAspects = true;
                Assert.IsNull(new Munger("Missing").GetValue(new RootModel()));
            }
            finally
            {
                Munger.IgnoreMissingAspects = previous;
            }
        }

        [TestMethod]
        public void GetValue_ReturnsDiagnosticForMissingAspectWhenNotIgnored()
        {
            var previous = Munger.IgnoreMissingAspects;
            try
            {
                Munger.IgnoreMissingAspects = false;
                var value = new Munger("Missing").GetValue(new RootModel());

                Assert.Contains("Missing", value as string);
                Assert.Contains(typeof(RootModel).FullName, value as string);
            }
            finally
            {
                Munger.IgnoreMissingAspects = previous;
            }
        }

        [TestMethod]
        public void GetValue_ReturnsDiagnosticForMissingIntermediateAspect()
        {
            var previous = Munger.IgnoreMissingAspects;
            try
            {
                Munger.IgnoreMissingAspects = false;
                var value = new Munger("Missing.Value").GetValue(new RootModel());

                Assert.Contains("Missing", value as string);
                Assert.Contains(typeof(RootModel).FullName, value as string);
            }
            finally
            {
                Munger.IgnoreMissingAspects = previous;
            }
        }

        [TestMethod]
        public void PutValue_UpdatesNestedAspect()
        {
            var model = new RootModel { Child = new ChildModel { Value = "before" } };
            var munger = new Munger("Child.Value");

            Assert.IsTrue(munger.PutValue(model, "after"));
            Assert.AreEqual("after", model.Child.Value);
        }

        [TestMethod]
        public void PutValue_UpdatesField()
        {
            var model = new RootModel { FieldValue = "before" };
            var munger = new Munger(nameof(RootModel.FieldValue));

            Assert.IsTrue(munger.PutValue(model, "after"));
            Assert.AreEqual("after", model.FieldValue);
        }

        [TestMethod]
        public void PutValue_InvokesSingleParameterMethod()
        {
            var model = new RootModel();
            var munger = new Munger(nameof(RootModel.SetName));

            Assert.IsTrue(munger.PutValue(model, "updated"));
            Assert.AreEqual("updated", model.Name);
        }

        [TestMethod]
        public void PutValue_ReturnsFalseWhenIntermediateAspectIsNull()
        {
            var model = new RootModel();
            var munger = new Munger("Child.Value");

            Assert.IsFalse(munger.PutValue(model, "after"));
        }

        [TestMethod]
        public void PutValue_ReturnsFalseForReadOnlyProperty()
        {
            var model = new RootModel();
            var munger = new Munger(nameof(RootModel.ReadOnlyValue));

            Assert.IsFalse(munger.PutValue(model, "after"));
            Assert.AreEqual("readonly", model.ReadOnlyValue);
        }

        private sealed class RootModel
        {
            public string Name { get; set; }
            public ChildModel Child { get; set; }
            public string FieldValue;
            public string ReadOnlyValue => "readonly";

            public string GetName() => Name;

            public void SetName(string value) => Name = value;
        }

        private sealed class ChildModel
        {
            public string Value { get; set; }
        }
    }
}
