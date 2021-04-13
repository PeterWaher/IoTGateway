using System;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named numeric-valued tag greater than a given value.
	/// </summary>
	public class NumericTagGreaterThan : SearchOperatorNumeric
	{
		/// <summary>
		/// Filters things with a named numeric-valued tag greater than a given value.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public NumericTagGreaterThan(string Name, double Value)
			: base(Name, Value)
		{
		}

		internal override string TagName
		{
			get { return "numGt"; }
		}
	}
}
