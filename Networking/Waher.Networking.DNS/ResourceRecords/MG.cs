using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mail group (Experimental)
	/// </summary>
	public class MG : ResourceNameRecord
	{
		/// <summary>
		/// Mail group (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Name2">Host which specifies a mailbox which is a
		/// member of the mail group specified by the domain name.</param>
		public MG(string Name, TYPE Type, CLASS Class, uint Ttl, string Name2)
			: base(Name, Type, Class, Ttl, Name2)
		{
		}
	}
}
