using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Software
{
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
            get => this.package;
            set => this.package = value;
        }
    }
}
