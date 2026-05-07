/*
 * Comparers - Various Comparer classes used within ObjectListView
 *
 * Author: Phillip Piper
 * Date: 25/11/2008 17:15
 *
 * Change log:
 * v2.8.1
 * 2014-12-03  JPP  - Added StringComparer
 * v2.3
 * 2009-08-24  JPP  - Added OLVGroupComparer
 * 2009-06-01  JPP  - ModelObjectComparer would crash if secondary sort column was null.
 * 2008-12-20  JPP  - Fixed bug with group comparisons when a group key was null (SF#2445761)
 * 2008-11-25  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2006-2014 Phillip Piper
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
using System.Windows.Forms;

namespace BrightIdeasSoftware.Implementation
{
    /// <summary>
    /// This comparer sort list view groups. OLVGroups have a "SortValue" property,
    /// which is used if present. Otherwise, the titles of the groups will be compared.
    /// </summary>
    public class OLVGroupComparer : IComparer<OLVGroup>
    {
        /// <summary>
        /// Create a group comparer
        /// </summary>
        /// <param name="order">The ordering for column values</param>
        public OLVGroupComparer(SortOrder order)
        {
            sortOrder = order;
        }

        /// <summary>
        /// Compare the two groups. OLVGroups have a "SortValue" property,
        /// which is used if present. Otherwise, the titles of the groups will be compared.
        /// </summary>
        /// <param name="x">group1</param>
        /// <param name="y">group2</param>
        /// <returns>An ordering indication: -1, 0, 1</returns>
        public int Compare(OLVGroup x, OLVGroup y)
        {
            // If we can compare the sort values, do that.
            // Otherwise do a case insensitive compare on the group header.
            int result;
            if (x.SortValue != null && y.SortValue != null)
            {
                result = x.SortValue.CompareTo(y.SortValue);
            }
            else
            {
                result = string.Compare(x.Header, y.Header, StringComparison.CurrentCultureIgnoreCase);
            }

            if (sortOrder == SortOrder.Descending)
            {
                result = 0 - result;
            }

            return result;
        }

        private readonly SortOrder sortOrder;
    }
}