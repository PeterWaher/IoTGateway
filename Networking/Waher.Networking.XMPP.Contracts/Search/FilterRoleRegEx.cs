using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts defining a role matching this regular expression.
	/// </summary>
	public class FilterRoleRegEx : FilterRole
	{
		/// <summary>
		/// Return public contracts defining a role matching this regular expression.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterRoleRegEx(string Value)
			: base(new LikeRegEx(Value))
		{
		}
	}
}
