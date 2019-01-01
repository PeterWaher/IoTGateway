using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

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
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Preference">Preference given to
		/// this RR among others at the same owner.Lower values
		/// are preferred.</param>
		/// <param name="Name2">Specifies a host willing to act as
		/// a mail exchange for the owner name.</param>
		public MX(string Name, TYPE Type, CLASS Class, uint Ttl, ushort Preference, string Name2)
			: base(Name, Type, Class, Ttl, Name2)
		{
			this.preference = Preference;
		}

		/// <summary>
		/// Preference given to this RR among others at the same owner.Lower values
		/// are preferred.
		/// </summary>
		public ushort Preference => this.preference;

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.preference.ToString();
		}
	}
}
