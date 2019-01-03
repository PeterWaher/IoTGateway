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
				case TYPE.A: Response = new A(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.NS: Response = new NS(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MD: Response = new MD(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MF: Response = new MF(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.CNAME: Response = new CNAME(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.SOA: Response = new SOA(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MB: Response = new MB(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MG: Response = new MG(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MR: Response = new MR(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.NULL: Response = new NULL(NAME, TYPE, CLASS, TTL, Data, EndPos); break;
				case TYPE.WKS: Response = new WKS(NAME, TYPE, CLASS, TTL, Data, EndPos); break;
				case TYPE.PTR: Response = new PTR(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.HINFO: Response = new HINFO(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MINFO: Response = new MINFO(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.MX: Response = new MX(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.TXT: Response = new TXT(NAME, TYPE, CLASS, TTL, Data, EndPos); break;
				case TYPE.AAAA: Response = new AAAA(NAME, TYPE, CLASS, TTL, Data); break;
				case TYPE.SRV: Response = new SRV(NAME, TYPE, CLASS, TTL, Data); break;

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
