using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Abstract base class of signatures
	/// </summary>
	public abstract class Signature
	{
		private DateTime timestamp = DateTime.MinValue;
		private byte[] digitalSignature = null;

		/// <summary>
		/// Timestamp of signature.
		/// </summary>
		public DateTime Timestamp
		{
			get => this.timestamp;
			set => this.timestamp = value;
		}

		/// <summary>
		/// Digital Signature
		/// </summary>
		public byte[] DigitalSignature
        {
			get => this.digitalSignature;
			set => this.digitalSignature = value;
		}

		/// <summary>
		/// Serializes the signature, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public abstract void Serialize(StringBuilder Xml);

	}
}
