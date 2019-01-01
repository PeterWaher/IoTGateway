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
	/// </summary>
	public static class DnsResolver
	{
		/// <summary>
		/// 53
		/// </summary>
		public const int DefaultDnsPort = 53;

		private static readonly Regex arpanetHostName = new Regex(@"^[a-zA-Z]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?([.][a-zA-Z]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled | RegexOptions.Singleline);

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

		internal static uint ReadUInt32(Stream Data)
		{
			ushort Result = ReadUInt16(Data);
			Result <<= 16;
			Result |= ReadUInt16(Data);

			return Result;
		}

		internal static ushort ReadUInt16(Stream Data)
		{
			ushort Result = (byte)Data.ReadByte();
			Result <<= 8;
			Result |= (byte)Data.ReadByte();

			return Result;
		}

		internal static string ReadName(Stream Data)
		{
			StringBuilder sb = null;
			string s;

			while (!string.IsNullOrEmpty(s = ReadName(Data)))
			{
				if (sb is null)
					sb = new StringBuilder();
				else
					sb.Append('.');

				sb.Append(s);
			}

			return sb?.ToString() ?? string.Empty;
		}

		internal static string ReadLabel(Stream Data)
		{
			int Len = Data.ReadByte();
			if (Len == 0)
				return string.Empty;

			switch (Len & 192)
			{
				case 0:
					byte[] Bin = new byte[Len];
					Data.Read(Bin, 0, Len);

					return Encoding.ASCII.GetString(Bin);

				case 192:
					long Bak = Data.Position;
					Data.Position = Len & 63;

					string s = ReadName(Data);

					Data.Position = Bak;
					return s;

				default:
					throw new NotSupportedException("Unsupported Label Type.");
			}
		}

		internal static ResourceRecord[] ReadResourceRecords(Stream Data, ushort Count)
		{
			List<ResourceRecord> Result = new List<ResourceRecord>();
			ResourceRecord Rec;

			while (Count > 0)
			{
				Count--;
				Rec = ResourceRecord.Create(Data);

				if (!(Rec is null))
					Result.Add(Rec);
			}

			return Result.ToArray();
		}

	}
}
