using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.Authentication;
using Waher.Security;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH Client Credentials authentication scheme, as defined in RFCs 6749.
	/// </summary>
	public class OAuthClientCredentialsAuthentication : HttpAuthenticationScheme
	{
		private readonly IUserSource users;

		/// <summary>
		/// OAUTH Client Credentials authentication scheme, as defined in RFCs 6749.
		/// </summary>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public OAuthClientCredentialsAuthentication(IUserSource Users)
			: this(false, 0, Users)
		{
		}

		/// <summary>
		/// OAUTH Client Credentials authentication scheme, as defined in RFCs 6749.
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="MinStrength">Minimum security strength of algorithms used.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public OAuthClientCredentialsAuthentication(bool RequireEncryption, int MinStrength, IUserSource Users)
			: base(RequireEncryption, MinStrength)
		{
			this.users = Users;
		}

		/// <summary>
		/// Collection of users to authenticate against.
		/// </summary>
		public IUserSource Users => this.users;

		/// <summary>
		/// Display name for authentication scheme.
		/// </summary>
		public override string DisplayName
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("OAUTH Client Credentials");
				this.AppendEncryptionRequirement(sb);

				return sb.ToString();
			}
		}

		/// <summary>
		/// Gets available challenges for the authenticating client to respond to.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>Challenge strings.</returns>
		public override string[] GetChallenges(HttpRequest Request)
		{
			return Array.Empty<string>();
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser?> IsAuthenticated(HttpRequest Request)
		{
			IUser? User;

			if (Request.Header.TryGetQueryParameter("client_id", out string ClientId) &&
				Request.Header.TryGetQueryParameter("client_secret", out string ClientSecret))
			{
				Log.Warning("Insecure authentication method used. Client credentials " +
					"should be sent in the Authorization header, not as query parameters.",
					Request.Header.ResourcePart, Request.RemoteEndPoint);

				User = await BasicAuthentication.IsAuthenticated(ClientId, ClientSecret, null, this.users, Request);
			}
			else if (Request.Header.TryGetQueryParameter("username", out ClientId) &&
				Request.Header.TryGetQueryParameter("password", out ClientSecret))
			{
				Log.Warning("Insecure authentication method used. Client credentials " +
					"should be sent in the Authorization header, not as query parameters.",
					Request.Header.ResourcePart, Request.RemoteEndPoint);

				User = await BasicAuthentication.IsAuthenticated(ClientId, ClientSecret, null, this.users, Request);
			}
			else if (Request.HasData)
			{
				ContentResponse Response = await Request.DecodeDataAsync();
				if (Response.HasError)
					return null;

				if (Response.Decoded is Dictionary<string, object> Data)
				{
					if (Data.TryGetValue("client_id", out object Obj) && Obj is string ClientId2 &&
						Data.TryGetValue("client_secret", out Obj) && Obj is string ClientSecret2)
					{
						User = await BasicAuthentication.IsAuthenticated(ClientId2, ClientSecret2, null, this.users, Request);
					}
					else if (Data.TryGetValue("username", out Obj) && Obj is string UserName &&
						Data.TryGetValue("password", out Obj) && Obj is string Password)
					{
						User = await BasicAuthentication.IsAuthenticated(UserName, Password, null, this.users, Request);
					}
					else 
						return null;
				}
				else if (Response.Decoded is Dictionary<string, string> Form)
				{
					if (Form.TryGetValue("client_id", out ClientId) &&
						Form.TryGetValue("client_secret", out ClientSecret))
					{
						User = await BasicAuthentication.IsAuthenticated(ClientId, ClientSecret, null, this.users, Request);
					}
					else if (Form.TryGetValue("username", out ClientId) &&
						Form.TryGetValue("password", out ClientSecret))
					{
						User = await BasicAuthentication.IsAuthenticated(ClientId, ClientSecret, null, this.users, Request);
					}
					else
						return null;
				}
				else
					return null;
			}
			else
				return null;

			return User;
		}
	}
}
