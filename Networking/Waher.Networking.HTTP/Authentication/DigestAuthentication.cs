using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// Delegate to hash function.
	/// </summary>
	/// <param name="Data">Data to be hashed.</param>
	/// <returns>Hash digest</returns>
	public delegate byte[] HashFunctionString(string Data);

	/// <summary>
	/// Digest algorithm
	/// </summary>
	public enum DigestAlgorithm
	{
		/// <summary>
		/// MD5 - obsolete
		/// </summary>
		MD5,

		/// <summary>
		/// SHA-256
		/// </summary>
		SHA256
	}

	/// <summary>
	/// Digest authentication mechanism, as defined in RFC 2617:
	/// https://tools.ietf.org/html/rfc2617
	/// </summary>
	public class DigestAuthentication : HttpAuthenticationScheme
	{
		private readonly Dictionary<string, DateTime> expirationByNonce = new Dictionary<string, DateTime>();
		private readonly SortedDictionary<DateTime, string> nonceByExpiration = new SortedDictionary<DateTime, string>();
		private readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private readonly string opaque;
		private readonly IUserSource users;
		private readonly string realm;
		private readonly DigestAlgorithm algorithm;
		private readonly int digestBytes;
		private readonly HashFunctionString h;

		/// <summary>
		/// Digest authentication mechanism, as defined in RFC 2617:
		/// https://tools.ietf.org/html/rfc2617
		/// </summary>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public DigestAuthentication(string Realm, IUserSource Users)
#if WINDOWS_UWP
			: this(false, DigestAlgorithm.MD5, Realm, Users)
#else
			: this(false, 0, DigestAlgorithm.MD5, Realm, Users)
#endif
		{
		}

#if WINDOWS_UWP
		/// <summary>
		/// Digest authentication mechanism, as defined in RFC 2617:
		/// https://tools.ietf.org/html/rfc2617
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="Algorithm">Digest Algorithm to use.</param>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public DigestAuthentication(bool RequireEncryption, DigestAlgorithm Algorithm, string Realm, IUserSource Users)
			: base(RequireEncryption)
#else
		/// <summary>
		/// Digest authentication mechanism, as defined in RFC 2617:
		/// https://tools.ietf.org/html/rfc2617
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="MinStrength">Minimum security strength of algorithms used.</param>
		/// <param name="Algorithm">Digest Algorithm to use.</param>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public DigestAuthentication(bool RequireEncryption, int MinStrength, DigestAlgorithm Algorithm, string Realm, IUserSource Users)
			: base(RequireEncryption, MinStrength)
#endif
		{
			this.algorithm = Algorithm;
			this.realm = Realm;
			this.users = Users;

			switch (Algorithm)
			{
				case DigestAlgorithm.MD5:
					this.digestBytes = 16;
					this.h = H_MD5;
					break;

				case DigestAlgorithm.SHA256:
				default:
					this.digestBytes = 32;
					this.h = H_SHA256;
					break;
			}

			this.opaque = this.NextRandomString(this.digestBytes);
		}

		private byte[] NextBytes(int Nr)
		{
			byte[] Bin = new byte[Nr];

			lock (this.rnd)
			{
				this.rnd.GetBytes(Bin);
			}

			return Bin;
		}

		private string NextRandomString(int NrBytes)
		{
			byte[] Bin = this.NextBytes(NrBytes);
			return Convert.ToBase64String(Bin);
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			string Nonce = this.NextRandomString(this.digestBytes);
			DateTime Expires = DateTime.Now.AddMinutes(1);

			while (true)
			{
				lock (this)
				{
					if (!this.nonceByExpiration.ContainsKey(Expires))
					{
						this.expirationByNonce[Nonce] = Expires;
						this.nonceByExpiration[Expires] = Nonce;
						break;
					}
				}

				byte[] b = this.NextBytes(1);

				Expires = Expires.AddTicks(b[0] & 15);
			}

			StringBuilder sb = new StringBuilder();

			sb.Append("Digest realm=\"");
			sb.Append(this.realm);
			sb.Append("\", qop=\"auth,auth-int\", algorithm=");

			switch (this.algorithm)
			{
				case DigestAlgorithm.MD5:
					sb.Append("MD5");
					break;

				case DigestAlgorithm.SHA256:
					sb.Append("SHA-256");
					break;
			}

			sb.Append(", nonce=\"");
			sb.Append(Nonce);
			sb.Append("\", opaque=\"");
			sb.Append(this.opaque);
			sb.Append("\"");

			return sb.ToString();
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			HttpFieldAuthorization Authorization = Request.Header.Authorization;
			if (Authorization != null && Authorization.Value.StartsWith("Digest ", StringComparison.CurrentCultureIgnoreCase))
			{
				HashFunctionString H = H_MD5;
				DigestAlgorithm Algorithm = DigestAlgorithm.MD5;
				string UserName = null;
				string Opaque = null;
				string Realm = null;
				string Nonce = null;
				string Cnonce = null;
				string Nc = null;
				string Uri = null;
				string Qop = null;
				string Response = null;
				bool Auth = false;
				bool AuthInt = false;

				foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(Authorization.Value.Substring(7)))
				{
					switch (P.Key.ToLower())
					{
						case "username":
							UserName = P.Value;
							break;

						case "opaque":
							Opaque = P.Value;
							break;

						case "realm":
							Realm = P.Value;
							break;

						case "nonce":
							Nonce = P.Value;
							break;

						case "cnonce":
							Cnonce = P.Value;
							break;

						case "nc":
							Nc = P.Value;
							break;

						case "uri":
							Uri = P.Value;
							break;

						case "qop":
							Qop = P.Value;
							string[] QopItems = Qop.Split(',');

							Auth = (Array.IndexOf(QopItems, "auth") >= 0);
							AuthInt = (Array.IndexOf(QopItems, "auth-int") >= 0);
							break;

						case "response":
							Response = P.Value;
							break;

						case "algorithm":
							switch (P.Value)
							{
								case "MD5":
									H = H_MD5;
									Algorithm = DigestAlgorithm.MD5;
									break;

								case "SHA-256":
									H = H_SHA256;
									Algorithm = DigestAlgorithm.SHA256;
									break;

								default:
									return null;
							}
							break;
					}
				}

				if (this.realm != Realm || Qop is null || Nonce is null || Cnonce is null || Nc is null ||
					Uri is null || Response is null || UserName is null || (!Auth && !AuthInt))
				{
					return null;
				}

				if (this.opaque != Opaque)
				{
					// We need to ignore the opaque value if it's a POST from a web form, since it can be used from the original GET
					// (which might have ocurred when another instance of the application ran).

					if (Request.Header.Method != "POST" || Request.Header.ContentType.Value != "application/x-www-form-urlencoded")
						return null;
				}

				DateTime TP = DateTime.Now;

				lock (this)
				{
					LinkedList<DateTime> ToRemove = null;

					foreach (KeyValuePair<DateTime, string> Pair in this.nonceByExpiration)
					{
						if (Pair.Key <= TP)
						{
							if (ToRemove is null)
								ToRemove = new LinkedList<DateTime>();

							ToRemove.AddLast(Pair.Key);
							this.expirationByNonce.Remove(Pair.Value);
						}
						else
							break;
					}

					if (!(ToRemove is null))
					{
						foreach (DateTime ExpiryDate in ToRemove)
							this.nonceByExpiration.Remove(ExpiryDate);
					}

					if (!this.expirationByNonce.TryGetValue(Nonce, out TP))
					{
						// We need to ignore the nonce value if it's a POST from a web form, since it can be used from the original GET
						// (which might have ocurred when another instance of the application ran).

						if (Request.Header.Method != "POST" || Request.Header.ContentType.Value != "application/x-www-form-urlencoded")
							return null;
					}

					if (Request.Header.Method != "HEAD")
					{
						this.expirationByNonce.Remove(Nonce);
						this.nonceByExpiration.Remove(TP);
					}
				}

				string HA1;
				string HA2;
				string Digest;

				IUser User = await this.users.TryGetUser(UserName);
				if (User is null)
					return null;

				switch (User.PasswordHashType)
				{
					case "":
						HA1 = ToHex(H(UserName + ":" + this.realm + ":" + User.PasswordHash));
						break;

					case "DIGEST-MD5":
						if (Algorithm == DigestAlgorithm.MD5)
							HA1 = User.PasswordHash;
						else
							return null;
						break;

					case "DIGEST-SHA-256":
						if (Algorithm == DigestAlgorithm.SHA256)
							HA1 = User.PasswordHash;
						else
							return null;
						break;

					default:
						return null;
				}

				if (AuthInt)
					HA2 = ToHex(H(Request.Header.Method + ":" + Uri + ":" + ToHex(H(string.Empty))));
				else
					HA2 = ToHex(H(Request.Header.Method + ":" + Uri));

				Digest = HA1 + ":" + Nonce + ":" + Nc + ":" + Cnonce + ":" + Qop + ":" + HA2;
				Digest = ToHex(H(Digest));

				if (Digest == Response)
				{
					LoginAuditor.Success("Login successful.", UserName, Request.RemoteEndPoint, "HTTP");
					return User;
				}
				else
				{
					LoginAuditor.Fail("Login attempt failed.", UserName, Request.RemoteEndPoint, "HTTP");
					return null;
				}
			}

			return null;
		}

		internal static byte[] H_MD5(string s)
		{
			return Hashes.ComputeMD5Hash(InternetContent.ISO_8859_1.GetBytes(s));
		}

		internal static byte[] H_SHA256(string s)
		{
			return Hashes.ComputeSHA256Hash(InternetContent.ISO_8859_1.GetBytes(s));
		}

		internal static string ToHex(byte[] Hash)
		{
			StringBuilder Result = new StringBuilder();

			foreach (byte b in Hash)
				Result.Append(b.ToString("x2"));

			return Result.ToString();
		}

	}
}
