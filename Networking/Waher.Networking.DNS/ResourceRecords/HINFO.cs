using System;
using System.Collections.Generic;
using System.Text;
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
		/// <param name="CPU">Specifies the CPU type.</param>
		/// <param name="OS">Specifies the Operating System type.</param>
		public HINFO(string Name, TYPE Type, CLASS Class, uint Ttl, string CPU, string OS)
			: base(Name, Type, Class, Ttl)
		{
			this.cpu = CPU;
			this.os = OS;
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
