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
            public int             /// <inheritdoc/>
VersionNumber = 1;
            public int             /// <inheritdoc/>
NumberOfColumns = 1;
            public View             /// <inheritdoc/>
CurrentView;
            public int             /// <inheritdoc/>
SortColumn = -1;
            public bool             /// <inheritdoc/>
IsShowingGroups;
            public SortOrder             /// <inheritdoc/>
LastSortOrder = SortOrder.None;
            public bool[]             /// <inheritdoc/>
ColumnIsVisible = Array.Empty<bool>();
            public int[]             /// <inheritdoc/>
ColumnDisplayIndicies = Array.Empty<int>();
            public int[]             /// <inheritdoc/>
ColumnWidths = Array.Empty<int>();

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
