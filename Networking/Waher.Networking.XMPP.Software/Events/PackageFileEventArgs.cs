using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Event arguments for software package file events.
    /// </summary>
    public class PackageFileEventArgs : PackageNotificationEventArgs
    {
        private readonly string localFileName;

        /// <summary>
        /// Event arguments for software package file events.
        /// </summary>
        /// <param name="Package">Package information object.</param>
        /// <param name="LocalFileName">File name of local package file.</param>
        /// <param name="e">Message event arguments.</param>
        public PackageFileEventArgs(Package Package, string LocalFileName, MessageEventArgs e)
            : base(Package, e)
        {
            this.localFileName = LocalFileName;
        }

        /// <summary>
        /// File name of local package file.
        /// </summary>
        public string LocalFileName => this.localFileName;
    }
}
