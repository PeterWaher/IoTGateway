using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Places restrictions on when public contracts were last updated.
	/// </summary>
	public class FilterUpdated : SearchFilter
	{
		/// <summary>
		/// Places restrictions on when public contracts were last updated.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public FilterUpdated(params SearchFilterOperand[] Operands)
				: base(Operands)
		{
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "updated";

		/// <summary>
		/// Sort order
		/// </summary>
		internal override int Order => 7;

		/// <summary>
		/// Maximum number of occurrences in a search.
		/// </summary>
		internal override int MaxOccurs => 1;
	}
}
