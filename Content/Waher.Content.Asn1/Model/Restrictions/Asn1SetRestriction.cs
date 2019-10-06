using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class for set-based restrictions
	/// </summary>
	public abstract class Asn1SetRestriction : Asn1Restriction
	{
		private readonly Asn1Values set;

		/// <summary>
		/// Abstract base class for set-based restrictions
		/// </summary>
		/// <param name="Set">Set</param>
		public Asn1SetRestriction(Asn1Values Set)
		{
			this.set = Set;
		}

		/// <summary>
		/// Set
		/// </summary>
		public Asn1Values Set => this.set;
	}
}
