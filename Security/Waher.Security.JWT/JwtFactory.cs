using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Security.JWT
{
	/// <summary>
	/// A factory that can create and validate JWT tokens.
	/// </summary>
	public class JwtFactory : IDisposable
	{
		private HMACSHA256 hmacSHA256;
		private TimeSpan timeMargin = TimeSpan.Zero;
		private string headerStr;
		private byte[] headerBin;
		private string header;

		/// <summary>
		/// A factory that can create and validate JWT tokens.
		/// </summary>
		public JwtFactory()
		{
			using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
			{
				byte[] Secret = new byte[32];
				Rnd.GetBytes(Secret);

				this.Init(Secret);
			}
		}

		/// <summary>
		/// A factory that can create and validate JWT tokens.
		/// </summary>
		/// <param name="Secret">Secret used for creating and validating signatures.</param>
		public JwtFactory(byte[] Secret)
		{
			this.Init(Secret);
		}

		private void Init(byte[] Secret)
		{
			this.hmacSHA256 = new HMACSHA256(Secret);

			this.headerStr = "{\"typ\":\"JWT\",\"alg\":\"HS256\"}";
			this.headerBin = Encoding.UTF8.GetBytes(this.headerStr);
			this.header = JwtToken.Base64UrlEncode(this.headerBin);
		}

		/// <summary>
		/// Time margin in validity checks, to cover for unsynchronized clocks.
		/// By default, it is set to <see cref="TimeSpan.Zero"/>.
		/// </summary>
		public TimeSpan TimeMargin
		{
			get { return this.timeMargin; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentException("Time margins must be zero or positive.", "TimeMargin");

				this.timeMargin = value;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.hmacSHA256 != null)
			{
				this.hmacSHA256.Dispose();
				this.hmacSHA256 = null;
			}
		}

		/// <summary>
		/// Checks if a token is valid and signed by the factory.
		/// </summary>
		/// <param name="Token">JWT token.</param>
		/// <returns>If the token is correctly signed and valid.</returns>
		public bool IsValid(JwtToken Token)
		{
			if (Token.Algorithm != JwtAlgorithm.HS256 || Token.Signature == null)
				return false;

			if (Token.Expiration != null || Token.NotBefore != null)
			{
				DateTime Now = DateTime.UtcNow;

				if (Token.Expiration != null && Now >= Token.Expiration.Value + this.timeMargin)
					return false;

				if (Token.NotBefore != null && Now < Token.NotBefore.Value - this.timeMargin)
					return false;
			}

			byte[] Signature = Token.Signature;
			byte[] Hash;
			int i, c;

			lock (this.hmacSHA256)
			{
				Hash = this.hmacSHA256.ComputeHash(Encoding.ASCII.GetBytes(Token.Header + "." + Token.Payload));
			}

			if ((c = Hash.Length) != Signature.Length)
				return false;

			for (i = 0; i < c; i++)
			{
				if (Hash[i] != Signature[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Creates a new JWT token.
		/// </summary>
		/// <param name="Claims">Claims to include in token.
		/// 
		/// For a list of public claim names, see:
		/// https://www.iana.org/assignments/jwt/jwt.xhtml</param>
		/// <returns>JWT token.</returns>
		public string Create(params KeyValuePair<string, object>[] Claims)
		{
			StringBuilder Json = new StringBuilder("{");
			bool First = true;

			foreach (KeyValuePair<string, object> Claim in Claims)
			{
				if (First)
					First = false;
				else
					Json.Append(',');

				Json.Append('"');
				Json.Append(JSON.Encode(Claim.Key));
				Json.Append("\":");

				if (Claim.Value == null)
					Json.Append("null");
				else if (Claim.Value is string s)
				{
					Json.Append('"');
					Json.Append(JSON.Encode(s));
					Json.Append('"');
				}
				else if (Claim.Value is bool b)
				{
					if (b)
						Json.Append("true");
					else
						Json.Append("false");
				}
				else if (Claim.Value is DateTime TP)
					Json.Append(((int)((TP.ToUniversalTime() - JwtToken.epoch).TotalSeconds)).ToString());
				else if (Claim.Value is int i)
					Json.Append(i.ToString());
				else if (Claim.Value is long l)
					Json.Append(l.ToString());
				else if (Claim.Value is short sh)
					Json.Append(sh.ToString());
				else if (Claim.Value is byte bt)
					Json.Append(bt.ToString());
				else
				{
					Json.Append('"');
					Json.Append(JSON.Encode(Claim.Value.ToString()));
					Json.Append('"');
				}
			}

			Json.Append('}');

			string PayloadStr = Json.ToString();
			byte[] PayloadBin = Encoding.UTF8.GetBytes(PayloadStr);
			string Payload = JwtToken.Base64UrlEncode(PayloadBin);
			byte[] Signature;
			string Token = this.header + "." + Payload;

			lock (this.hmacSHA256)
			{
				Signature = this.hmacSHA256.ComputeHash(Encoding.ASCII.GetBytes(Token));
			}

			Token += "." + JwtToken.Base64UrlEncode(Signature);

			return Token;
		}

	}
}
