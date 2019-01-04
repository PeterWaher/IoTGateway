using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// General information about a host.
	/// </summary>
	public class HINFO : ResourceRecord
	{
		private string cpu;
		private string os;

		/// <summary>
		/// General information about a host.
		/// </summary>
		public HINFO()
			: base()
		{
			this.cpu = string.Empty;
			this.os = string.Empty;
		}

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
			this.cpu = DnsClient.ReadString(Data);
			this.os = DnsClient.ReadString(Data);
		}

		/// <summary>
		/// Specifies the CPU type.
		/// </summary>
		[DefaultValueStringEmpty]
		public string CPU
		{
			get => this.cpu;
			set => this.cpu = value;
		}

		/// <summary>
		/// Specifies the OS type.
		/// </summary>
		[DefaultValueStringEmpty]
		public string OS
		{
			get => this.os;
			set => this.os = value;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.cpu + "\t" + this.os;
		}
	}
}
