using System;
using System.ComponentModel;

namespace BrightIdeasSoftware
{
    public partial class ObjectListView
    {
        /// <summary>
        /// Gets or sets the transparency used for overlays that do not implement
        /// <see cref="Rendering.ITransparentOverlay"/>.
        /// </summary>
        /// <remarks>
        /// This property is retained for compatibility with the original ObjectListView behavior.
        /// New overlay implementations should implement <see cref="Rendering.ITransparentOverlay"/>
        /// and set transparency on the overlay itself.
        /// </remarks>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OverlayTransparency {
            get => overlayTransparency;
            set => overlayTransparency = Math.Min(255, Math.Max(0, value));
        }

        private int overlayTransparency = 128;
    }
}
