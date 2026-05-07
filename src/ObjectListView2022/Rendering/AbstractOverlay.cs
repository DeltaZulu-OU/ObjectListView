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
using System.ComponentModel;
using System.Drawing;

namespace BrightIdeasSoftware.Rendering
{
    /// <summary>
    /// A null implementation of the IOverlay interface
    /// </summary>
    public class AbstractOverlay : ITransparentOverlay
    {
        #region IOverlay Members

        /// <summary>
        /// Draw this overlay
        /// </summary>
        /// <param name="olv">The ObjectListView that is being overlaid</param>
        /// <param name="g">The Graphics onto the given OLV</param>
        /// <param name="r">The content area of the OLV</param>
        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
        }

        #endregion IOverlay Members

        #region ITransparentOverlay Members

        /// <summary>
        /// How transparent should this overlay be?
        /// </summary>
        [Category("ObjectListView"),
         Description("How transparent should this overlay be"),
         DefaultValue(128),
         NotifyParentProperty(true)]
        public int Transparency {
            get => transparency; set => transparency = Math.Min(255, Math.Max(0, value));
        }

        private int transparency = 128;

        #endregion ITransparentOverlay Members
    }
}