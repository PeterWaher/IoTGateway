using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Abstract base class for Resource Address Records.
	/// </summary>
	public abstract class ResourceAddressRecord : ResourceRecord
	{
		private readonly IPAddress address;

		/// <summary>
		/// Abstract base class for Resource Address Records.
		/// </summary>
		public ResourceAddressRecord(IPAddress Address)
		{
			this.address = Address;
		}

		/// <summary>
		/// IP address
		/// </summary>
		public IPAddress Address => this.address;
	}
}
