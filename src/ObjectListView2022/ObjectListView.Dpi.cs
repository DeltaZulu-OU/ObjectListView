using System;
using System.Drawing;

namespace BrightIdeasSoftware
{
    public partial class ObjectListView
    {
        private const float DefaultDpi = 96.0f;
        private const float FontSizeTolerance = 0.05f;
        private float dpiBaseFontSize;

        /// <summary>
        /// Rescale ObjectListView-owned state when the control moves between monitors with different DPI settings.
        /// </summary>
        /// <param name="deviceDpiOld">The DPI before the change.</param>
        /// <param name="deviceDpiNew">The DPI after the change.</param>
        protected override void RescaleConstantsForDpi(int deviceDpiOld, int deviceDpiNew)
        {
            using var fontBeforeScale = (Font)Font.Clone();

            UpdateDpiFontBaseline(fontBeforeScale.Size, deviceDpiOld);
            base.RescaleConstantsForDpi(deviceDpiOld, deviceDpiNew);

            if (IsDisposed || Disposing || deviceDpiOld <= 0 || deviceDpiNew <= 0)
            {
                return;
            }

            SuspendLayout();
            BeginUpdate();
            try
            {
                var targetFontSize = (float)Math.Round(dpiBaseFontSize * deviceDpiNew / DefaultDpi, 1);
                if (Math.Abs(Font.Size - targetFontSize) > FontSizeTolerance)
                {
                    Font = new Font(
                        fontBeforeScale.FontFamily,
                        targetFontSize,
                        fontBeforeScale.Style,
                        fontBeforeScale.Unit,
                        fontBeforeScale.GdiCharSet,
                        fontBeforeScale.GdiVerticalFont);
                }

                if (VirtualMode)
                {
                    ClearCachedInfo();
                }
                else
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var item = GetItem(i);
                        if (item != null)
                        {
                            RefreshItem(item);
                        }
                    }
                }

                Invalidate();
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    EndUpdate();
                    ResumeLayout(false);
                }
            }
        }

        private void UpdateDpiFontBaseline(float currentFontSize, int currentDpi)
        {
            if (currentDpi <= 0)
            {
                return;
            }

            var expectedFontSize = dpiBaseFontSize * currentDpi / DefaultDpi;
            if (dpiBaseFontSize <= 0 || Math.Abs(currentFontSize - expectedFontSize) > FontSizeTolerance)
            {
                dpiBaseFontSize = currentFontSize * DefaultDpi / currentDpi;
            }
        }
    }
}
