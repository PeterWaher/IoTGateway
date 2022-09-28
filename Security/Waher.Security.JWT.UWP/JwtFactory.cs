using System;
using System.Collections.Generic;
using Waher.Security.JWS;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Reason a token is not valid.
	/// </summary>
	public enum Reason
	{
		/// <summary>
		/// No reason
		/// </summary>
		None,

		/// <summary>
		/// No algorithm defined.
		/// </summary>
		NoAlgorithm,

		/// <summary>
		/// Algorithm not supported
		/// </summary>
		UnsupportedAlgorithm,

		/// <summary>
		/// No signature
		/// </summary>
		NoSignature,

		/// <summary>
		/// Token expired
		/// </summary>
		Expired,

		/// <summary>
		/// Too early to use token
		/// </summary>
		TooEarly,

		/// <summary>
		/// Signature invalid
		/// </summary>
		InvalidSignature
	}

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
					throw new ArgumentOutOfRangeException("Time margins must be non-negative.", nameof(TimeMargin));

				this.timeMargin = value;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.algorithm?.Dispose();
			this.algorithm = null;
		}

		/// <summary>
		/// If the factory has been disposed.
		/// </summary>
		public bool Disposed => this.algorithm is null;

		/// <summary>
		/// Checks if a token is valid and signed by the factory.
		/// </summary>
		/// <param name="Token">JWT token.</param>
		/// <returns>If the token is correctly signed and valid.</returns>
		public bool IsValid(JwtToken Token)
		{
			return this.IsValid(Token, out _);
		}

		/// <summary>
		/// Checks if a token is valid and signed by the factory.
		/// </summary>
		/// <param name="Token">JWT token.</param>
		/// <param name="Reason">Reason token is not valid.</param>
		/// <returns>If the token is correctly signed and valid.</returns>
		public bool IsValid(JwtToken Token, out Reason Reason)
		{
			if (Token.Algorithm is null)
			{
				Reason = Reason.NoAlgorithm;
				return false;
			}
			else if (!(Token.Algorithm is HmacSha256))
			{
				Reason = Reason.UnsupportedAlgorithm;
				return false;
			}
			else if (Token.Signature is null)
			{
				Reason = Reason.NoSignature;
				return false;
			}

			if (Token.Expiration.HasValue || Token.NotBefore.HasValue)
			{
				DateTime Now = DateTime.UtcNow;

				if (Token.Expiration.HasValue && Now >= Token.Expiration.Value + this.timeMargin)
				{
					Reason = Reason.Expired;
					return false;
				}

				if (Token.NotBefore.HasValue && Now < Token.NotBefore.Value - this.timeMargin)
				{
					Reason = Reason.TooEarly;
					return false;
				}
			}

			if (!this.algorithm.IsValid(Token.Header, Token.Payload, Token.Signature))
			{
				Reason = Reason.InvalidSignature;
				return false;
			}

			Reason = Reason.None;
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
