using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Represents an ACME problem report.
	/// </summary>
	public class AcmeException : Exception
	{
		private readonly string type;
		private readonly int? status;
		private readonly AcmeException[] subproblems;

		internal AcmeException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Detail)
		{
			this.type = Type;
			this.status = Status;
			this.subproblems = Subproblems;
		}

		/// <summary>
		/// ACME exception type.
		/// </summary>
		public string Type => this.type;

		/// <summary>
		/// Status code, if reported.
		/// </summary>
		public int? Status => this.status;

		/// <summary>
		/// Sub-problems, if any.
		/// </summary>
		public AcmeException[] Subproblems => this.subproblems;
	}
}
