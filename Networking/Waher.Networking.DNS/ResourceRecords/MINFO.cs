using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mail information about a host. (Experimental)
	/// </summary>
	public class MINFO : ResourceRecord	
	{
		private readonly string rMailBx;
		private readonly string eMailBx;

		/// <summary>
		/// Mail information about a host. (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="RMailBx">Specifies a mailbox which is
		/// responsible for the mailing list or mailbox.</param>
		/// <param name="EMailBx">Specifies a mailbox which is to
		/// receive error messages related to the mailing list or
		/// mailbox specified by the owner of the MINFO RR(similar
		/// to the ERRORS-TO: field which has been proposed).</param>
		public MINFO(string Name, TYPE Type, CLASS Class, uint Ttl, string RMailBx, string EMailBx)
			: base(Name, Type, Class, Ttl)
		{
			this.rMailBx = RMailBx;
			this.eMailBx = EMailBx;
		}

		/// <summary>
		/// Specifies a mailbox which is
		/// responsible for the mailing list or mailbox.
		/// </summary>
		public string RMailBx => this.rMailBx;

		/// <summary>
		/// Specifies a mailbox which is to
		/// receive error messages related to the mailing list or
		/// mailbox specified by the owner of the MINFO RR(similar
		/// to the ERRORS-TO: field which has been proposed).
		/// </summary>
		public string EMailBx => this.eMailBx;

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.rMailBx + "\t" + this.eMailBx;
		}
	}
}
