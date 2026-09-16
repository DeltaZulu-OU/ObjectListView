using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace BrightIdeasSoftware.Design
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Compatibility type for design-time metadata and consumers that still refer to the historic
    /// BrightIdeasSoftware.Design namespace.
    /// </summary>
    public class ObjectListViewDesigner : BrightIdeasSoftware.ObjectListViewDesigner
    {
    }

    /// <summary>
    /// Compatibility type for design-time metadata that still refers to the historic
    /// BrightIdeasSoftware.Design namespace.
    /// </summary>
    public class OLVColumnCollectionEditor : BrightIdeasSoftware.OLVColumnCollectionEditor
    {
        /// <summary>
        /// Creates a column collection editor for the given collection type.
        /// </summary>
        public OLVColumnCollectionEditor(Type type)
            : base(type)
        {
        }
    }

    /// <summary>
    /// Compatibility type for overlay metadata that still refers to the historic
    /// BrightIdeasSoftware.Design namespace.
    /// </summary>
    internal class OverlayConverter : BrightIdeasSoftware.OverlayConverter
    {
    }
}
