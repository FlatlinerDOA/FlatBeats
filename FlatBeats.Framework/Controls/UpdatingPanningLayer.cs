// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace FlatBeats.Controls
{
    using Microsoft.Phone.Controls.Primitives;

    /// <summary>
    /// Control that is used to access the protected method to update the edges
    /// of the underlying panorama layer.
    /// </summary>
    public class UpdatingPanningLayer : PanningBackgroundLayer
    {
        /// <summary>
        /// Refreshes the edges / wrapping rectangles.
        /// </summary>
        public void RefreshEdges()
        {
            this.UpdateWrappingRectangles();
        }
    }
}
