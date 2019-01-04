using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Networking.DNS.ResourceRecords;

namespace Waher.Networking.DNS
{
	/// <summary>
	/// DNS resolver, as defined in:
	/// 
	/// RFC 1034: https://tools.ietf.org/html/rfc1034: DOMAIN NAMES - CONCEPTS AND FACILITIES
	/// RFC 1035: https://tools.ietf.org/html/rfc1035: DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION
	/// RFC 2782: https://tools.ietf.org/html/rfc2782: A DNS RR for specifying the location of services (DNS SRV)
	/// RFC 3596: https://tools.ietf.org/html/rfc3596: DNS Extensions to Support IP Version 6
	/// </summary>
	public static class DnsResolver
	{
		/// <summary>
		/// 53
		/// </summary>
		public const int DefaultDnsPort = 53;

		private static readonly Regex arpanetHostName = new Regex(@"^[a-zA-Z]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?([.][a-zA-Z]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly object synchObject = new object();
		private static ushort nextId = 0;

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

	}
}
