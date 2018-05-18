using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Security.JWS;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Contains information about a Java Web Token (JWT). JWT is defined in RFC 7519:
	/// https://tools.ietf.org/html/rfc7519
	/// 
	/// JWT are based on JSON Web Signature (JWS), defined in RFC 7515:
	/// https://tools.ietf.org/html/rfc7515
	/// 
	/// Signature algorithms are defined in RFC 7518:
	/// https://tools.ietf.org/html/rfc7518
	/// </summary>
	public class JwtToken
	{
		private readonly string token;
		private readonly IJwsAlgorithm algorithm;
		private readonly Dictionary<string, object> claims;
		private readonly string header;
		private readonly string payload;
		private readonly string signature = null;
		private readonly string type = null;
		private readonly string issuer = null;
		private readonly string subject = null;
		private readonly string id = null;
		private readonly string[] audience = null;
		private readonly DateTime? expiration = null;
		private readonly DateTime? notBefore = null;
		private readonly DateTime? issuedAt = null;

		/// <summary>
		/// Contains information about a Java Web Token (JWT). JWT is defined in RFC 7519:
		/// https://tools.ietf.org/html/rfc7519
		/// 
		/// JWT are based on JSON Web Signature (JWS), defined in RFC 7515:
		/// https://tools.ietf.org/html/rfc7515
		/// 
		/// Signature algorithms are defined in RFC 7518:
		/// https://tools.ietf.org/html/rfc7518
		/// </summary>
		public JwtToken(string Token)
		{
			this.token = Token;

			try
			{
				string[] Parts = Token.Split('.');
				byte[] HeaderBin = Base64Url.Decode(this.header = Parts[0]);
				string HeaderString = Encoding.UTF8.GetString(HeaderBin);

				if (JSON.Parse(HeaderString) is Dictionary<string, object> Header)
				{
					if (Header.TryGetValue("typ", out object Typ))
						this.type = Typ as string;

					if (!Header.TryGetValue("alg", out object Alg) || !(Alg is string AlgStr))
						throw new ArgumentException("Invalid alg header field.", nameof(Token));

					if (string.IsNullOrEmpty(AlgStr) || AlgStr.ToLower() == "none")
						this.algorithm = null;
					else if (!JwsAlgorithm.TryGetAlgorithm(AlgStr, out this.algorithm))
						throw new ArgumentException("Unrecognized algorithm reference in header field.", nameof(Token));
				}
				else
					throw new Exception("Invalid JSON header.");

				if (Parts.Length < 2)
					throw new Exception("Claims set missing.");

				byte[] ClaimsBin = Base64Url.Decode(this.payload = Parts[1]);
				string ClaimsString = Encoding.UTF8.GetString(ClaimsBin);

				if (JSON.Parse(ClaimsString) is Dictionary<string, object> Claims)
				{
					this.claims = Claims;

					foreach (KeyValuePair<string, object> P in Claims)
					{
						switch (P.Key)
						{
							case "iss":
								this.issuer = P.Value as string;
								break;

							case "sub":
								this.subject = P.Value as string;
								break;

							case "jti":
								this.id = P.Value as string;
								break;

							case "aud":
								if (P.Value is string AudStr)
									this.audience = AudStr.Split(',');
								else if (P.Value is Array)
								{
									List<string> Audience = new List<string>();

									foreach (object Item in (Array)P.Value)
										Audience.Add(Item.ToString());

									this.audience = Audience.ToArray();
								}
								break;

							case "exp":
								if (P.Value is int ExpInt)
									this.expiration = JSON.UnixEpoch.AddSeconds(ExpInt);
								break;

							case "nbf":
								if (P.Value is int NbfInt)
									this.notBefore = JSON.UnixEpoch.AddSeconds(NbfInt);
								break;

							case "iat":
								if (P.Value is int IatInt)
									this.issuedAt = JSON.UnixEpoch.AddSeconds(IatInt);
								break;

							case "expires":
								break;
						}
					}
				}
				else
					throw new Exception("Invalid JSON claims set.");

				if (Parts.Length < 3)
					throw new Exception("Signature missing.");

				this.signature = Parts[2];
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to parse JWT token.", ex);
			}
		}

		/// <summary>
		/// The Base64URL-encoded header.
		/// </summary>
		public string Header
		{
			get { return this.header; }
		}

		/// <summary>
		/// The Base64URL-encoded payload.
		/// </summary>
		public string Payload
		{
			get { return this.payload; }
		}

		/// <summary>
		/// The Base64URL-encoded signature.
		/// </summary>
		public string Signature
		{
			get { return this.signature; }
		}

		/// <summary>
		/// String token that can be embedded easily in web requests, etc.
		/// </summary>
		public string Token
		{
			get { return this.token; }
		}

		/// <summary>
		/// JWS signature algoritm.
		/// </summary>
		public IJwsAlgorithm Algorithm
		{
			get { return this.algorithm; }
		}

		/// <summary>
		/// Claims provided in token. For a list of public claim names, see:
		/// https://www.iana.org/assignments/jwt/jwt.xhtml
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> Claims
		{
			get { return this.claims; }
		}

		/// <summary>
		/// Tries to get a claim from the JWT token.
		/// </summary>
		/// <param name="Key">Claim key.</param>
		/// <param name="Value">Claim value.</param>
		/// <returns>If the corresponding claim was found.</returns>
		public bool TryGetClaim(string Key, out object Value)
		{
			return this.claims.TryGetValue(Key, out Value);
		}

		/// <summary>
		/// Type of token, if available, null otherwise.
		/// </summary>
		public string Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Issuer of token, if available, null otherwise.
		/// </summary>
		public string Issuer
		{
			get { return this.issuer; }
		}

		/// <summary>
		/// Subject of whom the token relates, if available, null otherwise.
		/// </summary>
		public string Subject
		{
			get { return this.subject; }
		}

		/// <summary>
		/// Token ID, if available, null otherwise.
		/// </summary>
		public string Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Indended audience, if available, null otherwise.
		/// </summary>
		public string[] Audience
		{
			get { return this.audience; }
		}

		/// <summary>
		/// Token expiration time, if available, null otherwise.
		/// </summary>
		public DateTime? Expiration
		{
			get { return this.expiration; }
		}

		/// <summary>
		/// Token not valid before this time, if available, null otherwise.
		/// </summary>
		public DateTime? NotBefore
		{
			get { return this.notBefore; }
		}

		/// <summary>
		/// Token issued at this time, if available, null otherwise.
		/// </summary>
		public DateTime? IssuedAt
		{
			get { return this.issuedAt; }
		}

	}
}
