using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// General information about a host.
	/// </summary>
	public class HINFO : ResourceRecord
	{
		private readonly string cpu;
		private readonly string os;

		/// <summary>
		/// General information about a host.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public HINFO(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl)
		{
			this.cpu = DnsResolver.ReadString(Data);
			this.os = DnsResolver.ReadString(Data);
		}

		/// <summary>
		/// Specifies the CPU type.
		/// </summary>
		public string CPU => this.cpu;

		/// <summary>
		/// Specifies the OS type.
		/// </summary>
		public string OS => this.os;

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.cpu + "\t" + this.os;
		}
	}
}
