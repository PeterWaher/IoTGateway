using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// GeneralString
	/// all registered graphic and character sets plus SPACE and DELETE
	/// </summary>
	public class Asn1GeneralString : Asn1StringType
	{
		/// <summary>
		/// GeneralString
		/// all registered graphic and character sets plus SPACE and DELETE
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1GeneralString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
