/*
 * DragSource.cs - Add drag source functionality to an ObjectListView
 *
 * Author: Phillip Piper
 * Date: 2009-03-17 5:15 PM
 *
 * Change log:
 * 2011-03-29   JPP  - Separate OLVDataObject.cs
 * v2.3
 * 2009-07-06   JPP  - Make sure Link is acceptable as an drop effect by default
 *                     (since MS didn't make it part of the 'All' value)
 * v2.2
 * 2009-04-15   JPP  - Separated DragSource.cs into DropSink.cs
 * 2009-03-17   JPP  - Initial version
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

using System.Windows.Forms;
using BrightIdeasSoftware.Implementation;

namespace BrightIdeasSoftware.DragDrop
{

    /// <summary>
    /// A do-nothing implementation of IDragSource that can be safely subclassed.
    /// </summary>
    public class AbstractDragSource : IDragSource
    {
        #region IDragSource Members

        /// <summary>
        /// See IDragSource documentation
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="button"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item) => null;

        /// <summary>
        /// See IDragSource documentation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual DragDropEffects GetAllowedEffects(object data) => DragDropEffects.None;

        /// <summary>
        /// See IDragSource documentation
        /// </summary>
        /// <param name="dragObject"></param>
        /// <param name="effect"></param>
        public virtual void EndDrag(object dragObject, DragDropEffects effect)
        {
        }

        #endregion IDragSource Members
    }
}