using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag lesser than a given value.
	/// </summary>
	public class StringTagLesserThan : SearchOperatorString
	{
		/// <summary>
		/// Filters things with a named string-valued tag lesser than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagLesserThan(string Name, string Value)
			: base(Name, Value)
		{
		}

		internal override string TagName
		{
			get { return "strLt"; }
		}
	}
}
