using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts in which the namespace of the contents is equal to this value.
	/// </summary>
	public class FilterMachineContentNamespaceEquality : FilterMachineContentNamespace
	{
		/// <summary>
		/// Return public contracts in which the namespace of the contents is equal to this value.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterMachineContentNamespaceEquality(string Value)
			: base(new EqualToString(Value))
		{
		}
	}
}
