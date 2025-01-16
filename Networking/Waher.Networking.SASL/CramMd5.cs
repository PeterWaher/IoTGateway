using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by CRAM-MD5 defined in RFC 2195:
	/// https://tools.ietf.org/html/rfc2195
	/// </summary>
	public class CramMd5 : Md5AuthenticationMechanism
	{
		/// <summary>
		/// Authentication done by CRAM-MD5 defined in RFC 2195:
		/// https://tools.ietf.org/html/rfc2195
		/// </summary>
		public CramMd5()
		{
		}

		/// <summary>
		/// Name of the mechanism.
		/// </summary>
		public override string Name
		{
			get { return "CRAM-MD5"; }
		}

		/// <summary>
		/// Weight of mechanisms. The higher the value, the more preferred.
		/// </summary>
		public override int Weight => 100;

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
			byte[] Nonce = PersistenceLayer.GetRandomNumbers(32);

			Connection.Tag = new object[] { Nonce };
			Connection.SaslChallenge(Convert.ToBase64String(Nonce));

			return Task.FromResult<bool?>(null);
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
			byte[] Bin = Convert.FromBase64String(Data);
			string Cram = Encoding.UTF8.GetString(Bin);
			object[] P = (object[])Connection.Tag;
			byte[] Nonce = (byte[])P[0];
			int i = Cram.LastIndexOf(' ');
			string UserName = string.Empty;

			if (i > 0)
			{
				UserName = Cram.Substring(0, i);
				Cram = Cram.Substring(i + 1);
			}

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

			Connection.SetUserIdentity(UserName);

			byte[] HMAC = this.HMAC(Encoding.UTF8.GetBytes(Account.Password), Nonce);
			string Cram2 = HEX(HMAC);

			if (Cram == Cram2)
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
			string Challenge = await Connection.Initiate(this, null);
			byte[] ChallengeBinary = Convert.FromBase64String(Challenge);

			byte[] HMAC = this.HMAC(Encoding.UTF8.GetBytes(Password), ChallengeBinary);
			string CRAM = UserName + " " + HEX(HMAC);

			await Connection.FinalResponse(this, Convert.ToBase64String(Encoding.UTF8.GetBytes(CRAM)));
			return true;
		}

	}
}
