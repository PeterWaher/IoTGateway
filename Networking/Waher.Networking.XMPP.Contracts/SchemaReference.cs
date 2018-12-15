using System;
using System.Collections.Generic;
using System.Text;
using Waher.Security;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// References a XML Schema used for validating machine-readable contents in smart contracts.
	/// </summary>
	public class SchemaReference
	{
		private readonly string ns = string.Empty;
		private readonly SchemaDigest[] digests = null;

		/// <summary>
		/// References a XML Schema used for validating machine-readable contents in smart contracts.
		/// </summary>
		/// <param name="Namespace">Schema namespace</param>
		/// <param name="Digests">Digests of available versions.</param>
		public SchemaReference(string Namespace, SchemaDigest[] Digests)
		{
			this.ns = Namespace;
			this.digests = Digests;
		}

		/// <summary>
		/// Namespace of schema
		/// </summary>
		public string Namespace => this.ns;

		/// <summary>
		/// Hash of schema
		/// </summary>
		public SchemaDigest[] Digests => this.digests;
	}
}
