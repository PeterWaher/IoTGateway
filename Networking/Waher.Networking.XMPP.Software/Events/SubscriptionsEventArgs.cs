using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Event arguments for subscription list responses responsess.
    /// </summary>
    public class SubscriptionsEventArgs : IqResultEventArgs
    {
        private readonly string[] fileNames;

        internal SubscriptionsEventArgs(string[] FileNames, IqResultEventArgs e)
            : base(e)
        {
            this.fileNames = FileNames;
        }

        /// <summary>
        /// Array of file names.
        /// </summary>
        public string[] FileNames => this.fileNames;
    }
}
