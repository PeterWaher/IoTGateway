using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// GeneralizedTime
	/// </summary>
	public class Asn1GeneralizedTime : Asn1Type
	{
		/// <summary>
		/// GeneralizedTime
		/// </summary>
		public Asn1GeneralizedTime()
			: base()
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
