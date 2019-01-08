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
		/// <returns>Result of SPF evaluation, together with an optional explanation string,
		/// if one exists, and if the result indicates a failure.</returns>
		public static Task<KeyValuePair<SpfResult, string>> CheckHost(IPAddress Address, string DomainName, string Sender,
			string HelloDomain, string HostDomain)
		{
			Term Term = new Term(Sender, DomainName, Address, HelloDomain, HostDomain);
			return CheckHost(Term);
		}

		/// <summary>
		/// Fetches SPF records, parses them, and
		/// evaluates them to determine whether a particular host is or is not
		/// permitted to send mail with a given identity.
		/// </summary>
		/// <param name="Term">Information about current query.</param>
		/// <returns>Result of SPF evaluation, together with an optional explanation string,
		/// if one exists, and if the result indicates a failure.</returns>
		internal static async Task<KeyValuePair<SpfResult, string>> CheckHost(Term Term)
		{
			Exp Explanation = null;
			string[] TermStrings = null;
			string s;

			try
			{
				string[] TXT = await DnsResolver.LookupText(Term.domain);

				foreach (string Row in TXT)
				{
					s = Row.Trim();

					if (s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"')
						s = s.Substring(1, s.Length - 2);

					if (!s.StartsWith("v=spf1"))
						continue;

					if (!(TermStrings is null))
						return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError, "Multiple SPF records found.");

					TermStrings = s.Substring(6).Trim().Split(space, StringSplitOptions.RemoveEmptyEntries);
				}
			}
			catch (Exception)
			{
				return new KeyValuePair<SpfResult, string>(SpfResult.None, "No SPF records found.");
			}

			if (TermStrings is null)
				return new KeyValuePair<SpfResult, string>(SpfResult.None, "No SPF records found.");

			// Syntax evaluation first, §4.6

			int c = TermStrings.Length;
			LinkedList<Mechanism> Mechanisms = new LinkedList<Mechanism>();
			Redirect Redirect = null;
			int i;

			try
			{
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
								return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError, "Multiple redirect modifiers foundin SPF record.");

							Redirect = new Redirect(Term, Qualifier);
							break;

						case "exp":
							if (!(Explanation is null))
								return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError, "Multiple exp modifiers foundin SPF record.");

							Explanation = new Exp(Term, Qualifier);
							break;

						default:
							throw new Exception("Syntax error.");
					}
				}

				foreach (Mechanism Mechanism in Mechanisms)
				{
					await Mechanism.Expand();

					SpfResult Result = await Mechanism.Matches();

					switch (Result)
					{
						case SpfResult.Pass:
							switch (Mechanism.Qualifier)
							{
								case SpfQualifier.Pass: return new KeyValuePair<SpfResult, string>(SpfResult.Pass, null);
								case SpfQualifier.Fail: return new KeyValuePair<SpfResult, string>(SpfResult.Fail, Explanation == null ? null : await Explanation.Evaluate());
								case SpfQualifier.Neutral: return new KeyValuePair<SpfResult, string>(SpfResult.Neutral, null);
								case SpfQualifier.SoftFail: return new KeyValuePair<SpfResult, string>(SpfResult.SoftFail, Explanation == null ? null : await Explanation.Evaluate());
							}
							break;

						case SpfResult.TemporaryError:
							return new KeyValuePair<SpfResult, string>(SpfResult.TemporaryError, Explanation == null ? null : await Explanation.Evaluate());

						case SpfResult.None:
						case SpfResult.PermanentError:
							return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError, Explanation == null ? null : await Explanation.Evaluate());
					}
				}

				if (!(Redirect is null))
				{
					await Redirect.Expand();

					string Bak = Term.domain;
					Term.domain = Redirect.Domain;
					try
					{
						KeyValuePair<SpfResult, string> Result = await SpfResolver.CheckHost(Term);

						if (Result.Key == SpfResult.None)
							return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError, Explanation == null ? null : await Explanation.Evaluate());
						else if (Result.Key != SpfResult.Pass && Result.Key != SpfResult.Neutral &&
							string.IsNullOrEmpty(Result.Value))
						{
							return new KeyValuePair<SpfResult, string>(Result.Key, Explanation == null ? null : await Explanation.Evaluate());
						}
						else
							return Result;
					}
					finally
					{
						Term.domain = Bak;
					}

				}
			}
			catch (Exception)
			{
				return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError, "Unable to evaluate SPF record.");
			}

			return new KeyValuePair<SpfResult, string>(SpfResult.Neutral, null);
		}

		private static readonly char[] space = new char[] { ' ' };
	}
}
