using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class for set-based restrictions
	/// </summary>
	public class Asn1SetRestriction : Asn1Restriction
	{
		private readonly Asn1Set set;

		/// <summary>
		/// Abstract base class for set-based restrictions
		/// </summary>
		/// <param name="Set">Set</param>
		public Asn1SetRestriction(Asn1Set Set)
		{
			this.set = Set;
		}

		/// <summary>
		/// Set
		/// </summary>
		public Asn1Set Set => this.set;
	}
}
