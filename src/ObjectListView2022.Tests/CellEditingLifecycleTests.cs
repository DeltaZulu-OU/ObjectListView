using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BrightIdeasSoftware.ObjectListView;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class CellEditingLifecycleTests
    {
        [TestMethod]
        public void CellEditStarting_CancelPreventsEditorInstallation()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            var startingCount = 0;
            listView.CellEditStarting += (_, e) => {
                startingCount++;
                Assert.AreEqual("before", e.Value);
                e.Cancel = true;
            };

            listView.EditSubItem(GetItem(listView, model), 0);

            Assert.AreEqual(1, startingCount);
            Assert.IsFalse(listView.IsCellEditing);
            Assert.IsNull(listView.CellEditor);
            Assert.AreEqual("before", model.Name);
        }

        [TestMethod]
        public void PossibleFinishCellEditing_ValidationCancelKeepsEditorActive()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            var finishingCount = 0;
            var finishedCount = 0;
            listView.CellEditValidating += (_, e) => e.Cancel = true;
            listView.CellEditFinishing += (_, _) => finishingCount++;
            listView.CellEditFinished += (_, _) => finishedCount++;
            var editor = StartEdit(listView, model);
            editor.Text = "after";

            var result = listView.PossibleFinishCellEditing();

            Assert.IsFalse(result);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.AreSame(editor, listView.CellEditor);
            Assert.AreEqual("before", model.Name);
            Assert.AreEqual(0, finishingCount);
            Assert.AreEqual(0, finishedCount);

            listView.CancelCellEdit();
        }

        [TestMethod]
        public void PossibleFinishCellEditing_CommitsValueAndFiresLifecycleInOrder()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            var events = new List<string>();
            listView.CellEditStarting += (_, _) => events.Add("starting");
            listView.CellEditValidating += (_, _) => events.Add("validating");
            listView.CellEditFinishing += (_, _) => events.Add("finishing");
            listView.CellEditFinished += (_, _) => events.Add("finished");
            var editor = StartEdit(listView, model);
            editor.Text = "after";

            var result = listView.PossibleFinishCellEditing();

            Assert.IsTrue(result);
            Assert.IsFalse(listView.IsCellEditing);
            Assert.AreEqual("after", model.Name);
            Assert.AreSequenceEqual(
                new[] { "starting", "validating", "finishing", "finished" },
                events.ToArray());
        }

        [TestMethod]
        public void CellEditFinishing_CancelSuppressesWriteButAllowsEditToFinish()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            var finishedCount = 0;
            listView.CellEditFinishing += (_, e) => e.Cancel = true;
            listView.CellEditFinished += (_, _) => finishedCount++;
            var editor = StartEdit(listView, model);
            editor.Text = "after";

            var result = listView.PossibleFinishCellEditing();

            Assert.IsTrue(result);
            Assert.IsFalse(listView.IsCellEditing);
            Assert.AreEqual("before", model.Name);
            Assert.AreEqual(1, finishedCount);
        }

        [TestMethod]
        public void CellEditFinishing_NewValueOverrideIsWrittenBack()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            object observedNewValue = null;
            listView.CellEditFinishing += (_, e) => {
                observedNewValue = e.NewValue;
                e.NewValue = "normalized";
            };
            var editor = StartEdit(listView, model);
            editor.Text = "typed";

            var result = listView.PossibleFinishCellEditing();

            Assert.IsTrue(result);
            Assert.AreEqual("typed", observedNewValue);
            Assert.AreEqual("normalized", model.Name);
        }

        [TestMethod]
        public void CancelCellEdit_DiscardsValueAndReportsCanceledFinishing()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            var validatingCount = 0;
            var finishingCount = 0;
            var finishedCount = 0;
            var finishingWasCanceled = false;
            listView.CellEditValidating += (_, _) => validatingCount++;
            listView.CellEditFinishing += (_, e) => {
                finishingCount++;
                finishingWasCanceled = e.Cancel;
            };
            listView.CellEditFinished += (_, _) => finishedCount++;
            var editor = StartEdit(listView, model);
            editor.Text = "after";

            listView.CancelCellEdit();

            Assert.IsFalse(listView.IsCellEditing);
            Assert.AreEqual("before", model.Name);
            Assert.AreEqual(0, validatingCount);
            Assert.AreEqual(1, finishingCount);
            Assert.IsTrue(finishingWasCanceled);
            Assert.AreEqual(0, finishedCount);
        }

        [TestMethod]
        public void CellEditStarting_CanReplaceEditorWithoutAutomaticDisposal()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out _);
            using var replacement = new TextBox { Text = "replacement" };
            listView.CellEditStarting += (_, e) => {
                e.Control = replacement;
                e.AutoDispose = false;
            };

            var editor = StartEdit(listView, model);

            Assert.AreSame(replacement, editor);
            Assert.AreEqual("replacement", replacement.Text);

            listView.CancelCellEdit();

            Assert.IsFalse(replacement.IsDisposed);
        }

        [TestMethod]
        public void EditSubItem_DoesNotStartEditingReadOnlyColumn()
        {
            var model = new Model("before");
            using var listView = CreateList(model, out var nameColumn);
            nameColumn.IsEditable = false;
            var startingCount = 0;
            listView.CellEditStarting += (_, _) => startingCount++;

            listView.EditSubItem(GetItem(listView, model), 0);

            Assert.IsFalse(listView.IsCellEditing);
            Assert.AreEqual(0, startingCount);
            Assert.AreEqual("before", model.Name);
        }

        private static ObjectListView CreateList(Model model, out OLVColumn nameColumn)
        {
            var listView = new ObjectListView {
                CellEditActivation = CellEditActivateMode.F2Only,
                ShowGroups = false,
                View = View.Details
            };
            nameColumn = new OLVColumn("Name", nameof(Model.Name)) { Width = 160 };
            listView.Columns.Add(nameColumn);
            listView.SetObjects(new[] { model });
            _ = listView.Handle;
            return listView;
        }

        private static Control StartEdit(ObjectListView listView, Model model)
        {
            listView.EditSubItem(GetItem(listView, model), 0);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.IsNotNull(listView.CellEditor);
            return listView.CellEditor;
        }

        private static OLVListItem GetItem(ObjectListView listView, object model)
        {
            var index = listView.IndexOf(model);
            Assert.IsGreaterThanOrEqualTo(0, index);
            var item = listView.GetItem(index);
            Assert.IsNotNull(item);
            return item;
        }

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
