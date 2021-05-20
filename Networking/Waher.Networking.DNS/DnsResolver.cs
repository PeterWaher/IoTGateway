using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Profiling;

namespace Waher.Networking.DNS
{
	/// <summary>
	/// DNS resolver, as defined in:
	/// 
	/// RFC 1034: https://tools.ietf.org/html/rfc1034: DOMAIN NAMES - CONCEPTS AND FACILITIES
	/// RFC 1035: https://tools.ietf.org/html/rfc1035: DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION
	/// RFC 2782: https://tools.ietf.org/html/rfc2782: A DNS RR for specifying the location of services (DNS SRV)
	/// RFC 3596: https://tools.ietf.org/html/rfc3596: DNS Extensions to Support IP Version 6
	/// RFC 5782: https://tools.ietf.org/html/rfc5782: DNS Blacklists and Whitelists
	/// RFC 7208: https://tools.ietf.org/html/rfc7208: Sender Policy Framework (SPF) for Authorizing Use of Domains in Email, Version 1
	/// RFC 8484: https://tools.ietf.org/html/rfc8484: DNS Queries over HTTPS (DoH)
	/// </summary>
	public static class DnsResolver
	{
		/// <summary>
		/// 53
		/// </summary>
		public const int DefaultDnsPort = 53;

