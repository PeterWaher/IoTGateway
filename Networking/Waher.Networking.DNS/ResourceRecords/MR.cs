using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mailbox Renaming (Experimental)
	/// </summary>
	public class MR : ResourceNameRecord
	{
		/// <summary>
		/// Mailbox Renaming (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Name2">specifies a mailbox which is the
		/// proper rename of the specified mailbox.</param>
		public MR(string Name, TYPE Type, CLASS Class, uint Ttl, string Name2)
			: base(Name, Type, Class, Ttl, Name2)
		{
		}
	}
}
