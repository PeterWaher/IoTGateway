using System;
using System.Collections.Generic;
using Waher.Security;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Authentication method: SCRAM-SHA-1
	/// 
	/// See RFC 5802 for a description of the SCRAM-SHA-1 method:
	/// http://tools.ietf.org/html/rfc5802
	/// </summary>
	public class ScramSha1 : ScramAuthenticationMethod
	{
		/// <summary>
		/// Authentication method: SCRAM-SHA-1
		/// 
		/// See RFC 5802 for a description of the SCRAM-SHA-1 method:
		/// http://tools.ietf.org/html/rfc5802
		/// </summary>
		/// <param name="Nonce">Nonce value.</param>
		public ScramSha1(string Nonce)
			: base(Nonce)
		{
		}

		/// <summary>
		/// Name of hash method.
		/// </summary>
		public override string HashMethodName => "SCRAM-SHA-1";

		/// <summary>
		/// Hash function
		/// </summary>
		/// <param name="Data">Data to hash.</param>
		/// <returns>Hash of data.</returns>
		public override byte[] H(byte[] Data)
		{
			return Hashes.ComputeSHA1Hash(Data);
		}

	}
}
