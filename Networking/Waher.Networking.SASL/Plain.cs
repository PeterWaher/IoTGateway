using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by the PLAIN authentication mechanism.
	/// https://tools.ietf.org/html/rfc4616
	/// </summary>
	public class Plain : AuthenticationMechanism
    {
		/// <summary>
		/// Authentication done by the PLAIN authentication mechanism.
		/// https://tools.ietf.org/html/rfc4616
		/// </summary>
		public Plain()
        {
        }

        /// <summary>
        /// Name of the mechanism.
        /// </summary>
        public override string Name
        {
            get { return "PLAIN"; }
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
			return (SslStream != null && SslStream.IsEncrypted && SslStream.CipherStrength >= 128);
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
			if (string.IsNullOrEmpty(Data))
			{
				Connection.SaslChallenge(string.Empty);
				return Task.FromResult<bool?>(null);
			}

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
            byte[] Bin = Convert.FromBase64String(Data);
            string Request = Encoding.UTF8.GetString(Bin);
			string[] Parts = Request.Split('\x00');

			if (Parts.Length != 3)
            {
				await Connection.SaslErrorMalformedRequest();
				LoginAuditor.Fail("Login attempt using malformed request.", string.Empty, Connection.RemoteEndpoint, Connection.Protocol);
				return null;
            }

			string UserName = Parts[1];
			string Password = Parts[2];

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

			await Connection.SetUserIdentity(UserName);

			if (Password == Account.Password)
			{
				Connection.SetAccount(Account);
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
			using (MemoryStream ms = new MemoryStream())
			{
				byte[] Bin = Encoding.UTF8.GetBytes(UserName);
				ms.Write(Bin, 0, Bin.Length);
				ms.WriteByte(0);

				ms.Write(Bin, 0, Bin.Length);
				ms.WriteByte(0);

				Bin = Encoding.UTF8.GetBytes(Password);
				ms.Write(Bin, 0, Bin.Length);

				await Connection.Initiate(this, Convert.ToBase64String(ms.ToArray()));
			}

			return true;
		}

	}
}
