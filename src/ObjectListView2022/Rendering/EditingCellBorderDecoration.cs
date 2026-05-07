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

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// This decoration puts a border around the cell being edited and
    /// optionally "lightboxes" the cell (makes the rest of the control dark).
    /// </summary>
    public class EditingCellBorderDecoration : BorderDecoration
    {
        #region Life and death

        /// <summary>
        /// Create a EditingCellBorderDecoration
        /// </summary>
        public EditingCellBorderDecoration()
        {
            FillBrush = null;
            BorderPen = new Pen(Color.DarkBlue, 2);
            CornerRounding = 8;
            BoundsPadding = new Size(10, 8);
        }

        /// <summary>
        /// Create a EditingCellBorderDecoration
        /// </summary>
        /// <param name="useLightBox">Should the decoration use a lighbox display style?</param>
        public EditingCellBorderDecoration(bool useLightBox) : this()
        {
            UseLightbox = useLightbox;
        }

        #endregion Life and death

        #region Configuration properties

        /// <summary>
        /// Gets or set whether the decoration should make the rest of
        /// the control dark when a cell is being edited
        /// </summary>
        /// <remarks>If this is true, FillBrush is used to overpaint
        /// the control.</remarks>
        public bool UseLightbox {
            get => useLightbox;
            set {
                if (useLightbox == value)
                {
                    return;
                }

                useLightbox = value;
                if (useLightbox)
                {
                    FillBrush ??= new SolidBrush(Color.FromArgb(64, Color.Black));
                }
            }
        }

        private bool useLightbox;

        #endregion Configuration properties

        #region Implementation

        /// <summary>
        /// Draw the decoration
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="g"></param>
        /// <param name="r"></param>
        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (!olv.IsCellEditing)
            {
                return;
            }

            var bounds = olv.CellEditor.Bounds;
            if (bounds.IsEmpty)
            {
                return;
            }

            bounds.Inflate(BoundsPadding);
            var path = GetRoundedRect(bounds, CornerRounding);
            if (FillBrush != null)
            {
                if (UseLightbox)
                {
                    using var newClip = new Region(r);
                    newClip.Exclude(path);
                    var originalClip = g.Clip;
                    g.Clip = newClip;
                    g.FillRectangle(FillBrush, r);
                    g.Clip = originalClip;
                }
                else
                {
                    g.FillPath(FillBrush, path);
                }
            }
            if (BorderPen != null)
            {
                g.DrawPath(BorderPen, path);
            }
        }

        #endregion Implementation
    }
}