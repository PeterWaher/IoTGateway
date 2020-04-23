using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Events;
using Waher.Security;
using Waher.Networking.HTTP.HeaderFields;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// Basic authentication mechanism, as defined in RFC 2617:
	/// https://tools.ietf.org/html/rfc2617
	/// </summary>
	public class BasicAuthentication : HttpAuthenticationScheme
	{
		private readonly IUserSource users;
		private readonly string realm;

		/// <summary>
		/// Basic authentication mechanism, as defined in RFC 2617:
		/// https://tools.ietf.org/html/rfc2617
		/// </summary>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public BasicAuthentication(string Realm, IUserSource Users)
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
			HttpFieldAuthorization Authorization = Request.Header.Authorization;
			if (Authorization != null && Authorization.Value.StartsWith("Basic ", StringComparison.CurrentCultureIgnoreCase))
			{
				byte[] Data = Convert.FromBase64String(Authorization.Value.Substring(6).Trim());
				string s = InternetContent.ISO_8859_1.GetString(Data);
				int i = s.IndexOf(':');
				if (i > 0)
				{
					string UserName = s.Substring(0, i);
					string Password = s.Substring(i + 1);

					if (!this.users.TryGetUser(UserName, out User))
					{
						Log.Notice("Login attempt using invalid user name.", UserName, Request.RemoteEndPoint, "LoginFailure",
							EventLevel.Minor, new KeyValuePair<string, object>("Protocol", "HTTP"));

						return false;
					}

					switch (User.PasswordHashType)
					{
						case "":
							break;

						case "DIGEST-MD5":
							Password = DigestAuthentication.ToHex(DigestAuthentication.H(UserName + ":" + this.realm + ":" + Password));
							break;

						default:
							User = null;
							return false;
					}

					if (Password == User.PasswordHash)
					{
						Log.Informational("Login successful.", UserName, Request.RemoteEndPoint, "LoginSuccessful",
							EventLevel.Minor, new KeyValuePair<string, object>("Protocol", "HTTP"));

						return true;
					}
					else
					{
						Log.Notice("Login attempt failed.", UserName, Request.RemoteEndPoint, "LoginFailure",
							EventLevel.Minor, new KeyValuePair<string, object>("Protocol", "HTTP"));

						User = null;
						return false;
					}
				}
			}

			User = null;
			return false;
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			return "Basic realm=\"" + this.realm + "\"";
		}
	}
}
