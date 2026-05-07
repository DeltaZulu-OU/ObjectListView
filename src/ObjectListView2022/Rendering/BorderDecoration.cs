/*
 * Decorations - Images, text or other things that can be rendered onto an ObjectListView
 *
 * Author: Phillip Piper
 * Date: 19/08/2009 10:56 PM
 *
 * Change log:
 * 2011-04-04   JPP  - Added ability to have a gradient background on BorderDecoration
 * v2.4
 * 2010-04-15   JPP  - Tweaked LightBoxDecoration a little
 * v2.3
 * 2009-09-23   JPP  - Added LeftColumn and RightColumn to RowBorderDecoration
 * 2009-08-23   JPP  - Added LightBoxDecoration
 * 2009-08-19   JPP  - Initial version. Separated from Overlays.cs
 *
 * To do:
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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// This decoration draws an optionally filled border around a rectangle.
    /// Subclasses must override CalculateBounds().
    /// </summary>
    public class BorderDecoration : AbstractDecoration
    {
        #region Constructors

        /// <summary>
        /// Create a BorderDecoration
        /// </summary>
        public BorderDecoration()
            : this(new Pen(Color.FromArgb(64, Color.Blue), 1))
        {
        }

        /// <summary>
        /// Create a BorderDecoration
        /// </summary>
        /// <param name="borderPen">The pen used to draw the border</param>
        public BorderDecoration(Pen borderPen)
        {
            BorderPen = borderPen;
        }

        /// <summary>
        /// Create a BorderDecoration
        /// </summary>
        /// <param name="borderPen">The pen used to draw the border</param>
        /// <param name="fill">The brush used to fill the rectangle</param>
        public BorderDecoration(Pen borderPen, Brush fill)
        {
            BorderPen = borderPen;
            FillBrush = fill;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the pen that will be used to draw the border
        /// </summary>
        public Pen BorderPen { get; set; }

        /// <summary>
        /// Gets or sets the padding that will be added to the bounds of the item
        /// before drawing the border and fill.
        /// </summary>
        public Size BoundsPadding {
            get => boundsPadding; set => boundsPadding = value;
        }

        private Size boundsPadding = new Size(-1, 2);

        /// <summary>
        /// How rounded should the corners of the border be? 0 means no rounding.
        /// </summary>
        /// <remarks>If this value is too large, the edges of the border will appear odd.</remarks>
        public float CornerRounding { get; set; } = 16.0f;

        /// <summary>
        /// Gets or sets the brush that will be used to fill the border
        /// </summary>
        /// <remarks>This value is ignored when using gradient brush</remarks>
        public Brush FillBrush { get; set; } = new SolidBrush(Color.FromArgb(64, Color.Blue));

        /// <summary>
        /// Gets or sets the color that will be used as the start of a gradient fill.
        /// </summary>
        /// <remarks>This and FillGradientTo must be given value to show a gradient</remarks>
        public Color? FillGradientFrom { get; set; }

        /// <summary>
        /// Gets or sets the color that will be used as the end of a gradient fill.
        /// </summary>
        /// <remarks>This and FillGradientFrom must be given value to show a gradient</remarks>
        public Color? FillGradientTo { get; set; }

        /// <summary>
        /// Gets or sets the fill mode that will be used for the gradient.
        /// </summary>
        public LinearGradientMode FillGradientMode { get; set; } = LinearGradientMode.Vertical;

        #endregion Properties

        #region IOverlay Members

        /// <summary>
        /// Draw a filled border
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="g"></param>
        /// <param name="r"></param>
        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            var bounds = CalculateBounds();
            if (!bounds.IsEmpty)
            {
                DrawFilledBorder(g, bounds);
            }
        }

        #endregion IOverlay Members

        #region Subclass responsibility

        /// <summary>
        /// Subclasses should override this to say where the border should be drawn
        /// </summary>
        /// <returns></returns>
        protected virtual Rectangle CalculateBounds() => Rectangle.Empty;

        #endregion Subclass responsibility

        #region Implementation utlities

        /// <summary>
        /// Do the actual work of drawing the filled border
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bounds"></param>
        protected void DrawFilledBorder(Graphics g, Rectangle bounds)
        {
            bounds.Inflate(BoundsPadding);
            var path = GetRoundedRect(bounds, CornerRounding);
            if (FillGradientFrom != null && FillGradientTo != null)
            {
                FillBrush?.Dispose();

                FillBrush = new LinearGradientBrush(bounds, FillGradientFrom.Value, FillGradientTo.Value, FillGradientMode);
            }
            if (FillBrush != null)
            {
                g.FillPath(FillBrush, path);
            }

            if (BorderPen != null)
            {
                g.DrawPath(BorderPen, path);
            }
        }

        /// <summary>
        /// Create a GraphicsPath that represents a round cornered rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="diameter">If this is 0 or less, the rectangle will not be rounded.</param>
        /// <returns></returns>
        protected GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
        {
            var path = new GraphicsPath();

            if (diameter <= 0.0f)
            {
                path.AddRectangle(rect);
            }
            else
            {
                var arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            }

            return path;
        }

        #endregion Implementation utlities
    }
}