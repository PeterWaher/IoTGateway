using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Networking.DNS.SPF.Mechanisms;

namespace Waher.Networking.DNS.SPF
{
	/// <summary>
	/// Resolves a SPF string, as defined in:
	/// 
	/// RFC 7208: https://tools.ietf.org/html/rfc7208: Sender Policy Framework (SPF) for Authorizing Use of Domains in Email, Version 1
	/// </summary>
	public static class SpfResolver
	{
		/// <summary>
		/// Fetches SPF records, parses them, and
		/// evaluates them to determine whether a particular host is or is not
		/// permitted to send mail with a given identity.
		/// </summary>
		/// <param name="Address">the IP address of the SMTP client that is emitting
		/// the mail, either IPv4 or IPv6.</param>
		/// <param name="DomainName">The domain that provides the sought-after authorization
		/// information; initially, the domain portion of the
		/// "MAIL FROM" or "HELO" identity.</param>
		/// <param name="Sender">the "MAIL FROM" or "HELO" identity.</param>
		/// <param name="HelloDomain">Domain as presented by the client in the HELO or EHLO command.</param>
		/// <param name="HostDomain">Domain of the current host, performing SPF authentication.</param>
		/// <returns>Result of SPF evaluation.</returns>
		public static async Task<SpfResult> CheckHost(IPAddress Address, string DomainName, string Sender,
			string HelloDomain, string HostDomain)
		{
			string[] TermStrings = null;
			string s;

			try
			{
				string[] TXT = await DnsResolver.LookupText(DomainName);

				foreach (string Row in TXT)
				{
					s = Row.Trim();

					if (s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"')
						s = s.Substring(1, s.Length - 2);

					if (!s.StartsWith("v=spf1"))
						continue;

					if (!(TermStrings is null))
						return SpfResult.PermanentError;

					TermStrings = s.Substring(6).Trim().Split(space, StringSplitOptions.RemoveEmptyEntries);
				}
			}
			catch (ArgumentException)
			{
				return SpfResult.None;
			}

			if (TermStrings is null)
				return SpfResult.None;

			// Syntax evaluation first, §4.6

			int c = TermStrings.Length;
			LinkedList<Mechanism> Mechanisms = new LinkedList<Mechanism>();
			Redirect Redirect = null;
			Exp Explanation = null;
			int i;

			try
			{
				Term Term = new Term(Sender, DomainName, Address, HelloDomain, HostDomain);

				for (i = 0; i < c; i++)
				{
					SpfQualifier Qualifier;

					Term.Reset(TermStrings[i]);
					Term.SkipWhiteSpace();

					switch (Term.PeekNextChar())
					{
						case '+':
							Term.pos++;
							Qualifier = SpfQualifier.Pass;
							break;

						case '-':
							Term.pos++;
							Qualifier = SpfQualifier.Fail;
							break;

						case '~':
							Term.pos++;
							Qualifier = SpfQualifier.SoftFail;
							break;

						case '?':
							Term.pos++;
							Qualifier = SpfQualifier.Neutral;
							break;

						default:
							Qualifier = SpfQualifier.Pass;
							break;
					}

					switch (Term.NextLabel().ToLower())
					{
						case "all":
							Mechanisms.AddLast(new All(Term, Qualifier));
							break;

						case "include":
							Mechanisms.AddLast(new Include(Term, Qualifier));
							break;

						case "a":
							Mechanisms.AddLast(new A(Term, Qualifier));
							break;

						case "mx":
							Mechanisms.AddLast(new Mx(Term, Qualifier));
							break;

						case "ptr":
							Mechanisms.AddLast(new Ptr(Term, Qualifier));
							break;

						case "ip4":
							Mechanisms.AddLast(new Ip4(Term, Qualifier));
							break;

						case "ip6":
							Mechanisms.AddLast(new Ip6(Term, Qualifier));
							break;

						case "exists":
							Mechanisms.AddLast(new Exists(Term, Qualifier));
							break;

						case "redirect":
							if (!(Redirect is null))
								return SpfResult.PermanentError;

							Redirect = new Redirect(Term, Qualifier);
							break;

						case "exp":
							if (!(Explanation is null))
								return SpfResult.PermanentError;

							Explanation = new Exp(Term, Qualifier);
							break;

						default:
							throw new Exception("Syntax error.");
					}
				}
			}
			catch (Exception)
			{
				return SpfResult.PermanentError;
			}

			foreach (Mechanism Mechanism in Mechanisms)
			{
			}

			return SpfResult.Neutral;
		}

		private static readonly char[] space = new char[] { ' ' };

	}
}
