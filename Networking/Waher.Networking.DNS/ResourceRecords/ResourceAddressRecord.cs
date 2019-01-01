using System;
using System.Collections.Generic;
using System.Net;
using Waher.Networking.DNS.Enumerations;

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
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Address">Referenced IP Address</param>
		public ResourceAddressRecord(string Name, TYPE Type, CLASS Class, uint Ttl, IPAddress Address)
			: base(Name, Type, Class, Ttl)
		{
			this.address = Address;
		}

		/// <summary>
		/// IP address
		/// </summary>
		public IPAddress Address => this.address;

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.address.ToString();
		}
	}
}
