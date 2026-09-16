using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
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
                        var overlay = new PlainOverlay();
                        objectListView.OverlayTransparency = 64;
                        form.Controls.Add(objectListView);
                        form.Show();

                        objectListView.AddOverlay(overlay);
                        objectListView.ShowOverlays();

                        actualOpacity = GetOverlayOpacity(objectListView, overlay);
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
        public void ObjectListView_TransparentOverlay_UsesItsOwnTransparency()
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
                        var overlay = new TransparentOverlay { Transparency = 192 };
                        objectListView.OverlayTransparency = 64;
                        form.Controls.Add(objectListView);
                        form.Show();

                        objectListView.AddOverlay(overlay);
                        objectListView.ShowOverlays();

                        actualOpacity = GetOverlayOpacity(objectListView, overlay);
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
            Assert.AreEqual(192 / 255.0, actualOpacity, 0.0001);
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

        private static double GetOverlayOpacity(ObjectListView objectListView, IOverlay overlay)
        {
            var glassPanelsField = typeof(ObjectListView).GetField(
                "glassPanels",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(glassPanelsField);

            var glassPanels = (IEnumerable)glassPanelsField.GetValue(objectListView);
            foreach (var glassPanel in glassPanels)
            {
                var overlayField = glassPanel.GetType().GetField(
                    "Overlay",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Assert.IsNotNull(overlayField);

                if (ReferenceEquals(overlayField.GetValue(glassPanel), overlay))
                {
                    return ((Form)glassPanel).Opacity;
                }
            }

            Assert.Fail("No glass panel was created for the requested overlay.");
            return -1;
        }

        private sealed class PlainOverlay : IOverlay
        {
            public void Draw(ObjectListView olv, Graphics g, Rectangle r)
            {
            }
        }

        private sealed class TransparentOverlay : ITransparentOverlay
        {
            public int Transparency { get; set; }

            public void Draw(ObjectListView olv, Graphics g, Rectangle r)
            {
            }
        }
    }
}
