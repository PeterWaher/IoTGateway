using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Set of values
	/// </summary>
	public class Asn1Values : Asn1Value
	{
		private readonly Asn1Value[] values;

		/// <summary>
		/// Set of values
		/// </summary>
		/// <param name="Values">Values</param>
		public Asn1Values(Asn1Value[] Values)
		{
			this.values = Values;
		}

		/// <summary>
		/// Value
		/// </summary>
		public Asn1Value[] Values => this.values;
	}
}
