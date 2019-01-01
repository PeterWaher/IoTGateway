using System;
using System.Collections.Generic;
using System.Text;

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
		/// <param name="Name">Host which has the specified mailbox.</param>
		public MB(string Name)
			: base(Name)
		{
		}
	}
}
