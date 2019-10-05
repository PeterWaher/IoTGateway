using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// PATTERN
	/// </summary>
	public class Asn1Pattern : Asn1Restriction
	{
		private readonly Asn1Node regex;

		/// <summary>
		/// PATTERN
		/// </summary>
		/// <param name="RegularExpression">Regular Expression</param>
		public Asn1Pattern(Asn1Node RegularExpression)
		{
			this.regex = RegularExpression;
		}

		/// <summary>
		/// Regular Expression
		/// </summary>
		public Asn1Node RegularExpression => this.regex;
	}
}
