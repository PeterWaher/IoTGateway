using System;
using System.Collections.Generic;
using System.Text;
using Waher.Security;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains a schema digest
	/// </summary>
	public class SchemaDigest
	{
		private readonly byte[] digest;
		private readonly HashFunction function;

		/// <summary>
		/// Contains a schema digest
		/// </summary>
		/// <param name="Function">Hash function</param>
		/// <param name="Digest">Digest</param>
		public SchemaDigest(HashFunction Function, byte[] Digest)
		{
			this.function = Function;
			this.digest = Digest;
		}

		/// <summary>
		/// Hash Digest of schema file
		/// </summary>
		public byte[] Digest => this.digest;

		/// <summary>
		/// Hash Function used to calculate the digest.
		/// </summary>
		public HashFunction Function => this.function;
	}
}
