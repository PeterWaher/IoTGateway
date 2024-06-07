using System;
using Waher.Layout.Layout2D.Model;

namespace Waher.Layout.Layout2D.Events
{
    /// <summary>
    /// Delegate for Drawing event handlers.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments</param>
    public delegate void DrawingEventHandler(object Sender, DrawingEventArgs e);

    /// <summary>
    /// Event raised when the layout model has been Drawing internally.
    /// </summary>
    public class DrawingEventArgs : EventArgs
    {
        private readonly Layout2DDocument doc;
        private readonly DrawingState state;

        /// <summary>
        /// Event raised when the layout model has been Drawing internally.
        /// </summary>
        /// <param name="Document">Document that has been Drawing.</param>
        /// <param name="State">Drawing state.</param>
        public DrawingEventArgs(Layout2DDocument Document, DrawingState State)
            : base()
        {
            this.doc = Document;
            this.state = State;
        }

        /// <summary>
        /// Document that has been Drawing.
        /// </summary>
        public Layout2DDocument Document => this.doc;

        /// <summary>
        /// Drawing state.
        /// </summary>
        public DrawingState State => this.state;
    }
}
