using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Base class for RSA algorithms
	/// </summary>
	public abstract class Rsa : SignatureAlgorithm
	{
		private readonly RSACryptoServiceProvider rsa;

		/// <summary>
		/// Base class for RSA algorithms
		/// </summary>
		/// <param name="RSA">RSA cryptogaphic service provider.</param>
		public Rsa(RSACryptoServiceProvider RSA)
		{
			this.rsa = RSA;
		}

		/// <summary>
		/// Object Identity for the PKI algorithm.
		/// </summary>
		public override string PkiAlgorithmOID => "1.2.840.113549.1.1.1";

		/// <summary>
		/// Name of hash algorithm to use for signatues.
		/// </summary>
		public abstract HashAlgorithmName HashAlgorithmName
		{
			get;
		}

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
			return this.rsa.SignData(Data, this.HashAlgorithmName, RSASignaturePadding.Pkcs1);
		}
	}
}
