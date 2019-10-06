using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Array of values
	/// </summary>
	public class Asn1Array : Asn1Value
	{
		private readonly Asn1Value[] values;

		/// <summary>
		/// Array of values
		/// </summary>
		/// <param name="Values">Values</param>
		public Asn1Array(Asn1Value[] Values)
		{
			this.values = Values;
		}

		/// <summary>
		/// Value
		/// </summary>
		public Asn1Value[] Values => this.values;
	}
}
