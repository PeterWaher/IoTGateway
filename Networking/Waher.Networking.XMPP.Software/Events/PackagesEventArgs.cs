using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Event arguments for Software packages events.
    /// </summary>
    public class PackagesEventArgs : IqResultEventArgs
    {
        private Package[] packages;

        /// <summary>
        /// Event arguments for Software packages events.
        /// </summary>
        /// <param name="Packages">Array of package information objects.</param>
        /// <param name="e">Response event arguments.</param>
        public PackagesEventArgs(Package[] Packages, IqResultEventArgs e)
            : base(e)
        {
            this.packages = Packages;
        }

        /// <summary>
        /// Array of package information objects.
        /// </summary>
        public Package[] Packages
        {
            get => this.packages;
            set => this.packages = value;
        }
    }
}
