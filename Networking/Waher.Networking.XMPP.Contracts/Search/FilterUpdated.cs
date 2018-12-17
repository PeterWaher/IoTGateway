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
	}
}
