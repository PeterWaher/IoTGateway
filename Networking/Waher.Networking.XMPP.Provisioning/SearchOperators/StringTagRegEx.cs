using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag matching a regular expression.
	/// </summary>
	public class StringTagRegEx : SearchOperatorString
	{
		/// <summary>
		/// Filters things with a named string-valued tag matching a regular expression.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagRegEx(string Name, string Value)
			: base(Name, Value)
		{
		}

		internal override string TagName
		{
			get { return "strRegEx"; }
		}
	}
}
