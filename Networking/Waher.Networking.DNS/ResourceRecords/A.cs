using System;
using System.Collections.Generic;
using System.Net;

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
		public A(IPAddress Address)
			: base(Address)
		{
		}
	}
}
