using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Integer value
	/// </summary>
	public class Asn1IntegerValue : Asn1Value
	{
		private readonly long value;

		/// <summary>
		/// Integer value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1IntegerValue(long Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public long Value => this.value;
	}
}
