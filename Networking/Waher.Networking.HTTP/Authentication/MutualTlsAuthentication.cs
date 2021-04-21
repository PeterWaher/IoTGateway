#if !WINDOWS_UWP

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Security;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// mTLS authentication mechanism, where identity is taken from a valid client certificate.
	/// </summary>
	public class MutualTlsAuthentication : HttpAuthenticationScheme
	{
		private readonly IUserSource users;

		/// <summary>
		/// mTLS authentication mechanism, where identity is taken from a valid client certificate.
		/// </summary>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public MutualTlsAuthentication(IUserSource Users)
			: this(0, Users)
		{
		}

		/// <summary>
		/// mTLS authentication mechanism, where identity is taken from a valid client certificate.
		/// </summary>
		/// <param name="MinStrength">Minimum security strength of algorithms used.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		public MutualTlsAuthentication(int MinStrength, IUserSource Users)
			: base(true, MinStrength)
		{
			this.users = Users;
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			return null;
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			HttpClientConnection Connection = Request.clientConnection;
			if (Connection is null)
				return null;

			BinaryTcpClient Client = Connection.Client;
			if (Client is null)
				return null;

			X509Certificate Certificate = Client.RemoteCertificate;
			if (Certificate is null)
				return null;

			string UserName = Certificate.Subject;

			if (!Client.RemoteCertificateValid)
			{
				LoginAuditor.Fail("Login attempt failed.", UserName, Request.RemoteEndPoint, "HTTP");
				return null;
			}

			IUser User = await this.users.TryGetUser(UserName);
			if (User is null)
			{
				LoginAuditor.Fail("Login attempt using invalid user name.", Certificate.Subject, Request.RemoteEndPoint, "HTTP");
				return null;
			}
			else
			{
				LoginAuditor.Success("Login successful.", Certificate.Subject, Request.RemoteEndPoint, "HTTP");
				return User;
			}
		}

	}
}
#endif
