using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class for value-based restrictions
	/// </summary>
	public abstract class Asn1ValueRestriction : Asn1Restriction
	{
		private readonly Asn1Value value;

		/// <summary>
		/// Abstract base class for set-based restrictions
		/// </summary>
		/// <param name="Value">Item</param>
		public Asn1ValueRestriction(Asn1Value Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Item
		/// </summary>
		public Asn1Node Value => this.value;
	}
}
