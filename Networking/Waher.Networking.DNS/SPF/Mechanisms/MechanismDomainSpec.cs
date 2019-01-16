using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// Abstract base class for SPF mechanisms with a domain specification.
	/// </summary>
	public abstract class MechanismDomainSpec : Mechanism
	{
		private string domain;
		private bool expanded = false;

		/// <summary>
		/// Abstract base class for SPF mechanisms with a domain specification.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public MechanismDomainSpec(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
			if (Term.PeekNextChar() == this.Separator)
			{
				Term.NextChar();

				int Start = Term.pos;
				char ch;

				while ((ch = Term.PeekNextChar()) > ' ' && ch != '/')
					Term.pos++;

				this.domain = Term.s.Substring(Start, Term.pos - Start);
			}
			else if (this.DomainRequired)
				throw new Exception(this.Separator + " expected.");
		}

		/// <summary>
		/// Expands any macros in the domain specification.
		/// </summary>
		public override async Task Expand()
		{
			if (this.expanded)
				return;

			this.expanded = true;
			this.term.Reset(this.domain);

			StringBuilder sb = new StringBuilder();
			char ch;

			while ((ch = this.term.PeekNextChar()) > ' ')
			{
				this.term.pos++;

				if (ch == '%')
				{
					switch (ch = this.term.PeekNextChar())
					{
						case (char)0:
							sb.Append('%');
							break;

						case '%':
							this.term.pos++;
							sb.Append('%');
							break;

						case '_':
							this.term.pos++;
							sb.Append(' ');
							break;

						case '-':
							this.term.pos++;
							sb.Append("%20");
							break;

						case '{':
							this.term.pos++;

							char MacroLetter = char.ToLower(this.term.NextChar());
							int? Digit;
							bool Reverse;

							if (char.IsDigit(this.term.PeekNextChar()))
							{
								Digit = this.term.NextInteger();

								if (Digit == 0)
									throw new Exception("Invalid number of digits.");
							}
							else
								Digit = null;

							if (char.ToLower(this.term.PeekNextChar()) == 'r')
							{
								this.term.pos++;
								Reverse = true;
							}
							else
								Reverse = false;

							int Start = this.term.pos;
							while ((ch = this.term.PeekNextChar()) == '.' || ch == '-' || ch == '+' ||
								ch == ',' || ch == '/' || ch == '_' || ch == '=')
							{
								this.term.pos++;
							}

							string Delimiter = this.term.s.Substring(Start, this.term.pos - Start);

							ch = this.term.NextChar();
							if (ch != '}')
								throw new Exception("Expected }");

							string s;

							switch (MacroLetter)
							{
								case 's':   // sender
									s = this.term.sender;
									break;

								case 'l':   // local-part of sender
									s = this.term.sender;
									int i = s.IndexOf('@');
									if (i < 0)
										s = string.Empty;
									else
										s = s.Substring(0, i);
									break;

								case 'o':   // domain of sender
									s = this.term.sender;
									i = s.IndexOf('@');
									if (i >= 0)
										s = s.Substring(i + 1);
									break;

								case 'd':   // domain
									s = this.term.domain;
									break;

								case 'i':
									switch (this.term.ip.AddressFamily)
									{
										case AddressFamily.InterNetwork:
											s = this.term.ip.ToString();
											break;

										case AddressFamily.InterNetworkV6:
											byte[] Bin = this.term.ip.GetAddressBytes();

											StringBuilder sb2 = new StringBuilder();
											byte b, b2;

											for (i = 0; i < 16; i++)
											{
												b = Bin[i];

												b2 = (byte)(b >> 4);
												if (b2 < 10)
													sb2.Append((char)('0' + b2));
												else
													sb2.Append((char)('a' + b2 - 10));

												sb2.Append('.');

												b2 = (byte)(b & 15);
												if (b2 < 10)
													sb2.Append((char)('0' + b2));
												else
													sb2.Append((char)('a' + b2 - 10));

												if (i < 15)
													sb2.Append('.');
											}

											s = sb2.ToString();
											break;

										default:
											throw new Exception("Invalid client address.");
									}
									break;

								case 'p':
									try
									{
										if (this.term.dnsLookupsLeft-- <= 0)
											throw new Exception("DNS Lookup maximum reached.");

										string[] DomainNames = await DnsResolver.LookupDomainName(this.term.ip);

										// First check if domain is found.

										s = null;
										foreach (string DomainName in DomainNames)
										{
											if (string.Compare(DomainName, this.term.domain, true) == 0 &&
												await this.MatchReverseIp(DomainName))
											{
												s = DomainName;
												break;
											}
										}

										if (s is null)
										{
											// Second, check if sub-domain is found.

											foreach (string DomainName in DomainNames)
											{
												if (DomainName.EndsWith("." + this.term.domain, StringComparison.CurrentCultureIgnoreCase) &&
													await this.MatchReverseIp(DomainName))
												{
													s = DomainName;
													break;
												}
											}

											if (s is null)
											{
												if (DomainNames.Length == 0)
													s = "unknown";
												else
													s = DomainNames[DnsResolver.Next(DomainNames.Length)];
											}
										}
									}
									catch (ArgumentException)
									{
										s = "unknown";
									}
									catch (TimeoutException)
									{
										s = "unknown";
									}
									break;

								case 'v':
									switch (this.term.ip.AddressFamily)
									{
										case AddressFamily.InterNetwork:
											s = "in-addr";
											break;

										case AddressFamily.InterNetworkV6:
											s = "ip6";
											break;

										default:
											throw new Exception("Invalid client address.");
									}
									break;

								case 'h':
									s = this.term.helloDomain;
									break;

								case 'c':
									this.AssertExp();
									s = this.term.ip.ToString();
									break;

								case 'r':
									this.AssertExp();
									s = this.term.hostDomain;
									break;

								case 't':
									this.AssertExp();
									int Seconds = (int)Math.Round((DateTime.UtcNow - UnixEpoch).TotalSeconds);
									s = Seconds.ToString();
									break;

								default:
									throw new Exception("Unknown macro.");
							}

							if (Reverse || Digit.HasValue || !string.IsNullOrEmpty(Delimiter))
							{
								if (string.IsNullOrEmpty(Delimiter))
									Delimiter = ".";

								string[] Parts = s.Split(new string[] { Delimiter }, StringSplitOptions.None);
								int i = Parts.Length;

								if (Reverse)
									Array.Reverse(Parts);

								if (Digit.HasValue && Digit.Value < i)
									i = Digit.Value;

								bool First = true;
								int j = Parts.Length - i;

								while (i-- > 0)
								{
									if (First)
										First = false;
									else
										sb.Append('.');

									sb.Append(Parts[j++]);
								}
							}
							else
								sb.Append(s);
							break;

						default:
							this.term.pos++;
							sb.Append('%');
							sb.Append(ch);
							break;
					}
				}
				else
					sb.Append(ch);
			}

			this.domain = sb.ToString();
		}

		internal async Task<bool> MatchReverseIp(string DomainName)
		{
			if (this.term.dnsLookupsLeft-- <= 0)
				throw new Exception("DNS Lookup maximum reached.");

			IPAddress[] Addresses;

			switch (this.term.ip.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					Addresses = await DnsResolver.LookupIP4Addresses(DomainName);
					break;

				case AddressFamily.InterNetworkV6:
					Addresses = await DnsResolver.LookupIP6Addresses(DomainName);
					break;

				default:
					throw new Exception("Invalid client address.");
			}

			string Temp = this.term.ip.ToString();

			foreach (IPAddress Addr in Addresses)
			{
				if (string.Compare(Addr.ToString(), Temp, true) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// UNIX Epoch, started at 1970-01-01, 00:00:00 (GMT)
		/// </summary>
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private void AssertExp()
		{
			if (!(this is Exp))
				throw new Exception("Macro only available in exp");
		}

		/// <summary>
		/// Mechanism separator
		/// </summary>
		public virtual char Separator
		{
			get => ':';
		}

		/// <summary>
		/// If the domain specification is required.
		/// </summary>
		public virtual bool DomainRequired => true;

		/// <summary>
		/// Domain specification
		/// </summary>
		public string Domain => this.domain;

		/// <summary>
		/// Target domain.
		/// </summary>
		public string TargetDomain => string.IsNullOrEmpty(this.domain) ? this.term.domain : this.domain;
	}
}
