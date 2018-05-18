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
		private KeyValuePair<string, object>[] jwk;

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

			this.Init();
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

			this.Init();
		}

		/// <summary>
		/// Deletes the key from the CSP, and disposes the object.
		/// </summary>
		public void DeleteRsaKeyFromCsp()
		{
			this.rsa.PersistKeyInCsp = false;
			this.Dispose();
		}

		private void Init()
		{
			RSAParameters Parameters = this.rsa.ExportParameters(false);

			this.jwk = new KeyValuePair<string, object>[]
			{
				new KeyValuePair<string, object>("kty", "RSA"),
				new KeyValuePair<string, object>("n", Base64Url.Encode(Parameters.Modulus)),
				new KeyValuePair<string, object>("e", Base64Url.Encode(Parameters.Exponent))/*,
				new KeyValuePair<string, object>("d", Base64Url.Encode(Parameters.D)),
				new KeyValuePair<string, object>("p", Base64Url.Encode(Parameters.P)),
				new KeyValuePair<string, object>("q", Base64Url.Encode(Parameters.Q)),
				new KeyValuePair<string, object>("dp", Base64Url.Encode(Parameters.DP)),
				new KeyValuePair<string, object>("dq", Base64Url.Encode(Parameters.DQ)),
				new KeyValuePair<string, object>("qi", Base64Url.Encode(Parameters.InverseQ))*/
			};

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
		/// If the algorithm has a public key.
		/// </summary>
		public override bool HasPublicWebKey => true;

		/// <summary>
		/// The public JSON web key, if supported.
		/// </summary>
		public override IEnumerable<KeyValuePair<string, object>> PublicWebKey => this.jwk;

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
