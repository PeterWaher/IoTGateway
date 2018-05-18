using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Waher.Content;

namespace Waher.Security.JWS
{
	/// <summary>
	/// RSASSA-PKCS1-v1_5 SHA-256 algorithm.
	/// https://tools.ietf.org/html/rfc3447#page-32
	/// </summary>
	public class RsaSsaPkcsSha256 : JwsAlgorithm
	{
		private RSACryptoServiceProvider rsa;
		private SHA256 sha;

		/// <summary>
		/// RSASSA-PKCS1-v1_5 SHA-256 algorithm.
		/// https://tools.ietf.org/html/rfc3447#page-32
		/// </summary>
		public RsaSsaPkcsSha256()
		{
			try
			{
				this.rsa = new RSACryptoServiceProvider(4096);
				this.sha = SHA256.Create();
			}
			catch (CryptographicException ex)
			{
				throw new CryptographicException("Unable to get access to cryptographic key. Was application initially run using another user?", ex);
			}
		}

		/// <summary>
		/// RSASSA-PKCS1-v1_5 SHA-256 algorithm.
		/// https://tools.ietf.org/html/rfc3447#page-32
		/// </summary>
		/// <param name="KeySize">Key size.</param>
		/// <param name="KeyContainerName">CSP Key container name for the private RSA key.</param>
		public RsaSsaPkcsSha256(int KeySize, string KeyContainerName)
		{
			CspParameters CspParams = new CspParameters()
			{
				Flags = CspProviderFlags.UseMachineKeyStore,
				KeyContainerName = KeyContainerName
			};

			try
			{
				this.rsa = new RSACryptoServiceProvider(KeySize, CspParams);
			}
			catch (CryptographicException ex)
			{
				throw new CryptographicException("Unable to get access to cryptographic key. Was application initially run using another user?", ex);
			}

			RSAParameters Parameters = rsa.ExportParameters(true);

			this.sha = SHA256.Create();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (this.rsa != null)
			{
				this.rsa.Dispose();
				this.rsa = null;
			}

			if (this.sha != null)
			{
				this.sha.Dispose();
				this.sha = null;
			}
		}

		/// <summary>
		/// Short name for algorithm.
		/// </summary>
		public override string Name => "RS256";

		/// <summary>
		/// Signs data.
		/// </summary>
		/// <param name="HeaderEncoded">Encoded properties to include in the header.</param>
		/// <param name="PayloadEncoded">Encoded properties to include in the payload.</param>
		/// <returns>Signature</returns>
		public override string Sign(string HeaderEncoded, string PayloadEncoded)
		{
			byte[] SignatureBin;
			string Token = HeaderEncoded + "." + PayloadEncoded;
			byte[] TokenBin = Encoding.ASCII.GetBytes(Token);

			lock (this.rsa)
			{
				SignatureBin = this.rsa.SignHash(this.sha.ComputeHash(TokenBin), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			}

			return Base64Url.Encode(SignatureBin);
		}
	}
}
