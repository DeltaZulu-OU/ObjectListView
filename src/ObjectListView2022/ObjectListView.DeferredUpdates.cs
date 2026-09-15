using System;

namespace BrightIdeasSoftware
{
    public partial class ObjectListView
    {
        /// <summary>
        /// Freeze view updates until the returned scope is disposed.
        /// </summary>
        /// <remarks>
        /// Scopes may be nested. The view is rebuilt when the outermost scope is disposed.
        /// </remarks>
        /// <returns>A scope that unfreezes the view when disposed.</returns>
        public IDisposable DeferViewUpdates()
        {
            Freeze();
            return new DeferredViewUpdateScope(this);
        }

        private sealed class DeferredViewUpdateScope : IDisposable
        {
            private ObjectListView owner;

            public DeferredViewUpdateScope(ObjectListView owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                var listView = owner;
                if (listView == null)
                {
                    return;
                }

                owner = null;
                if (!listView.IsDisposed && !listView.Disposing)
                {
                    listView.Unfreeze();
                }
            }
        }
    }
}
