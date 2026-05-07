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
using System.Drawing.Drawing2D;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// An adornment that will draw text
    /// </summary>
    public class TextAdornment : GraphicAdornment
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the background color of the text
        /// Set this to Color.Empty to not draw a background
        /// </summary>
        [Category("ObjectListView"),
         Description("The background color of the text"),
         DefaultValue(typeof(Color), "")]
        public Color BackColor { get; set; } = Color.Empty;

        /// <summary>
        /// Gets the brush that will be used to paint the text
        /// </summary>
        [Browsable(false)]
        public Brush BackgroundBrush => new SolidBrush(Color.FromArgb(workingTransparency, BackColor));

        /// <summary>
        /// Gets or sets the color of the border around the billboard.
        /// Set this to Color.Empty to remove the border
        /// </summary>
        [Category("ObjectListView"),
         Description("The color of the border around the text"),
         DefaultValue(typeof(Color), "")]
        public Color BorderColor { get; set; } = Color.Empty;

        /// <summary>
        /// Gets the brush that will be used to paint the text
        /// </summary>
        [Browsable(false)]
        public Pen BorderPen => new Pen(Color.FromArgb(workingTransparency, BorderColor), BorderWidth);

        /// <summary>
        /// Gets or sets the width of the border around the text
        /// </summary>
        [Category("ObjectListView"),
         Description("The width of the border around the text"),
         DefaultValue(0.0f)]
        public float BorderWidth { get; set; }

        /// <summary>
        /// How rounded should the corners of the border be? 0 means no rounding.
        /// </summary>
        /// <remarks>If this value is too large, the edges of the border will appear odd.</remarks>
        [Category("ObjectListView"),
         Description("How rounded should the corners of the border be? 0 means no rounding."),
         DefaultValue(16.0f),
         NotifyParentProperty(true)]
        public float CornerRounding { get; set; } = 16.0f;

        /// <summary>
        /// Gets or sets the font that will be used to draw the text
        /// </summary>
        [Category("ObjectListView"),
         Description("The font that will be used to draw the text"),
         DefaultValue(null),
         NotifyParentProperty(true)]
        public Font Font { get; set; }

        /// <summary>
        /// Gets the font that will be used to draw the text or a reasonable default
        /// </summary>
        [Browsable(false)]
        public Font FontOrDefault => Font ?? new Font("Tahoma", 16);

        /// <summary>
        /// Does this text have a background?
        /// </summary>
        [Browsable(false)]
        public bool HasBackground => BackColor != Color.Empty;

        /// <summary>
        /// Does this overlay have a border?
        /// </summary>
        [Browsable(false)]
        public bool HasBorder => BorderColor != Color.Empty && BorderWidth > 0;

        /// <summary>
        /// Gets or sets the maximum width of the text. Text longer than this will wrap.
        /// 0 means no maximum.
        /// </summary>
        [Category("ObjectListView"),
         Description("The maximum width the text (0 means no maximum). Text longer than this will wrap"),
         DefaultValue(0)]
        public int MaximumTextWidth { get; set; }

        /// <summary>
        /// Gets or sets the formatting that should be used on the text
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual StringFormat StringFormat {
            get {
                if (stringFormat == null)
                {
                    stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    if (!Wrap)
                    {
                        stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                    }
                }
                return stringFormat;
            }

            set => stringFormat = value;
        }

        private StringFormat stringFormat;

        /// <summary>
        /// Gets or sets the text that will be drawn
        /// </summary>
        [Category("ObjectListView"),
         Description("The text that will be drawn over the top of the ListView"),
         DefaultValue(null),
         NotifyParentProperty(true),
         Localizable(true)]
        public string Text { get; set; }

        /// <summary>
        /// Gets the brush that will be used to paint the text
        /// </summary>
        [Browsable(false)]
        public Brush TextBrush => new SolidBrush(Color.FromArgb(workingTransparency, TextColor));

        /// <summary>
        /// Gets or sets the color of the text
        /// </summary>
        [Category("ObjectListView"),
         Description("The color of the text"),
         DefaultValue(typeof(Color), "DarkBlue"),
         NotifyParentProperty(true)]
        public Color TextColor { get; set; } = Color.DarkBlue;

        /// <summary>
        /// Gets or sets whether the text will wrap when it exceeds its bounds
        /// </summary>
        [Category("ObjectListView"),
         Description("Will the text wrap?"),
         DefaultValue(true)]
        public bool Wrap { get; set; } = true;

        #endregion Public properties

        #region Implementation

        /// <summary>
        /// Draw our text with our stored configuration in relation to the given
        /// reference rectangle
        /// </summary>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The reference rectangle in relation to which the text will be drawn</param>
        public virtual void DrawText(Graphics g, Rectangle r) => DrawText(g, r, Text, Transparency);

        /// <summary>
        /// Draw the given text with our stored configuration
        /// </summary>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The reference rectangle in relation to which the text will be drawn</param>
        /// <param name="s">The text to draw</param>
        /// <param name="transparency">How opaque should be text be</param>
        public virtual void DrawText(Graphics g, Rectangle r, string s, int transparency)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }

            var textRect = CalculateTextBounds(g, r, s);
            DrawBorderedText(g, textRect, s, transparency);
        }

        /// <summary>
        /// Draw the text with a border
        /// </summary>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="textRect">The bounds within which the text should be drawn</param>
        /// <param name="text">The text to draw</param>
        /// <param name="transparency">How opaque should be text be</param>
        protected virtual void DrawBorderedText(Graphics g, Rectangle textRect, string text, int transparency)
        {
            var borderRect = textRect;
            borderRect.Inflate((int)BorderWidth / 2, (int)BorderWidth / 2);
            borderRect.Y -= 1; // Looker better a little higher

            try
            {
                ApplyRotation(g, textRect);
                using var path = GetRoundedRect(borderRect, CornerRounding);
                workingTransparency = transparency;
                if (HasBackground)
                {
                    using var b = BackgroundBrush;
                    g.FillPath(b, path);
                }

                using (var b = TextBrush)
                {
                    g.DrawString(text, FontOrDefault, b, textRect, StringFormat);
                }

                if (HasBorder)
                {
                    using var p = BorderPen;
                    g.DrawPath(p, path);
                }
            }
            finally
            {
                UnapplyRotation(g);
            }
        }

        /// <summary>
        /// Return the rectangle that will be the precise bounds of the displayed text
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns>The bounds of the text</returns>
        protected virtual Rectangle CalculateTextBounds(Graphics g, Rectangle r, string s)
        {
            var maxWidth = MaximumTextWidth <= 0 ? r.Width : MaximumTextWidth;
            var sizeF = g.MeasureString(s, FontOrDefault, maxWidth, StringFormat);
            var size = new Size(1 + (int)sizeF.Width, 1 + (int)sizeF.Height);
            return CreateAlignedRectangle(r, size);
        }

        /// <summary>
        /// Return a GraphicPath that is a round cornered rectangle
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="diameter">The diameter of the corners</param>
        /// <returns>A round cornered rectagle path</returns>
        /// <remarks>If I could rely on people using C# 3.0+, this should be
        /// an extension method of GraphicsPath.</remarks>
        protected virtual GraphicsPath GetRoundedRect(Rectangle rect, float diameter)
        {
            var path = new GraphicsPath();

            if (diameter > 0)
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
            else
            {
                path.AddRectangle(rect);
            }

            return path;
        }

        #endregion Implementation

        private int workingTransparency;
    }
}