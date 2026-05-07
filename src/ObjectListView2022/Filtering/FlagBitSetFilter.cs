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
using System.Collections;
using System.Collections.Generic;
using BrightIdeasSoftware.Implementation;

namespace BrightIdeasSoftware.Filtering
{
    /// <summary>
    /// Instances of this class match a property of a model objects against
    /// a list of bit flags. The property should be an xor-ed collection
    /// of bits flags.
    /// </summary>
    /// <remarks>Both the property compared and the list of possible values
    /// must be convertible to ulongs.</remarks>
    public class FlagBitSetFilter : OneOfFilter
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="valueGetter"></param>
        /// <param name="possibleValues"></param>
        public FlagBitSetFilter(AspectGetterDelegate valueGetter, ICollection possibleValues) : base(valueGetter, possibleValues)
        {
            ConvertPossibleValues();
        }

        /// <summary>
        /// Gets or sets the collection of values that will be matched.
        /// These must be ulongs (or convertible to ulongs).
        /// </summary>
        public override IList PossibleValues {
            get => base.PossibleValues;
            set {
                base.PossibleValues = value;
                ConvertPossibleValues();
            }
        }

        private void ConvertPossibleValues()
        {
            possibleValuesAsUlongs = new List<ulong>();
            foreach (var x in PossibleValues)
            {
                possibleValuesAsUlongs.Add(Convert.ToUInt64(x));
            }
        }

        /// <summary>
        /// Decides if the given property is a match for the values in the PossibleValues collection
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override bool DoesValueMatch(object result)
        {
            try
            {
                var value = Convert.ToUInt64(result);
                foreach (var flag in possibleValuesAsUlongs)
                {
                    if ((value & flag) == flag)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private List<ulong> possibleValuesAsUlongs = new List<ulong>();
    }
}