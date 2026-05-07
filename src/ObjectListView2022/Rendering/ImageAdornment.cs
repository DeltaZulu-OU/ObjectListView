/*
 * Adornments - Adornments are the basis for overlays and decorations -- things that can be rendered over the top of a ListView
 *
 * Author: Phillip Piper
 * Date: 16/08/2009 1:02 AM
 *
 * Change log:
 * v2.6
 * 2012-08-18   JPP  - Correctly dispose of brush and pen resources
 * v2.3
 * 2009-09-22   JPP  - Added Wrap property to TextAdornment, to allow text wrapping to be disabled
 *                   - Added ShrinkToWidth property to ImageAdornment
 * 2009-08-17   JPP  - Initial version
 *
 * To do:
 * - Use IPointLocator rather than Corners
 * - Add RotationCenter property ratherr than always using middle center
 *
 * Copyright (C) 2009-2014 Phillip Piper
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip.piper@gmail.com.
 */

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// An overlay that will draw an image over the top of the ObjectListView
    /// </summary>
    public class ImageAdornment : GraphicAdornment
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the image that will be drawn
        /// </summary>
        [Category("ObjectListView"),
         Description("The image that will be drawn"),
         DefaultValue(null),
         NotifyParentProperty(true)]
        public Image Image { get; set; }

        /// <summary>
        /// Gets or sets if the image will be shrunk to fit with its horizontal bounds
        /// </summary>
        [Category("ObjectListView"),
         Description("Will the image be shrunk to fit within its width?"),
         DefaultValue(false)]
        public bool ShrinkToWidth { get; set; }

        #endregion Public properties

        #region Commands

        /// <summary>
        /// Draw the image in its specified location
        /// </summary>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The bounds of the rendering</param>
        public virtual void DrawImage(Graphics g, Rectangle r)
        {
            if (ShrinkToWidth)
            {
                DrawScaledImage(g, r, Image, Transparency);
            }
            else
            {
                DrawImage(g, r, Image, Transparency);
            }
        }

        /// <summary>
        /// Draw the image in its specified location
        /// </summary>
        /// <param name="image">The image to be drawn</param>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The bounds of the rendering</param>
        /// <param name="transparency">How transparent should the image be (0 is completely transparent, 255 is opaque)</param>
        public virtual void DrawImage(Graphics g, Rectangle r, Image image, int transparency)
        {
            if (image != null)
            {
                DrawImage(g, r, image, image.Size, transparency);
            }
        }

        /// <summary>
        /// Draw the image in its specified location
        /// </summary>
        /// <param name="image">The image to be drawn</param>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The bounds of the rendering</param>
        /// <param name="sz">How big should the image be?</param>
        /// <param name="transparency">How transparent should the image be (0 is completely transparent, 255 is opaque)</param>
        public virtual void DrawImage(Graphics g, Rectangle r, Image image, Size sz, int transparency)
        {
            if (image == null)
            {
                return;
            }

            var adornmentBounds = CreateAlignedRectangle(r, sz);
            try
            {
                ApplyRotation(g, adornmentBounds);
                DrawTransparentBitmap(g, adornmentBounds, image, transparency);
            }
            finally
            {
                UnapplyRotation(g);
            }
        }

        /// <summary>
        /// Draw the image in its specified location, scaled so that it is not wider
        /// than the given rectangle. Height is scaled proportional to the width.
        /// </summary>
        /// <param name="image">The image to be drawn</param>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The bounds of the rendering</param>
        /// <param name="transparency">How transparent should the image be (0 is completely transparent, 255 is opaque)</param>
        public virtual void DrawScaledImage(Graphics g, Rectangle r, Image image, int transparency)
        {
            if (image == null)
            {
                return;
            }

            // If the image is too wide to be drawn in the space provided, proportionally scale it down.
            // Too tall images are not scaled.
            var size = image.Size;
            if (image.Width > r.Width)
            {
                var scaleRatio = r.Width / (float)image.Width;
                size.Height = (int)(image.Height * scaleRatio);
                size.Width = r.Width - 1;
            }

            DrawImage(g, r, image, size, transparency);
        }

        /// <summary>
        /// Utility to draw a bitmap transparenly.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="image"></param>
        /// <param name="transparency"></param>
        protected virtual void DrawTransparentBitmap(Graphics g, Rectangle r, Image image, int transparency)
        {
            ImageAttributes imageAttributes = null;
            if (transparency != 255)
            {
                imageAttributes = new ImageAttributes();
                var a = transparency / 255.0f;
                float[][] colorMatrixElements = {
                    new float[] {1,  0,  0,  0, 0},
                    new float[] {0,  1,  0,  0, 0},
                    new float[] {0,  0,  1,  0, 0},
                    new float[] {0,  0,  0,  a, 0},
                    new float[] {0,  0,  0,  0, 1}};

                imageAttributes.SetColorMatrix(new ColorMatrix(colorMatrixElements));
            }

            g.DrawImage(image,
               r,                                          // destination rectangle
               0, 0, image.Size.Width, image.Size.Height,  // source rectangle
               GraphicsUnit.Pixel,
               imageAttributes);
        }

        #endregion Commands
    }
}