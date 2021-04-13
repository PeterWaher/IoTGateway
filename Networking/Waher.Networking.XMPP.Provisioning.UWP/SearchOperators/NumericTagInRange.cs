using System;

namespace Waher.Networking.XMPP.Provisioning.SearchOperators
{
	/// <summary>
	/// Filters things with a named numeric-valued tag within a given range.
	/// </summary>
	public class NumericTagInRange : NumericTagRange 
	{
		/// <summary>
		/// Filters things with a named numeric-valued tag within a given range.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="MinIncluded">If the minimum value is included in the range.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="MaxIncluded">If the maximum value is included in the range.</param>
		public NumericTagInRange(string Name, double Min, bool MinIncluded, double Max, bool MaxIncluded)
			: base(Name, Min, MinIncluded, Max, MaxIncluded)
		{
		}

		internal override string TagName
		{
			get { return "numRange"; }
		}
	}
}
