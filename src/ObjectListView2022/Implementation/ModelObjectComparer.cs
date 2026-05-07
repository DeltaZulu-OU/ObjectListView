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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BrightIdeasSoftware.Implementation
{
    /// <summary>
    /// This comparer can be used to sort a collection of model objects by a given column
    /// </summary>
    /// <remarks>
    /// <para>This is used by virtual ObjectListViews. Non-virtual lists use
    /// ColumnComparer</para>
    /// </remarks>
    public class ModelObjectComparer : IComparer, IComparer<object>
    {
        /// <summary>
        /// Gets or sets the method that will be used to compare two strings.
        /// The default is to compare on the current culture, case-insensitive
        /// </summary>
        public static StringCompareDelegate StringComparer { get; set; }

        /// <summary>
        /// Create a model object comparer
        /// </summary>
        /// <param name="col"></param>
        /// <param name="order"></param>
        public ModelObjectComparer(OLVColumn col, SortOrder order)
        {
            column = col;
            sortOrder = order;
        }

        /// <summary>
        /// Create a model object comparer with a secondary sorting column
        /// </summary>
        /// <param name="col"></param>
        /// <param name="order"></param>
        /// <param name="col2"></param>
        /// <param name="order2"></param>
        public ModelObjectComparer(OLVColumn col, SortOrder order, OLVColumn col2, SortOrder order2)
            : this(col, order)
        {
            // There is no point in secondary sorting on the same column
            if (col != col2 && col2 != null && order2 != SortOrder.None)
            {
                secondComparer = new ModelObjectComparer(col2, order2);
            }
        }

        /// <summary>
        /// Compare the two model objects
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            var result = 0;
            var x1 = column.GetValue(x);
            var y1 = column.GetValue(y);

            if (sortOrder == SortOrder.None)
            {
                return 0;
            }

            // Handle nulls. Null values come last
            var xIsNull = x1 == null || x1 == DBNull.Value;
            var yIsNull = y1 == null || y1 == DBNull.Value;
            if (xIsNull || yIsNull)
            {
                if (xIsNull && yIsNull)
                {
                    result = 0;
                }
                else
                {
                    result = xIsNull ? -1 : 1;
                }
            }
            else
            {
                result = CompareValues(x1, y1);
            }

            if (sortOrder == SortOrder.Descending)
            {
                result = 0 - result;
            }

            // If the result was equality, use the secondary comparer to resolve it
            if (result == 0 && secondComparer != null)
            {
                result = secondComparer.Compare(x, y);
            }

            return result;
        }

        /// <summary>
        /// Compare the actual values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareValues(object x, object y)
        {
            // Force case insensitive compares on strings
            if (x is string xStr)
            {
                return CompareStrings(xStr, y as string);
            }

            return x is IComparable comparable ? comparable.CompareTo(y) : 0;
        }

        private static int CompareStrings(string x, string y)
        {
            if (StringComparer == null)
            {
                return string.Compare(x, y, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return StringComparer(x, y);
            }
        }

        private readonly OLVColumn column;
        private readonly SortOrder sortOrder;
        private readonly ModelObjectComparer secondComparer;
    }
}