using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class BackgroundThreadBehaviorTests
    {
        [TestMethod]
        public void WinFormsHost_UsesStaUiThreadAndDistinctMtaWorker()
        {
            using var host = CreateHost(() => new ThreadRecordingObjectListView());
            var invokeRequired = false;

            host.RunFromWorker(listView => invokeRequired = listView.InvokeRequired);

            Assert.AreEqual(ApartmentState.STA, host.UiApartmentState);
            Assert.AreEqual(ApartmentState.MTA, host.WorkerApartmentState);
            Assert.AreNotEqual(host.UiThreadId, host.WorkerThreadId);
            Assert.IsTrue(invokeRequired);
        }

        [TestMethod]
        public void ObjectListView_PrimaryCollectionCommandsCanBeCalledFromBackgroundThread()
        {
            using var host = CreateHost(() => new ThreadRecordingObjectListView());
            var first = new Model("first", 1);
            var second = new Model("second", 2);

            host.RunFromWorker(listView => listView.SetObjects(new[] { first, second }));

            Assert.AreEqual(2, host.Read(listView => listView.GetItemCount()));
            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.BuildListThreadId));
            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.ItemsChangedThreadId));

            var third = new Model("third", 3);
            host.Invoke(listView => listView.ResetThreadRecords());
            host.RunFromWorker(listView => listView.AddObject(third));

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.ItemsAddingThreadId));
            Assert.AreEqual(3, host.Read(listView => listView.GetItemCount()));

            var fourth = new Model("fourth", 4);
            var fifth = new Model("fifth", 5);
            host.Invoke(listView => listView.ResetThreadRecords());
            host.RunFromWorker(listView => listView.AddObjects(new[] { fourth, fifth }));

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.ItemsAddingThreadId));
            Assert.AreEqual(5, host.Read(listView => listView.GetItemCount()));

            var inserted = new Model("inserted", 6);
            host.RunFromWorker(listView => listView.InsertObjects(1, new[] { inserted }));

            Assert.AreSame(inserted, host.Read(listView => listView.GetModelObject(1)));

            third.Name = "third-updated";
            host.Invoke(listView => listView.ResetThreadRecords());
            host.RunFromWorker(listView => listView.RefreshObjects(new object[] { third }));

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.RefreshItemThreadId));
            Assert.AreEqual(
                "third-updated",
                host.Read(listView => listView.GetItem(listView.IndexOf(third)).Text));

            host.Invoke(listView => listView.ResetThreadRecords());
            host.RunFromWorker(listView => listView.RemoveObject(fourth));

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.ItemsRemovingThreadId));
            Assert.AreEqual(-1, host.Read(listView => listView.IndexOf(fourth)));

            host.Invoke(listView => listView.ResetThreadRecords());
            host.RunFromWorker(listView => listView.RemoveObjects(new[] { second, fifth }));

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.ItemsRemovingThreadId));
            Assert.AreEqual(-1, host.Read(listView => listView.IndexOf(second)));
            Assert.AreEqual(-1, host.Read(listView => listView.IndexOf(fifth)));

            host.RunFromWorker(listView => listView.ClearObjects());

            Assert.AreEqual(0, host.Read(listView => listView.GetItemCount()));
        }

        [TestMethod]
        public void ObjectListView_BuildListCanBeCalledFromBackgroundThread()
        {
            using var host = CreateHost(() => new ThreadRecordingObjectListView());
            var model = new Model("before", 1);
            host.Invoke(listView => {
                listView.SetObjects(new[] { model });
                listView.ResetThreadRecords();
            });
            model.Name = "after";

            host.RunFromWorker(listView => listView.BuildList());

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.BuildListThreadId));
            Assert.AreEqual("after", host.Read(listView => listView.GetItem(0).Text));
        }

        [TestMethod]
        public void ObjectListView_SortCanBeCalledFromBackgroundThread()
        {
            using var host = CreateHost(() => new ThreadRecordingObjectListView());
            var first = new Model("first", 1);
            var second = new Model("second", 3);
            var third = new Model("third", 2);
            OLVColumn rankColumn = null;
            host.Invoke(listView => {
                listView.ShowGroups = false;
                listView.SetObjects(new[] { first, second, third });
                rankColumn = listView.GetColumn(1);
                listView.ResetThreadRecords();
            });

            host.RunFromWorker(listView => listView.Sort(rankColumn, SortOrder.Descending));

            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.BeforeSortingThreadId));
            Assert.AreSame(second, host.Read(listView => listView.GetModelObject(0)));
            Assert.AreSame(third, host.Read(listView => listView.GetModelObject(1)));
            Assert.AreSame(first, host.Read(listView => listView.GetModelObject(2)));
        }

        [TestMethod]
        public void FastObjectListView_SetAndClearCanBeCalledFromBackgroundThread()
        {
            using var host = CreateHost(() => new ThreadRecordingFastObjectListView());
            var first = new Model("first", 1);
            var second = new Model("second", 2);

            host.RunFromWorker(listView => listView.SetObjects(new[] { first, second }));

            Assert.AreEqual(2, host.Read(listView => listView.GetItemCount()));
            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.BuildListThreadId));
            Assert.AreEqual(host.UiThreadId, host.Read(listView => listView.ItemsChangedThreadId));

            host.RunFromWorker(listView => listView.ClearObjects());

            Assert.AreEqual(0, host.Read(listView => listView.GetItemCount()));
        }

        private static WinFormsHost<T> CreateHost<T>(Func<T> factory) where T : ObjectListView => new WinFormsHost<T>(() => {
            var listView = factory();
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)));
            listView.Columns.Add(new OLVColumn("Rank", nameof(Model.Rank)));
            return listView;
        });

        private sealed class WinFormsHost<T> : IDisposable where T : ObjectListView
        {
            private const int TimeoutMilliseconds = 10000;
            private readonly Func<T> factory;
            private readonly ManualResetEventSlim ready = new ManualResetEventSlim();
            private readonly Thread uiThread;
            private Exception startupException;
            private Form form;

            public WinFormsHost(Func<T> factory)
            {
                this.factory = factory;
                uiThread = new Thread(RunUiThread) {
                    IsBackground = true,
                    Name = $"{typeof(T).Name} test UI thread"
                };
                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.Start();

                if (!ready.Wait(TimeoutMilliseconds))
                {
                    throw new TimeoutException("The WinForms test UI thread did not become ready.");
                }

                if (startupException != null)
                {
                    ExceptionDispatchInfo.Capture(startupException).Throw();
                }
            }

            public T ListView { get; private set; }
            public int UiThreadId { get; private set; }
            public ApartmentState UiApartmentState { get; private set; }
            public int WorkerThreadId { get; private set; }
            public ApartmentState WorkerApartmentState { get; private set; }

            public void RunFromWorker(Action<T> action)
            {
                Exception workerException = null;
                var worker = new Thread(() => {
                    WorkerThreadId = Thread.CurrentThread.ManagedThreadId;
                    WorkerApartmentState = Thread.CurrentThread.GetApartmentState();
                    try
                    {
                        action(ListView);
                    }
                    catch (Exception exception)
                    {
                        workerException = exception;
                    }
                }) {
                    IsBackground = true,
                    Name = $"{typeof(T).Name} test worker"
                };
                worker.SetApartmentState(ApartmentState.MTA);
                worker.Start();

                if (!worker.Join(TimeoutMilliseconds))
                {
                    throw new TimeoutException("A background ObjectListView operation did not complete. This may indicate a UI-thread marshaling deadlock.");
                }

                if (workerException != null)
                {
                    ExceptionDispatchInfo.Capture(workerException).Throw();
                }
            }

            public void Invoke(Action<T> action) => ListView.Invoke((MethodInvoker)(() => action(ListView)));

            public TResult Read<TResult>(Func<T, TResult> func)
            {
                TResult result = default;
                ListView.Invoke((MethodInvoker)(() => result = func(ListView)));
                return result;
            }

            public void Dispose()
            {
                try
                {
                    if (form != null && !form.IsDisposed && form.IsHandleCreated)
                    {
                        form.BeginInvoke((MethodInvoker)form.Close);
                    }
                }
                catch (InvalidOperationException)
                {
                    // The UI thread may already be shutting down after a test failure.
                }

                if (uiThread.IsAlive && !uiThread.Join(TimeoutMilliseconds))
                {
                    throw new TimeoutException("The WinForms test UI thread did not shut down.");
                }

                ready.Dispose();
            }

            private void RunUiThread()
            {
                try
                {
                    UiThreadId = Thread.CurrentThread.ManagedThreadId;
                    UiApartmentState = Thread.CurrentThread.GetApartmentState();
                    using var localForm = new Form {
                        ShowInTaskbar = false,
                        Width = 320,
                        Height = 240
                    };
                    form = localForm;
                    ListView = factory();
                    ListView.Dock = DockStyle.Fill;
                    localForm.Controls.Add(ListView);
                    localForm.Shown += (_, __) => {
                        _ = localForm.Handle;
                        _ = ListView.Handle;
                        ready.Set();
                    };

                    Application.Run(localForm);
                }
                catch (Exception exception)
                {
                    startupException = exception;
                    ready.Set();
                }
            }
        }

        private class ThreadRecordingObjectListView : ObjectListView
        {
            public int ItemsAddingThreadId { get; private set; }
            public int ItemsRemovingThreadId { get; private set; }
            public int ItemsChangedThreadId { get; private set; }
            public int BeforeSortingThreadId { get; private set; }
            public int BuildListThreadId { get; private set; }
            public int RefreshItemThreadId { get; private set; }

            public void ResetThreadRecords()
            {
                ItemsAddingThreadId = 0;
                ItemsRemovingThreadId = 0;
                ItemsChangedThreadId = 0;
                BeforeSortingThreadId = 0;
                BuildListThreadId = 0;
                RefreshItemThreadId = 0;
            }

            public override void BuildList(bool shouldPreserveState)
            {
                BuildListThreadId = Thread.CurrentThread.ManagedThreadId;
                base.BuildList(shouldPreserveState);
            }

            public override void RefreshItem(OLVListItem olvi)
            {
                RefreshItemThreadId = Thread.CurrentThread.ManagedThreadId;
                base.RefreshItem(olvi);
            }

            protected override void OnItemsAdding(ItemsAddingEventArgs e)
            {
                ItemsAddingThreadId = Thread.CurrentThread.ManagedThreadId;
                base.OnItemsAdding(e);
            }

            protected override void OnItemsRemoving(ItemsRemovingEventArgs e)
            {
                ItemsRemovingThreadId = Thread.CurrentThread.ManagedThreadId;
                base.OnItemsRemoving(e);
            }

            protected override void OnItemsChanged(ItemsChangedEventArgs e)
            {
                ItemsChangedThreadId = Thread.CurrentThread.ManagedThreadId;
                base.OnItemsChanged(e);
            }

            protected override void OnBeforeSorting(BeforeSortingEventArgs e)
            {
                BeforeSortingThreadId = Thread.CurrentThread.ManagedThreadId;
                base.OnBeforeSorting(e);
            }
        }

        private sealed class ThreadRecordingFastObjectListView : FastObjectListView
        {
            public int ItemsChangedThreadId { get; private set; }
            public int BuildListThreadId { get; private set; }

            public override void BuildList(bool shouldPreserveState)
            {
                BuildListThreadId = Thread.CurrentThread.ManagedThreadId;
                base.BuildList(shouldPreserveState);
            }

            protected override void OnItemsChanged(ItemsChangedEventArgs e)
            {
                ItemsChangedThreadId = Thread.CurrentThread.ManagedThreadId;
                base.OnItemsChanged(e);
            }
        }

        private sealed class Model
        {
            public Model(string name, int rank)
            {
                Name = name;
                Rank = rank;
            }

            public string Name { get; set; }
            public int Rank { get; }
        }
    }
}
