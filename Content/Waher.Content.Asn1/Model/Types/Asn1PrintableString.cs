using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// PrintableString
	/// a-z, A-Z, ' () +,-.?:/= and SPACE
	/// </summary>
	public class Asn1PrintableString : Asn1StringType
	{
		/// <summary>
		/// PrintableString
		/// a-z, A-Z, ' () +,-.?:/= and SPACE
		/// </summary>
		public Asn1PrintableString()
			: base()
		{
		}
	}
}
