/*
 * CellEditors - Several slightly modified controls that are used as celleditors within ObjectListView.
 *
 * Author: Phillip Piper
 * Date: 20/10/2008 5:15 PM
 *
 * Change log:
 * v2.6
 * 2012-08-02   JPP  - Make most editors public so they can be reused/subclassed
 * v2.3
 * 2009-08-13   JPP  - Standardized code formatting
 * v2.2.1
 * 2008-01-18   JPP  - Added special handling for enums
 * 2008-01-16   JPP  - Added EditorRegistry
 * v2.0.1
 * 2008-10-20   JPP  - Separated from ObjectListView.cs
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

using System.ComponentModel;
using System.Windows.Forms;

namespace BrightIdeasSoftware.CellEditing
{
    /// <summary>
    /// This editor simply shows and edits unsigned integer values.
    /// </summary>
    /// <remarks>This class can't be made public because unsigned int is not a
    /// CLS-compliant type. If you want to use, just copy the code to this class
    /// into your project and use it from there.</remarks>
    [ToolboxItem(false)]
    internal class UintUpDown : NumericUpDown
    {
        public UintUpDown()
        {
            DecimalPlaces = 0;
            Minimum = 0;
            Maximum = 9999999;
        }

        public new uint Value {
            get => decimal.ToUInt32(base.Value); set => base.Value = new decimal(value);
        }
    }
}