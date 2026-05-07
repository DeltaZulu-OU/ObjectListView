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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using BrightIdeasSoftware.Implementation;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// An adorment is the common base for overlays and decorations.
    /// </summary>
    public class GraphicAdornment
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the corner of the adornment that will be positioned at the reference corner
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ContentAlignment AdornmentCorner { get; set; } = ContentAlignment.MiddleCenter;

        /// <summary>
        /// Gets or sets location within the reference rectange where the adornment will be drawn
        /// </summary>
        /// <remarks>This is a simplied interface to ReferenceCorner and AdornmentCorner </remarks>
        [Category("ObjectListView"),
         Description("How will the adornment be aligned"),
         DefaultValue(ContentAlignment.BottomRight),
         NotifyParentProperty(true)]
        public ContentAlignment Alignment {
            get => alignment;
            set {
                alignment = value;
                ReferenceCorner = value;
                AdornmentCorner = value;
            }
        }

        private ContentAlignment alignment = ContentAlignment.BottomRight;

        /// <summary>
        /// Gets or sets the offset by which the position of the adornment will be adjusted
        /// </summary>
        [Category("ObjectListView"),
         Description("The offset by which the position of the adornment will be adjusted"),
         DefaultValue(typeof(Size), "0,0")]
        public Size Offset {
            get => offset; set => offset = value;
        }

        private Size offset = new Size();

        /// <summary>
        /// Gets or sets the point of the reference rectangle to which the adornment will be aligned.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ContentAlignment ReferenceCorner { get; set; } = ContentAlignment.MiddleCenter;

        /// <summary>
        /// Gets or sets the degree of rotation by which the adornment will be transformed.
        /// The centre of rotation will be the center point of the adornment.
        /// </summary>
        [Category("ObjectListView"),
         Description("The degree of rotation that will be applied to the adornment."),
         DefaultValue(0),
         NotifyParentProperty(true)]
        public int Rotation { get; set; }

        /// <summary>
        /// Gets or sets the transparency of the overlay.
        /// 0 is completely transparent, 255 is completely opaque.
        /// </summary>
        [Category("ObjectListView"),
         Description("The transparency of this adornment. 0 is completely transparent, 255 is completely opaque."),
         DefaultValue(128)]
        public int Transparency {
            get => transparency; set => transparency = Math.Min(255, Math.Max(0, value));
        }

        private int transparency = 128;

        #endregion Public properties

        #region Calculations

        /// <summary>
        /// Calculate the location of rectangle of the given size,
        /// so that it's indicated corner would be at the given point.
        /// </summary>
        /// <param name="pt">The point</param>
        /// <param name="size"></param>
        /// <param name="corner">Which corner will be positioned at the reference point</param>
        /// <returns></returns>
        /// <example>CalculateAlignedPosition(new Point(50, 100), new Size(10, 20), System.Drawing.ContentAlignment.TopLeft) -> Point(50, 100)</example>
        /// <example>CalculateAlignedPosition(new Point(50, 100), new Size(10, 20), System.Drawing.ContentAlignment.MiddleCenter) -> Point(45, 90)</example>
        /// <example>CalculateAlignedPosition(new Point(50, 100), new Size(10, 20), System.Drawing.ContentAlignment.BottomRight) -> Point(40, 80)</example>
        public virtual Point CalculateAlignedPosition(Point pt, Size size, ContentAlignment corner) => corner switch
        {
            ContentAlignment.TopLeft => pt,
            ContentAlignment.TopCenter => new Point(pt.X - size.Width / 2, pt.Y),
            ContentAlignment.TopRight => new Point(pt.X - size.Width, pt.Y),
            ContentAlignment.MiddleLeft => new Point(pt.X, pt.Y - size.Height / 2),
            ContentAlignment.MiddleCenter => new Point(pt.X - size.Width / 2, pt.Y - size.Height / 2),
            ContentAlignment.MiddleRight => new Point(pt.X - size.Width, pt.Y - size.Height / 2),
            ContentAlignment.BottomLeft => new Point(pt.X, pt.Y - size.Height),
            ContentAlignment.BottomCenter => new Point(pt.X - size.Width / 2, pt.Y - size.Height),
            ContentAlignment.BottomRight => new Point(pt.X - size.Width, pt.Y - size.Height),
            // Should never reach here
            _ => pt,
        };

        /// <summary>
        /// Calculate a rectangle that has the given size which is positioned so that
        /// its alignment point is at the reference location of the given rect.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="sz"></param>
        /// <returns></returns>
        public virtual Rectangle CreateAlignedRectangle(Rectangle r, Size sz) => CreateAlignedRectangle(r, sz, ReferenceCorner, AdornmentCorner, Offset);

        /// <summary>
        /// Create a rectangle of the given size which is positioned so that
        /// its indicated corner is at the indicated corner of the reference rect.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="sz"></param>
        /// <param name="corner"></param>
        /// <param name="referenceCorner"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>Creates a rectangle so that its bottom left is at the centre of the reference:
        /// corner=BottomLeft, referenceCorner=MiddleCenter</para>
        /// <para>This is a powerful concept that takes some getting used to, but is
        /// very neat once you understand it.</para>
        /// </remarks>
        public virtual Rectangle CreateAlignedRectangle(Rectangle r, Size sz,
            ContentAlignment corner, ContentAlignment referenceCorner, Size offset)
        {
            var referencePt = CalculateCorner(r, referenceCorner);
            var topLeft = CalculateAlignedPosition(referencePt, sz, corner);
            return new Rectangle(topLeft + offset, sz);
        }

        /// <summary>
        /// Return the point at the indicated corner of the given rectangle (it doesn't
        /// have to be a corner, but a named location)
        /// </summary>
        /// <param name="r">The reference rectangle</param>
        /// <param name="corner">Which point of the rectangle should be returned?</param>
        /// <returns>A point</returns>
        /// <example>CalculateReferenceLocation(new Rectangle(0, 0, 50, 100), System.Drawing.ContentAlignment.TopLeft) -> Point(0, 0)</example>
        /// <example>CalculateReferenceLocation(new Rectangle(0, 0, 50, 100), System.Drawing.ContentAlignment.MiddleCenter) -> Point(25, 50)</example>
        /// <example>CalculateReferenceLocation(new Rectangle(0, 0, 50, 100), System.Drawing.ContentAlignment.BottomRight) -> Point(50, 100)</example>
        public virtual Point CalculateCorner(Rectangle r, ContentAlignment corner) => corner switch
        {
            ContentAlignment.TopLeft => new Point(r.Left, r.Top),
            ContentAlignment.TopCenter => new Point(r.X + r.Width / 2, r.Top),
            ContentAlignment.TopRight => new Point(r.Right, r.Top),
            ContentAlignment.MiddleLeft => new Point(r.Left, r.Top + r.Height / 2),
            ContentAlignment.MiddleCenter => new Point(r.X + r.Width / 2, r.Top + r.Height / 2),
            ContentAlignment.MiddleRight => new Point(r.Right, r.Top + r.Height / 2),
            ContentAlignment.BottomLeft => new Point(r.Left, r.Bottom),
            ContentAlignment.BottomCenter => new Point(r.X + r.Width / 2, r.Bottom),
            ContentAlignment.BottomRight => new Point(r.Right, r.Bottom),
            // Should never reach here
            _ => r.Location,
        };

        /// <summary>
        /// Given the item and the subitem, calculate its bounds.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subItem"></param>
        /// <returns></returns>
        public virtual Rectangle CalculateItemBounds(OLVListItem item, OLVListSubItem subItem)
        {
            if (item == null)
            {
                return Rectangle.Empty;
            }

            if (subItem == null)
            {
                return item.Bounds;
            }

            return item.GetSubItemBounds(item.SubItems.IndexOf(subItem));
        }

        #endregion Calculations

        #region Commands

        /// <summary>
        /// Apply any specified rotation to the Graphic content.
        /// </summary>
        /// <param name="g">The Graphics to be transformed</param>
        /// <param name="r">The rotation will be around the centre of this rect</param>
        protected virtual void ApplyRotation(Graphics g, Rectangle r)
        {
            if (Rotation == 0)
            {
                return;
            }

            // THINK: Do we want to reset the transform? I think we want to push a new transform
            g.ResetTransform();
            var m = new Matrix();
            m.RotateAt(Rotation, new Point(r.Left + r.Width / 2, r.Top + r.Height / 2));
            g.Transform = m;
        }

        /// <summary>
        /// Reverse the rotation created by ApplyRotation()
        /// </summary>
        /// <param name="g"></param>
        protected virtual void UnapplyRotation(Graphics g)
        {
            if (Rotation != 0)
            {
                g.ResetTransform();
            }
        }

        #endregion Commands
    }
}