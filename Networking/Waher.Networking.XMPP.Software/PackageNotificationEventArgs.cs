using System;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Event arguments for software package notification events.
    /// </summary>
    public abstract class PackageNotificationEventArgs : MessageEventArgs
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
