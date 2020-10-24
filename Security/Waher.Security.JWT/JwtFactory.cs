using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Security.JWS;

namespace Waher.Security.JWT
{
	/// <summary>
	/// A factory that can create and validate JWT tokens.
	/// </summary>
	public class JwtFactory : IDisposable
	{
		private HmacSha256 algorithm;
		private TimeSpan timeMargin = TimeSpan.Zero;
		private readonly KeyValuePair<string, object>[] header = new KeyValuePair<string, object>[]
		{
			new KeyValuePair<string, object>("typ", "JWT")
		};

		/// <summary>
		/// A factory that can create and validate JWT tokens.
		/// </summary>
		public JwtFactory()
		{
			this.algorithm = new HmacSha256();
		}

		/// <summary>
		/// A factory that can create and validate JWT tokens.
		/// </summary>
		/// <param name="Secret">Secret used for creating and validating signatures.</param>
		public JwtFactory(byte[] Secret)
		{
			this.algorithm = new HmacSha256(Secret);
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
					throw new ArgumentOutOfRangeException("Time margins must be zero or positive.", nameof(TimeMargin));

				this.timeMargin = value;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!(this.algorithm is null))
			{
				this.algorithm.Dispose();
				this.algorithm = null;
			}
		}

		/// <summary>
		/// Checks if a token is valid and signed by the factory.
		/// </summary>
		/// <param name="Token">JWT token.</param>
		/// <returns>If the token is correctly signed and valid.</returns>
		public bool IsValid(JwtToken Token)
		{
			if (Token.Algorithm is null || !(Token.Algorithm is HmacSha256) || Token.Signature is null)
				return false;

			if (Token.Expiration != null || Token.NotBefore != null)
			{
				DateTime Now = DateTime.UtcNow;

				if (Token.Expiration != null && Now >= Token.Expiration.Value + this.timeMargin)
					return false;

				if (Token.NotBefore != null && Now < Token.NotBefore.Value - this.timeMargin)
					return false;
			}

			return this.algorithm.IsValid(Token.Header, Token.Payload, Token.Signature);
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
			return this.Create((IEnumerable<KeyValuePair<string, object>>)Claims);
		}

		/// <summary>
		/// Creates a new JWT token.
		/// </summary>
		/// <param name="Claims">Claims to include in token.
		/// 
		/// For a list of public claim names, see:
		/// https://www.iana.org/assignments/jwt/jwt.xhtml</param>
		/// <returns>JWT token.</returns>
		public string Create(IEnumerable<KeyValuePair<string, object>> Claims)
		{
			this.algorithm.Sign(this.header, Claims, out string Header, out string Payload, 
				out string Signature);

			return Header + "." + Payload + "." + Signature;
		}

	}
}
