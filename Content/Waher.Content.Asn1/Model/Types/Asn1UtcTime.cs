using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// UTCTime
	/// </summary>
	public class Asn1UtcTime : Asn1Type
	{
		/// <summary>
		/// UTCTime
		/// </summary>
		public Asn1UtcTime()
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
