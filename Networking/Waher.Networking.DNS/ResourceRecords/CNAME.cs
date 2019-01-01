using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Canonical NAME
	/// </summary>
	public class CNAME : ResourceNameRecord
	{
		/// <summary>
		/// Canonical NAME
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Name2">
		/// A domain name which specifies the canonical or primary
		/// name for the owner.The owner name is an alias.
		/// </param>
		public CNAME(string Name, TYPE Type, CLASS Class, uint Ttl, string Name2)
			: base(Name, Type, Class, Ttl, Name2)
		{
		}
	}
}
