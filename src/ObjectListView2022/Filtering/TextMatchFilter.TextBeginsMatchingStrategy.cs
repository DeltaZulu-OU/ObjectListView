/*
 * TextMatchFilter - Text based filtering on ObjectListViews
 *
 * Author: Phillip Piper
 * Date: 31/05/2011 7:45am
 *
 * Change log:
 * v2.6
 * 2012-10-13  JPP  Allow filtering to consider additional columns
 * v2.5.1
 * 2011-06-22  JPP  Handle searching for empty strings
 * v2.5.0
 * 2011-05-31  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2011-2014 Phillip Piper
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
using System.Drawing;

namespace BrightIdeasSoftware.Filtering
{
public partial class TextMatchFilter
    {
        /// <summary>
        /// This component provides text begins with matching strategy.
        /// </summary>
        protected class TextBeginsMatchingStrategy : TextMatchingStrategy
        {
            /// <summary>
            /// Create a text begins strategy
            /// </summary>
            /// <param name="filter"></param>
            /// <param name="text"></param>
            public TextBeginsMatchingStrategy(TextMatchFilter filter, string text)
            {
                TextFilter = filter;
                Text = text;
            }

            /// <summary>
            /// Does the given text match the filter
            /// </summary>
            /// <remarks>
            /// <para>this.Text will not be null or empty when this is called.</para>
            /// </remarks>
            /// <param name="cellText">The text of the cell we want to search</param>
            /// <returns>Return true if the given cellText matches our strategy</returns>
            public override bool MatchesText(string cellText) => cellText.StartsWith(Text, StringComparison);

            /// <summary>
            /// Find all the ways in which this filter matches the given string.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This is used by the renderer to decide which bits of
            /// the string should be highlighted.
            /// </para>
            /// <para>this.Text will not be null or empty when this is called.</para>
            /// </remarks>
            /// <param name="cellText">The text of the cell we want to search</param>
            /// <returns>A list of character ranges indicating the matched substrings</returns>
            public override IEnumerable<CharacterRange> FindAllMatchedRanges(string cellText)
            {
                var ranges = new List<CharacterRange>();

                if (cellText.StartsWith(Text, StringComparison))
                {
                    ranges.Add(new CharacterRange(0, Text.Length));
                }

                return ranges;
            }
        }
    }
}