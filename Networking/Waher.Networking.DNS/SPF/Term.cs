using System;
using System.Collections.Generic;
using System.Net;
using Waher.Networking.DNS.SPF.Mechanisms;

namespace Waher.Networking.DNS.SPF
{
	/// <summary>
	/// SPF Mechanism qualifier
	/// </summary>
	public enum SpfQualifier
	{
		/// <summary>
		/// If a mechanism matches, it results in a pass
		/// </summary>
		Pass,

		/// <summary>
		/// If a mechanism matches, it results in a fail
		/// </summary>
		Fail,

		/// <summary>
		/// If a mechanism matches, it results in a softFail
		/// </summary>
		SoftFail,

		/// <summary>
		/// If a mechanism matches, it results in a neutral
		/// </summary>
		Neutral
	}

	/// <summary>
	/// SPF Term
	/// </summary>
	public class Term
	{
		internal string s = null;
		internal int len = 0;
		internal int pos = 0;
		internal int dnsLookupsLeft = 10;
		internal readonly string sender;
		internal string domain;
		internal readonly string helloDomain;
		internal readonly string hostDomain;
		internal readonly IPAddress ip;

		/// <summary>
		/// SPF Term
		/// </summary>
		/// <param name="Sender">the "MAIL FROM" or "HELO" identity.</param>
		/// <param name="Domain">The domain that provides the sought-after authorization
		/// information; initially, the domain portion of the
		/// "MAIL FROM" or "HELO" identity.</param>
		/// <param name="Ip">the IP address of the SMTP client that is emitting
		/// the mail, either IPv4 or IPv6.</param>
		/// <param name="HelloDomain">Domain as presented by the client in the HELO or EHLO command.</param>
		/// <param name="HostDomain">Domain of the current host, performing SPF authentication.</param>
		/// <returns>Result of SPF evaluation.</returns>
		public Term(string Sender, string Domain, IPAddress Ip, string HelloDomain,
			string HostDomain)
		{
			this.sender = Sender;
			this.domain = Domain;
			this.ip = Ip;
			this.helloDomain = HelloDomain;
			this.hostDomain = HostDomain;
		}

		/// <summary>
		/// Resets the string representation of the term.
		/// </summary>
		/// <param name="String">String-representation of term.</param>
		public void Reset(string String)
		{
			this.s = String.Trim();
			this.len = this.s.Length;
			this.pos = 0;
		}

		internal void SkipWhiteSpace()
		{
			while (this.pos < this.len && this.s[this.pos] <= ' ')
				this.pos++;
		}

		internal char PeekNextChar()
		{
			if (this.pos >= this.len)
				return (char)0;
			else
				return this.s[this.pos];
		}

		internal char NextChar()
		{
			if (this.pos >= this.len)
				throw new Exception("SPF syntax error.");

			return this.s[this.pos++];
		}

		internal int NextInteger()
		{
			int Start = this.pos;

			while (char.IsDigit(this.PeekNextChar()))
				this.pos++;

			return int.Parse(this.s.Substring(Start, this.pos - Start));
		}

		internal string NextLabel()
		{
			int Start = this.pos;
			char ch;

			if (char.IsLetter(ch = this.PeekNextChar()))
			{
				this.pos++;

				while (char.IsLetter(ch = this.PeekNextChar()) || char.IsDigit(ch) ||
					ch == '-' || ch == '_' || ch == '.')
				{
					this.pos++;
				}
			}

			return this.s.Substring(Start, this.pos - Start);
		}

	}
}
