using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for Schema Reference events.
	/// </summary>
	public class SchemaReferenceEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for Schema Reference events.
		/// </summary>
		/// <param name="Namespace">Namespace of schema</param>
		/// <param name="SchemaDigest">Hash Digest of schema file.</param>
		public SchemaReferenceEventArgs(string Namespace, SchemaDigest SchemaDigest)
			: base()
		{
			this.Namespace = Namespace;
			this.SchemaDigest = SchemaDigest;
		}

		/// <summary>
		/// Namespace of schema
		/// </summary>
		public string Namespace { get; }

		/// <summary>
		/// Hash Digest of schema file.
		/// </summary>
		public SchemaDigest SchemaDigest { get; }

		/// <summary>
		/// XML Schema definition, if available.
		/// </summary>
		public byte[] XmlSchema { get; set; }
	}
}
