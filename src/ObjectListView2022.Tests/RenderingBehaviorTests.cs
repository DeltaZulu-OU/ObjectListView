using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
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

        [TestMethod]
        public void ObjectListView_CustomOverlayWithoutTransparency_UsesLegacyOverlayTransparency()
        {
            Exception failure = null;
            double actualOpacity = -1;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var form = new Form())
                    using (var objectListView = new ObjectListView { Dock = DockStyle.Fill })
                    {
                        objectListView.OverlayTransparency = 64;
                        form.Controls.Add(objectListView);
                        form.Show();

                        objectListView.AddOverlay(new PlainOverlay());
                        objectListView.ShowOverlays();

                        var glassPanelsField = typeof(ObjectListView).GetField(
                            "glassPanels",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        var glassPanels = (IEnumerable)glassPanelsField.GetValue(objectListView);
                        foreach (var glassPanel in glassPanels)
                        {
                            actualOpacity = ((Form)glassPanel).Opacity;
                            break;
                        }

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
            Assert.AreEqual(64 / 255.0, actualOpacity, 0.0001);
        }

        [TestMethod]
        public void ObjectListView_OverlayTransparency_ClampsToByteRange()
        {
            using (var objectListView = new ObjectListView())
            {
                objectListView.OverlayTransparency = -1;
                Assert.AreEqual(0, objectListView.OverlayTransparency);

                objectListView.OverlayTransparency = 256;
                Assert.AreEqual(255, objectListView.OverlayTransparency);
            }
        }

        private sealed class PlainOverlay : IOverlay
        {
            public void Draw(ObjectListView olv, Graphics g, Rectangle r)
            {
            }
        }
    }
}
