using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Authentication method: DIGEST-MD5
	/// 
	/// See RFC 2831 for a description of the DIGEST-MD5 method:
	/// http://tools.ietf.org/html/rfc2831
	/// </summary>
	public class DigestMd5 : MD5AuthenticationMethod
	{
		/// <summary>
		/// Authentication method: DIGEST-MD5
		/// 
		/// See RFC 2831 for a description of the DIGEST-MD5 method:
		/// http://tools.ietf.org/html/rfc2831
		/// </summary>
		public DigestMd5()
		{
		}

		/// <summary>
		/// <see cref="AuthenticationMethod.Challenge"/>
		/// </summary>
		public override string Challenge(string Challenge, XmppClient Client)
		{
			byte[] ChallengeBinary = Convert.FromBase64String(Challenge);
			string ChallengeString = System.Text.Encoding.UTF8.GetString(ChallengeBinary);

			string Realm = Client.Domain;
			string Nonce = string.Empty;
			string Qop = string.Empty;
			string[] Qops = null;
			string Stale = string.Empty;
			string Maxbuf = string.Empty;
			string Charset = string.Empty;
			string Algorithm = string.Empty;
			string Cipher = string.Empty;
			string Token = string.Empty;
			byte[] A1;
			string A2;

			if (ChallengeString.StartsWith("rspauth="))
				return string.Empty;
			else
			{
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

						case "stale":
							Stale = Pair.Value;
							break;

						case "maxbuf":
							Maxbuf = Pair.Value;
							break;

						case "charset":
							Charset = Pair.Value;
							break;

						case "algorithm":
							Algorithm = Pair.Value;
							break;

						case "cipher":
							Cipher = Pair.Value;
							break;

						case "token":
							Token = Pair.Value;
							break;
					}
				}

				bool Auth = Qops == null ? false : (Array.IndexOf<string>(Qops, "auth") >= 0);
				bool AuthInt = Qops == null ? false : (Array.IndexOf<string>(Qops, "auth-int") >= 0);

				StringBuilder sb = new StringBuilder();

				sb.Append("username=\"");
				sb.Append(Client.UserName.Replace("\"", "\\\""));
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

				string DigestUri = "xmpp/" + Client.Domain;

				string ClientNonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

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

				if (string.IsNullOrEmpty(Client.BaseJid))
					A1 = CONCAT(H(CONCAT(Client.UserName, ":", Realm, ":", Client.Password)), ":", Nonce, ":", ClientNonce);
				else
					A1 = CONCAT(H(CONCAT(Client.UserName, ":", Realm, ":", Client.Password)), ":", Nonce, ":", ClientNonce, ":", Client.BaseJid);

				if (Qop == "auth")
					A2 = CONCAT("AUTHENTICATE:", DigestUri);
				else
					A2 = CONCAT("AUTHENTICATE:", DigestUri, ":00000000000000000000000000000000");

				string ResponseString = HEX(KD(HEX(H(A1)), CONCAT(Nonce, ":00000001:", ClientNonce, ":", Qop, ":", HEX(H(A2)))));

				sb.Append(",response=");
				sb.Append(ResponseString);

				sb.Append(",charset=utf-8");

				if (!string.IsNullOrEmpty(Client.BaseJid))
				{
					sb.Append(",authzid=\"");
					sb.Append(Client.BaseJid);
					sb.Append("\"");
				}

				return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
			}
		}

		/// <summary>
		/// <see cref="AuthenticationMethod.CheckSuccess"/>
		/// </summary>
		public override bool CheckSuccess(string Success, XmppClient Client)
		{
			return true;
		}

	}
}
