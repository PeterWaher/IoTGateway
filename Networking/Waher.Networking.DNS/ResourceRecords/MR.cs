using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="Name">specifies a mailbox which is the
		/// proper rename of the specified mailbox.</param>
		public MR(string Name)
			: base(Name)
		{
		}
	}
}
