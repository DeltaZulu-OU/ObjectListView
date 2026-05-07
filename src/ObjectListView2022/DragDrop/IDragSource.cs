/*
 * IDragSource.cs - Add drag source functionality to an ObjectListView
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
 * 2009-04-15   JPP  - Separated IDragSource.cs into DropSink.cs
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
    /// An IDragSource controls how drag out from the ObjectListView will behave
    /// </summary>
    public interface IDragSource
    {
        /// <summary>
        /// A drag operation is beginning. Return the data object that will be used
        /// for data transfer. Return null to prevent the drag from starting. The data
        /// object will normally include all the selected objects.
        /// </summary>
        /// <remarks>
        /// The returned object is later passed to the GetAllowedEffect() and EndDrag()
        /// methods.
        /// </remarks>
        /// <param name="olv">What ObjectListView is being dragged from.</param>
        /// <param name="button">Which mouse button is down?</param>
        /// <param name="item">What item was directly dragged by the user? There may be more than just this
        /// item selected.</param>
        /// <returns>The data object that will be used for data transfer. This will often be a subclass
        /// of DataObject, but does not need to be.</returns>
        object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item);

        /// <summary>
        /// What operations are possible for this drag? This controls the icon shown during the drag
        /// </summary>
        /// <param name="dragObject">The data object returned by StartDrag()</param>
        /// <returns>A combination of DragDropEffects flags</returns>
        DragDropEffects GetAllowedEffects(object dragObject);

        /// <summary>
        /// The drag operation is complete. Do whatever is necessary to complete the action.
        /// </summary>
        /// <param name="dragObject">The data object returned by StartDrag()</param>
        /// <param name="effect">The value returned from GetAllowedEffects()</param>
        void EndDrag(object dragObject, DragDropEffects effect);
    }
}