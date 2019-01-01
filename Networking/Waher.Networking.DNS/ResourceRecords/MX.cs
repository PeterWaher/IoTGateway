using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mailbox eXchange
	/// </summary>
	public class MX : ResourceNameRecord
	{
		private readonly ushort preference;

		/// <summary>
		/// Mailbox eXchange
		/// </summary>
		/// <param name="Preference">Preference given to
		/// this RR among others at the same owner.Lower values
		/// are preferred.</param>
		/// <param name="Name">Specifies a host willing to act as
		/// a mail exchange for the owner name.</param>
		public MX(ushort Preference, string Name)
			: base(Name)
		{
			this.preference = Preference;
		}

		/// <summary>
		/// Preference given to this RR among others at the same owner.Lower values
		/// are preferred.
		/// </summary>
		public ushort Preference => this.preference;
	}
}
