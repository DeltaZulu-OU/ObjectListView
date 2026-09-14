using System;

namespace BrightIdeasSoftware.Implementation
{
    /// <summary>
    /// A DataSourceAdapter that preserves an explicitly empty selection during initial binding.
    /// </summary>
    internal sealed class InitialSelectionDataSourceAdapter : DataSourceAdapter
    {
        public InitialSelectionDataSourceAdapter(ObjectListView listView)
            : base(listView)
        {
        }

        protected override void HandleCurrencyManagerPositionChanged(object sender, EventArgs e)
        {
            // InitializeDataSource() invokes this handler directly to synchronize the
            // CurrencyManager's initial position. Do not turn that synthetic sync into
            // an implicit first-row selection when the control has no selection.
            if (sender == null && e == null && ListView.SelectedObject == null)
            {
                return;
            }

            base.HandleCurrencyManagerPositionChanged(sender, e);
        }
    }
}
