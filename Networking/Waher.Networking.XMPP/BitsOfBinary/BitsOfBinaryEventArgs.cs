using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.BitsOfBinary
{
	/// <summary>
	/// Delegate for bits-of-binary data request callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task BitsOfBinaryEventHandler(object Sender, BitsOfBinaryEventArgs e);

	/// <summary>
	/// Event argument for a bits-of-byte data request.
	/// </summary>
	public class BitsOfBinaryEventArgs : IqResultEventArgs
	{
		private readonly string contentId;
		private readonly string contentType;
		private readonly byte[] data;
		private readonly DateTime? expires;

		/// <summary>
		/// Event argument for a bits-of-byte data request.
		/// </summary>
		/// <param name="e">IQ response.</param>
		public BitsOfBinaryEventArgs(IqResultEventArgs e)
			: base(e)
		{
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null)
			{
				this.contentId = XML.Attribute(E, "cid");
				this.contentType = XML.Attribute(E, "type");

				int MaxAge = XML.Attribute(E, "max-age", -1);
				if (MaxAge > 0)
					this.expires = DateTime.Now.AddSeconds(MaxAge);

				this.data = Convert.FromBase64String(E.InnerText);
			}
			else
			{
				this.contentId = null;
				this.contentType = null;
				this.data = null;
				this.expires = null;
			}
		}

		/// <summary>
		/// Content ID
		/// </summary>
		public string ContentId => this.contentId;

		/// <summary>
		/// Content Type
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Binary data
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Optional timestamp when data expires.
		/// </summary>
		public DateTime? Expires => this.expires;
	}
}
