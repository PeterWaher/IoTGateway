using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Boolean value
	/// </summary>
	public class Asn1BooleanValue : Asn1Value
	{
		private readonly bool value;

		/// <summary>
		/// Boolean value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1BooleanValue(bool Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public bool Value => this.value;
	}
}
