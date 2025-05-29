using System;
using Waher.Security;

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
		/// <param name="SchemaDigestHashFunction">Hash Function used to create the schema digest.</param>
		public SchemaReferenceEventArgs(string Namespace, byte[] SchemaDigest,
			HashFunction SchemaDigestHashFunction)
			: base()
		{
			this.Namespace = Namespace;
			this.SchemaDigest = SchemaDigest;
			this.SchemaDigestHashFunction = SchemaDigestHashFunction;
		}

		/// <summary>
		/// Namespace of schema
		/// </summary>
		public string Namespace { get; }

		/// <summary>
		/// Hash Digest of schema file.
		/// </summary>
		public byte[] SchemaDigest { get; }

		/// <summary>
		/// Hash Function used to create the schema digest.
		/// </summary>
		public HashFunction SchemaDigestHashFunction { get; }

		/// <summary>
		/// XML Schema definition, if available.
		/// </summary>
		public byte[] XmlSchema { get; set; }
	}
}
