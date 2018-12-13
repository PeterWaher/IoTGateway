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
		private byte[] s1 = null;
		private byte[] s2 = null;

		/// <summary>
		/// Timestamp of signature.
		/// </summary>
		public DateTime Timestamp
		{
			get => this.timestamp;
			set => this.timestamp = value;
		}

		/// <summary>
		/// Signature 1
		/// </summary>
		public byte[] S1
		{
			get => this.s1;
			set => this.s1 = value;
		}

		/// <summary>
		/// Signature 2 (might be required, based on cryptographic algorithm, or null if not required).
		/// </summary>
		public byte[] S2
		{
			get => this.s2;
			set => this.s2 = value;
		}

		/// <summary>
		/// Serializes the signature, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public abstract void Serialize(StringBuilder Xml);

	}
}
