using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="Name">
		/// Host which has a mail agent for the domain which should be able to 
		/// deliver mail for the domain.
		/// </param>
		public MD(string Name)
			: base(Name)
		{
		}
	}
}
