using System;
using System.Drawing;
using BrightIdeasSoftware;

namespace ObjectListViewConsumerCompatibility
{
    internal sealed class LegacyObjectListView : ObjectListView
    {
        [Obsolete]
        protected override bool HandleHeaderRightClick() => base.HandleHeaderRightClick();

        [Obsolete]
        protected override void ShowColumnSelectMenu(Point pt) => base.ShowColumnSelectMenu(pt);

        [Obsolete]
        protected override void PrepareAlternateBackColors() => base.PrepareAlternateBackColors();

        [Obsolete]
        protected override void SetAllSubItemImages() => base.SetAllSubItemImages();
    }

    internal sealed class LegacyRenderer : BaseRenderer
    {
        [Obsolete]
        protected override Color GetTextBackgroundColor() => base.GetTextBackgroundColor();
    }

    internal static class ConsumerApiUsage
    {
        private static readonly BrightIdeasSoftware.Design.ObjectListViewDesigner designer = new BrightIdeasSoftware.Design.ObjectListViewDesigner();

        [Obsolete]
        public static void ExerciseCompileTimeSurface(ObjectListView listView, OLVColumn column)
        {
            listView.HeaderFont = SystemFonts.DefaultFont;
            listView.HighlightBackgroundColor = Color.Navy;
            listView.HighlightForegroundColor = Color.White;
            listView.UnfocusedHighlightBackgroundColor = Color.Gray;
            listView.UnfocusedHighlightForegroundColor = Color.Black;

            var selectedBack = listView.HighlightBackgroundColorOrDefault;
            var selectedFore = listView.HighlightForegroundColorOrDefault;
            var unfocusedBack = listView.UnfocusedHighlightBackgroundColorOrDefault;
            var unfocusedFore = listView.UnfocusedHighlightForegroundColorOrDefault;
            var hotItemIndex = listView.HotItemIndex;
            var tileColumns = listView.ColumnsForTileView;

            var selected = listView.GetSelectedObject();
            var selectedObjects = listView.GetSelectedObjects();
            var checkedObject = listView.GetCheckedObject();
            var checkedObjects = listView.GetCheckedObjects();

            var header = new HeaderControl(listView)
            {
                HotFontStyle = FontStyle.Bold
            };
            var hotFontStyle = header.HotFontStyle;

            var overlay = new TextOverlay
            {
                RoundCorneredBorder = true
            };
            var rounded = overlay.RoundCorneredBorder;

            var dataObject = new OLVDataObject(listView, selectedObjects);
            var html = dataObject.CreateHtml();

            Consume(column, selectedBack, selectedFore, unfocusedBack, unfocusedFore,
                hotItemIndex, tileColumns, selected, selectedObjects, checkedObject,
                checkedObjects, hotFontStyle, rounded, html, designer);
        }

        private static void Consume(params object[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }
        }
    }
}
