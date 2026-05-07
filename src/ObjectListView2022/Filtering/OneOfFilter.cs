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
using BrightIdeasSoftware.Implementation;

namespace BrightIdeasSoftware.Filtering
{
    /// <summary>
    /// Instances of this class extract a value from the model object
    /// and compare that value to a list of fixed values. The model
    /// object is included if the extracted value is in the list
    /// </summary>
    /// <remarks>If there is no delegate installed or there are
    /// no values to match, no model objects will be matched</remarks>
    public class OneOfFilter : IModelFilter
    {
        /// <summary>
        /// Create a filter that will use the given delegate to extract values
        /// </summary>
        /// <param name="valueGetter"></param>
        public OneOfFilter(AspectGetterDelegate valueGetter) :
            this(valueGetter, new ArrayList())
        {
        }

        /// <summary>
        /// Create a filter that will extract values using the given delegate
        /// and compare them to the values in the given list.
        /// </summary>
        /// <param name="valueGetter"></param>
        /// <param name="possibleValues"></param>
        public OneOfFilter(AspectGetterDelegate valueGetter, ICollection possibleValues)
        {
            ValueGetter = valueGetter;
            PossibleValues = new ArrayList(possibleValues);
        }

        /// <summary>
        /// Gets or sets the delegate that will be used to extract values
        /// from model objects
        /// </summary>
        public virtual AspectGetterDelegate ValueGetter {
            get => valueGetter; set => valueGetter = value;
        }

        private AspectGetterDelegate valueGetter;

        /// <summary>
        /// Gets or sets the list of values that the value extracted from
        /// the model object must match in order to be included.
        /// </summary>
        public virtual IList PossibleValues {
            get => possibleValues; set => possibleValues = value;
        }

        private IList possibleValues;

        /// <summary>
        /// Should the given model object be included?
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns></returns>
        public virtual bool Filter(object modelObject)
        {
            if (ValueGetter == null || PossibleValues == null || PossibleValues.Count == 0)
            {
                return false;
            }

            var result = ValueGetter(modelObject);
            if (result is string || result is not IEnumerable enumerable)
            {
                return DoesValueMatch(result);
            }

            foreach (var x in enumerable)
            {
                if (DoesValueMatch(x))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Decides if the given property is a match for the values in the PossibleValues collection
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual bool DoesValueMatch(object result) => PossibleValues.Contains(result);
    }
}