		private static readonly Regex arpanetHostName = new Regex(@"^[a-zA-Z]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?([.][a-zA-Z]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly object synchObject = new object();
		private static readonly Random rnd = new Random();
		private static ushort nextId = 0;
		private static DnsHttpsClient httpsClient = null;
		private static DnsUdpClient udpClient = null;
		private static bool networkChanged = false;
		private static int nestingDepth = 0;

		static DnsResolver()
		{
			try
			{
				NetworkChange.NetworkAddressChanged += (sender, e) => networkChanged = true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// URI used in DNS over HTTPS (DoH) requests. Setting the property to null disables DNS over HTTPS (DoH).
		/// </summary>
		public static Uri DnsOverHttpsUri
		{
			get => httpsClient?.Uri;
			set
			{
				if (value is null)
				{
					httpsClient?.Dispose();
					httpsClient = null;
				}
				else if (httpsClient is null)
					httpsClient = new DnsHttpsClient(value);
				else
					httpsClient.Uri = value;
			}
		}

		/// <summary>
		/// Available DNS Server Addresses.
		/// </summary>
		public static IPAddress[] DnsServerAddresses
		{
			get
			{
				List<IPAddress> Addresses = new List<IPAddress>();

				NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface Interface in Interfaces)
				{
					if (Interface.OperationalStatus == OperationalStatus.Up)
					{
						foreach (IPAddress Address in Interface.GetIPProperties().DnsAddresses)
						{
							if (!Addresses.Contains(Address))
								Addresses.Add(Address);
						}
					}
				}

				return Addresses.ToArray();
			}
		}

		/// <summary>
		/// Checks if a host name is a valid ARPHANET host name.
		/// </summary>
		/// <param name="HostName">Host Name</param>
		/// <returns>If <paramref name="HostName"/> is a valid ARPHANET host name.</returns>
		public static bool IsValidArpanetHostName(string HostName)
		{
			if (HostName.Length > 255)
				return false;

			Match M;

			lock (arpanetHostName)
			{
				M = arpanetHostName.Match(HostName);
			}

			return (M.Success && M.Index == 0 && M.Length == HostName.Length);
		}

		/// <summary>
		/// Next Message ID
		/// </summary>
		internal static ushort NextID
		{
			get
			{
				lock (synchObject)
				{
					return nextId++;
				}
			}
		}

		/// <summary>
		/// Resolves a DNS name.
		/// </summary>
		/// <param name="Name">Domain Name to resolve</param>
		/// <param name="TYPE">Resource Record Type of interest.</param>
		/// <param name="CLASS">Resource Record Class of interest.</param>
		/// <returns>Answer to the query</returns>
		/// <exception cref="IOException">If the domain name could not be resolved for the TYPE and CLASS provided.</exception>
		public static Task<ResourceRecord[]> Resolve(string Name, QTYPE TYPE, QCLASS CLASS)
		{
			return Resolve(Name, TYPE, null, CLASS, null);
		}

		/// <summary>
		/// Resolves a DNS name.
		/// </summary>
		/// <param name="Name">Domain Name to resolve</param>
		/// <param name="TYPE">Resource Record Type of interest.</param>
		/// <param name="CLASS">Resource Record Class of interest.</param>
		/// <param name="Thread">Optional profiling thread.</param>
		/// <returns>Answer to the query</returns>
		/// <exception cref="IOException">If the domain name could not be resolved for the TYPE and CLASS provided.</exception>
		public static Task<ResourceRecord[]> Resolve(string Name, QTYPE TYPE, QCLASS CLASS, ProfilerThread Thread)
		{
			return Resolve(Name, TYPE, null, CLASS, Thread);
		}

		/// <summary>
		/// Resolves a DNS name.
		/// </summary>
		/// <param name="Name">Domain Name to resolve</param>
		/// <param name="TYPE">Resource Record Type of interest.</param>
		/// <param name="ExceptionType">If no answer of type <paramref name="TYPE"/> is found, records of this type can also be accepted.</param>
		/// <param name="CLASS">Resource Record Class of interest.</param>
		/// <returns>Answer to the query</returns>
		/// <exception cref="IOException">If the domain name could not be resolved for the TYPE and CLASS provided.</exception>
		public static Task<ResourceRecord[]> Resolve(string Name, QTYPE TYPE, TYPE? ExceptionType, QCLASS CLASS)
		{
			return Resolve(Name, TYPE, ExceptionType, CLASS, null);
		}

		/// <summary>
		/// Resolves a DNS name.
		/// </summary>
		/// <param name="Name">Domain Name to resolve</param>
		/// <param name="TYPE">Resource Record Type of interest.</param>
		/// <param name="ExceptionType">If no answer of type <paramref name="TYPE"/> is found, records of this type can also be accepted.</param>
		/// <param name="CLASS">Resource Record Class of interest.</param>
		/// <param name="Thread">Optional profiling thread.</param>
		/// <returns>Answer to the query</returns>
		/// <exception cref="IOException">If the domain name could not be resolved for the TYPE and CLASS provided.</exception>
		public static async Task<ResourceRecord[]> Resolve(string Name, QTYPE TYPE, TYPE? ExceptionType, QCLASS CLASS, ProfilerThread Thread)
		{
			LinkedList<KeyValuePair<string, IPEndPoint>> Backup = null;
			TYPE? ExpectedType;
			IPEndPoint Destination = null;
			int Timeout = 2000;     // Local timeout

			if (Enum.TryParse<TYPE>(TYPE.ToString(), out TYPE T))
				ExpectedType = T;
			else
				ExpectedType = null;

			Thread?.NewState("Client");

			lock (synchObject)
			{
				if (nestingDepth == 0 && networkChanged)
				{
					networkChanged = false;
					udpClient?.Dispose();
					udpClient = null;
				}

				if (udpClient is null)
					udpClient = new DnsUdpClient();

				nestingDepth++;
			}

			try
			{
				while (true)
				{
					Thread?.NewState("Query");

					DnsResponse Response = await Query(Name, TYPE, CLASS, Timeout, Destination, false, Thread);
					string CName = null;

					if (Response is null)
					{
						Destination = await NextDestination(Backup);
						if (Destination is null)
							throw new IOException("Unable to resolve DNS query: " + Name + ", " + TYPE.ToString() + ", " + CLASS.ToString());

						continue;   // Check an alternative
					}

					if (!(Response.Answer is null))
					{
						if (!ExpectedType.HasValue)
							return Response.Answer;

						foreach (ResourceRecord RR in Response.Answer)
						{
							if (RR.Type == ExpectedType.Value)
								return Response.Answer;

							if (CName is null && RR.Type == Enumerations.TYPE.CNAME && RR is CNAME CNAME)
								CName = CNAME.Name2;
						}

						if (ExceptionType.HasValue)
						{
							foreach (ResourceRecord RR in Response.Answer)
							{
								if (RR.Type == ExceptionType.Value)
									return Response.Answer;
							}
						}

						if (!(CName is null))
						{
							Name = CName;
							Backup = null;
							continue;
						}
					}

					if (!(Response.Authority is null))
					{
						foreach (ResourceRecord RR in Response.Authority)
						{
							if (RR is NS NS)
							{
								string Authority = NS.Name2;
								IPAddress AuthorityAddress = null;

								if (!(Response.Additional is null))
								{
									foreach (ResourceRecord RR2 in Response.Additional)
									{
										if (RR2 is A A)
										{
											AuthorityAddress = A.Address;
											break;
										}
										else if (RR2 is AAAA AAAA)
										{
											AuthorityAddress = AAAA.Address;
											break;
										}
									}
								}

								if (Backup is null)
									Backup = new LinkedList<KeyValuePair<string, IPEndPoint>>();

								if (AuthorityAddress is null)
									Backup.AddLast(new KeyValuePair<string, IPEndPoint>(Authority, null));
								else
									Backup.AddLast(new KeyValuePair<string, IPEndPoint>(null, new IPEndPoint(AuthorityAddress, DefaultDnsPort)));
							}
						}
					}

					Destination = await NextDestination(Backup);
					if (Destination is null)
						throw new IOException("Unable to resolve DNS query: " + Name + ", " + TYPE.ToString() + ", " + CLASS.ToString());

					Timeout = 5000;
				}
			}
			finally
			{
				lock (synchObject)
				{
					nestingDepth--;
				}
			}
		}

		/// <summary>
		/// Resolves a DNS name.
		/// </summary>
		/// <param name="Name">Domain Name to resolve</param>
		/// <param name="TYPE">Resource Record Type of interest.</param>
		/// <param name="CLASS">Resource Record Class of interest.</param>
		/// <returns>Answer to the query</returns>
		/// <exception cref="IOException">If the domain name could not be resolved for the TYPE and CLASS provided.</exception>
		public static Task<DnsResponse> Query(string Name, QTYPE TYPE, QCLASS CLASS)
		{
			return Query(Name, TYPE, CLASS, null);
		}

		/// <summary>
		/// Resolves a DNS name.
		/// </summary>
		/// <param name="Name">Domain Name to resolve</param>
		/// <param name="TYPE">Resource Record Type of interest.</param>
		/// <param name="CLASS">Resource Record Class of interest.</param>
		/// <param name="Thread">Optional profiling thread.</param>
		/// <returns>Answer to the query</returns>
		/// <exception cref="IOException">If the domain name could not be resolved for the TYPE and CLASS provided.</exception>
		public static Task<DnsResponse> Query(string Name, QTYPE TYPE, QCLASS CLASS, ProfilerThread Thread)
		{
			lock (synchObject)
			{
				if (nestingDepth == 0 && networkChanged)
				{
					networkChanged = false;
					udpClient?.Dispose();
					udpClient = null;
				}

				if (udpClient is null)
					udpClient = new DnsUdpClient();

				nestingDepth++;
			}

			try
			{
				return Query(Name, TYPE, CLASS, 2000, null, true, Thread);
			}
			finally
			{
				lock (synchObject)
				{
					nestingDepth--;
				}
			}
		}

		private static async Task<DnsResponse> Query(string Name, QTYPE TYPE, QCLASS CLASS, int Timeout, IPEndPoint Destination,
			bool ReturnRaw, ProfilerThread Thread)
		{
			DnsResponse Response;
			DnsMessage Message;

			if (Database.HasProvider)
			{
				try
				{
					Thread?.NewState("Search");

					IEnumerable<DnsResponse> Records = await Database.Find<DnsResponse>();

					Response = await Database.FindFirstDeleteRest<DnsResponse>(new FilterAnd(
						new FilterFieldEqualTo("Name", Name),
						new FilterFieldEqualTo("Type", TYPE),
						new FilterFieldEqualTo("Class", CLASS)));

					if (!(Response is null) && (Response.Expires <= DateTime.Now || Response.Raw is null))
					{
						await Database.DeleteLazy(Response);
						Response = null;
					}
				}
				catch (Exception ex)
				{
					Thread?.Exception(ex);

					// Some inconsistency in database. Clear collection to get fresh set of DNS entries.
					await Database.Clear("DnsCache");
					Response = null;
				}
			}
			else
				Response = null;

			if (Response is null)
			{
				bool Save = true;
				DnsClient Client;

				if (Destination is null)
				{
					Client = httpsClient;
					if (Client is null)
					{
						Client = udpClient;
						Thread?.NewState("UDP");
					}
					else
						Thread?.NewState("HTTPS");
				}
				else
				{
					Client = udpClient;
					Thread?.NewState("UDP");
				}

				Client.Thread = Thread;

				try
				{
					Message = await Client.SendRequestAsync(OpCode.Query, true, new Question[]
					{
						new Question(Name, TYPE, CLASS)
					}, Destination, Timeout);

					switch (Message.RCode)
					{
						case RCode.NXDomain:
							if (ReturnRaw)
								Save = false;
							else
								throw new GenericException("Domain name not found: " + Name, Object: Name);
							break;
					}
				}
				catch (TimeoutException ex)
				{
					Thread?.Exception(ex);
					Message = null;
				}
				finally
				{
					Client.Thread = null;
				}

				if (Message is null || Message.RCode != RCode.NoError)
				{
					if (ReturnRaw)
						Save = false;
					else
						return null;
				}

				uint Ttl = 60 * 60 * 24 * 30;       // Maximum TTL = 30 days

				Response = new DnsResponse()
				{
					Name = Name,
					Type = TYPE,
					Class = CLASS,
					Answer = CheckTtl(ref Ttl, Message?.Answer),
					Authority = CheckTtl(ref Ttl, Message?.Authority),
					Additional = CheckTtl(ref Ttl, Message?.Additional),
					Raw = Message?.Binary
				};

				Response.Expires = DateTime.Now.AddSeconds(Ttl);

				if (Save && Database.HasProvider)
				{
					Thread?.NewState("Store");
					try
					{
						await Database.InsertLazy(Response);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}

			return Response;
		}

		private static async Task<IPEndPoint> NextDestination(LinkedList<KeyValuePair<string, IPEndPoint>> Backup)
		{
			IPEndPoint Destination = null;

			while (Destination is null && !(Backup?.First is null))
			{
				KeyValuePair<string, IPEndPoint> P = Backup.First.Value;
				Backup.RemoveFirst();

				Destination = P.Value;

				if (Destination is null)
				{
					IPAddress[] Addresses;

					try
					{
						Addresses = await LookupIP4Addresses(P.Key);
					}
					catch (Exception)
					{
						Addresses = null;
					}

					if (Addresses is null || Addresses.Length == 0)
					{
						try
						{
							Addresses = await LookupIP6Addresses(P.Key);
						}
						catch (Exception)
						{
							Addresses = null;
						}
					}

					if (!(Addresses is null))
					{
						foreach (IPAddress Address in Addresses)
						{
							IPEndPoint EP = new IPEndPoint(Address, DefaultDnsPort);

							if (Destination is null)
								Destination = EP;
							else
							{
								if (Backup is null)
									Backup = new LinkedList<KeyValuePair<string, IPEndPoint>>();

								Backup.AddLast(new KeyValuePair<string, IPEndPoint>(null, EP));
							}
						}
					}
				}
			}

			return Destination;
		}

		private static ResourceRecord[] CheckTtl(ref uint Ttl, ResourceRecord[] Records)
		{
			if (!(Records is null))
			{
				foreach (ResourceRecord RR in Records)
				{
					if (RR.Ttl < Ttl)
						Ttl = RR.Ttl;
				}
			}

			return Records;
		}

		/// <summary>
		/// Looks up the IPv4 addresses related to a given domain name.
		/// </summary>
		/// <param name="DomainName">Domain name.</param>
		/// <returns>IPv4 address found related to the domain name.</returns>
		/// <exception cref="ArgumentException">If the name does not exist.</exception>
		/// <exception cref="IOException">If the name could not be resolved.</exception>
		public static async Task<IPAddress[]> LookupIP4Addresses(string DomainName)
		{
			List<IPAddress> Result = new List<IPAddress>();

			foreach (ResourceRecord RR in await Resolve(DomainName, QTYPE.A, QCLASS.IN))
			{
				if (RR is A A)
					Result.Add(A.Address);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Looks up the IPv6 addresses related to a given domain name.
		/// </summary>
		/// <param name="DomainName">Domain name.</param>
		/// <returns>IPv6 address found related to the domain name.</returns>
		/// <exception cref="ArgumentException">If the name does not exist.</exception>
		/// <exception cref="IOException">If the name could not be resolved.</exception>
		public static async Task<IPAddress[]> LookupIP6Addresses(string DomainName)
		{
			List<IPAddress> Result = new List<IPAddress>();

			foreach (ResourceRecord RR in await Resolve(DomainName, QTYPE.AAAA, QCLASS.IN))
			{
				if (RR is AAAA AAAA)
					Result.Add(AAAA.Address);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Looks up the Mail Exchanges related to a given domain name.
		/// </summary>
		/// <param name="DomainName">Domain name.</param>
		/// <returns>Mail Exchanges found related to the domain name.</returns>
		/// <exception cref="ArgumentException">If the name does not exist.</exception>
		/// <exception cref="IOException">If the name could not be resolved.</exception>
		public static async Task<string[]> LookupMailExchange(string DomainName)
		{
			List<MX> Records = new List<MX>();
			ResourceRecord[] RRs = await Resolve(DomainName, QTYPE.MX, TYPE.A, QCLASS.IN);

			foreach (ResourceRecord RR in RRs)
			{
				if (RR is MX MX)
					Records.Add(MX);
			}

			if (Records.Count == 0)
			{
				foreach (ResourceRecord RR in RRs)
				{
					if (RR is A A)
						Records.Add(new MX() { Exchange = A.Address.ToString() });
				}
			}
			else
				Records.Sort((r1, r2) => r2.Preference - r1.Preference);    // Descending

			int i, c = Records.Count;
			string[] Result = new string[c];

			for (i = 0; i < c; i++)
				Result[i] = Records[i].Exchange;

			return Result;
		}

		/// <summary>
		/// Converts an IP Address to a domain name for reverse IP lookup, or DNSBL lookup.
		/// </summary>
		/// <param name="Address">IP Address</param>
		/// <param name="IP4DomainName">IP4 Domain Name</param>
		/// <param name="IP6DomainName">IP6 Domain Name</param>
		/// <returns>Domain Name representing IP address.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If Address Type does not have a corresponding domain name.</exception>
		public static string AddressToName(IPAddress Address, string IP4DomainName, string IP6DomainName)
		{
			byte[] Bin = Address.GetAddressBytes();

			switch (Bin.Length)
			{
				case 4:
					if (string.IsNullOrEmpty(IP4DomainName))
						throw new ArgumentOutOfRangeException("IPv4 addresses not supported.");

					StringBuilder sb = new StringBuilder();
					int i;

					for (i = 3; i >= 0; i--)
					{
						sb.Append(Bin[i].ToString());
						sb.Append('.');
					}

					sb.Append(IP4DomainName);

					return sb.ToString();

				case 16:
					if (string.IsNullOrEmpty(IP6DomainName))
						throw new ArgumentOutOfRangeException("IPv6 addresses not supported.");

					byte b, b2;

					sb = new StringBuilder();

					for (i = 15; i >= 0; i--)
					{
						b = Bin[i];
						b2 = (byte)(b & 15);
						if (b2 < 10)
							sb.Append((char)('0' + b2));
						else
							sb.Append((char)('A' + b2 - 10));

						sb.Append('.');

						b2 = (byte)(b >> 4);
						if (b2 < 10)
							sb.Append((char)('0' + b2));
						else
							sb.Append((char)('A' + b2 - 10));

						sb.Append('.');
					}

					sb.Append(IP6DomainName);

					return sb.ToString();

				default:
					throw new ArgumentOutOfRangeException("Unrecognized IP address.", nameof(Address));
			}
		}

		/// <summary>
		/// Looks up the domain name pointing to a specific IP address.
		/// 
		/// Note: The response is not necessarily unique or authoritative.
		/// </summary>
		/// <param name="Address">IP Address</param>
		/// <returns>Domain Name pointing to the address</returns>
		public static async Task<string[]> LookupDomainName(IPAddress Address)
		{
			string Name = AddressToName(Address, "IN-ADDR.ARPA", "IP6.ARPA");
			List<string> Result = new List<string>();

			foreach (ResourceRecord RR in await Resolve(Name, QTYPE.PTR, QCLASS.IN))
			{
				if (RR is PTR PTR)
					Result.Add(PTR.Name2);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Looks up text (TXT) records for a name.
		/// </summary>
		/// <param name="Name">Domain name.</param>
		/// <returns>Available text.</returns>
		public static async Task<string[]> LookupText(string Name)
		{
			List<string> Result = new List<string>();

			foreach (ResourceRecord RR in await Resolve(Name, QTYPE.TXT, QCLASS.IN))
			{
				if (RR is TXT TXT)
					Result.AddRange(TXT.Text);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Looks up an IP Address in a DNS Block List.
		/// </summary>
		/// <param name="Address">IP Address</param>
		/// <param name="BlackListDomainName">Black List Domain Name.</param>
		/// <returns>null, if IP Address is NOT on list. Otherwise, it contains the reasons why it is on the list.</returns>
		/// <exception cref="ArgumentOutOfRangeException">IP Address not supported.</exception>
		public static async Task<string[]> LookupBlackList(IPAddress Address, string BlackListDomainName)
		{
			string Name = AddressToName(Address, BlackListDomainName, null);
			ResourceRecord[] As;

			try
			{
				As = await Resolve(Name, QTYPE.A, QCLASS.IN);
			}
			catch (ArgumentException)
			{
				return null;
			}

			List<string> Result = null;

			try
			{
				foreach (ResourceRecord RR in await Resolve(Name, QTYPE.TXT, QCLASS.IN))
				{
					if (RR is TXT TXT)
					{
						if (Result is null)
							Result = new List<string>();

						Result.AddRange(TXT.Text);
					}
				}

				if (!(Result is null))
					return Result.ToArray();
			}
			catch (Exception)
			{
				// Ignore
			}

			foreach (ResourceRecord RR in As)
			{
				if (RR is A A)
				{
					if (Result is null)
						Result = new List<string>();

					Result.Add(A.Address.ToString());
				}
			}

			return Result?.ToArray();
		}

		/// <summary>
		/// Looks up a service endpoint for a domain. If multiple are available,
		/// an appropriate one is selected as defined by RFC 2782.
		/// </summary>
		/// <param name="DomainName">Domain name.</param>
		/// <param name="ServiceName">Service name.</param>
		/// <param name="Protocol">Protocol name. Example: "tcp", "udp", etc.</param>
		/// <returns>Service Endpoint for service.</returns>
		/// <exception cref="IOException">If service endpoint could not be resolved.</exception>
		public static Task<SRV> LookupServiceEndpoint(string DomainName, string ServiceName, string Protocol)
		{
			return LookupServiceEndpoint(DomainName, ServiceName, Protocol, null);
		}

		/// <summary>
		/// Looks up a service endpoint for a domain. If multiple are available,
		/// an appropriate one is selected as defined by RFC 2782.
		/// </summary>
		/// <param name="DomainName">Domain name.</param>
		/// <param name="ServiceName">Service name.</param>
		/// <param name="Protocol">Protocol name. Example: "tcp", "udp", etc.</param>
		/// <param name="Thread">Optional profiling thread.</param>
		/// <returns>Service Endpoint for service.</returns>
		/// <exception cref="IOException">If service endpoint could not be resolved.</exception>
		public static async Task<SRV> LookupServiceEndpoint(string DomainName, string ServiceName, string Protocol, ProfilerThread Thread)
		{
			Thread?.NewState("Resolve");

			string Name = "_" + ServiceName + "._" + Protocol.ToLower() + "." + DomainName;
			SortedDictionary<ushort, List<SRV>> ServicesByPriority = new SortedDictionary<ushort, List<SRV>>();
			List<SRV> SamePriority;

			foreach (ResourceRecord RR in await Resolve(Name, QTYPE.SRV, QCLASS.IN, Thread))
			{
				if (RR is SRV SRV)
				{
					if (!ServicesByPriority.TryGetValue(SRV.Priority, out SamePriority))
					{
						SamePriority = new List<SRV>();
						ServicesByPriority[SRV.Priority] = SamePriority;
					}

					SamePriority.Add(SRV);
				}
			}

			Thread?.NewState("Select");

			while (true)
			{
				ushort? FirstKey = null;

				SamePriority = null;
				foreach (KeyValuePair<ushort, List<SRV>> P in ServicesByPriority)
				{
					FirstKey = P.Key;
					SamePriority = P.Value;
					break;
				}

				if (!FirstKey.HasValue)
					throw new IOException("Service Endpoint not found.");

				int TotWeight = 0;
				int i;

				foreach (SRV SRV in SamePriority)
					TotWeight += SRV.Weight;

				SRV Selected = null;

				if (TotWeight > 0)
				{
					lock (rnd)
					{
						i = rnd.Next(TotWeight);
					}

					foreach (SRV SRV in SamePriority)
					{
						if (i < SRV.Weight)
						{
							Selected = SRV;
							SamePriority.Remove(SRV);
							if (SamePriority.Count == 0)
								ServicesByPriority.Remove(FirstKey.Value);
							break;
						}
						else
							i -= SRV.Weight;
					}
				}
				else
				{
					foreach (SRV SRV in SamePriority)
					{
						Selected = SRV;
						SamePriority.Remove(SRV);
						if (SamePriority.Count == 0)
							ServicesByPriority.Remove(FirstKey.Value);
						break;
					}
				}

				// TODO: Check host availability on the given port... If not available, continue with next.

				if (Selected is null)
					ServicesByPriority.Remove(FirstKey.Value);
				else if (Selected.TargetHost != ".")
					return Selected;
			}
		}

		/// <summary>
		/// Looks up a available service endpoints for a domain.
		/// </summary>
		/// <param name="DomainName">Domain name.</param>
		/// <param name="ServiceName">Service name.</param>
		/// <param name="Protocol">Protocol name. Example: "tcp", "udp", etc.</param>
		/// <returns>Service Endpoints for service.</returns>
		/// <exception cref="IOException">If service endpoint could not be resolved.</exception>
		public static async Task<SRV[]> LookupServiceEndpoints(string DomainName, string ServiceName, string Protocol)
		{
			string Name = "_" + ServiceName + "._" + Protocol.ToLower() + "." + DomainName;
			List<SRV> Result = new List<SRV>();

			foreach (ResourceRecord RR in await Resolve(Name, QTYPE.SRV, QCLASS.IN))
			{
				if (RR is SRV SRV)
					Result.Add(SRV);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="MaxValue">The exclusive upper bound of the random number to be generated. maxValue must
		/// be greater than or equal to 0.</param>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue;
		/// that is, the range of return values ordinarily includes 0 but not maxValue. However,
		/// if maxValue equals 0, maxValue is returned.</returns>
		/// <exception cref="ArgumentOutOfRangeException">maxValue is less than 0</exception>
		public static int Next(int MaxValue)
		{
			lock (rnd)
			{
				return rnd.Next(MaxValue);
			}
		}

	}
}
