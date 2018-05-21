using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Abstract base class for signature algorithms
	/// </summary>
    public abstract class SignatureAlgorithm
    {
		/// <summary>
		/// Object Identity for the algorithm.
		/// </summary>
		public abstract string OID
		{
			get;
		}

		/// <summary>
		/// Exports the public key using DER.
		/// </summary>
		/// <param name="Output">Encoded output.</param>
		public abstract void ExportPublicKey(DerEncoder Output);

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <returns>Signature.</returns>
		public abstract byte[] Sign(byte[] Data);
    }
}
