using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Delegate for package event handlers.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate Task PackageEventHandler(object Sender, PackageEventArgs e);

    /// <summary>
    /// Event arguments for software package events.
    /// </summary>
    public class PackageEventArgs : IqResultEventArgs
    {
        private Package package;

        /// <summary>
        /// Event arguments for Software package events.
        /// </summary>
        /// <param name="Package">Package information object.</param>
        /// <param name="e">Response event arguments.</param>
        public PackageEventArgs(Package Package, IqResultEventArgs e)
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
