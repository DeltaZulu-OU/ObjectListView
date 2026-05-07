/*
 * Overlays - Images, text or other things that can be rendered over the top of a ListView
 *
 * Author: Phillip Piper
 * Date: 14/04/2009 4:36 PM
 *
 * Change log:
 * v2.3
 * 2009-08-17   JPP  - Overlays now use Adornments
 *                   - Added ITransparentOverlay interface. Overlays can now have separate transparency levels
 * 2009-08-10   JPP  - Moved decoration related code to new file
 * v2.2.1
 * 200-07-24    JPP  - TintedColumnDecoration now works when last item is a member of a collapsed
 *                     group (well, it no longer crashes).
 * v2.2
 * 2009-06-01   JPP  - Make sure that TintedColumnDecoration reaches to the last item in group view
 * 2009-05-05   JPP  - Unified BillboardOverlay text rendering with that of TextOverlay
 * 2009-04-30   JPP  - Added TintedColumnDecoration
 * 2009-04-14   JPP  - Initial version
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

using System;
using System.Drawing;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// A Billboard overlay is a TextOverlay positioned at an absolute point
    /// </summary>
    public class BillboardOverlay : TextOverlay
    {
        /// <summary>
        /// Create a BillboardOverlay
        /// </summary>
        public BillboardOverlay()
        {
            Transparency = 255;
            BackColor = Color.PeachPuff;
            TextColor = Color.Black;
            BorderColor = Color.Empty;
            Font = new Font("Tahoma", 10);
        }

        /// <summary>
        /// Gets or sets where should the top left of the billboard be placed
        /// </summary>
        public Point Location {
            get => location; set => location = value;
        }

        private Point location;

        /// <summary>
        /// Draw this overlay
        /// </summary>
        /// <param name="olv">The ObjectListView being decorated</param>
        /// <param name="g">The Graphics used for drawing</param>
        /// <param name="r">The bounds of the rendering</param>
        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            // Calculate the bounds of the text, and then move it to where it should be
            var textRect = CalculateTextBounds(g, r, Text);
            textRect.Location = Location;

            // Make sure the billboard is within the bounds of the List, as far as is possible
            if (textRect.Right > r.Width)
            {
                textRect.X = Math.Max(r.Left, r.Width - textRect.Width);
            }

            if (textRect.Bottom > r.Height)
            {
                textRect.Y = Math.Max(r.Top, r.Height - textRect.Height);
            }

            DrawBorderedText(g, textRect, Text, 255);
        }
    }
}