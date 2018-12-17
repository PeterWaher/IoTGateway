using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Abstract base class for search filters relating to the template of contracts.
	/// </summary>
	public abstract class FilterTemplate : SearchFilter
	{
		/// <summary>
		/// Abstract base class for search filters relating to the template of contracts.
		/// </summary>
		/// <param name="Operands">Operands</param>
		public FilterTemplate(params SearchFilterOperand[] Operands)
				: base(Operands)
		{
		}

		/// <summary>
		/// Local XML element name of filter.
		/// </summary>
		public override string ElementName => "template";
	}
}
