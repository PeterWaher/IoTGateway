using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mail information about a host. (Experimental)
	/// </summary>
	public class MINFO : ResourceRecord
	{
		private string rMailBx;
		private string eMailBx;

		/// <summary>
		/// Mail information about a host. (Experimental)
		/// </summary>
		public MINFO()
			: base()
		{
			this.rMailBx = string.Empty;
			this.eMailBx = string.Empty;
		}

		/// <summary>
		/// Mail information about a host. (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public MINFO(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl)
		{
			this.rMailBx = DnsClient.ReadName(Data);
			this.eMailBx = DnsClient.ReadName(Data);
		}

		/// <summary>
		/// Specifies a mailbox which is
		/// responsible for the mailing list or mailbox.
		/// </summary>
		[DefaultValueStringEmpty]
		public string RMailBx
		{
			get => this.rMailBx;
			set => this.rMailBx = value;
		}

		/// <summary>
		/// Specifies a mailbox which is to
		/// receive error messages related to the mailing list or
		/// mailbox specified by the owner of the MINFO RR(similar
		/// to the ERRORS-TO: field which has been proposed).
		/// </summary>
		[DefaultValueStringEmpty]
		public string EMailBx
		{
			get => this.eMailBx;
			set => this.eMailBx = value;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.rMailBx + "\t" + this.eMailBx;
		}
	}
}
