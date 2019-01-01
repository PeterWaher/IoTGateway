using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mail delivery (Obsolete)
	/// </summary>
	public class MD : ResourceNameRecord
	{
		/// <summary>
		/// Mail delivery (Obsolete)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Name2">
		/// Host which has a mail agent for the domain which should be able to 
		/// deliver mail for the domain.
		/// </param>
		public MD(string Name, TYPE Type, CLASS Class, uint Ttl, string Name2)
			: base(Name, Type, Class, Ttl, Name2)
		{
		}
	}
}
