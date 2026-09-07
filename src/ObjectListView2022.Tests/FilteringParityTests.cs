using System;
using System.Collections.Generic;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class FilteringParityTests
    {
        [TestMethod]
        public void CompositeFilters_PreserveAllAndAnySemantics()
        {
            var startsWithA = new ModelFilter(x => ((Model)x).Name.StartsWith("a", StringComparison.OrdinalIgnoreCase));
            var containsPh = new ModelFilter(x => ((Model)x).Name.IndexOf("ph", StringComparison.OrdinalIgnoreCase) >= 0);
            var startsWithZ = new ModelFilter(x => ((Model)x).Name.StartsWith("z", StringComparison.OrdinalIgnoreCase));

            var all = new CompositeAllFilter(new List<IModelFilter> { startsWithA, containsPh });
            var any = new CompositeAnyFilter(new List<IModelFilter> { startsWithZ, containsPh });

            var alpha = new Model { Name = "alpha" };
            var atom = new Model { Name = "atom" };

            Assert.IsTrue(all.Filter(alpha));
            Assert.IsFalse(all.Filter(atom));
            Assert.IsTrue(any.Filter(alpha));
            Assert.IsFalse(any.Filter(atom));
        }

        [TestMethod]
        public void OneOfFilter_MatchesValuesFromEnumerableAspects()
        {
            var filter = new OneOfFilter(
                x => ((Model)x).Tags,
                new[] { "red", "green" });

            Assert.IsTrue(filter.Filter(new Model { Tags = new[] { "blue", "green" } }));
            Assert.IsFalse(filter.Filter(new Model { Tags = new[] { "blue", "yellow" } }));
        }

        [TestMethod]
        public void FlagBitSetFilter_MatchesConfiguredFlags()
        {
            var filter = new FlagBitSetFilter(
                x => ((Model)x).Flags,
                new[] { 0x02, 0x08 });

            Assert.IsTrue(filter.Filter(new Model { Flags = 0x0A }));
            Assert.IsFalse(filter.Filter(new Model { Flags = 0x04 }));
        }

        [TestMethod]
        public void TextMatchFilter_PreservesContainsPrefixAndRegexSemantics()
        {
            using var listView = new ObjectListView();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            var alpha = new Model { Name = "alpha" };
            var beta = new Model { Name = "beta" };

            Assert.IsTrue(TextMatchFilter.Contains(listView, "PHA").Filter(alpha));
            Assert.IsFalse(TextMatchFilter.Contains(listView, "PHA").Filter(beta));

            Assert.IsTrue(TextMatchFilter.Prefix(listView, "AL").Filter(alpha));
            Assert.IsFalse(TextMatchFilter.Prefix(listView, "AL").Filter(beta));

            Assert.IsTrue(TextMatchFilter.Regex(listView, "^a.*a$").Filter(alpha));
            Assert.IsFalse(TextMatchFilter.Regex(listView, "^a.*a$").Filter(beta));
        }

        [TestMethod]
        public void TextMatchFilter_UsesAdditionalColumns()
        {
            using var listView = new ObjectListView();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));

            var codeColumn = new OLVColumn("Code", nameof(Model.Code));
            var filter = TextMatchFilter.Contains(listView, "X-42");
            filter.AdditionalColumns = new[] { codeColumn };

            Assert.IsTrue(filter.Filter(new Model { Name = "alpha", Code = "X-42" }));
            Assert.IsFalse(filter.Filter(new Model { Name = "alpha", Code = "Y-11" }));
        }

        private sealed class Model
        {
            public string Name { get; set; }
            public string[] Tags { get; set; }
            public int Flags { get; set; }
            public string Code { get; set; }
        }
    }
}
