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
using System.Windows.Forms;
using BrightIdeasSoftware.Implementation;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// This decoration draws a slight tint over a column of the
    /// owning listview. If no column is explicitly set, the selected
    /// column in the listview will be used.
    /// The selected column is normally the sort column, but does not have to be.
    /// </summary>
    public class TintedColumnDecoration : AbstractDecoration
    {
        #region Constructors

        /// <summary>
        /// Create a TintedColumnDecoration
        /// </summary>
        public TintedColumnDecoration()
        {
            Tint = Color.FromArgb(15, Color.Blue);
        }

        /// <summary>
        /// Create a TintedColumnDecoration
        /// </summary>
        /// <param name="column"></param>
        public TintedColumnDecoration(OLVColumn column)
            : this()
        {
            ColumnToTint = column;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the column that will be tinted
        /// </summary>
        public OLVColumn ColumnToTint { get; set; }

        /// <summary>
        /// Gets or sets the color that will be 'tinted' over the selected column
        /// </summary>
        public Color Tint {
            get => tint;
            set {
                if (tint == value)
                {
                    return;
                }

                if (tintBrush != null)
                {
                    tintBrush.Dispose();
                    tintBrush = null;
                }

                tint = value;
                tintBrush = new SolidBrush(tint);
            }
        }

        private Color tint;
        private SolidBrush tintBrush;

        #endregion Properties

        #region IOverlay Members

        /// <summary>
        /// Draw a slight colouring over our tinted column
        /// </summary>
        /// <remarks>
        /// This overlay only works when:
        /// - the list is in Details view
        /// - there is at least one row
        /// - there is a selected column (or a specified tint column)
        /// </remarks>
        /// <param name="olv"></param>
        /// <param name="g"></param>
        /// <param name="r"></param>
        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (olv.View != View.Details)
            {
                return;
            }

            if (olv.GetItemCount() == 0)
            {
                return;
            }

            var column = ColumnToTint ?? olv.SelectedColumn;
            if (column == null)
            {
                return;
            }

            var sides = NativeMethods.GetScrolledColumnSides(olv, column.Index);
            if (sides.X == -1)
            {
                return;
            }

            var columnBounds = new Rectangle(sides.X, r.Top, sides.Y - sides.X, r.Bottom);

            // Find the bottom of the last item. The tinting should extend only to there.
            var lastItem = olv.GetLastItemInDisplayOrder();
            if (lastItem != null)
            {
                var lastItemBounds = lastItem.Bounds;
                if (!lastItemBounds.IsEmpty && lastItemBounds.Bottom < columnBounds.Bottom)
                {
                    columnBounds.Height = lastItemBounds.Bottom - columnBounds.Top;
                }
            }
            g.FillRectangle(tintBrush, columnBounds);
        }

        #endregion IOverlay Members
    }
}