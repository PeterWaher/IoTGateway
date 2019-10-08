using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// T61String
	/// </summary>
	public class Asn1T61String : Asn1StringType
	{
		/// <summary>
		/// T61String
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1T61String(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
