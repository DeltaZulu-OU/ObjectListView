/*
 * Filters - Filtering on ObjectListViews
 *
 * Author: Phillip Piper
 * Date: 03/03/2010 17:00
 *
 * Change log:
 * 2011-03-01  JPP  Added CompositeAllFilter, CompositeAnyFilter and OneOfFilter
 * v2.4.1
 * 2010-06-23  JPP  Extended TextMatchFilter to handle regular expressions and string prefix matching.
 * v2.4
 * 2010-03-03  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2010-2014 Phillip Piper
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

using System.Collections;

namespace BrightIdeasSoftware.Filtering
{
    /// <summary>
    /// Base class for whole list filters
    /// </summary>
    public class AbstractListFilter : IListFilter
    {
        /// <summary>
        /// Return a subset of the given list of model objects as the new
        /// contents of the ObjectListView
        /// </summary>
        /// <param name="modelObjects">The collection of model objects that the list will possibly display</param>
        /// <returns>The filtered collection that holds the model objects that will be displayed.</returns>
        public virtual IEnumerable Filter(IEnumerable modelObjects) => modelObjects;
    }
}