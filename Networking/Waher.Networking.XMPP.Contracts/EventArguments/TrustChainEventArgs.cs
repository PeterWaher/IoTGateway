using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
    /// <summary>
    /// Event arguments for Service Provider callback methods.
    /// </summary>
    public class TrustChainEventArgs : IqResultEventArgs
    {
        private readonly string[] domains;

        internal TrustChainEventArgs(IqResultEventArgs e, string[] Domains)
            : base(e)
        {
            this.domains = Domains;
        }

        /// <summary>
        /// Domains
        /// </summary>
        public string[] Domains => this.domains;
    }
}
