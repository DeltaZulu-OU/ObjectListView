using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware.Implementation;

namespace BrightIdeasSoftware
{
    public partial class ObjectListView
    {
        /// <summary>
        /// Route keyboard context-menu activation through the same cell event used by mouse input.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (IsKeyboardContextMenuKey(keyData) && HandleKeyboardContextMenu())
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static bool IsKeyboardContextMenuKey(Keys keyData) =>
            keyData == Keys.Apps || keyData == (Keys.Shift | Keys.F10);

        /// <summary>
        /// Raise CellRightClick for the focused row when the context menu is requested from the keyboard.
        /// </summary>
        /// <returns>
        /// True when the event handled the request or supplied a menu; otherwise false so WinForms can
        /// continue its normal context-menu processing.
        /// </returns>
        protected virtual bool HandleKeyboardContextMenu()
        {
            if (FocusedItem is not OLVListItem item)
            {
                return false;
            }

            var subItem = item.GetSubItem(0);
            var bounds = subItem?.Bounds ?? item.Bounds;
            var location = new Point(
                bounds.Left + bounds.Width / 2,
                bounds.Top + bounds.Height / 2);
            var hitTest = new OlvListViewHitTestInfo(
                item,
                subItem,
                (int)ListViewHitTestLocations.Label,
                null,
                0);

            var args = new CellRightClickEventArgs();
            BuildCellEvent(args, location, hitTest);
            OnCellRightClick(args);

            if (args.Handled)
            {
                return true;
            }

            if (args.MenuStrip == null)
            {
                return false;
            }

            args.MenuStrip.Show(this, location);
            return true;
        }
    }
}
