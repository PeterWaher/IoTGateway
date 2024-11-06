using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
    /// <summary>
    /// Event arguments for events where a client URL needs to be displayed
    /// when performing a petition.
    /// </summary>
    public class PetitionClientUrlEventArgs : MessageEventArgs
    {
        private readonly string clientUrl;
        private readonly string petitionId;

        /// <summary>
        /// Event arguments for events where a client URL needs to be displayed
        /// when reviewing an identity application.
        /// </summary>
        /// <param name="e">IQ result event arguments.</param>
        /// <param name="PetitionId">Petition ID.</param>
        /// <param name="ClientUrl">Client URL.</param>
        public PetitionClientUrlEventArgs(MessageEventArgs e, string PetitionId, string ClientUrl)
            : base(e)
        {
            this.petitionId = PetitionId;
            this.clientUrl = ClientUrl;
        }

        /// <summary>
        /// ID of peer review petition.
        /// </summary>
        public string PetitionId => this.petitionId;

        /// <summary>
        /// URL client needs to open to complete the peer review.
        /// </summary>
        public string ClientUrl => this.clientUrl;
    }
}
