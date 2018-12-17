using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Places restrictions on when public contracts were created.
	/// </summary>
	public class FilterDuration : SearchFilter
	{
		/// <summary>
		/// Places restrictions on when public contracts were created.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public FilterDuration(params SearchFilterOperand[] Operands)
				: base(Operands)
		{
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "duration";

		/// <summary>
		/// Sort order
		/// </summary>
		internal override int Order => 10;

		/// <summary>
		/// Maximum number of occurrences in a search.
		/// </summary>
		internal override int MaxOccurs => 1;
	}
}
