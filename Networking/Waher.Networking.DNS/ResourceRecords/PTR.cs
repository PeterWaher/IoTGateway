using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Pointer
	/// </summary>
	public class PTR : ResourceNameRecord
	{
		/// <summary>
		/// Pointer
		/// </summary>
		/// <param name="Name">Points to some location in the domain name space.</param>
		public PTR(string Name)
			: base(Name)
		{
		}
	}
}
