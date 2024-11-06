using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by the LOGIN authentication mechanism.
	/// https://tools.ietf.org/html/draft-murchison-sasl-login-00
	/// </summary>
	public class Login : AuthenticationMechanism
	{
		/// <summary>
		/// Authentication done by the LOGIN authentication mechanism.
		/// https://tools.ietf.org/html/draft-murchison-sasl-login-00
		/// </summary>
		public Login()
		{
		}

		/// <summary>
		/// Name of the mechanism.
		/// </summary>
		public override string Name
		{
			get { return "LOGIN"; }
		}

		/// <summary>
		/// Weight of mechanisms. The higher the value, the more preferred.
		/// </summary>
		public override int Weight => 0;

		/// <summary>
		/// Checks if a mechanism is allowed during the current conditions.
		/// </summary>
		/// <param name="SslStream">SSL stream, if available.</param>
		/// <returns>If mechanism is allowed.</returns>
		public override bool Allowed(SslStream SslStream)
		{
			return (!(SslStream is null) && SslStream.IsEncrypted && SslStream.CipherStrength >= 128);
		}

		/// <summary>
		/// Authentication request has been made.
		/// </summary>
		/// <param name="Data">Data in authentication request.</param>
		/// <param name="Connection">Connection performing the authentication.</param>
		/// <param name="PersistenceLayer">Persistence layer.</param>
		/// <returns>If authentication was successful (true). If null, mechanism must send the corresponding challenge. If false, connection must close.</returns>
		public override Task<bool?> AuthenticationRequest(string Data, ISaslServerSide Connection, ISaslPersistenceLayer PersistenceLayer)
		{
			Connection.Tag = null;
			return this.ResponseRequest(Data, Connection, PersistenceLayer);
		}

		/// <summary>
		/// Response request has been made.
		/// </summary>
		/// <param name="Data">Data in response request.</param>
		/// <param name="Connection">Connection performing the authentication.</param>
		/// <param name="PersistenceLayer">Persistence layer.</param>
		/// <returns>If authentication was successful (true). If null, mechanism must send the corresponding error. If false, connection must close.</returns>
		public override async Task<bool?> ResponseRequest(string Data, ISaslServerSide Connection, ISaslPersistenceLayer PersistenceLayer)
		{
			if (string.IsNullOrEmpty(Data))
			{
				await Connection.SaslChallenge(Convert.ToBase64String(Encoding.UTF8.GetBytes("User name\x00")));
				return null;
			}

			byte[] Bin = Convert.FromBase64String(Data);
			string s = Encoding.UTF8.GetString(Bin);

			if (Connection.Tag is null)
			{
				Connection.Tag = s;   // User name
				await Connection.SaslChallenge(Convert.ToBase64String(Encoding.UTF8.GetBytes("Password\x00")));

				await Connection.SetUserIdentity(s);
				return null;
			}

			string UserName = (string)Connection.Tag;
			IAccount Account = await PersistenceLayer.GetAccount(UserName);
			if (Account is null)
			{
				LoginAuditor.Fail("Login attempt using invalid user name.", UserName, Connection.RemoteEndpoint, Connection.Protocol,
					new KeyValuePair<string, object>("UserName", UserName));
				await Connection.SaslErrorNotAuthorized();
				return null;
			}

			if (!Account.Enabled)
			{
				LoginAuditor.Fail("Login attempt using disabled account.", UserName, Connection.RemoteEndpoint, Connection.Protocol);
				await Connection.SaslErrorAccountDisabled();
				return null;
			}

			if (s == Account.Password)
			{
				await Connection.SetAccount(Account);
				Connection.ResetState(true);
				await Connection.SaslSuccess(null);

				LoginAuditor.Success("Login successful.", UserName, Connection.RemoteEndpoint, Connection.Protocol);
			}
			else
			{
				await Connection.SaslErrorNotAuthorized();
				LoginAuditor.Fail("Login attempt failed.", UserName, Connection.RemoteEndpoint, Connection.Protocol);
			}

			return null;
		}

		/// <summary>
		/// Performs intitialization of the mechanism. Can be used to set
		/// static properties that will be used through-out the runtime of the
		/// server.
		/// </summary>
		public override Task Initialize()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Authenticates the user using the provided credentials.
		/// </summary>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Connection">Connection</param>
		/// <returns>If authentication was successful or not. If null is returned, the mechanism did not perform authentication.</returns>
		public override async Task<bool?> Authenticate(string UserName, string Password, ISaslClientSide Connection)
		{
			/* string UserNameLabel = */ await Connection.Initiate(this, null);
			/* string PasswordLabel = */ await Connection.ChallengeResponse(this, Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName)));
			await Connection.FinalResponse(this, Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));
			return true;
		}

	}
}
