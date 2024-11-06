using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Mail
{
	/// <summary>
	/// Message object event arguments.
	/// </summary>
	public class MessageObjectEventArgs : IqResultEventArgs
	{
		private readonly string contentType;
		private readonly byte[] data;

		/// <summary>
		/// Message object event arguments.
		/// </summary>
		/// <param name="e">Result event arguments.</param>
		/// <param name="ContentType">Content-Type of message object.</param>
		/// <param name="Data">Binary representation of message object.</param>
		public MessageObjectEventArgs(IqResultEventArgs e, string ContentType, byte[] Data)
			: base(e)
		{
			this.contentType = ContentType;
			this.data = Data;
		}

		/// <summary>
		/// Content-Type of message object.
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Binary representation of message object.
		/// </summary>
		public byte[] Data => this.data;

	}
}
