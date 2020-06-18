using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Delegate for package deletion event handlers.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate Task PackageDeletedEventHandler(object Sender, PackageDeletedEventArgs e);

    /// <summary>
    /// Event arguments for software package deletion events.
    /// </summary>
    public class PackageDeletedEventArgs : PackageNotificationEventArgs
    {
        private bool delete = true;

        /// <summary>
        /// Event arguments for software package deletion events.
        /// </summary>
        /// <param name="Package">Package information object.</param>
        /// <param name="e">Message event arguments.</param>
        public PackageDeletedEventArgs(Package Package, MessageEventArgs e)
            : base(Package, e)
        {
        }

        /// <summary>
        /// If the downloaded package is to be deleted. Default=true.
        /// </summary>
        public bool Delete
        {
            get { return this.delete; }
            set { this.delete = value; }
        }

    }
}
