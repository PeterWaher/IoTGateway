using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// See RFC 5802 and 7677 for a description of the SCRAM-SHA-* methods:
	/// http://tools.ietf.org/html/rfc5802
	/// http://tools.ietf.org/html/rfc7677
	/// </summary>
	public abstract class ScramAuthenticationMethod : HashedAuthenticationMethod
	{
		private byte[] salt;
		private readonly string nonce;
		private string serverNonce;
		private string saltString;
		private string serverSignature;
		private int nrIterations;

		/// <summary>
		/// See RFC 5802 and 7677 for a description of the SCRAM-SHA-* methods:
		/// http://tools.ietf.org/html/rfc5802
		/// http://tools.ietf.org/html/rfc7677
		/// </summary>
		/// <param name="Nonce">Nonce value.</param>
		public ScramAuthenticationMethod(string Nonce)
		{
			this.nonce = Nonce;
		}

		/// <summary>
		/// <see cref="AuthenticationMethod.Challenge"/>
		/// </summary>
		public override string Challenge(string Challenge, XmppClient Client)
		{
			byte[] ChallengeBinary = Convert.FromBase64String(Challenge);
			string ChallengeString = Encoding.UTF8.GetString(ChallengeBinary);

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

			if (string.IsNullOrEmpty(this.serverNonce) || !this.serverNonce.StartsWith(this.nonce) ||
				this.salt is null || this.nrIterations <= 0)
			{
				throw new XmppException("Invalid challenge.");
			}

			byte[] SaltedPassword;

			if (string.IsNullOrEmpty(Client.PasswordHash))
			{
				SaltedPassword = Hi(Encoding.UTF8.GetBytes(Client.Password), this.salt, this.nrIterations);     // Client.Pasword.Normalize()	- Normalize method avaialble in .NET 2.0
				Client.PasswordHash = Convert.ToBase64String(SaltedPassword);
				Client.PasswordHashMethod = this.HashMethodName;
			}
			else
			{
				try
				{
					SaltedPassword = Convert.FromBase64String(Client.PasswordHash);
				}
				catch (Exception)
				{
					throw new Exception("Invalid password hash provided.");
				}
			}

			byte[] ClientKey = HMAC(SaltedPassword, Encoding.UTF8.GetBytes("Client Key"));
			byte[] StoredKey = H(ClientKey);

			StringBuilder sb;

			sb = new StringBuilder();
			sb.Append("n=");
			sb.Append(Client.UserName);
			sb.Append(",r=");
			sb.Append(this.nonce);
			sb.Append(',');
			sb.Append(ChallengeString);
			sb.Append(",c=biws,r=");
			sb.Append(this.serverNonce);

			byte[] AuthenticationMessage = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] ClientSignature = HMAC(StoredKey, AuthenticationMessage);
			byte[] ClientProof = XOR(ClientKey, ClientSignature);

			byte[] ServerKey = HMAC(SaltedPassword, Encoding.UTF8.GetBytes("Server Key"));
			byte[] ServerSignature = HMAC(ServerKey, AuthenticationMessage);

			this.serverSignature = Convert.ToBase64String(ServerSignature);

			sb = new StringBuilder();
			sb.Append("c=biws,r=");     // biws="n,,"
			sb.Append(this.serverNonce);
			sb.Append(",p=");
			sb.Append(Convert.ToBase64String(ClientProof));

			return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
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
			string ResponseString = Encoding.UTF8.GetString(ResponseBinary);

			foreach (KeyValuePair<string, string> Pair in this.ParseCommaSeparatedParameterList(ResponseString))
			{
				if (Pair.Key.ToLower() == "v")
					return (Pair.Value == this.serverSignature);
			}

			return false;
		}

	}
}
