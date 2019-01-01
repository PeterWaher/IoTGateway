using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="Name">Host which specifies a mailbox which is a
		/// member of the mail group specified by the domain name.</param>
		public MG(string Name)
			: base(Name)
		{
		}
	}
}
