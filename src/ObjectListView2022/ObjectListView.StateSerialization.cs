using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace BrightIdeasSoftware
{
    public partial class ObjectListView
    {
        /// <summary>
        /// XmlSerializer-backed compatibility adapter for the legacy SaveState()/RestoreState()
        /// implementation. The nested type shadows BinaryFormatter only inside ObjectListView,
        /// preserving the public state-persistence API while avoiding object-graph deserialization.
        /// </summary>
        private sealed class BinaryFormatter
        {
            private const long MaximumStateSize = 1024 * 1024;
            private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(SerializedObjectListViewState));

            // Retained because SaveState() initializes this property on the legacy formatter.
            public FormatterAssemblyStyle AssemblyFormat { get; set; }

            public void Serialize(Stream stream, object graph)
            {
                if (graph is not ObjectListViewState state)
                {
                    throw new SerializationException("Only ObjectListViewState can be serialized.");
                }

                Serializer.Serialize(stream, SerializedObjectListViewState.FromState(state));
            }

            public object Deserialize(Stream stream)
            {
                try
                {
                    using var reader = XmlReader.Create(stream, new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Prohibit,
                        XmlResolver = null,
                        MaxCharactersInDocument = MaximumStateSize
                    });

                    var state = Serializer.Deserialize(reader) as SerializedObjectListViewState;
                    return state?.ToState();
                }
                catch (InvalidOperationException ex)
                {
                    throw new SerializationException("Invalid ObjectListView state.", ex);
                }
                catch (XmlException ex)
                {
                    throw new SerializationException("Invalid ObjectListView state.", ex);
                }
            }
        }

        /// <summary>
        /// XML serialization contract used by SaveState()/RestoreState().
        /// </summary>
        [Serializable]
        [XmlRoot("ObjectListViewState")]
        public sealed class SerializedObjectListViewState
        {
            /// <summary>
            ///     State version number. This is used to detect incompatible changes to the state format.
            /// </summary>
            public int VersionNumber = 1;

            /// <summary>
            ///     Number of columns in the list view. This is used to validate the lengths of the column-related arrays.
            /// </summary>
            public int NumberOfColumns = 1;

            /// <summary>
            ///     View mode of the list view. This is used to restore the view mode when restoring state.
            /// </summary>
            public View CurrentView;

            /// <summary>
            ///     Column index of the last sorted column. This is used to restore the sort order when restoring state.
            /// </summary>
            public int SortColumn = -1;

            /// <summary>
            ///     True if the list view is currently showing groups, false otherwise. This is used to restore the group visibility when restoring state.
            /// </summary>
            public bool IsShowingGroups;

            /// <summary>
            ///     Sort order of the last sorted column. This is used to restore the sort order when restoring state.
            /// </summary>
            public SortOrder LastSortOrder = SortOrder.None;

            /// <summary>
            ///     True if the corresponding column is visible, false otherwise. This is used to restore the column visibility when restoring state.
            /// </summary>
            public bool[] ColumnIsVisible = Array.Empty<bool>();

            /// <summary>
            ///     Index of the corresponding column in the display order. This is used to restore the column display order when restoring state.
            /// </summary>
            public int[] ColumnDisplayIndicies = Array.Empty<int>();

            /// <summary>
            ///     Width of the corresponding column in pixels. This is used to restore the column widths when restoring state.
            /// </summary>
            public int[] ColumnWidths = Array.Empty<int>();

            internal static SerializedObjectListViewState FromState(ObjectListViewState state) => new SerializedObjectListViewState
            {
                VersionNumber = state.VersionNumber,
                NumberOfColumns = state.NumberOfColumns,
                CurrentView = state.CurrentView,
                SortColumn = state.SortColumn,
                IsShowingGroups = state.IsShowingGroups,
                LastSortOrder = state.LastSortOrder,
                ColumnIsVisible = ToBooleanArray(state.ColumnIsVisible),
                ColumnDisplayIndicies = ToIntegerArray(state.ColumnDisplayIndicies),
                ColumnWidths = ToIntegerArray(state.ColumnWidths)
            };

            internal ObjectListViewState ToState()
            {
                Validate();
                return new ObjectListViewState
                {
                    VersionNumber = VersionNumber,
                    NumberOfColumns = NumberOfColumns,
                    CurrentView = CurrentView,
                    SortColumn = SortColumn,
                    IsShowingGroups = IsShowingGroups,
                    LastSortOrder = LastSortOrder,
                    ColumnIsVisible = new ArrayList(ColumnIsVisible),
                    ColumnDisplayIndicies = new ArrayList(ColumnDisplayIndicies),
                    ColumnWidths = new ArrayList(ColumnWidths)
                };
            }

            private void Validate()
            {
                if (VersionNumber != 1 ||
                    NumberOfColumns < 0 ||
                    ColumnIsVisible == null ||
                    ColumnDisplayIndicies == null ||
                    ColumnWidths == null ||
                    ColumnIsVisible.Length != NumberOfColumns ||
                    ColumnDisplayIndicies.Length != NumberOfColumns ||
                    ColumnWidths.Length != NumberOfColumns ||
                    SortColumn < -1 || SortColumn >= NumberOfColumns)
                {
                    throw new SerializationException("ObjectListView state is structurally inconsistent.");
                }
            }

            private static bool[] ToBooleanArray(ArrayList source)
            {
                var values = new bool[source.Count];
                for (var i = 0; i < source.Count; i++)
                {
                    values[i] = (bool)source[i];
                }

                return values;
            }

            private static int[] ToIntegerArray(ArrayList source)
            {
                var values = new int[source.Count];
                for (var i = 0; i < source.Count; i++)
                {
                    values[i] = (int)source[i];
                }

                return values;
            }
        }
    }
}
