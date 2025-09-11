using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Runtime.Collections;
using Waher.Script;
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
		private string token;
		private IJwsAlgorithm algorithm;
		private Dictionary<string, object> claims;
		private string header;
		private string payload;
		private string signature = null;
		private string type = null;
		private string issuer = null;
		private string subject = null;
		private string id = null;
		private string[] audience = null;
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
		/// <param name="Token">String-representation of token.</param>
		public JwtToken(string Token)
		{
			if (!ParseInto(Token, this, out string Reason))
				throw new Exception("Unable to parse JWT token: " + Token + "\r\nReason: " + Reason);
		}

		/// <summary>
		/// Contains information about a Java Web Token (JWT). JWT is defined in RFC 7519:
		/// </summary>
		private JwtToken()
		{
		}

		/// <summary>
		/// Tries to parse a JWT token.
		/// </summary>
		/// <param name="Token">String-representation of token.</param>
		/// <param name="ParsedToken">Parsed token, if successful.</param>
		/// <returns>If successful in parsing token.</returns>
		public static bool TryParse(string Token, out JwtToken ParsedToken)
		{
			return TryParse(Token, out ParsedToken, out _);
		}

		/// <summary>
		/// Tries to parse a JWT token.
		/// </summary>
		/// <param name="Token">String-representation of token.</param>
		/// <param name="ParsedToken">Parsed token, if successful.</param>
		/// <param name="Reason">Reason, if not successful.</param>
		/// <returns>If successful in parsing token.</returns>
		public static bool TryParse(string Token, out JwtToken ParsedToken, out string Reason)
		{
			ParsedToken = new JwtToken();
			return ParseInto(Token, ParsedToken, out Reason);
		}

		/// <summary>
		/// Parses a token into a provided JwtToken object.
		/// </summary>
		/// <param name="Token">String-representation of token.</param>
		/// <param name="ParsedToken">Token to receive parsed information.</param>
		/// <param name="Reason">Reason, if not successful.</param>
		/// <returns>If successful in parsing token.</returns>
		public static bool ParseInto(string Token, JwtToken ParsedToken, out string Reason)
		{
			try
			{
				ParsedToken.token = Token;
				
				string[] Parts = Token.Split('.');
				ParsedToken.header = Parts[0];

				byte[] HeaderBin = Base64Url.Decode(ParsedToken.header);
				string HeaderString = Encoding.UTF8.GetString(HeaderBin);

				if (!(JSON.Parse(HeaderString) is Dictionary<string, object> Header))
				{
					Reason = "Invalid JSON header.";
					return false;
				}

				if (Header.TryGetValue("typ", out object Typ))
					ParsedToken.type = Typ as string;

				if (!Header.TryGetValue("alg", out object Alg) || !(Alg is string AlgStr))
				{
					Reason = "Invalid alg header field.";
					return false;
				}

				if (string.IsNullOrEmpty(AlgStr) || string.Compare(AlgStr, "none", true) == 0)
					ParsedToken.algorithm = null;
				else if (!JwsAlgorithm.TryGetAlgorithm(AlgStr, out ParsedToken.algorithm))
				{
					Reason = "Unrecognized algorithm reference in header field.";
					return false;
				}

				if (Parts.Length < 2)
				{
					Reason = "Claims set missing.";
					return false;
				}

				byte[] ClaimsBin = Base64Url.Decode(ParsedToken.payload = Parts[1]);
				string ClaimsString = Encoding.UTF8.GetString(ClaimsBin);

				if (!(JSON.Parse(ClaimsString) is Dictionary<string, object> Claims))
				{
					Reason = "Invalid JSON claims set.";
					return false;
				}

				ParsedToken.claims = Claims;

				foreach (KeyValuePair<string, object> P in Claims)
				{
					switch (P.Key)
					{
						case JwtClaims.Issuer:
							ParsedToken.issuer = P.Value as string;
							break;

						case JwtClaims.Subject:
							ParsedToken.subject = P.Value as string;
							break;

						case JwtClaims.JwtId:
							ParsedToken.id = P.Value as string;
							break;

						case JwtClaims.Audience:
							if (P.Value is string AudStr)
								ParsedToken.audience = AudStr.Split(',');
							else if (P.Value is Array A)
							{
								ChunkedList<string> Audience2 = new ChunkedList<string>();

								foreach (object Item in A)
									Audience2.Add(Item.ToString());

								ParsedToken.audience = Audience2.ToArray();
							}
							break;

						case JwtClaims.ExpirationTime:
							ParsedToken.expiration = JSON.UnixEpoch.AddSeconds(Expression.ToDouble(P.Value));
							break;

						case JwtClaims.NotBeforeTime:
							ParsedToken.notBefore = JSON.UnixEpoch.AddSeconds(Expression.ToDouble(P.Value));
							break;

						case JwtClaims.IssueTime:
							ParsedToken.issuedAt = JSON.UnixEpoch.AddSeconds(Expression.ToDouble(P.Value));
							break;

						case "expires":
							break;
					}
				}

				if (Parts.Length < 3)
				{
					Reason = "Signature missing.";
					return false;
				}

				ParsedToken.signature = Parts[2];

				Reason = null;
				return true;
			}
			catch (Exception ex)
			{
				Reason = ex.Message;
				return false;
			}
		}

		/// <summary>
		/// The Base64URL-encoded header.
		/// </summary>
		public string Header => this.header;

		/// <summary>
		/// The Base64URL-encoded payload.
		/// </summary>
		public string Payload => this.payload;

		/// <summary>
		/// The Base64URL-encoded signature.
		/// </summary>
		public string Signature => this.signature;

		/// <summary>
		/// String token that can be embedded easily in web requests, etc.
		/// </summary>
		public string Token => this.token;

		/// <summary>
		/// JWS signature algoritm.
		/// </summary>
		public IJwsAlgorithm Algorithm => this.algorithm;

		/// <summary>
		/// Claims provided in token. For a list of public claim names, see:
		/// https://www.iana.org/assignments/jwt/jwt.xhtml
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> Claims => this.claims;

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
		public string Type => this.type;

		/// <summary>
		/// Issuer of token, if available, null otherwise.
		/// </summary>
		public string Issuer => this.issuer;

		/// <summary>
		/// Subject of whom the token relates, if available, null otherwise.
		/// </summary>
		public string Subject => this.subject;

		/// <summary>
		/// Token ID, if available, null otherwise.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Indended audience, if available, null otherwise.
		/// </summary>
		public string[] Audience => this.audience;

		/// <summary>
		/// Token expiration time, if available, null otherwise.
		/// </summary>
		public DateTime? Expiration => this.expiration;

		/// <summary>
		/// Token not valid before this time, if available, null otherwise.
		/// </summary>
		public DateTime? NotBefore => this.notBefore;

		/// <summary>
		/// Token issued at this time, if available, null otherwise.
		/// </summary>
		public DateTime? IssuedAt => this.issuedAt;
	}
}
