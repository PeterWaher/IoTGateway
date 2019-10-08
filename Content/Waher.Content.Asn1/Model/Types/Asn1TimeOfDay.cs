using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// TIME-OF-DAY
	/// </summary>
	public class Asn1TimeOfDay : Asn1Type
	{
		/// <summary>
		/// TIME-OF-DAY
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1TimeOfDay(bool Implicit)
			: base(Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "TimeSpan";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;
	}
}
