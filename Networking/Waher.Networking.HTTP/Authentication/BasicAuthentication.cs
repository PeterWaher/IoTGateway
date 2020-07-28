using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Security.LoginMonitor;

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
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			return "Basic realm=\"" + this.realm + "\"";
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser> IsAuthenticated(HttpRequest Request)
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

					IUser User = await this.users.TryGetUser(UserName);
					if (User is null)
					{
						LoginAuditor.Fail("Login attempt using invalid user name.", UserName, Request.RemoteEndPoint, "HTTP");
						return null;
					}

					switch (User.PasswordHashType)
					{
						case "":
							break;

						case "DIGEST-MD5":
							Password = DigestAuthentication.ToHex(DigestAuthentication.H(UserName + ":" + this.realm + ":" + Password));
							break;

						default:
							return null;
					}

					if (Password == User.PasswordHash)
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
			}

			return null;
		}
	}
}
