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
using System.Drawing;
using System.Windows.Forms;

namespace BrightIdeasSoftware.CellEditing
{
    /// <summary>
    /// This editor simply shows and edits boolean values using a checkbox
    /// </summary>
    [ToolboxItem(false)]
    public class BooleanCellEditor2 : CheckBox
    {
        /// <summary>
        /// Gets or sets the value shown by this editor
        /// </summary>
        public bool? Value {
            get => CheckState switch
            {
                CheckState.Checked => true,
                CheckState.Indeterminate => null,
                _ => false,
            };
            set {
                if (value.HasValue)
                {
                    CheckState = value.Value ? CheckState.Checked : CheckState.Unchecked;
                }
                else
                {
                    CheckState = CheckState.Indeterminate;
                }
            }
        }

        /// <summary>
        /// Gets or sets how the checkbox will be aligned
        /// </summary>
        public new HorizontalAlignment TextAlign {
            get => CheckAlign switch
            {
                ContentAlignment.MiddleRight => HorizontalAlignment.Right,
                ContentAlignment.MiddleCenter => HorizontalAlignment.Center,
                _ => HorizontalAlignment.Left,
            };
            set {
                switch (value)
                {
                    case HorizontalAlignment.Left:
                        CheckAlign = ContentAlignment.MiddleLeft;
                        break;

                    case HorizontalAlignment.Center:
                        CheckAlign = ContentAlignment.MiddleCenter;
                        break;

                    case HorizontalAlignment.Right:
                        CheckAlign = ContentAlignment.MiddleRight;
                        break;
                }
            }
        }
    }
}