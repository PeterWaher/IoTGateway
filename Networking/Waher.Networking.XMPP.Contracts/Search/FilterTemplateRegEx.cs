using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.Search
{
	/// <summary>
	/// Return public contracts based on a template that matches this regular expression.
	/// </summary>
	public class FilterTemplateRegEx : FilterTemplate
	{
		/// <summary>
		/// Return public contracts based on a template that matches this regular expression.
		/// </summary>
		/// <param name="Value">String value</param>
		public FilterTemplateRegEx(string Value)
			: base(new LikeRegEx(Value))
		{
		}
	}
}
