using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Waher.Networking.DNS.Enumerations;
namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Abstract base class for a resource record.
	/// </summary>
	public abstract class ResourceRecord
	{
		/// <summary>
		/// Creates a resource record from its binary representation-
		/// </summary>
		/// <param name="Data">Binary representation of a resource record.</param>
		/// <returns>Resource Record.</returns>
		internal static ResourceRecord Create(Stream Data)
		{
			string NAME = DnsResolver.ReadName(Data);
			TYPE TYPE = (TYPE)DnsResolver.ReadUInt16(Data);
			CLASS CLASS = (CLASS)DnsResolver.ReadUInt16(Data);
			uint TTL = DnsResolver.ReadUInt32(Data);
			ushort RDLENGTH = DnsResolver.ReadUInt16(Data);
			long EndPos = Data.Position + RDLENGTH;
			ResourceRecord Response;

			switch (TYPE)
			{
				case TYPE.A:
					byte[] Bin = new byte[4];
					Data.Read(Bin, 0, 4);
					IPAddress Address = new IPAddress(Bin);

					Response = new A(Address);
					break;

				case TYPE.NS:
					string Name = DnsResolver.ReadName(Data);
					Response = new NS(Name);
					break;

				case TYPE.MD:
					Name = DnsResolver.ReadName(Data);
					Response = new MD(Name);
					break;

				case TYPE.MF:
					Name = DnsResolver.ReadName(Data);
					Response = new MF(Name);
					break;

				case TYPE.CNAME:
					Name = DnsResolver.ReadName(Data);
					Response = new CNAME(Name);
					break;

				case TYPE.SOA:
					string MNAME = DnsResolver.ReadName(Data);
					string RNAME = DnsResolver.ReadName(Data);
					uint SERIAL = DnsResolver.ReadUInt32(Data);
					uint REFRESH = DnsResolver.ReadUInt32(Data);
					uint RETRY = DnsResolver.ReadUInt32(Data);
					uint EXPIRE = DnsResolver.ReadUInt32(Data);
					uint MINIMUM = DnsResolver.ReadUInt32(Data);
					Response = new SOA(MNAME, RNAME, SERIAL, REFRESH, RETRY, EXPIRE, MINIMUM);
					break;

				case TYPE.MB:
					Name = DnsResolver.ReadName(Data);
					Response = new MB(Name);
					break;

				case TYPE.MG:
					Name = DnsResolver.ReadName(Data);
					Response = new MG(Name);
					break;

				case TYPE.MR:
					Name = DnsResolver.ReadName(Data);
					Response = new MR(Name);
					break;

				case TYPE.NULL:
					int c = (int)(EndPos - Data.Position);
					Bin = new byte[c];
					Data.Read(Bin, 0, c);

					Response = new NULL(Bin);
					break;

				case TYPE.WKS:
					Bin = new byte[4];
					Data.Read(Bin, 0, 4);
					Address = new IPAddress(Bin);
					byte Protocol = (byte)Data.ReadByte();
					c = (int)(EndPos - Data.Position);
					Bin = new byte[c];
					Data.Read(Bin, 0, c);
					BitArray BitMap = new BitArray(Bin);

					Response = new WKS(Address, Protocol, BitMap);
					break;

				case TYPE.PTR:
					Name = DnsResolver.ReadName(Data);
					Response = new PTR(Name);
					break;

				case TYPE.HINFO:
					string Cpu = DnsResolver.ReadLabel(Data);
					string Os = DnsResolver.ReadLabel(Data);
					Response = new HINFO(Cpu, Os);
					break;

				case TYPE.MINFO:
					string RMailBx = DnsResolver.ReadName(Data);
					string EMailBx = DnsResolver.ReadName(Data);
					Response = new MINFO(RMailBx, EMailBx);
					break;

				case TYPE.MX:
					ushort Preference = DnsResolver.ReadUInt16(Data);
					string Exchange = DnsResolver.ReadName(Data);
					Response = new MX(Preference, Exchange);
					break;

				case TYPE.TXT:
					List<string> Text = new List<string>();

					while (Data.Position < EndPos)
						Text.Add(DnsResolver.ReadLabel(Data));

					Response = new TXT(Text.ToArray());
					break;

				default:
					Response = null;	// Unrecognized Resource Record.
					break;
			}

			Data.Position = EndPos;

			return Response;
		}

	}
}
