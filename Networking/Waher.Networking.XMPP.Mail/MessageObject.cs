using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Mail
{
	/// <summary>
	/// Contains the binary representation of a message object.
	/// </summary>
	public class MessageObject
	{
		private byte[] data;
		private string contentType;

		/// <summary>
		/// Contains the binary representation of a message object.
		/// </summary>
		/// <param name="Data">Binary representation of object.</param>
		/// <param name="ContentType">Internet Content Type</param>
		public MessageObject(byte[] Data, string ContentType)
		{
			this.data = Data;
			this.contentType = ContentType;
		}

		/// <summary>
		/// Binary representation of message object.
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Internet Content Type.
		/// </summary>
		public string ContentType => this.contentType;

	}
}
