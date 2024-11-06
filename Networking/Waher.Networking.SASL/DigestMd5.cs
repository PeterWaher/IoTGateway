using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Waher.Security.LoginMonitor;

namespace Waher.Networking.SASL
{
	/// <summary>
	/// Authentication done by DIGEST-MD5 defined in RFC 2831:
	/// https://tools.ietf.org/html/rfc2831
	/// </summary>
	public class DigestMd5 : Md5AuthenticationMechanism
	{
		/// <summary>
		/// Authentication done by DIGEST-MD5 defined in RFC 2831:
		/// https://tools.ietf.org/html/rfc2831
		/// </summary>
		public DigestMd5()
		{
		}

		/// <summary>
		/// Name of the mechanism.
		/// </summary>
		public override string Name
		{
			get { return "DIGEST-MD5"; }
		}

		/// <summary>
		/// Weight of mechanisms. The higher the value, the more preferred.
		/// </summary>
		public override int Weight => 200;

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
			StringBuilder Challenge = new StringBuilder();
			string Nonce = Convert.ToBase64String(PersistenceLayer.GetRandomNumbers(32));

			Challenge.Append("realm=");
			Challenge.Append(PersistenceLayer.Domain);
			Challenge.Append(",nonce=");
			Challenge.Append(Nonce);
			Challenge.Append(",qop=auth,algorithm=md5-sess,charset=utf-8");

			byte[] Bin = Encoding.UTF8.GetBytes(Challenge.ToString());

			Connection.Tag = new object[] { Nonce };
			Connection.SaslChallenge(Convert.ToBase64String(Bin));

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
			Encoding Encoding = Encoding.UTF8;
			byte[] Bin = Convert.FromBase64String(Data);
			string Request = Encoding.UTF8.GetString(Bin);
			object[] P = (object[])Connection.Tag;
			string Nonce = (string)P[0];
			string UserName = string.Empty;
			string Realm = null;
			string Nonce2 = null;
			string ClientNonce = null;
			string QualityOfProtection = "auth";
			string DigestUri = string.Empty;
			string Response = string.Empty;
			int NonceCount = 0;
			bool Ok = true;

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(Request))
			{
				switch (Pair.Key.ToLower())
				{
					case "username":
						UserName = Pair.Value;
						break;

					case "realm":
						Realm = Pair.Value;
						break;

					case "nonce":
						Nonce2 = Pair.Value;
						break;

					case "cnonce":
						ClientNonce = Pair.Value;
						break;

					case "nc":
						if (!int.TryParse(Pair.Value, System.Globalization.NumberStyles.HexNumber, null, out NonceCount))
							NonceCount = 0;
						break;

					case "qop":
						QualityOfProtection = Pair.Value;
						break;

					case "digest-uri":
						DigestUri = Pair.Value;
						break;

					case "response":
						Response = Pair.Value;
						break;

					case "charset":
						try
						{
							Encoding = Encoding.GetEncoding(Pair.Value);
						}
						catch (Exception)
						{
							Ok = false;
						}
						break;
				}
			}

			if (string.IsNullOrEmpty(Realm) ||
				string.IsNullOrEmpty(UserName) ||
				string.IsNullOrEmpty(ClientNonce) ||
				string.IsNullOrEmpty(QualityOfProtection) ||
				string.IsNullOrEmpty(Response) ||
				NonceCount != 1 ||
				!Ok ||
				Realm != PersistenceLayer.Domain ||
				(QualityOfProtection != "auth" && QualityOfProtection != "auth-int"))
			{
				await Connection.SaslErrorMalformedRequest();

				LoginAuditor.Fail("Login attempt missing required parameters, or using erroneous parameters.",
					UserName, Connection.RemoteEndpoint, Connection.Protocol);

				return null;
			}

