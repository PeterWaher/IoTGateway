using System;
using System.Xml;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Delegate for package notification event handlers.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void PackageNotificationEventHandler(object Sender, PackageNotificationEventArgs e);

    /// <summary>
    /// Event arguments for software package notification events.
    /// </summary>
    public class PackageNotificationEventArgs : MessageEventArgs
    {
        private Package package;

        /// <summary>
        /// Event arguments for software package noticication events.
        /// </summary>
        /// <param name="Package">Package information object.</param>
        /// <param name="e">Message event arguments.</param>
        public PackageNotificationEventArgs(Package Package, MessageEventArgs e)
            : base(e)
        {
            this.package = Package;
        }

        /// <summary>
        /// Package information object.
        /// </summary>
        public Package Package
        {
            get { return this.package; }
            set { this.package = value; }
        }

    }
}
