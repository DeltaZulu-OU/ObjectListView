/*
 * Attributes - Attributes that can be attached to properties of models to allow columns to be
 *              built from them directly
 *
 * Author: Phillip Piper
 * Date: 15/08/2009 22:01
 *
 * Change log:
 * v2.6
 * 2012-08-16  JPP  - Added [OLVChildren] and [OLVIgnore]
 *                  - OLV attributes can now only be set on properties
 * v2.4
 * 2010-04-14  JPP  - Allow Name property to be set
 *
 * v2.3
 * 2009-08-15  JPP  - Initial version
 *
 * To do:
 *
 * Copyright (C) 2009-2014 Phillip Piper
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
using System.Windows.Forms;

namespace BrightIdeasSoftware.Implementation
{
    /// <summary>
    /// This attribute is used to mark a property of a model
    /// class that should be noticed by Generator class.
    /// </summary>
    /// <remarks>
    /// All the attributes of this class match their equivilent properties on OLVColumn.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class OLVColumnAttribute : Attribute
    {
        #region Constructor

        // There are several property where we actually want nullable value (bool?, int?),
        // but it seems attribute properties can't be nullable types.
        // So we explicitly track if those properties have been set.

        /// <summary>
        /// Create a new OLVColumnAttribute
        /// </summary>
        public OLVColumnAttribute()
        {
        }

        /// <summary>
        /// Create a new OLVColumnAttribute with the given title
        /// </summary>
        /// <param name="title">The title of the column</param>
        public OLVColumnAttribute(string title)
        {
            Title = title;
        }

        #endregion Constructor

        #region Public properties

        /// <summary>
        ///
        /// </summary>
        public string AspectToStringFormat { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool CheckBoxes {
            get => checkBoxes;
            set {
                checkBoxes = value;
                IsCheckBoxesSet = true;
            }
        }

        private bool checkBoxes;
        internal bool IsCheckBoxesSet = false;

        /// <summary>
        ///
        /// </summary>
        public int DisplayIndex { get; set; } = -1;

        /// <summary>
        ///
        /// </summary>
        public bool FillsFreeSpace { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int FreeSpaceProportion {
            get => freeSpaceProportion;
            set {
                freeSpaceProportion = value;
                IsFreeSpaceProportionSet = true;
            }
        }

        private int freeSpaceProportion;
        internal bool IsFreeSpaceProportionSet = false;

        /// <summary>
        /// An array of IComparables that mark the cutoff points for values when
        /// grouping on this column.
        /// </summary>
        public object[] GroupCutoffs { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string[] GroupDescriptions { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string GroupWithItemCountFormat { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string GroupWithItemCountSingularFormat { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool Hyperlink { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ImageAspectName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsEditable {
            get => isEditable;
            set {
                isEditable = value;
                IsEditableSet = true;
            }
        }

        private bool isEditable = true;
        internal bool IsEditableSet = false;

        /// <summary>
        ///
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        ///
        /// </summary>
        public bool IsTileViewColumn { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int MaximumWidth { get; set; } = -1;

        /// <summary>
        ///
        /// </summary>
        public int MinimumWidth { get; set; } = -1;

        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public HorizontalAlignment TextAlign {
            get => textAlign;
            set {
                textAlign = value;
                IsTextAlignSet = true;
            }
        }

        private HorizontalAlignment textAlign = HorizontalAlignment.Left;
        internal bool IsTextAlignSet = false;

        /// <summary>
        ///
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ToolTipText { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool TriStateCheckBoxes {
            get => triStateCheckBoxes;
            set {
                triStateCheckBoxes = value;
                IsTriStateCheckBoxesSet = true;
            }
        }

        private bool triStateCheckBoxes;
        internal bool IsTriStateCheckBoxesSet = false;

        /// <summary>
        ///
        /// </summary>
        public bool UseInitialLetterForGroup { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int Width { get; set; } = 150;

        #endregion Public properties
    }

    /// <summary>
    /// Properties marked with [OLVChildren] will be used as the children source in a TreeListView.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OLVChildrenAttribute : Attribute
    {
    }

    /// <summary>
    /// Properties marked with [OLVIgnore] will not have columns generated for them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OLVIgnoreAttribute : Attribute
    {
    }
}