using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// String value
	/// </summary>
	public class Asn1StringValue : Asn1Value
	{
		private readonly string value;

		/// <summary>
		/// String value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1StringValue(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public string Value => this.value;
	}
}
