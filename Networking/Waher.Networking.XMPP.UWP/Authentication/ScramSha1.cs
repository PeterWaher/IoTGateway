using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Authentication method: SCRAM-SHA-1
	/// 
	/// See RFC 5802 for a description of the SCRAM-SHA-1 method:
	/// http://tools.ietf.org/html/rfc5802
	/// </summary>
	public class ScramSha1 : SHA1AuthenticationMethod
	{
		private byte[] salt;
		private string nonce;
		private string serverNonce;
		private string saltString;
		private string serverSignature;
		private int nrIterations;

		/// <summary>
		/// Authentication method: SCRAM-SHA-1
		/// 
		/// See RFC 5802 for a description of the SCRAM-SHA-1 method:
		/// http://tools.ietf.org/html/rfc5802
		/// </summary>
		/// <param name="Nonce">Nonce value.</param>
		public ScramSha1(string Nonce)
		{
			this.nonce = Nonce;
		}

		/// <summary>
		/// <see cref="AuthenticationMethod.Challenge"/>
		/// </summary>
		public override string Challenge(string Challenge, XmppClient Client)
		{
			byte[] ChallengeBinary = Convert.FromBase64String(Challenge);
			string ChallengeString = System.Text.Encoding.UTF8.GetString(ChallengeBinary);

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(ChallengeString))
			{
				switch (Pair.Key.ToLower())
				{
					case "r":
						this.serverNonce = Pair.Value;
						break;

					case "s":
						this.saltString = Pair.Value;
						this.salt = Convert.FromBase64String(this.saltString);
						break;

					case "i":
						this.nrIterations = int.Parse(Pair.Value);
						break;
				}
			}

			if (string.IsNullOrEmpty(this.serverNonce) || this.salt == null || this.nrIterations <= 0)
				throw new XmppException("Invalid challenge.");

			byte[] SaltedPassword;

			if (string.IsNullOrEmpty(Client.PasswordHash))
			{
				SaltedPassword = Hi(System.Text.Encoding.UTF8.GetBytes(Client.Password.Normalize()), this.salt, this.nrIterations);
				Client.PasswordHash = Convert.ToBase64String(SaltedPassword);
				Client.PasswordHashMethod = "SCRAM-SHA-1";
			}
			else
				SaltedPassword = Convert.FromBase64String(Client.PasswordHash);

			byte[] ClientKey = HMAC(SaltedPassword, System.Text.Encoding.UTF8.GetBytes("Client Key"));
			byte[] StoredKey = H(ClientKey);

			StringBuilder sb;

			sb = new StringBuilder();
			sb.Append("n=");
			sb.Append(Client.UserName);
			sb.Append(",r=");
			sb.Append(this.nonce);
			sb.Append(",r=");
			sb.Append(this.serverNonce);
			sb.Append(",s=");
			sb.Append(this.saltString);
			sb.Append(",i=");
			sb.Append(this.nrIterations.ToString());
			sb.Append(",c=biws,r=");
			sb.Append(this.serverNonce);

			byte[] AuthenticationMessage = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
			byte[] ClientSignature = HMAC(StoredKey, AuthenticationMessage);
			byte[] ClientProof = XOR(ClientKey, ClientSignature);

			byte[] ServerKey = HMAC(SaltedPassword, System.Text.Encoding.UTF8.GetBytes("Server Key"));
			byte[] ServerSignature = HMAC(ServerKey, AuthenticationMessage);

			this.serverSignature = Convert.ToBase64String(ServerSignature);

			sb = new StringBuilder();
			sb.Append("c=biws,r=");     // biws="n,,"
			sb.Append(this.serverNonce);
			sb.Append(",p=");
			sb.Append(Convert.ToBase64String(ClientProof));

			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
		}

		private byte[] Hi(byte[] String, byte[] Salt, int NrIterations)
		{
			byte[] U1 = HMAC(String, CONCAT(Salt, One));
			byte[] U2 = HMAC(String, U1);
			byte[] Response = XOR(U1, U2);

			while (NrIterations > 2)
			{
				U1 = U2;
				U2 = HMAC(String, U1);
				Response = XOR(Response, U2);
				NrIterations--;
			}

			return Response;
		}

		private static readonly byte[] One = new byte[] { 0, 0, 0, 1 };

		/// <summary>
		/// <see cref="AuthenticationMethod.CheckSuccess"/>
		/// </summary>
		public override bool CheckSuccess(string Success, XmppClient Client)
		{
			byte[] ResponseBinary = Convert.FromBase64String(Success);
			string ResponseString = System.Text.Encoding.UTF8.GetString(ResponseBinary);

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(ResponseString))
			{
				if (Pair.Key.ToLower() == "v")
					return (Pair.Value == this.serverSignature);
			}

			return false;
		}

	}
}
