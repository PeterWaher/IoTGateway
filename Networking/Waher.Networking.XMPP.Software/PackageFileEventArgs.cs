using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Software
{
    /// <summary>
    /// Delegate for package file event handlers.
    /// </summary>
    /// <param name="Sender">Sender of event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate Task PackageFileEventHandler(object Sender, PackageFileEventArgs e);

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
        public string LocalFileName
        {
            get { return this.localFileName; }
        }

    }
}
