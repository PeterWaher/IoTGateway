using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mailbox (Experimental)
	/// </summary>
	public class MB : ResourceNameRecord
	{
		/// <summary>
		/// Mailbox (Experimental)
		/// </summary>
		public MB()
			: base()
		{
		}

		/// <summary>
		/// Mailbox (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public MB(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl, Data)
		{
		}
	}
}
