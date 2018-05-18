using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Visit the "instance" URL and take actions specified there
	/// </summary>
	public class AcmeUserActionRequiredException : AcmeException
	{
		private readonly Uri link;
		private readonly Uri termsOfService;

		internal AcmeUserActionRequiredException(string Type, string Detail, int? Status, 
			AcmeException[] Subproblems, Uri Link, Uri TermsOfService)
			: base(Type, Detail, Status, Subproblems)
		{
			this.link = Link;
			this.termsOfService = TermsOfService;
		}

		/// <summary>
		/// Link a client should direct a human user to visit in order for instructions on 
		/// how to agree to the terms
		/// </summary>
		public Uri Link => this.link;

		/// <summary>
		/// Link a client should direct a human user to visit in order for instructions on 
		/// how to agree to the terms
		/// </summary>
		public Uri TermsOfService => this.termsOfService;
	}
}
