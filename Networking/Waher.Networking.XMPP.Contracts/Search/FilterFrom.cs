using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Places restrictions on when public contracts became legally binding.
	/// </summary>
	public class FilterFrom : SearchFilter
	{
		/// <summary>
		/// Places restrictions on when public contracts became legally binding.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public FilterFrom(params SearchFilterOperand[] Operands)
				: base(Operands)
		{
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "from";
	}
}
