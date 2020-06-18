using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Delegate for subscription list responses callback methods.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate Task SubscriptionsEventHandler(object Sender, SubscriptionsEventArgs e);

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
        public string[] FileNames
        {
            get { return this.fileNames; }
        }
    }
}
