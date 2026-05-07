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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace BrightIdeasSoftware.Filtering
{
public partial class TextMatchFilter
    {
        /// <summary>
        /// This component provides regex matching strategy.
        /// </summary>
        protected class TextRegexMatchingStrategy : TextMatchingStrategy
        {
            /// <summary>
            /// Creates a regex strategy
            /// </summary>
            /// <param name="filter"></param>
            /// <param name="text"></param>
            public TextRegexMatchingStrategy(TextMatchFilter filter, string text)
            {
                TextFilter = filter;
                Text = text;
            }

            /// <summary>
            /// Gets or sets the options that will be used when compiling the regular expression.
            /// </summary>
            public RegexOptions RegexOptions => TextFilter.RegexOptions;

            /// <summary>
            /// Gets or sets a compilex regular expression, based on our current Text and RegexOptions.
            /// </summary>
            /// <remarks>
            /// If Text fails to compile as a regular expression, this will return a Regex object
            /// that will match all strings.
            /// </remarks>
            protected Regex Regex {
                get {
                    if (regex == null)
                    {
                        try
                        {
                            regex = new Regex(Text, RegexOptions);
                        }
                        catch (ArgumentException)
                        {
                            regex = InvalidRegexMarker;
                        }
                    }
                    return regex;
                }

                set => regex = value;
            }

            private Regex regex;

            /// <summary>
            /// Gets whether or not our current regular expression is a valid regex
            /// </summary>
            protected bool IsRegexInvalid => Regex == InvalidRegexMarker;

            private static readonly Regex InvalidRegexMarker = new Regex(".*");

            /// <summary>
            /// Does the given text match the filter
            /// </summary>
            /// <remarks>
            /// <para>this.Text will not be null or empty when this is called.</para>
            /// </remarks>
            /// <param name="cellText">The text of the cell we want to search</param>
            /// <returns>Return true if the given cellText matches our strategy</returns>
            public override bool MatchesText(string cellText)
            {
                if (IsRegexInvalid)
                {
                    return true;
                }

                return Regex.Match(cellText).Success;
            }

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

                if (!IsRegexInvalid)
                {
                    foreach (Match match in Regex.Matches(cellText))
                    {
                        if (match.Length > 0)
                        {
                            ranges.Add(new CharacterRange(match.Index, match.Length));
                        }
                    }
                }

                return ranges;
            }
        }
    }
}