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
    }
}