			if (Nonce2 is null || Nonce2 != Nonce)
			{
				await Connection.SaslErrorMalformedRequest();
				LoginAuditor.Fail("Login attempt using erroneous server nonce. Replay?", UserName, Connection.RemoteEndpoint, Connection.Protocol);
				return null;
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

			await Connection.SetUserIdentity(UserName);

			byte[] HPass = this.H(CONCAT(Account.UserName, ":", Realm, ":", Account.Password));
			byte[] A1;
			string A2;

			if (string.IsNullOrEmpty(Connection.AuthId))
				A1 = CONCAT(HPass, Encoding.GetBytes(CONCAT(":", Nonce, ":", ClientNonce)));
			else
				A1 = CONCAT(HPass, Encoding.GetBytes(CONCAT(":", Nonce, ":", ClientNonce, ":", Connection.AuthId)));

			if (QualityOfProtection == "auth")
				A2 = CONCAT("AUTHENTICATE:", DigestUri);
			else
				A2 = CONCAT("AUTHENTICATE:", DigestUri, ":00000000000000000000000000000000");

			string ResponseString = HEX(this.KD(HEX(this.H(A1)), CONCAT(Nonce, ":00000001:", ClientNonce, ":", QualityOfProtection, ":", HEX(this.H(A2)))));

			if (ResponseString == Response)
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
			string ChallengeString = Encoding.UTF8.GetString(ChallengeBinary);

			string Realm = Connection.Domain;
			string Nonce = string.Empty;
			string Qop = string.Empty;
			string[] Qops = null;
			//string Stale = string.Empty;
			//string Maxbuf = string.Empty;
			//string Charset = string.Empty;
			//string Algorithm = string.Empty;
			//string Cipher = string.Empty;
			//string Token = string.Empty;
			byte[] A1;
			string A2;

			if (ChallengeString.StartsWith("rspauth="))
				return null;

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(ChallengeString))
			{
				switch (Pair.Key.ToLower())
				{
					case "realm":
						Realm = Pair.Value;
						break;

					case "nonce":
						Nonce = Pair.Value;
						break;

					case "qop":
						Qop = Pair.Value;
						Qops = Qop.Split(',');
						break;

					//	case "stale":
					//		Stale = Pair.Value;
					//		break;
					//	
					//	case "maxbuf":
					//		Maxbuf = Pair.Value;
					//		break;
					//	
					//	case "charset":
					//		Charset = Pair.Value;
					//		break;
					//	
					//	case "algorithm":
					//		Algorithm = Pair.Value;
					//		break;
					//	
					//	case "cipher":
					//		Cipher = Pair.Value;
					//		break;
					//	
					//	case "token":
					//		Token = Pair.Value;
					//		break;
				}
			}

			bool Auth = !(Qops is null) && (Array.IndexOf(Qops, "auth") >= 0);
			bool AuthInt = !(Qops is null) && (Array.IndexOf(Qops, "auth-int") >= 0);

			StringBuilder sb = new StringBuilder();

			sb.Append("username=\"");
			sb.Append(UserName.Replace("\"", "\\\""));
			sb.Append("\"");

			if (!string.IsNullOrEmpty(Realm))
			{
				sb.Append(",realm=\"");
				sb.Append(Realm.Replace("\"", "\\\""));
				sb.Append("\"");
			}

			sb.Append(",nonce=\"");
			sb.Append(Nonce.Replace("\"", "\\\""));
			sb.Append("\"");

			string DigestUri = "xmpp/" + Connection.Domain;

			string ClientNonce = Convert.ToBase64String(GetRandomBytes(16));

			sb.Append(",cnonce=\"");
			sb.Append(ClientNonce.Replace("\"", "\\\""));
			sb.Append("\",nc=00000001");

			if (!string.IsNullOrEmpty(Qop))
			{
				sb.Append(",qop=");

				if (AuthInt)
					sb.Append("auth-int");
				else if (Auth)
					sb.Append("auth");
				else
					sb.Append(Qop);
			}

			sb.Append(",digest-uri=\"");
			sb.Append(DigestUri);
			sb.Append("\"");

			byte[] HPass = this.H(CONCAT(UserName, ":", Realm, ":", Password));
			string AuthId = UserName + "@" + Connection.Domain;

			A1 = CONCAT(HPass, ":", Nonce, ":", ClientNonce, ":", AuthId);

			if (Qop == "auth")
				A2 = CONCAT("AUTHENTICATE:", DigestUri);
			else
				A2 = CONCAT("AUTHENTICATE:", DigestUri, ":00000000000000000000000000000000");

			string ResponseString = HEX(this.KD(HEX(this.H(A1)), CONCAT(Nonce, ":00000001:", ClientNonce, ":", Qop, ":", HEX(this.H(A2)))));

			sb.Append(",response=");
			sb.Append(ResponseString);

			sb.Append(",charset=utf-8");

			sb.Append(",authzid=\"");
			sb.Append(AuthId);
			sb.Append("\"");

			await Connection.FinalResponse(this, Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())));
			return true;
		}

	}
}
