using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Waher.Content;
using Waher.Security;
using Waher.Networking.HTTP.HeaderFields;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// Digest authentication mechanism, as defined in RFC 2617:
	/// https://tools.ietf.org/html/rfc2617
	/// </summary>
	public class DigestAuthentication : HttpAuthenticationScheme
	{
		private readonly Dictionary<string, DateTime> expirationByNonce = new Dictionary<string, DateTime>();
		private readonly SortedDictionary<DateTime, string> nonceByExpiration = new SortedDictionary<DateTime, string>();
		private Random rnd = new Random();
		private readonly string opaque = Guid.NewGuid().ToString().Replace("-", string.Empty);
		private readonly IUserSource users;
		private readonly string realm;

		/// <summary>
		/// Digest authentication mechanism, as defined in RFC 2617:
		/// https://tools.ietf.org/html/rfc2617
		/// </summary>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public DigestAuthentication(string Realm, IUserSource Users)
		{
			this.realm = Realm;
			this.users = Users;
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="User">User object, if authenticated.</param>
		/// <returns>If the request is authorized.</returns>
		public override bool IsAuthenticated(HttpRequest Request, out IUser User)
		{
			User = null;

			HttpFieldAuthorization Authorization = Request.Header.Authorization;
			if (Authorization != null && Authorization.Value.StartsWith("Digest ", StringComparison.CurrentCultureIgnoreCase))
			{
				string UserName = null;
				string Opaque = null;
				string Realm = null;
				string Nonce = null;
				string Cnonce = null;
				string Nc = null;
				string Uri = null;
				string Qop = null;
				string[] QopItems = null;
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
							QopItems = Qop.Split(',');

							Auth = (Array.IndexOf(QopItems, "auth") >= 0);
							AuthInt = (Array.IndexOf(QopItems, "auth-int") >= 0);
							break;

						case "response":
							Response = P.Value;
							break;
					}
				}

				if (this.realm != Realm || Qop is null || Nonce is null || Cnonce is null || Nc is null ||
					Uri is null || Response is null || UserName is null || (!Auth && !AuthInt))
				{
					return false;
				}

				if (this.opaque != Opaque)
				{
					// We need to ignore the opaque value if it's a POST from a web form, since it can be used from the original GET
					// (which might have ocurred when another instance of the application ran).

					if (Request.Header.Method != "POST" || Request.Header.ContentType.Value != "application/x-www-form-urlencoded")
						return false;
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

					if (ToRemove != null)
					{
						foreach (DateTime ExpiryDate in ToRemove)
							this.nonceByExpiration.Remove(ExpiryDate);
					}

					if (!this.expirationByNonce.TryGetValue(Nonce, out TP))
					{
						// We need to ignore the nonce value if it's a POST from a web form, since it can be used from the original GET
						// (which might have ocurred when another instance of the application ran).

						if (Request.Header.Method != "POST" || Request.Header.ContentType.Value != "application/x-www-form-urlencoded")
							return false;
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

				if (!this.users.TryGetUser(UserName, out User))
					return false;

				switch (User.PasswordHashType)
				{
					case "":
						HA1 = ToHex(H(UserName + ":" + this.realm + ":" + User.PasswordHash));
						break;

					case "DIGEST-MD5":
						HA1 = User.PasswordHash;
						break;

					default:
						User = null;
						return false;
				}

				if (AuthInt)
					HA2 = ToHex(H(Request.Header.Method + ":" + Uri + ":" + ToHex(H(string.Empty))));
				else
					HA2 = ToHex(H(Request.Header.Method + ":" + Uri));

				Digest = HA1 + ":" + Nonce + ":" + Nc + ":" + Cnonce + ":" + Qop + ":" + HA2;
				Digest = ToHex(H(Digest));

				if (Digest == Response)
					return true;

				User = null;
			}

			return false;
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			string Nonce = Guid.NewGuid().ToString().Replace("-", string.Empty);
			DateTime Expires = DateTime.Now.AddMinutes(1);

			lock (this)
			{
				while (this.nonceByExpiration.ContainsKey(Expires))
					Expires = Expires.AddTicks(rnd.Next(1, 10));

				this.expirationByNonce[Nonce] = Expires;
				this.nonceByExpiration[Expires] = Nonce;
			}

			return "Digest realm=\"" + this.realm + "\", qop=\"auth,auth-int\", nonce=\"" + Nonce + "\", opaque=\"" + this.opaque + "\"";
		}

		internal static byte[] H(byte[] Data)
		{
			return Hashes.ComputeMD5Hash(Data);
		}

		internal static byte[] H(string s)
		{
			return H(InternetContent.ISO_8859_1.GetBytes(s));
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
