using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Name Server
	/// </summary>
	public class NS : ResourceNameRecord
	{
		/// <summary>
		/// Name Server
		/// </summary>
		/// <param name="Name">Specifies a host which should be
		/// authoritative for the specified class and domain.</param>
		public NS(string Name)
			: base(Name)
		{
		}
	}
}
