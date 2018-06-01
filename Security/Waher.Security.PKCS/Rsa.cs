using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Base class for RSA algorithms
	/// </summary>
	public abstract class Rsa : SignatureAlgorithm
	{
		private readonly RSA rsa;

		/// <summary>
		/// Base class for RSA algorithms
		/// </summary>
		/// <param name="RSA">RSA cryptogaphic service provider.</param>
		public Rsa(RSA RSA)
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
		/// Exports the private key using DER.
		/// </summary>
		/// <param name="Output">Encoded output.</param>
		public override void ExportPrivateKey(DerEncoder Output)
		{
			RSAParameters Parameters = this.rsa.ExportParameters(true);

			Output.StartSEQUENCE();
			Output.INTEGER(0);	// Version
			Output.INTEGER(Parameters.Modulus, false);
			Output.INTEGER(Parameters.Exponent, false);
			Output.INTEGER(Parameters.D, false);
			Output.INTEGER(Parameters.P, false);
			Output.INTEGER(Parameters.Q, false);
			Output.INTEGER(Parameters.DP, false);
			Output.INTEGER(Parameters.DQ, false);
			Output.INTEGER(Parameters.InverseQ, false);
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
