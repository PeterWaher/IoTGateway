using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Iso646String
	/// </summary>
	public class Asn1Iso646String : Asn1StringType
	{
		/// <summary>
		/// Iso646String
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Iso646String(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
