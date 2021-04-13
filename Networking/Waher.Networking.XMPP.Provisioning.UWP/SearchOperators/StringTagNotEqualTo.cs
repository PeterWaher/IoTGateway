using System;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named string-valued tag not equal to a given value.
	/// </summary>
	public class StringTagNotEqualTo : SearchOperatorString
	{
		/// <summary>
		/// Filters things with a named string-valued tag not equal to a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public StringTagNotEqualTo(string Name, string Value)
			: base(Name, Value)
		{
		}

		internal override string TagName
		{
			get { return "strNEq"; }
		}
	}
}
