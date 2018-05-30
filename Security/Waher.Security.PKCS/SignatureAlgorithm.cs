using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Abstract base class for signature algorithms
	/// </summary>
    public abstract class SignatureAlgorithm
    {
		/// <summary>
		/// Object Identity for the PKI algorithm.
		/// </summary>
		public abstract string PkiAlgorithmOID
		{
			get;
		}

		/// <summary>
		/// Object Identity for the Hash algorithm.
		/// </summary>
		public abstract string HashAlgorithmOID
		{
			get;
		}

		/// <summary>
		/// Exports the public key using DER.
		/// </summary>
		/// <param name="Output">Encoded output.</param>
		public abstract void ExportPublicKey(DerEncoder Output);

		/// <summary>
		/// Exports the private key using DER.
		/// </summary>
		/// <param name="Output">Encoded output.</param>
		public abstract void ExportPrivateKey(DerEncoder Output);

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <returns>Signature.</returns>
		public abstract byte[] Sign(byte[] Data);
    }
}
