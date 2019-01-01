using System;
using System.Collections.Generic;
using System.Net;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Internet Address
	/// </summary>
	public class A : ResourceAddressRecord
	{
		/// <summary>
		/// Internet Address
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Address">Referenced IP Address</param>
		public A(string Name, TYPE Type, CLASS Class, uint Ttl, IPAddress Address)
			: base(Name, Type, Class, Ttl, Address)
		{
		}
	}
}
