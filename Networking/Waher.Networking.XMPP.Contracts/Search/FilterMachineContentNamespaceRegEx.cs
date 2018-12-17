using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts in which the namespace of the contents matches this regular expression.
	/// </summary>
	public class FilterMachineContentNamespaceRegEx : FilterMachineContentNamespace
	{
		/// <summary>
		/// Return public contracts in which the namespace of the contents matches this regular expression.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterMachineContentNamespaceRegEx(string Value)
			: base(new LikeRegEx(Value))
		{
		}
	}
}
