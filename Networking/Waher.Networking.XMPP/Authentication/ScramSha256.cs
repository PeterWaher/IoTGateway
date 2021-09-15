using System;
using Waher.Security;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Authentication method: SCRAM-SHA-256
	/// 
	/// See RFC 7677 for a description of the SCRAM-SHA-256 method:
	/// http://tools.ietf.org/html/rfc7677
	/// </summary>
	public class ScramSha256 : ScramAuthenticationMethod
	{
		/// <summary>
		/// Authentication method: SCRAM-SHA-256
		/// 
		/// See RFC 7677 for a description of the SCRAM-SHA-256 method:
		/// http://tools.ietf.org/html/rfc7677
		/// </summary>
		/// <param name="Nonce">Nonce value.</param>
		public ScramSha256(string Nonce)
			: base(Nonce)
		{
		}

		/// <summary>
		/// Name of hash method.
		/// </summary>
		public override string HashMethodName => "SCRAM-SHA-256";

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
