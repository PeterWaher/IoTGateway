using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Contains information about a DNS Question
	/// </summary>
	public class Question
	{
		private readonly string qNAME;
		private readonly QTYPE qTYPE;
		private readonly QCLASS qCLASS;

		/// <summary>
		/// Contains information about a DNS Question
		/// </summary>
		/// <param name="QNAME">Qujery Name</param>
		/// <param name="QTYPE">Query Type</param>
		/// <param name="QCLASS">Query Class</param>
		public Question(string QNAME, QTYPE QTYPE, QCLASS QCLASS)
		{
			this.qNAME = QNAME;
			this.qTYPE = QTYPE;
			this.qCLASS = QCLASS;
		}

		/// <summary>
		/// Query Name
		/// </summary>
		public string QNAME => this.qNAME;

		/// <summary>
		/// Query TYPE
		/// </summary>
		public QTYPE QTYPE => this.qTYPE;

		/// <summary>
		/// Query CLASS
		/// </summary>
		public QCLASS QCLASS => this.qCLASS;

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.qNAME + "\t" + this.qTYPE.ToString() + "\t" + this.qCLASS.ToString();
		}
	}
}
