using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="Name">
		/// A domain name which specifies the canonical or primary
		/// name for the owner.The owner name is an alias.
		/// </param>
		public CNAME(string Name)
			: base(Name)
		{
		}
	}
}
