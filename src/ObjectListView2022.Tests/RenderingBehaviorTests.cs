using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class RenderingBehaviorTests
    {
        [TestMethod]
        public void EditingCellBorderDecoration_ConstructorHonorsEnabledLightboxFlag()
        {
            var decoration = new EditingCellBorderDecoration(true);

            Assert.IsTrue(decoration.UseLightbox);
            Assert.IsNotNull(decoration.FillBrush);
        }

        [TestMethod]
        public void EditingCellBorderDecoration_ConstructorHonorsDisabledLightboxFlag()
        {
            var decoration = new EditingCellBorderDecoration(false);

            Assert.IsFalse(decoration.UseLightbox);
            Assert.IsNull(decoration.FillBrush);
        }

        [TestMethod]
        public void ObjectListView_CustomOverlayWithoutTransparency_CanBeShown()
        {
            Exception failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var form = new Form())
                    using (var objectListView = new ObjectListView { Dock = DockStyle.Fill })
                    {
                        form.Controls.Add(objectListView);
                        form.Show();

                        objectListView.AddOverlay(new PlainOverlay());
                        objectListView.ShowOverlays();

                        form.Close();
                    }
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.IsNull(failure, failure?.ToString());
        }

        private sealed class PlainOverlay : IOverlay
        {
            public void Draw(ObjectListView olv, Graphics g, Rectangle r)
            {
            }
        }
    }
}
