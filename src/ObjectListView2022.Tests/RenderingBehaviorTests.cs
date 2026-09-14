using BrightIdeasSoftware.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class RenderingBehaviorTests
    {
        [TestMethod]
        public void EditingCellBorderDecoration_ConstructorHonorsLightboxFlag()
        {
            var decoration = new EditingCellBorderDecoration(true);

            Assert.IsTrue(decoration.UseLightbox);
            Assert.IsNotNull(decoration.FillBrush);
        }
    }
}
