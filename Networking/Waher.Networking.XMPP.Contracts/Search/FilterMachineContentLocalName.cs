using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for search filters relating to the local name of the machine-readable content of contracts.
	/// </summary>
	public abstract class FilterMachineContentLocalName : SearchFilter
	{
		/// <summary>
		/// Abstract base class for search filters relating to the local name of the machine-readable content of contracts.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public FilterMachineContentLocalName(params SearchFilterOperand[] Operands)
			: base(Operands)
		{
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "localName";

		/// <summary>
		/// Sort order
		/// </summary>
		internal override int Order => 1;

		/// <summary>
		/// Maximum number of occurrences in a search.
		/// </summary>
		internal override int MaxOccurs => 1;
	}
}
