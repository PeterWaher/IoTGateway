using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Places restrictions on when public contracts cease to be legally binding.
	/// </summary>
	public class FilterTo : SearchFilter
	{
		/// <summary>
		/// Places restrictions on when public contracts cease to be legally binding.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public FilterTo(params SearchFilterOperand[] Operands)
				: base(Operands)
		{
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "to";
	}
}
