using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Abuse
{
    /// <summary>
    /// Delegate for block-list callback methods.
    /// </summary>
    /// <param name="Sender">Sender</param>
    /// <param name="e">Response.</param>
    public delegate void BlockListEventHandler(object Sender, BlockListEventArgs e);

    /// <summary>
    /// Event arguments for block-list response callbacks.
    /// </summary>
    public class BlockListEventArgs : IqResultEventArgs
    {
        private readonly string[] jids;

        /// <summary>
        /// Event arguments for block-list response callbacks.
        /// </summary>
        /// <param name="e">IQ response.</param>
        /// <param name="JIDs">Blocked JIDs.</param>
        /// <param name="State">State object to pass on to callback method.</param>
        public BlockListEventArgs(IqResultEventArgs e, string[] JIDs, object State)
            : base(e)
        {
            this.jids = JIDs;
            this.State = State;
        }

        /// <summary>
        /// JIDs in block-list.
        /// </summary>
        public string[] JIDs
        {
            get { return this.jids; }
        }
    }
}
