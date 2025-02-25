using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Settings;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by SCRAM-* defined in RFC 5802 &amp; 7677:
	/// https://tools.ietf.org/html/rfc5802
	/// https://tools.ietf.org/html/rfc7677
	/// </summary>
	public abstract class ScramAuthenticationMechanism : HashedAuthenticationMechanism
	{
		private static readonly byte[] clientKey = Encoding.UTF8.GetBytes("Client Key");
		private static readonly byte[] serverKey = Encoding.UTF8.GetBytes("Server Key");

		private static byte[] salt = null;
		private static string saltBase64 = null;

		/// <summary>
		/// Authentication done by SCRAM-* defined in RFC 5802 &amp; 7677:
		/// https://tools.ietf.org/html/rfc5802
		/// https://tools.ietf.org/html/rfc7677
		/// </summary>
		public ScramAuthenticationMechanism()
		{
		}

		/// <summary>
		/// Checks if a mechanism is allowed during the current conditions.
		/// </summary>
		/// <param name="SslStream">SSL stream, if available.</param>
		/// <returns>If mechanism is allowed.</returns>
		public override bool Allowed(SslStream SslStream)
		{
			return true;
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
			byte[] Bin = Convert.FromBase64String(Data);
			string Request = Encoding.UTF8.GetString(Bin);
			string UserName = null;
			string Nonce = null;
			int NrIterations = 4096;

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(Request))
			{
				switch (Pair.Key.ToLower())
				{
					case "n":
						UserName = Pair.Value;
						break;

					case "r":
						Nonce = Pair.Value;
						break;
				}
			}

			if (UserName is null || Nonce is null)
			{
				Connection.SaslErrorMalformedRequest();
				LoginAuditor.Fail("Login attempt using malformed request.", UserName, Connection.RemoteEndPoint, Connection.Protocol);
				return Task.FromResult<bool?>(null);
			}

			Connection.SetUserIdentity(UserName);

			string ServerNonce = Nonce + Convert.ToBase64String(PersistenceLayer.GetRandomNumbers(32));

			string Challenge = "r=" + ServerNonce + ",s=" + saltBase64 + ",i=" + NrIterations.ToString();
			Bin = Encoding.UTF8.GetBytes(Challenge);
			string ChallengeBase64 = Convert.ToBase64String(Bin);

			Connection.Tag = new object[] { UserName, Nonce, NrIterations, ServerNonce, Request, Challenge };
			Connection.SaslChallenge(ChallengeBase64);

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
			string Request = Encoding.UTF8.GetString(Bin);
			object[] P = (object[])Connection.Tag;
			string UserName = (string)P[0];
			//string Nonce = (string)P[1];
			int NrIterations = (int)P[2];
			string ServerNonce = (string)P[3];
			string ClientRequest = (string)P[4];
			string ServerChallenge = (string)P[5];
			string c;
			string Proof = null;
			bool ServerNonceChecked = false;

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(Request))
			{
				switch (Pair.Key.ToLower())
				{
					case "c":
						c = Pair.Value;
						break;

					case "r":
						ServerNonceChecked = true;
						if (ServerNonce != Pair.Value)
						{
							await Connection.SaslErrorMalformedRequest();
							LoginAuditor.Fail("Login attempt using malformed request.", UserName, Connection.RemoteEndPoint, Connection.Protocol);
							return null;
						}
						break;

					case "p":
						Proof = Pair.Value;
						break;
				}
			}

			if (!ServerNonceChecked)
			{
				await Connection.SaslErrorMalformedRequest();
				LoginAuditor.Fail("Login attempt using malformed request.", UserName, Connection.RemoteEndPoint, Connection.Protocol);
				return null;
			}

			IAccount Account = await PersistenceLayer.GetAccount(UserName);
			if (Account is null)
			{
				LoginAuditor.Fail("Login attempt using invalid user name.", UserName, Connection.RemoteEndPoint, Connection.Protocol,
					new KeyValuePair<string, object>("UserName", UserName));
				await Connection.SaslErrorNotAuthorized();
				return null;
			}

			if (!Account.Enabled)
			{
				LoginAuditor.Fail("Login attempt using disabled account.", UserName, Connection.RemoteEndPoint, Connection.Protocol);
				await Connection.SaslErrorAccountDisabled();
				return null;
			}

			byte[] SaltedPassword = this.Hi(Encoding.UTF8.GetBytes(Account.Password.Normalize()), salt, NrIterations);
			byte[] ClientKey = this.HMAC(SaltedPassword, clientKey);
			byte[] StoredKey = this.H(ClientKey);
			StringBuilder sb;

			sb = new StringBuilder();

			int i;

			i = ClientRequest.IndexOf("n=");
			if (i < 0)
				sb.Append(ClientRequest);
			else
				sb.Append(ClientRequest.Substring(i));

			sb.Append(',');
			sb.Append(ServerChallenge);
			sb.Append(',');

			i = Request.IndexOf(",p=");
			if (i < 0)
				sb.Append(Request);
			else
				sb.Append(Request.Substring(0, i));

			byte[] AuthenticationMessage = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] ClientSignature = this.HMAC(StoredKey, AuthenticationMessage);
			byte[] ClientProof = XOR(ClientKey, ClientSignature);

			byte[] ServerKey = this.HMAC(SaltedPassword, serverKey);
			byte[] ServerSignature = this.HMAC(ServerKey, AuthenticationMessage);

			string ClientProofStr = Convert.ToBase64String(ClientProof);
			if (Proof == ClientProofStr)
			{
				await Connection.SetAccount(Account);

				string Response = "v=" + Convert.ToBase64String(ServerSignature);
				Response = Convert.ToBase64String(Encoding.UTF8.GetBytes(Response));

				Connection.ResetState(true);
				await Connection.SaslSuccess(Response);
				LoginAuditor.Success("Login successful.", UserName, Connection.RemoteEndPoint, Connection.Protocol);
			}
			else
			{
				await Connection.SaslErrorNotAuthorized();
				LoginAuditor.Fail("Login attempt failed.", UserName, Connection.RemoteEndPoint, Connection.Protocol);
			}

			return null;
		}

		private byte[] Hi(byte[] String, byte[] Salt, int NrIterations)
		{
			byte[] U1 = this.HMAC(String, CONCAT(Salt, One));
			byte[] U2 = this.HMAC(String, U1);
			byte[] Response = XOR(U1, U2);

			while (NrIterations > 2)
			{
				U1 = U2;
				U2 = this.HMAC(String, U1);
				Response = XOR(Response, U2);
				NrIterations--;
			}

			return Response;
		}

		private static readonly byte[] One = new byte[] { 0, 0, 0, 1 };

		/// <summary>
		/// Performs intitialization of the mechanism. Can be used to set
		/// static properties that will be used through-out the runtime of the
		/// server.
		/// </summary>
		public override async Task Initialize()
		{
			saltBase64 = await GetSaltBase64("XMPP.SCRAM.Server.Salt");
			salt = Convert.FromBase64String(saltBase64);
		}

		/// <summary>
		/// Gets a salt value, given a key.
		/// </summary>
		/// <param name="Key">Salt key.</param>
		/// <returns>Base64-encoded salt value.§</returns>
		public static async Task<string> GetSaltBase64(string Key)
		{
			string Result = await RuntimeSettings.GetAsync(Key, string.Empty);
			if (string.IsNullOrEmpty(Result))
			{
				byte[] Bin = new byte[32];

				using (RandomNumberGenerator Rnd = RandomNumberGenerator.Create())
				{
					Rnd.GetBytes(Bin);
				}

				Result = Convert.ToBase64String(Bin);
				await RuntimeSettings.SetAsync(Key, Result);
			}

			return Result;
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
			string Nonce = Convert.ToBase64String(GetRandomBytes(16));
			string s = "n,,n=" + UserName + ",r=" + Nonce;
			byte[] Data = Encoding.UTF8.GetBytes(s);
			string Challenge = await Connection.Initiate(this, Convert.ToBase64String(Data));
			byte[] ChallengeBinary = Convert.FromBase64String(Challenge);
			string ChallengeString = Encoding.UTF8.GetString(ChallengeBinary);
			string ServerNonce = null;
			string SaltString;
			int NrIterations = 0;
			byte[] Salt = null;

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(ChallengeString))
			{
				switch (Pair.Key.ToLower())
				{
					case "r":
						ServerNonce = Pair.Value;
						break;

					case "s":
						SaltString = Pair.Value;
						Salt = Convert.FromBase64String(SaltString);
						break;

					case "i":
						NrIterations = int.Parse(Pair.Value);
						break;
				}
			}

			if (string.IsNullOrEmpty(ServerNonce) || !ServerNonce.StartsWith(Nonce) || Salt is null || NrIterations <= 0)
				return null;

			byte[] SaltedPassword = this.Hi(Encoding.UTF8.GetBytes(Password.Normalize()), Salt, NrIterations);

			byte[] ClientKey = this.HMAC(SaltedPassword, Encoding.UTF8.GetBytes("Client Key"));
			byte[] StoredKey = this.H(ClientKey);

			StringBuilder sb;

			sb = new StringBuilder();
			sb.Append("n=");
			sb.Append(UserName);
			sb.Append(",r=");
			sb.Append(Nonce);
			sb.Append(',');
			sb.Append(ChallengeString);
			sb.Append(",c=biws,r=");
			sb.Append(ServerNonce);

			byte[] AuthenticationMessage = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] ClientSignature = this.HMAC(StoredKey, AuthenticationMessage);
			byte[] ClientProof = XOR(ClientKey, ClientSignature);

			byte[] ServerKey = this.HMAC(SaltedPassword, Encoding.UTF8.GetBytes("Server Key"));
			byte[] ServerSignatureBinary = this.HMAC(ServerKey, AuthenticationMessage);

			string ServerSignature = Convert.ToBase64String(ServerSignatureBinary);

			sb = new StringBuilder();
			sb.Append("c=biws,r=");     // biws="n,,"
			sb.Append(ServerNonce);
			sb.Append(",p=");
			sb.Append(Convert.ToBase64String(ClientProof));

			string Success = await Connection.FinalResponse(this, Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())));
			if (string.IsNullOrEmpty(Success))
				return true;

			byte[] ResponseBinary = Convert.FromBase64String(Success);
			string ResponseString = Encoding.UTF8.GetString(ResponseBinary);

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(ResponseString))
			{
				if (string.Compare(Pair.Key, "v", true) == 0)
					return (Pair.Value == ServerSignature);
			}

			return false;
		}

	}
}
