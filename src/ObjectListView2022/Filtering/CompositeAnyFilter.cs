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

using System.Collections.Generic;

namespace BrightIdeasSoftware.Filtering
{
    /// <summary>
    /// A CompositeAllFilter joins several other filters together.
    /// A model object must only satisfy one of the filters to be included.
    /// If there are no filters, all model objects are included
    /// </summary>
    public class CompositeAnyFilter : CompositeFilter
    {
        /// <summary>
        /// Create a filter from the given filters
        /// </summary>
        /// <param name="filters"></param>
        public CompositeAnyFilter(List<IModelFilter> filters)
            : base(filters)
        {
        }

        /// <summary>
        /// Decide whether or not the given model should be included by the filter
        /// </summary>
        /// <remarks>Filters is guaranteed to be non-empty when this method is called</remarks>
        /// <param name="modelObject">The model object under consideration</param>
        /// <returns>True if the object is included by the filter</returns>
        public override bool FilterObject(object modelObject)
        {
            foreach (var filter in Filters)
            {
                if (filter.Filter(modelObject))
                {
                    return true;
                }
            }

            return false;
        }
    }
}