using System;
using System.Xml;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Delegate for package update event handlers.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void PackageUpdatedEventHandler(object Sender, PackageUpdatedEventArgs e);

    /// <summary>
    /// Event arguments for software package update events.
    /// </summary>
    public class PackageUpdatedEventArgs : PackageNotificationEventArgs
    {
        private bool download = true;

        /// <summary>
        /// Event arguments for software package update events.
        /// </summary>
        /// <param name="Package">Package information object.</param>
        /// <param name="e">Message event arguments.</param>
        public PackageUpdatedEventArgs(Package Package, MessageEventArgs e)
            : base(Package, e)
        {
        }

        /// <summary>
        /// If the package is to be downloaded. Default=true.
        /// </summary>
        public bool Download
        {
            get { return this.download; }
            set { this.download = value; }
        }

    }
}
