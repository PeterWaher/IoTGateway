using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// BmpString (utf-16-be encoded string)
	/// Basic Multilingual Plane of ISO/IEC/ITU 10646-1
	/// </summary>
	public class Asn1BmpString : Asn1StringType
	{
		/// <summary>
		/// BmpString (utf-16-be encoded string)
		/// Basic Multilingual Plane of ISO/IEC/ITU 10646-1
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1BmpString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
