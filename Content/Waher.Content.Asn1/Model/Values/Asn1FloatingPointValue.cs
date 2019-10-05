using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Floating-point value
	/// </summary>
	public class Asn1FloatingPointValue : Asn1Value
	{
		private readonly double value;

		/// <summary>
		/// Floating-point value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1FloatingPointValue(double Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public double Value => this.value;
	}
}
