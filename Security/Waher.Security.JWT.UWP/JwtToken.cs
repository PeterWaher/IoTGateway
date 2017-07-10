using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Signing algorithms supported for JWT tokens.
	/// </summary>
	public enum JwtAlgorithm
	{
		/// <summary>
		/// HMAC SHA-256
		/// </summary>
		HS256,

		/// <summary>
		/// HMAC SHA-384
		/// </summary>
		HS384,

		/// <summary>
		/// HMAC SHA-512
		/// </summary>
		HS512,

		/// <summary>
		/// Unsecure token.
		/// </summary>
		none
	}

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
		private string token;
		private JwtAlgorithm algorithm;
		private Dictionary<string, object> claims;
		private string header;
		private string payload;
		private string type = null;
		private string issuer = null;
		private string subject = null;
		private string id = null;
		private string[] audience = null;
		private byte[] signature = null;
		private DateTime? expiration = null;
		private DateTime? notBefore = null;
		private DateTime? issuedAt = null;

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
				byte[] HeaderBin = Base64UrlDecode(this.header = Parts[0]);
				string HeaderString = Encoding.UTF8.GetString(HeaderBin);

				if (JSON.Parse(HeaderString) is Dictionary<string, object> Header)
				{
					if (Header.TryGetValue("typ", out object Typ))
						this.type = Typ as string;

					if (!Header.TryGetValue("alg", out object Alg) || !(Alg is string AlgStr) || !Enum.TryParse<JwtAlgorithm>(AlgStr, true, out this.algorithm))
						throw new ArgumentException("Invalid alg header field.", "alg");
				}
				else
					throw new Exception("Invalid JSON header.");

				if (Parts.Length < 2)
					throw new Exception("Claims set missing.");

				byte[] ClaimsBin = Base64UrlDecode(this.payload = Parts[1]);
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
									this.expiration = epoch.AddSeconds(ExpInt);
								break;

							case "nbf":
								if (P.Value is int NbfInt)
									this.notBefore = epoch.AddSeconds(NbfInt);
								break;

							case "iat":
								if (P.Value is int IatInt)
									this.issuedAt = epoch.AddSeconds(IatInt);
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

				this.signature = Base64UrlDecode(Parts[2]);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to parse JWT token.", ex);
			}
		}

		internal static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Converts a Base64URL-encoded string to its binary representation.
		/// </summary>
		/// <param name="Base64Url">Base64URL-encoded string.</param>
		/// <returns>Binary representation.</returns>
		public static byte[] Base64UrlDecode(string Base64Url)
		{
			int c = Base64Url.Length;
			int i = c & 3;

			Base64Url = Base64Url.Replace('-', '+'); // 62nd char of encoding
			Base64Url = Base64Url.Replace('_', '/'); // 63rd char of encoding

			switch (i)
			{
				case 1:
					Base64Url += "A==";
					break;

				case 2:
					Base64Url += "==";
					break;

				case 3:
					Base64Url += "=";
					break;
			}

			return Convert.FromBase64String(Base64Url);
		}

		/// <summary>
		/// Converts a binary block of data to a Base64URL-encoded string.
		/// </summary>
		/// <param name="Data">Data to encode.</param>
		/// <returns>Base64URL-encoded string.</returns>
		public static string Base64UrlEncode(byte[] Data)
		{
			string s = Convert.ToBase64String(Data);
			int c = Data.Length;
			int i = c % 3;

			if (i == 1)
				s = s.Substring(0, s.Length - 2);
			else if (i == 2)
				s = s.Substring(0, s.Length - 1);

			s = s.Replace('+', '-'); // 62nd char of encoding
			s = s.Replace('/', '_'); // 63rd char of encoding

			return s;
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
		/// String token that can be embedded easily in web requests, etc.
		/// </summary>
		public string Token
		{
			get { return this.token; }
		}

		/// <summary>
		/// JWT signature algoritm.
		/// </summary>
		public JwtAlgorithm Algorithm
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

		/// <summary>
		/// Signature of token.
		/// </summary>
		public byte[] Signature
		{
			get { return this.signature; }
		}

	}
}
