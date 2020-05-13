using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Authentication method: CRAM-MD5
	/// 
	/// See RFC 2195 for a description of the CRAM-MD5 method:
	/// http://tools.ietf.org/html/rfc2195 
	/// </summary>
	public class CramMd5 : MD5AuthenticationMethod
	{
		/// <summary>
		/// Authentication method: CRAM-MD5
		/// 
		/// See RFC 2195 for a description of the CRAM-MD5 method:
		/// http://tools.ietf.org/html/rfc2195 
		/// </summary>
		public CramMd5()
		{
		}

		/// <summary>
		/// Name of hash method.
		/// </summary>
		public override string HashMethodName => "CRAM-MD5";

		/// <summary>
		/// <see cref="AuthenticationMethod.Challenge"/>
		/// </summary>
		public override string Challenge(string Challenge, XmppClient Client)
		{
			byte[] ChallengeBinary = Convert.FromBase64String(Challenge);

			string Pwd = Client.PasswordHash;
			if (string.IsNullOrEmpty(Pwd))
			{
				Pwd = Client.Password;
				Client.PasswordHash = Pwd;
				Client.PasswordHashMethod = this.HashMethodName;
			}

			byte[] HMAC = this.HMAC(Encoding.UTF8.GetBytes(Pwd), ChallengeBinary);
			string CRAM = Client.UserName + " " + HEX(HMAC);

			return Convert.ToBase64String(Encoding.UTF8.GetBytes(CRAM));
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
