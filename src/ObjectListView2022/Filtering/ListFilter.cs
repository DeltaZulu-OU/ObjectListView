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
    /// Instance of this class implement delegate based whole list filtering
    /// </summary>
    public class ListFilter : AbstractListFilter
    {
        /// <summary>
        /// A delegate that filters on a whole list
        /// </summary>
        /// <param name="rowObjects"></param>
        /// <returns></returns>
        public delegate IEnumerable ListFilterDelegate(IEnumerable rowObjects);

        /// <summary>
        /// Create a ListFilter
        /// </summary>
        /// <param name="function"></param>
        public ListFilter(ListFilterDelegate function)
        {
            Function = function;
        }

        /// <summary>
        /// Gets or sets the delegate that will filter the list
        /// </summary>
        public ListFilterDelegate Function { get; set; }

        /// <summary>
        /// Do the actual work of filtering
        /// </summary>
        /// <param name="modelObjects"></param>
        /// <returns></returns>
        public override IEnumerable Filter(IEnumerable modelObjects)
        {
            if (Function == null)
            {
                return modelObjects;
            }

            return Function(modelObjects);
        }
    }
}