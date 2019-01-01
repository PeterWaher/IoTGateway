using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="CPU">Specifies the CPU type.</param>
		/// <param name="OS">Specifies the Operating System type.</param>
		public HINFO(string CPU, string OS)
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
	}
}
