using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts based on a template that is equal to this value.
	/// </summary>
	public class FilterTemplateEquality : FilterTemplate
	{
		/// <summary>
		/// Return public contracts based on a template that matches this regular expression.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterTemplateEquality(string Value)
			: base(new EqualToString(Value))
		{
		}
	}
}
