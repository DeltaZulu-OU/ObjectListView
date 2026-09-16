using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace BrightIdeasSoftware
#pragma warning restore IDE0130 // Namespace does not match folder structure
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
