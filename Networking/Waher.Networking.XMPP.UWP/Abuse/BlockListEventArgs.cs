using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Abuse
{
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
        public string[] JIDs => this.jids;
    }
}
