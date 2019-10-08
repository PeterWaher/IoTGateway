using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// DURATION
	/// </summary>
	public class Asn1Duration : Asn1Type
	{
		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "Duration";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;
	}
}
