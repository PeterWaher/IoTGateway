using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Waher.Runtime.Collections;
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
		InvalidSignature,

		/// <summary>
		/// Token has been deprecated
		/// </summary>
		Deprecated
	}

	/// <summary>
	/// A factory that can create and validate JWT tokens.
	/// </summary>
	public class JwtFactory : IDisposable
	{
		private static readonly Dictionary<DateTime, ChunkedList<string>> deprecatedByExpiry = new Dictionary<DateTime, ChunkedList<string>>();

		private IJwsAlgorithm algorithm;
		private TimeSpan timeMargin = TimeSpan.Zero;
		private readonly KeyValuePair<string, object>[] header = new KeyValuePair<string, object>[]
		{
			new KeyValuePair<string, object>("typ", "JWT")
		};

		/// <summary>
		/// A factory that can create and validate JWT tokens.
		/// </summary>
		/// <param name="Algorithm">JWS Algorithm to use for signatures</param>
		public JwtFactory(IJwsAlgorithm Algorithm)
		{
			this.algorithm = Algorithm;
		}

		/// <summary>
		/// A factory that can create and validate JWT tokens using the HMAC-SHA256 algorithm.
		/// </summary>
		[Obsolete("Use any of the static Create methods instead, or specify the JWS algorithm explicitly.")]
		public JwtFactory()
			: this(new HmacSha256())
		{
		}

		/// <summary>
		/// A factory that can create and validate JWT tokens using the HMAC-SHA256 algorithm.
		/// </summary>
		/// <param name="Secret">Secret used for creating and validating signatures.</param>
		[Obsolete("Use any of the static Create methods instead, or specify the JWS algorithm explicitly.")]
		public JwtFactory(byte[] Secret)
			: this(new HmacSha256(Secret))
		{
		}

		/// <summary>
		/// Creates a JWT factory that can create and validate JWT tokens using the HMAC-SHA256 algorithm.
		/// </summary>
		public static JwtFactory CreateHmacSha256()
		{
			return new JwtFactory(new HmacSha256());
		}

		/// <summary>
		/// Creates a JWT factory that can create and validate JWT tokens using the HMAC-SHA256 algorithm.
		/// </summary>
		/// <param name="Secret">Secret used for creating and validating signatures.</param>
		public static JwtFactory CreateHmacSha256(byte[] Secret)
		{
			return new JwtFactory(new HmacSha256(Secret));
		}

		/// <summary>
		/// Creates a JWT factory that can create and validate JWT tokens using the RSA256 algorithm.
		/// </summary>
		public static JwtFactory CreateRsa256()
		{
			return new JwtFactory(new RsaSsaPkcsSha256());
		}

		/// <summary>
		/// Creates a JWT factory that can create and validate JWT tokens using the RSA256 algorithm.
		/// </summary>
		/// <param name="KeySize">Key size.</param>
		public static JwtFactory CreateRsa256(int KeySize)
		{
			return new JwtFactory(new RsaSsaPkcsSha256(KeySize));
		}

		/// <summary>
		/// Creates a JWT factory that can create and validate JWT tokens using the RSA256 algorithm.
		/// </summary>
		/// <param name="Algorithm">RSA Algorithm used for creating and validating signatures.</param>
		public static JwtFactory CreatRsa256(RSA Algorithm)
		{
			return new JwtFactory(new RsaSsaPkcsSha256(Algorithm));
		}

		/// <summary>
		/// Creates a JWT factory that can create and validate JWT tokens using the RSA256 algorithm.
		/// </summary>
		/// <param name="Parameters">RSA Parameters</param>
		public static JwtFactory CreateRsa256(RSAParameters Parameters)
		{
			return new JwtFactory(new RsaSsaPkcsSha256(Parameters));
		}

		/// <summary>
		/// Time margin in validity checks, to cover for unsynchronized clocks.
		/// By default, it is set to <see cref="TimeSpan.Zero"/>.
		/// </summary>
		public TimeSpan TimeMargin
		{
			get => this.timeMargin;
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("Time margins must be non-negative.", nameof(this.TimeMargin));

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
			else if (Token.Algorithm is null)
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

			try
			{
				if (!this.algorithm.IsValid(Token.Header, Token.Payload, Token.Signature))
				{
					Reason = Reason.InvalidSignature;
					return false;
				}
			}
			catch (Exception)
			{
				Reason = Reason.UnsupportedAlgorithm;
				return false;
			}

			if (IsDeprecated(Token))
			{
				Reason = Reason.Deprecated;
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
			return this.Create(null, (IEnumerable<KeyValuePair<string, object>>)Claims);
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
			return this.Create(null, Claims);
		}

		/// <summary>
		/// Creates a new JWT token.
		/// </summary>
		/// <param name="Headers">Optional additional headers to include in token.</param>
		/// <param name="Claims">Claims to include in token.
		/// 
		/// For a list of public claim names, see:
		/// https://www.iana.org/assignments/jwt/jwt.xhtml</param>
		/// <returns>JWT token.</returns>
		public string Create(KeyValuePair<string, object>[] Headers, KeyValuePair<string, object>[] Claims)
		{
			return this.Create((IEnumerable<KeyValuePair<string, object>>)Headers, (IEnumerable<KeyValuePair<string, object>>)Claims);
		}

		/// <summary>
		/// Creates a new JWT token.
		/// </summary>
		/// <param name="Headers">Optional additional headers to include in token.</param>
		/// <param name="Claims">Claims to include in token.
		/// 
		/// For a list of public claim names, see:
		/// https://www.iana.org/assignments/jwt/jwt.xhtml</param>
		/// <returns>JWT token.</returns>
		public string Create(IEnumerable<KeyValuePair<string, object>> Headers, IEnumerable<KeyValuePair<string, object>> Claims)
		{
			IEnumerable<KeyValuePair<string, object>> Headers2;

			if (Headers is null)
				Headers2 = this.header;
			else
			{
				ChunkedList<KeyValuePair<string, object>> Union = new ChunkedList<KeyValuePair<string, object>>();

				Union.AddRange(this.header);

				foreach (KeyValuePair<string, object> P in Headers)
					Union.Add(P);

				Headers2 = Union.ToArray();
			}

			this.algorithm.Sign(Headers2, Claims, out string Header, out string Payload,
				out string Signature);

			return Header + "." + Payload + "." + Signature;
		}

		/// <summary>
		/// Deprecates a token.
		/// </summary>
		/// <param name="Token">Token to deprecate.</param>
		public static void Deprecate(JwtToken Token)
		{
			lock (deprecatedByExpiry)
			{
				DateTime Expiry = Token.Expiration ?? DateTime.MaxValue;

				if (!deprecatedByExpiry.TryGetValue(Expiry, out ChunkedList<string> List))
				{
					List = new ChunkedList<string>();
					deprecatedByExpiry[Expiry] = List;
				}

				if (!List.Contains(Token.Token))
					List.Add(Token.Token);

				ChunkedList<DateTime> ToRemove = null;
				DateTime Yesterday = DateTime.UtcNow.AddDays(-1);

				foreach (KeyValuePair<DateTime, ChunkedList<string>> P in deprecatedByExpiry)
				{
					if (P.Key > Yesterday)
						break;

					if (ToRemove is null)
						ToRemove = new ChunkedList<DateTime>();

					ToRemove.Add(P.Key);
				}

				if (!(ToRemove is null))
				{
					foreach (DateTime TP in ToRemove)
						deprecatedByExpiry.Remove(TP);
				}
			}
		}

		/// <summary>
		/// Checks if a token is deprecated.
		/// </summary>
		/// <param name="Token">Token</param>
		/// <returns>If token has been deprecated.</returns>
		public static bool IsDeprecated(JwtToken Token)
		{
			lock (deprecatedByExpiry)
			{
				DateTime Expiry = Token.Expiration ?? DateTime.MaxValue;

				if (!deprecatedByExpiry.TryGetValue(Expiry, out ChunkedList<string> List))
					return false;

				return List.Contains(Token.Token);
			}
		}
	}
}
