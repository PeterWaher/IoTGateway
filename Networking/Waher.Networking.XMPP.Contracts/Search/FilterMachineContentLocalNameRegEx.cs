using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts in which the local name of the contents matches this regular expression.
	/// </summary>
	public class FilterMachineContentLocalNameRegEx : FilterMachineContentLocalName
	{
		/// <summary>
		/// Return public contracts in which the local name of the contents matches this regular expression.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterMachineContentLocalNameRegEx(string Value)
			: base(new LikeRegEx(Value))
		{
		}
	}
}
