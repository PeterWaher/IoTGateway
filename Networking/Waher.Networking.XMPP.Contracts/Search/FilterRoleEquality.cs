using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts defining a role equal to this value.
	/// </summary>
	public class FilterRoleEquality : FilterRole
	{
		/// <summary>
		/// Return public contracts defining a role equal to this value.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterRoleEquality(string Value)
			: base(new EqualToString(Value))
		{
		}
	}
}
