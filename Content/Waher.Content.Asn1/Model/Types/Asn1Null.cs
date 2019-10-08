using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// NULL
	/// </summary>
	public class Asn1Null : Asn1Type
	{
		/// <summary>
		/// NULL
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Null(bool Implicit)
			: base(Implicit)
		{
		}

	}
}
