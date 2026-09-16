using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class ToolTipGdiOwnershipTests
    {
        private const uint GrGdiObjects = 0;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetGuiResources(IntPtr process, uint flags);

        [TestMethod]
        public void Font_RepeatedReplacementDoesNotAccumulateGdiHandles()
        {
            using var parent = new Form();
            _ = parent.Handle;

            var toolTip = new ToolTipProbe();
            toolTip.Create(parent.Handle);
            var fonts = new List<Font>();

            try
            {
                var baseline = GetCurrentGdiObjectCount();

                for (var i = 0; i < 24; i++)
                {
                    var font = new Font(
                        Control.DefaultFont.FontFamily,
                        Control.DefaultFont.Size + ((i + 1) * 0.25f),
                        Control.DefaultFont.Style,
                        GraphicsUnit.Point);
                    fonts.Add(font);
                    toolTip.Font = font;
                }

                var afterReplacement = GetCurrentGdiObjectCount();
                var increase = (int)(afterReplacement - baseline);

                Assert.IsLessThanOrEqualTo(
                    2, increase,
                    $"Replacing the tooltip font increased the process GDI handle count by {increase}. " +
                    "The tooltip should release each owned HFONT after replacing it.");
            }
            finally
            {
                toolTip.DestroyHandleForTest();
                foreach (var font in fonts)
                {
                    font.Dispose();
                }
            }
        }

        private static uint GetCurrentGdiObjectCount()
        {
            using var process = Process.GetCurrentProcess();
            return GetGuiResources(process.Handle, GrGdiObjects);
        }

        private sealed class ToolTipProbe : ToolTipControl
        {
            public void DestroyHandleForTest() => DestroyHandle();
        }
    }
}
