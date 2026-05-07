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

using System;

namespace BrightIdeasSoftware.Filtering
{
    /// <summary>
    /// This filter calls a given Predicate to decide if a model object should be included
    /// </summary>
    public class ModelFilter : IModelFilter
    {
        /// <summary>
        /// Create a filter based on the given predicate
        /// </summary>
        /// <param name="predicate">The function that will filter objects</param>
        public ModelFilter(Predicate<object> predicate)
        {
            Predicate = predicate;
        }

        /// <summary>
        /// Gets or sets the predicate used to filter model objects
        /// </summary>
        protected Predicate<object> Predicate { get; set; }

        /// <summary>
        /// Should the given model object be included?
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns></returns>
        public virtual bool Filter(object modelObject) => Predicate == null ? true : Predicate(modelObject);
    }
}