using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class CellEditingNavigationTests
    {
        [TestMethod]
        public void Tab_CommitsCurrentCellAndMovesToNextEditableColumn()
        {
            var model = new Model("first", "one");
            using var listView = CreateList(new[] { model });
            var startedCells = RecordStartedCells(listView);
            var editor = StartEdit(listView, model, 0);
            editor.Text = "changed";

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Tab);

            Assert.IsTrue(handled);
            Assert.AreEqual("changed", model.Name);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.AreEqual(2, startedCells.Count);
            Assert.AreSame(model, startedCells[1].Model);
            Assert.AreEqual(1, startedCells[1].ColumnIndex);

            listView.CancelCellEdit();
        }

        [TestMethod]
        public void TabChangesRows_FromLastColumnMovesToNextRowFirstColumn()
        {
            var first = new Model("first", "one");
            var second = new Model("second", "two");
            using var listView = CreateList(new[] { first, second });
            listView.CellEditTabChangesRows = true;
            var startedCells = RecordStartedCells(listView);
            var editor = StartEdit(listView, first, 1);
            editor.Text = "changed";

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Tab);

            Assert.IsTrue(handled);
            Assert.AreEqual("changed", first.Code);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.AreEqual(2, startedCells.Count);
            Assert.AreSame(second, startedCells[1].Model);
            Assert.AreEqual(0, startedCells[1].ColumnIndex);

            listView.CancelCellEdit();
        }

        [TestMethod]
        public void ShiftTabChangesRows_FromFirstColumnMovesToPreviousRowLastColumn()
        {
            var first = new Model("first", "one");
            var second = new Model("second", "two");
            using var listView = CreateList(new[] { first, second });
            listView.CellEditTabChangesRows = true;
            var startedCells = RecordStartedCells(listView);
            StartEdit(listView, second, 0);

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Tab | Keys.Shift);

            Assert.IsTrue(handled);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.AreEqual(2, startedCells.Count);
            Assert.AreSame(first, startedCells[1].Model);
            Assert.AreEqual(1, startedCells[1].ColumnIndex);

            listView.CancelCellEdit();
        }

        [TestMethod]
        public void EnterChangesRows_SkipsDisabledRows()
        {
            var first = new Model("first", "one");
            var disabled = new Model("disabled", "two");
            var third = new Model("third", "three");
            using var listView = CreateList(new[] { first, disabled, third });
            listView.CellEditEnterChangesRows = true;
            listView.DisableObject(disabled);
            var startedCells = RecordStartedCells(listView);
            var editor = StartEdit(listView, first, 0);
            editor.Text = "changed";

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Enter);

            Assert.IsTrue(handled);
            Assert.AreEqual("changed", first.Name);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.AreEqual(2, startedCells.Count);
            Assert.AreSame(third, startedCells[1].Model);
            Assert.AreEqual(0, startedCells[1].ColumnIndex);

            listView.CancelCellEdit();
        }

        [TestMethod]
        public void ValidationVeto_PreventsTabNavigation()
        {
            var model = new Model("first", "one");
            using var listView = CreateList(new[] { model });
            var startedCells = RecordStartedCells(listView);
            listView.CellEditValidating += (_, e) => e.Cancel = true;
            var editor = StartEdit(listView, model, 0);
            editor.Text = "changed";

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Tab);

            Assert.IsTrue(handled);
            Assert.IsTrue(listView.IsCellEditing);
            Assert.AreSame(editor, listView.CellEditor);
            Assert.AreEqual("first", model.Name);
            Assert.AreEqual(1, startedCells.Count);
            Assert.AreSame(model, startedCells[0].Model);
            Assert.AreEqual(0, startedCells[0].ColumnIndex);

            listView.CancelCellEdit();
        }

        [TestMethod]
        public void Escape_CancelsEditWithoutWritingBack()
        {
            var model = new Model("first", "one");
            using var listView = CreateList(new[] { model });
            var editor = StartEdit(listView, model, 0);
            editor.Text = "changed";

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Escape);

            Assert.IsTrue(handled);
            Assert.IsFalse(listView.IsCellEditing);
            Assert.AreEqual("first", model.Name);
        }

        [TestMethod]
        public void Enter_DefaultBehaviourCommitsAndEndsEdit()
        {
            var model = new Model("first", "one");
            using var listView = CreateList(new[] { model });
            var editor = StartEdit(listView, model, 0);
            editor.Text = "changed";

            var handled = listView.CellEditKeyEngine.HandleKey(listView, Keys.Enter);

            Assert.IsTrue(handled);
            Assert.IsFalse(listView.IsCellEditing);
            Assert.AreEqual("changed", model.Name);
        }

        [TestMethod]
        public void HandleKey_NullListViewThrowsBeforeCheckingWhetherKeyIsMapped()
        {
            var engine = new CellEditKeyEngine();

            try
            {
                engine.HandleKey(null, Keys.F12);
                Assert.Fail("A null ObjectListView should always throw ArgumentNullException.");
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual("olv", e.ParamName);
            }
        }

        private static ObjectListView CreateList(Model[] models)
        {
            var listView = new ObjectListView {
                CellEditActivation = ObjectListView.CellEditActivateMode.F2Only,
                ShowGroups = false,
                View = View.Details
            };
            listView.Columns.Add(new OLVColumn("Name", nameof(Model.Name)) {
                IsEditable = true,
                Width = 140
            });
            listView.Columns.Add(new OLVColumn("Code", nameof(Model.Code)) {
                IsEditable = true,
                Width = 140
            });
            listView.SetObjects(models);
            _ = listView.Handle;
            return listView;
        }

        private static List<StartedCell> RecordStartedCells(ObjectListView listView)
        {
            var cells = new List<StartedCell>();
            listView.CellEditStarting += (_, e) => cells.Add(new StartedCell(e.RowObject, e.SubItemIndex));
            return cells;
        }

        private static Control StartEdit(ObjectListView listView, object model, int columnIndex)
        {
            listView.EditSubItem(GetItem(listView, model), columnIndex);
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

        private sealed class StartedCell
        {
            public StartedCell(object model, int columnIndex)
            {
                Model = model;
                ColumnIndex = columnIndex;
            }

            public object Model { get; }
            public int ColumnIndex { get; }
        }

        private sealed class Model
        {
            public Model(string name, string code)
            {
                Name = name;
                Code = code;
            }

            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
