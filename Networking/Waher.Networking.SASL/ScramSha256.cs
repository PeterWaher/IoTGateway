using System;
using System.Collections.Generic;
using Waher.Security;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by SCRAM-SHA-256 defined in RFC 7677:
	/// https://tools.ietf.org/html/rfc7677
	/// </summary>
	public class ScramSha256 : ScramAuthenticationMechanism
	{
		/// <summary>
		/// Authentication done by SCRAM-SHA-256 defined in RFC 7677:
		/// https://tools.ietf.org/html/rfc7677
		/// </summary>
		public ScramSha256()
		{
		}

		/// <summary>
		/// Name of the mechanism.
		/// </summary>
		public override string Name
		{
			get { return "SCRAM-SHA-256"; }
		}

		/// <summary>
		/// Weight of mechanisms. The higher the value, the more preferred.
		/// </summary>
		public override int Weight => 1000;

		/// <summary>
		/// Hash function
		/// </summary>
		/// <param name="Data">Data to hash.</param>
		/// <returns>Hash of data.</returns>
		public override byte[] H(byte[] Data)
		{
			return Hashes.ComputeSHA256Hash(Data);
		}

	}
}
