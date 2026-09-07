using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class PerformanceBenchmarks
    {
        [TestMethod]
        public void CheckedObjects_SparseSetter()
        {
            if (!BenchmarksEnabled)
            {
                return;
            }

            using var listView = new ObjectListView { CheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));

            var models = Enumerable.Range(0, 5000)
                .Select(i => new Model("model-" + i))
                .ToArray();
            listView.SetObjects(models);
            var checkedObjects = new ArrayList { models[10], models[2500], models[4990] };

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 25; i++)
            {
                listView.CheckedObjects = checkedObjects;
            }
            stopwatch.Stop();

            Console.WriteLine(
                "CheckedObjects sparse setter: {0} ms total, {1:F3} ms/iteration",
                stopwatch.ElapsedMilliseconds,
                stopwatch.Elapsed.TotalMilliseconds / 25.0);
        }

        [TestMethod]
        public void FastObjectListView_CheckedObjectsSparseSetter()
        {
            if (!BenchmarksEnabled)
            {
                return;
            }

            using var listView = new FastObjectListView { CheckBoxes = true };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));

            var models = Enumerable.Range(0, 50000)
                .Select(i => new Model("model-" + i))
                .ToArray();
            listView.SetObjects(models);
            var checkedObjects = new ArrayList { models[10], models[25000], models[49990] };

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 25; i++)
            {
                listView.CheckedObjects = checkedObjects;
            }
            stopwatch.Stop();

            Console.WriteLine(
                "FastObjectListView CheckedObjects sparse setter: {0} ms total, {1:F3} ms/iteration",
                stopwatch.ElapsedMilliseconds,
                stopwatch.Elapsed.TotalMilliseconds / 25.0);
        }

        [TestMethod]
        public void Munger_RepeatedPropertyRead()
        {
            if (!BenchmarksEnabled)
            {
                return;
            }

            var model = new Model("model");
            var munger = new Munger(nameof(Model.Name));

            var stopwatch = Stopwatch.StartNew();
            object value = null;
            for (var i = 0; i < 1000000; i++)
            {
                value = munger.GetValue(model);
            }
            stopwatch.Stop();

            Assert.AreEqual("model", value);
            Console.WriteLine(
                "Munger repeated property read: {0} ms total for 1,000,000 reads",
                stopwatch.ElapsedMilliseconds);
        }

        private static bool BenchmarksEnabled =>
            string.Equals(
                Environment.GetEnvironmentVariable("OLV_RUN_BENCHMARKS"),
                "1",
                StringComparison.Ordinal);

        private sealed class Model
        {
            public Model(string name)
            {
                Name = name;
            }

            public string Name { get; set; }
        }
    }
}
