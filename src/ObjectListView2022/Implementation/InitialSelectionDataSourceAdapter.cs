using System;

namespace BrightIdeasSoftware.Implementation
{
    internal static class DataSourceSelectionPolicy
    {
        internal static bool ShouldApplyPositionChange(ObjectListView listView, object sender, EventArgs e) =>
            sender != null || e != null || listView.SelectedObject != null;
    }

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
            if (!DataSourceSelectionPolicy.ShouldApplyPositionChange(ListView, sender, e))
            {
                return;
            }

            base.HandleCurrencyManagerPositionChanged(sender, e);
        }
    }
}
