using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.JWS
{
	/// <summary>
	/// Abstract base class for HMAC SHA JWS algorithms.
	/// </summary>
    public abstract class HmacSha : JwsAlgorithm
    {
		/// <summary>
		/// If the algorithm has a public key.
		/// </summary>
		public override bool HasPublicWebKey
		{
			get { return false; }
		}
	}
}
