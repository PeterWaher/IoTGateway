using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Waher.Security.PKCS.Passwords
{
	/// <summary>
	/// Implements pbeWithSHAAnd3-KeyTripleDES-CBC
	/// </summary>
	public class PbeWithShaAnd3KeyTripleDesCbc : PbePkcs12
	{
		/// <summary>
		/// Implements pbeWithSHAAnd3-KeyTripleDES-CBC
		/// </summary>
		/// <param name="Password">Password</param>
		/// <param name="Iterations">Number of iterations</param>
		public PbeWithShaAnd3KeyTripleDesCbc(string Password, int Iterations)
			: base(Password, Iterations, 168, HashFunction.SHA1)
		{
		}

		/// <summary>
		/// Object Identity for the algorithm.
		/// </summary>
		public override string AlgorithmOID => "1.2.840.113549.1.12.1.3";

		/// <summary>
		/// Encrypts data.
		/// </summary>
		/// <param name="PlainText">Data to encrypt.</param>
		/// <returns>Encrypted data.</returns>
		public override byte[] Encrypt(byte[] PlainText)
		{
			using (TripleDES C = TripleDES.Create())
			{
				C.Key = this.Key;
				C.Mode = CipherMode.CBC;
				C.Padding = PaddingMode.PKCS7;

				using (ICryptoTransform E = C.CreateEncryptor())
				{
					C.IV = PfxEncoder.PRF(HashFunction.SHA1, this.Iterations,
						PfxEncoder.FormatPassword(Password), this.Salt, 64, 2);

					return E.TransformFinalBlock(PlainText, 0, PlainText.Length);
				}
			}
		}

	}
}
