using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
    public partial class ObjectListView
    {
        /// <summary>
        /// Return the model object of the row that is checked or null if no row is checked
        /// or more than one row is checked.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual object CheckedObject {
            get {
                var checkedObjects = CheckedObjects;
                return checkedObjects.Count == 1 ? checkedObjects[0] : null;
            }
            set => CheckedObjects = new ArrayList(new[] { value });
        }

        /// <summary>
        /// Get or set the collection of model objects that are checked.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IList CheckedObjects {
            get => CollectCheckedObjects();
            set => ApplyCheckedObjects(value);
        }

        /// <summary>
        /// Gets or sets the checked objects from an enumerable.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable CheckedObjectsEnumerable {
            get => CheckedObjects;
            set => CheckedObjects = EnumerableToArray(value, true);
        }

        private IList CollectCheckedObjects()
        {
            var list = new ArrayList();
            if (!CheckBoxes)
            {
                return list;
            }

            for (var i = 0; i < GetItemCount(); i++)
            {
                var item = GetItem(i);
                if (item.CheckState == CheckState.Checked)
                {
                    list.Add(item.RowObject);
                }
            }

            return list;
        }

        private void ApplyCheckedObjects(IList value)
        {
            if (!CheckBoxes)
            {
                return;
            }

            var sw = Stopwatch.StartNew();
            var checkedLookup = BuildCheckedObjectLookup(value);

            BeginUpdate();
            foreach (var model in Objects)
            {
                SetObjectCheckedness(model, checkedLookup.ContainsKey(model) ? CheckState.Checked : CheckState.Unchecked);
            }
            EndUpdate();

            Debug.WriteLine(string.Format("PERF - Setting CheckedObjects on {2} objects took {0}ms / {1} ticks", sw.ElapsedMilliseconds, sw.ElapsedTicks, GetItemCount()));
        }

        private Hashtable BuildCheckedObjectLookup(IList value)
        {
            var table = new Hashtable(GetItemCount());
            if (value == null)
            {
                return table;
            }

            foreach (var model in value)
            {
                table[model] = true;
            }

            return table;
        }
    }
}
