using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Waher.Security.ACME
{
	/// <summary>
	/// RSA with SHA-256 signatures
	/// </summary>
	public class RsaSha256 : SignatureAlgorithm
	{
		private readonly RSACryptoServiceProvider rsa;

		/// <summary>
		/// RSA with SHA-256 signatures
		/// </summary>
		/// <param name="RSA">RSA cryptogaphic service provider.</param>
		public RsaSha256(RSACryptoServiceProvider RSA)
		{
			this.rsa = RSA;
		}

		/// <summary>
		/// Object Identity for the algorithm.
		/// </summary>
		public override string OID => "1.2.840.113549.1.1.11";

		/// <summary>
		/// Exports the public key using DER.
		/// </summary>
		/// <param name="Output">Encoded output.</param>
		public override void ExportPublicKey(DerEncoder Output)
		{
			RSAParameters Parameters = this.rsa.ExportParameters(false);
			
			Output.StartSEQUENCE();
			Output.INTEGER(Parameters.Modulus, false);
			Output.INTEGER(Parameters.Exponent, false);
			Output.EndSEQUENCE();
		}

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="Data">Binary data to sign.</param>
		/// <returns>Signature.</returns>
		public override byte[] Sign(byte[] Data)
		{
			return this.rsa.SignData(Data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		}
	}
}
