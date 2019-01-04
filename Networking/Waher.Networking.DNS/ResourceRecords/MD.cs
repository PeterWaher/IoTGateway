using System;
using System.Collections.Generic;
using System.IO;
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
		public MD()
			: base()
		{
		}

		/// <summary>
		/// Mail delivery (Obsolete)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public MD(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl, Data)
		{
		}
	}
}
