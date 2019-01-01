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
		private readonly string name;
		private readonly TYPE type;
		private readonly CLASS _class;
		private readonly uint ttl;

		/// <summary>
		/// Abstract base class for a resource record.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		public ResourceRecord(string Name, TYPE Type, CLASS Class, uint Ttl)
		{
			this.name = Name;
			this.type = Type;
			this._class = Class;
			this.ttl = Ttl;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Resource Record Type
		/// </summary>
		public TYPE Type => this.type;

		/// <summary>
		/// Resource Record Class
		/// </summary>
		public CLASS Class => this._class;

		/// <summary>
		/// Time To Live
		/// </summary>
		public uint Ttl => this.ttl;

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

					Response = new A(NAME, TYPE, CLASS, TTL, Address);
					break;

				case TYPE.NS:
					string Name2 = DnsResolver.ReadName(Data);
					Response = new NS(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.MD:
					Name2 = DnsResolver.ReadName(Data);
					Response = new MD(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.MF:
					Name2 = DnsResolver.ReadName(Data);
					Response = new MF(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.CNAME:
					Name2 = DnsResolver.ReadName(Data);
					Response = new CNAME(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.SOA:
					string MNAME = DnsResolver.ReadName(Data);
					string RNAME = DnsResolver.ReadName(Data);
					uint SERIAL = DnsResolver.ReadUInt32(Data);
					uint REFRESH = DnsResolver.ReadUInt32(Data);
					uint RETRY = DnsResolver.ReadUInt32(Data);
					uint EXPIRE = DnsResolver.ReadUInt32(Data);
					uint MINIMUM = DnsResolver.ReadUInt32(Data);
					Response = new SOA(NAME, TYPE, CLASS, TTL, MNAME, RNAME, SERIAL, REFRESH, RETRY, EXPIRE, MINIMUM);
					break;

				case TYPE.MB:
					Name2 = DnsResolver.ReadName(Data);
					Response = new MB(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.MG:
					Name2 = DnsResolver.ReadName(Data);
					Response = new MG(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.MR:
					Name2 = DnsResolver.ReadName(Data);
					Response = new MR(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.NULL:
					int c = (int)(EndPos - Data.Position);
					Bin = new byte[c];
					Data.Read(Bin, 0, c);

					Response = new NULL(NAME, TYPE, CLASS, TTL, Bin);
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

					Response = new WKS(NAME, TYPE, CLASS, TTL, Address, Protocol, BitMap);
					break;

				case TYPE.PTR:
					Name2 = DnsResolver.ReadName(Data);
					Response = new PTR(NAME, TYPE, CLASS, TTL, Name2);
					break;

				case TYPE.HINFO:
					string Cpu = DnsResolver.ReadString(Data);
					string Os = DnsResolver.ReadString(Data);
					Response = new HINFO(NAME, TYPE, CLASS, TTL, Cpu, Os);
					break;

				case TYPE.MINFO:
					string RMailBx = DnsResolver.ReadName(Data);
					string EMailBx = DnsResolver.ReadName(Data);
					Response = new MINFO(NAME, TYPE, CLASS, TTL, RMailBx, EMailBx);
					break;

				case TYPE.MX:
					ushort Preference = DnsResolver.ReadUInt16(Data);
					string Exchange = DnsResolver.ReadName(Data);
					Response = new MX(NAME, TYPE, CLASS, TTL, Preference, Exchange);
					break;

				case TYPE.TXT:
					List<string> Text = new List<string>();

					while (Data.Position < EndPos)
						Text.Add(DnsResolver.ReadString(Data));

					Response = new TXT(NAME, TYPE, CLASS, TTL, Text.ToArray());
					break;

				default:
					Response = null;    // Unrecognized Resource Record.
					break;
			}

			Data.Position = EndPos;

			return Response;
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.name + "\t" + this.type.ToString() + "\t" + this._class.ToString() +
				"\t" + this.ttl.ToString();
		}

	}
}
