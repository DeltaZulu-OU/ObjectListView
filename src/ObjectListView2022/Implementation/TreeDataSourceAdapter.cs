using System;
using System.Collections;
using System.ComponentModel;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace BrightIdeasSoftware
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// A TreeDataSourceAdapter knows how to build a tree structure from a binding list.
    /// </summary>
    /// <remarks>To build a tree</remarks>
    public class TreeDataSourceAdapter : DataSourceAdapter
    {
        #region Life and death

        /// <summary>
        /// Create a data source adaptor that knows how to build a tree structure
        /// </summary>
        /// <param name="tlv"></param>
        public TreeDataSourceAdapter(DataTreeListView tlv)
            : base(tlv)
        {
            TreeListView = tlv;
            TreeListView.CanExpandGetter = delegate (object model) { return CalculateHasChildren(model); };
            TreeListView.ChildrenGetter = delegate (object model) { return CalculateChildren(model); };
        }

        #endregion Life and death

        #region Properties

        /// <summary>
        /// Gets or sets the name of the property/column that uniquely identifies each row.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value contained by this column must be unique across all rows
        /// in the data source. Odd and unpredictable things will happen if two
        /// rows have the same id.
        /// </para>
        /// <para>Null cannot be a valid key value.</para>
        /// </remarks>
        public virtual string KeyAspectName {
            get => keyAspectName;
            set {
                if (keyAspectName == value)
                {
                    return;
                }

                keyAspectName = value;
                keyMunger = new Munger(KeyAspectName);
                InitializeDataSource();
            }
        }

        private string keyAspectName;

        /// <summary>
        /// Gets or sets the name of the property/column that contains the key of
        /// the parent of a row.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test condition for deciding if one row is the parent of another is functionally
        /// equivilent to this:
        /// <code>
        /// Object.Equals(candidateParentRow[this.KeyAspectName], row[this.ParentKeyAspectName])
        /// </code>
        /// </para>
        /// <para>Unlike key value, parent keys can be null but a null parent key can only be used
        /// to identify root objects.</para>
        /// </remarks>
        public virtual string ParentKeyAspectName {
            get => parentKeyAspectName;
            set {
                if (parentKeyAspectName == value)
                {
                    return;
                }

                parentKeyAspectName = value;
                parentKeyMunger = new Munger(ParentKeyAspectName);
                InitializeDataSource();
            }
        }

        private string parentKeyAspectName;

        /// <summary>
        /// Gets or sets the value that identifies a row as a root object.
        /// When the ParentKey of a row equals the RootKeyValue, that row will
        /// be treated as root of the TreeListView.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test condition for deciding a root object is functionally
        /// equivilent to this:
        /// <code>
        /// Object.Equals(candidateRow[this.ParentKeyAspectName], this.RootKeyValue)
        /// </code>
        /// </para>
        /// <para>The RootKeyValue can be null.</para>
        /// </remarks>
        public virtual object RootKeyValue {
            get => rootKeyValue;
            set {
                if (Equals(rootKeyValue, value))
                {
                    return;
                }

                rootKeyValue = value;
                InitializeDataSource();
            }
        }

        private object rootKeyValue;

        /// <summary>
        /// Gets or sets whether or not the key columns (id and parent id) should
        /// be shown to the user.
        /// </summary>
        /// <remarks>This must be set before the DataSource is set. It has no effect
        /// afterwards.</remarks>
        public virtual bool ShowKeyColumns {
            get => showKeyColumns; set => showKeyColumns = value;
        }

        private bool showKeyColumns = true;

        #endregion Properties

        #region Implementation properties

        /// <summary>
        /// Gets the DataTreeListView that is being managed
        /// </summary>
        protected DataTreeListView TreeListView { get; }

        #endregion Implementation properties

        #region Implementation

        /// <summary>
        ///
        /// </summary>
        protected override void InitializeDataSource()
        {
            base.InitializeDataSource();
            TreeListView.RebuildAll(true);
        }

        /// <summary>
        ///
        /// </summary>
        protected override void SetListContents() => TreeListView.Roots = CalculateRoots();

        /// <summary>
        ///
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected override bool ShouldCreateColumn(PropertyDescriptor property)
        {
            if (!ShowKeyColumns && (property.Name == KeyAspectName || property.Name == ParentKeyAspectName))
            {
                return false;
            }

            return base.ShouldCreateColumn(property);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void HandleListChangedItemChanged(ListChangedEventArgs e)
        {
            if (e.PropertyDescriptor != null &&
                (e.PropertyDescriptor.Name == KeyAspectName ||
                 e.PropertyDescriptor.Name == ParentKeyAspectName))
            {
                InitializeDataSource();
            }
            else
            {
                base.HandleListChangedItemChanged(e);
            }
        }

        /// <inheritdoc/>
        protected override void HandleCurrencyManagerPositionChanged(object sender, EventArgs e)
        {
            if (!DataSourceSelectionPolicy.ShouldApplyPositionChange(TreeListView, sender, e))
            {
                return;
            }

            var index = CurrencyManager.Position;
            if (index < 0 || index >= CurrencyManager.List.Count)
            {
                return;
            }

            ChangePosition(index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        protected override void ChangePosition(int index)
        {
            var model = CurrencyManager.List[index];
            var parent = CalculateParent(model);
            while (parent != null && !TreeListView.IsExpanded(parent))
            {
                TreeListView.Expand(parent);
                parent = CalculateParent(parent);
            }

            base.ChangePosition(index);
        }

        private IEnumerable CalculateRoots()
        {
            foreach (var x in CurrencyManager.List)
            {
                var parentKey = GetParentValue(x);
                if (Equals(RootKeyValue, parentKey))
                {
                    yield return x;
                }
            }
        }

        private bool CalculateHasChildren(object model)
        {
            var keyValue = GetKeyValue(model);
            if (keyValue == null)
            {
                return false;
            }

            foreach (var x in CurrencyManager.List)
            {
                var parentKey = GetParentValue(x);
                if (Equals(keyValue, parentKey))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable CalculateChildren(object model)
        {
            var keyValue = GetKeyValue(model);
            if (keyValue != null)
            {
                foreach (var x in CurrencyManager.List)
                {
                    var parentKey = GetParentValue(x);
                    if (Equals(keyValue, parentKey))
                    {
                        yield return x;
                    }
                }
            }
        }

        private object CalculateParent(object model)
        {
            var parentValue = GetParentValue(model);
            if (parentValue == null)
            {
                return null;
            }

            foreach (var x in CurrencyManager.List)
            {
                var key = GetKeyValue(x);
                if (Equals(parentValue, key))
                {
                    return x;
                }
            }
            return null;
        }

        private object GetKeyValue(object model) => keyMunger?.GetValue(model);

        private object GetParentValue(object model) => parentKeyMunger?.GetValue(model);

        #endregion Implementation

        private Munger keyMunger;
        private Munger parentKeyMunger;
    }
}