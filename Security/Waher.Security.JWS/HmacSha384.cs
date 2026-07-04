using System.Security.Cryptography;
using System.Text;
using Waher.Content;

namespace Waher.Security.JWS
{
	/// <summary>
	/// HMAC SHA-384 algorithm.
	/// </summary>
	public class HmacSha384 : HmacSha
	{
		private HMACSHA384 hmacSHA384;

		/// <summary>
		/// HMAC SHA-384 algorithm.
		/// </summary>
		public HmacSha384()
		{
			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				byte[] Secret = new byte[48];
				Rnd.GetBytes(Secret);

				this.Init(Secret);
			}
		}

		/// <summary>
		/// HMAC SHA-384 algorithm.
		/// </summary>
		/// <param name="Secret">Secret used for creating and validating signatures.</param>
		public HmacSha384(byte[] Secret)
		{
			this.Init(Secret);
		}

		private void Init(byte[] Secret)
		{
			this.hmacSHA384 = new HMACSHA384(Secret);
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			if (!(this.hmacSHA384 is null))
			{
				this.hmacSHA384.Dispose();
				this.hmacSHA384 = null;
			}
		}

		/// <summary>
		/// Short name for algorithm.
		/// </summary>
		public override string Name => "HS384";

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

			lock (this.hmacSHA384)
			{
				SignatureBin = this.hmacSHA384.ComputeHash(TokenBin);
			}

			return Base64Url.Encode(SignatureBin);
		}
	}
}
