using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DeferredViewUpdateTests
    {
        [TestMethod]
        public void DeferViewUpdates_DefersRebuildUntilScopeIsDisposed()
        {
            using var listView = CreateListView();
            listView.SetObjects(new[] { new Model("first") });
            Assert.AreEqual(1, listView.GetItemCount());

            using (listView.DeferViewUpdates())
            {
                listView.SetObjects(new[] {
                    new Model("first"),
                    new Model("second"),
                    new Model("third")
                });

                Assert.IsTrue(listView.Frozen);
                Assert.AreEqual(1, listView.GetItemCount());
            }

            Assert.IsFalse(listView.Frozen);
            Assert.AreEqual(3, listView.GetItemCount());
        }

        [TestMethod]
        public void DeferViewUpdates_NestedScopesRebuildOnlyAfterOutermostScope()
        {
            using var listView = CreateListView();
            listView.SetObjects(new[] { new Model("first") });

            using (listView.DeferViewUpdates())
            {
                using (listView.DeferViewUpdates())
                {
                    listView.SetObjects(new[] {
                        new Model("first"),
                        new Model("second")
                    });
                }

                Assert.IsTrue(listView.Frozen);
                Assert.AreEqual(1, listView.GetItemCount());
            }

            Assert.IsFalse(listView.Frozen);
            Assert.AreEqual(2, listView.GetItemCount());
        }

        [TestMethod]
        public void DeferViewUpdates_UnfreezesWhenScopeExitsThroughException()
        {
            using var listView = CreateListView();

            try
            {
                using (listView.DeferViewUpdates())
                {
                    Assert.IsTrue(listView.Frozen);
                    throw new TestException();
                }
            }
            catch (TestException)
            {
            }

            Assert.IsFalse(listView.Frozen);
        }

        private static ObjectListView CreateListView()
        {
            var listView = new ObjectListView();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            listView.CreateControl();
            return listView;
        }

        private sealed class Model
        {
            public Model(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class TestException : System.Exception
        {
        }
    }
}
