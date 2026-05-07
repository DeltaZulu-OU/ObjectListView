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
    /// Instances of this class draw a border around the decorated subitem.
    /// </summary>
    public class CellBorderDecoration : BorderDecoration
    {
        /// <summary>
        /// Calculate the boundaries of the border
        /// </summary>
        /// <returns></returns>
        protected override Rectangle CalculateBounds() => CellBounds;
    }
}