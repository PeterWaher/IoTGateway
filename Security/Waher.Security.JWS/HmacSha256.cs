using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Waher.Content;

namespace Waher.Security.JWS
{
	/// <summary>
	/// HMAC SHA-256 algorithm.
	/// </summary>
	public class HmacSha256 : HmacSha
	{
		private HMACSHA256 hmacSHA256;

		/// <summary>
		/// HMAC SHA-256 algorithm.
		/// </summary>
		public HmacSha256()
		{
			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				byte[] Secret = new byte[32];
				Rnd.GetBytes(Secret);

				this.Init(Secret);
			}
		}

		/// <summary>
		/// HMAC SHA-256 algorithm.
		/// </summary>
		/// <param name="Secret">Secret used for creating and validating signatures.</param>
		public HmacSha256(byte[] Secret)
		{
			this.Init(Secret);
		}

		private void Init(byte[] Secret)
		{
			this.hmacSHA256 = new HMACSHA256(Secret);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (this.hmacSHA256 != null)
			{
				this.hmacSHA256.Dispose();
				this.hmacSHA256 = null;
			}
		}

		/// <summary>
		/// Short name for algorithm.
		/// </summary>
		public override string Name => "HS256";

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

			lock (this.hmacSHA256)
			{
				SignatureBin = this.hmacSHA256.ComputeHash(TokenBin);
			}

			return Base64Url.Encode(SignatureBin);
		}
	}
}
