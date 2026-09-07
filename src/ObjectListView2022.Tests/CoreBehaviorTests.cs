using System.Collections;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Filtering;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class CoreBehaviorTests
    {
        [TestMethod]
        public void OlvColumn_ReadsConfiguredAspect()
        {
            var column = new OLVColumn("Name", nameof(Model.Name));
            var model = new Model { Name = "alpha" };

            Assert.AreEqual("alpha", column.GetValue(model));
        }

        [TestMethod]
        public void OlvColumn_AspectGetterOverridesAspectName()
        {
            var column = new OLVColumn("Name", nameof(Model.Name)) {
                AspectGetter = _ => "override"
            };

            Assert.AreEqual("override", column.GetValue(new Model { Name = "alpha" }));
        }

        [TestMethod]
        public void ModelFilter_UsesPredicate()
        {
            var filter = new ModelFilter(x => ((Model)x).Name.StartsWith("a"));

            Assert.IsTrue(filter.Filter(new Model { Name = "alpha" }));
            Assert.IsFalse(filter.Filter(new Model { Name = "beta" }));
        }

        [TestMethod]
        public void ListFilter_WithNoDelegateReturnsOriginalSequence()
        {
            var values = new[] { "a", "b" };
            var filter = new ListFilter(null);

            Assert.AreSame(values, filter.Filter(values));
        }

        [TestMethod]
        public void ListFilter_UsesConfiguredDelegate()
        {
            var values = new[] { "alpha", "beta", "atom" };
            var filter = new ListFilter(items =>
                ((IEnumerable)items).Cast<string>().Where(x => x.StartsWith("a")).ToArray());

            CollectionAssert.AreEqual(new[] { "alpha", "atom" },
                filter.Filter(values).Cast<string>().ToArray());
        }

        [TestMethod]
        public void ColumnComparer_SortsByConfiguredAspect()
        {
            var column = new OLVColumn("Name", nameof(Model.Name));
            var comparer = new ColumnComparer(column, SortOrder.Ascending);
            var alpha = new OLVListItem(new Model { Name = "alpha" });
            var beta = new OLVListItem(new Model { Name = "beta" });

            Assert.IsTrue(comparer.Compare(alpha, beta) < 0);
            Assert.IsTrue(comparer.Compare(beta, alpha) > 0);
            Assert.AreEqual(0, comparer.Compare(alpha, alpha));
        }

        [TestMethod]
        public void OlvGroupComparer_UsesSortValueWhenPresent()
        {
            var first = new OLVGroup("z") { SortValue = 1 };
            var second = new OLVGroup("a") { SortValue = 2 };
            var comparer = new OLVGroupComparer(SortOrder.Ascending);

            Assert.IsTrue(comparer.Compare(first, second) < 0);
        }

        private sealed class Model
        {
            public string Name { get; set; }
        }
    }
}
