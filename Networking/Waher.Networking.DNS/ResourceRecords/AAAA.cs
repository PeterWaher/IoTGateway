using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Internet Address
	/// </summary>
	public class AAAA : ResourceAddressRecord
	{
		/// <summary>
		/// Internet Address
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public AAAA(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl, Data)
		{
		}

		/// <summary>
		/// IP Address size.
		/// </summary>
		protected override int AddressSize => 16;
	}
}
