using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mail forwarding (Obsolete)
	/// </summary>
	public class MF : ResourceNameRecord
	{
		/// <summary>
		/// Mail forwarding (Obsolete)
		/// </summary>
		/// <param name="Name">
		/// Host which has a mail agent for the domain which should be able to 
		/// forward mail for the domain.
		/// </param>
		public MF(string Name)
			: base(Name)
		{
		}
	}
}
