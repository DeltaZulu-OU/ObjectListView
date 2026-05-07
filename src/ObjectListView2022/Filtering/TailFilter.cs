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
    /// Filter the list so only the last N entries are displayed
    /// </summary>
    public class TailFilter : AbstractListFilter
    {
        /// <summary>
        /// Create a no-op tail filter
        /// </summary>
        public TailFilter()
        {
        }

        /// <summary>
        /// Create a filter that includes on the last N model objects
        /// </summary>
        /// <param name="numberOfObjects"></param>
        public TailFilter(int numberOfObjects)
        {
            Count = numberOfObjects;
        }

        /// <summary>
        /// Gets or sets the number of model objects that will be
        /// returned from the tail of the list
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Return the last N subset of the model objects
        /// </summary>
        /// <param name="modelObjects"></param>
        /// <returns></returns>
        public override IEnumerable Filter(IEnumerable modelObjects)
        {
            if (Count <= 0)
            {
                return modelObjects;
            }

            var list = ObjectListView.EnumerableToArray(modelObjects, false);

            if (Count > list.Count)
            {
                return list;
            }

            var tail = new object[Count];
            list.CopyTo(list.Count - Count, tail, 0, Count);
            return new ArrayList(tail);
        }
    }
}